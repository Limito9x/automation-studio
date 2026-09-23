import os
import sys
import json
import time
import shutil
import logging
import zipfile
import subprocess
import pika

from core.config import load_config, RABBITMQ_HOST, RABBITMQ_PORT, RABBITMQ_USER, RABBITMQ_PASSWORD, RABBITMQ_VHOST
from commands.detect_environment import scan_all_executors

logger = logging.getLogger("inspector_worker")
logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")

QUEUE_INSPECT_TASKS = "tasks.inspect"
QUEUE_INSPECT_RESULTS = "inspection_results"

class InspectorConsumer:
    """
    RabbitMQ consumer 2 chiều:
    - Lắng nghe queue 'tasks.inspect' để nhận job.
    - Tiến hành cache script, thực thi subprocess headless (Blender/Python).
    - Phản hồi kết quả bằng message trực tiếp vào queue 'inspection_results'.
    """

    def __init__(self, host: str = None, port: int = None):
        self.host = host or RABBITMQ_HOST
        self.port = port or RABBITMQ_PORT
        self.config = load_config() or {}
        self.cache_base_dir = os.path.join(os.path.expanduser("~"), ".automation", "cache", "scripts")
        os.makedirs(self.cache_base_dir, exist_ok=True)

        credentials = pika.PlainCredentials(RABBITMQ_USER, RABBITMQ_PASSWORD)
        self._connection = pika.BlockingConnection(
            pika.ConnectionParameters(
                host=self.host,
                port=self.port,
                virtual_host=RABBITMQ_VHOST,
                credentials=credentials,
                heartbeat=0,
                blocked_connection_timeout=300
            )
        )
        self._channel = self._connection.channel()
        
        # Declare queue an toàn tương thích với Wolverine RabbitMQ conventions
        queue_args = {"x-dead-letter-exchange": "wolverine-dead-letter-queue"}
        try:
            self._channel.queue_declare(queue=QUEUE_INSPECT_TASKS, durable=True, arguments=queue_args)
        except Exception:
            self._channel = self._connection.channel()
            try:
                self._channel.queue_declare(queue=QUEUE_INSPECT_TASKS, passive=True)
            except Exception:
                self._channel = self._connection.channel()
                self._channel.queue_declare(queue=QUEUE_INSPECT_TASKS, durable=True)

        try:
            self._channel.queue_declare(queue=QUEUE_INSPECT_RESULTS, durable=True, arguments=queue_args)
        except Exception:
            self._channel = self._connection.channel()
            try:
                self._channel.queue_declare(queue=QUEUE_INSPECT_RESULTS, passive=True)
            except Exception:
                self._channel = self._connection.channel()
                self._channel.queue_declare(queue=QUEUE_INSPECT_RESULTS, durable=True)

        self._channel.basic_qos(prefetch_count=1)

    def start(self):
        """Bắt đầu lắng nghe message từ tasks.inspect (Blocking loop)."""
        logger.info(f"[*] Inspector Consumer đang lắng nghe trên queue '{QUEUE_INSPECT_TASKS}'. Nhấn Ctrl+C để dừng.")
        self._channel.basic_consume(
            queue=QUEUE_INSPECT_TASKS,
            on_message_callback=self._on_task_received,
        )
        try:
            self._channel.start_consuming()
        except KeyboardInterrupt:
            logger.info("[STOP] Đã dừng Inspector Consumer.")
            self._channel.stop_consuming()
            self._connection.close()

    def _on_task_received(self, ch, method, properties, body):
        """Callback khi nhận message InspectResourceTask từ RabbitMQ."""
        start_time = time.time()
        task_data = {}
        try:
            task_data = json.loads(body.decode("utf-8"))
            inspection_id = task_data.get("InspectionId") or task_data.get("inspectionId")
            resource_version_id = task_data.get("ResourceVersionId") or task_data.get("resourceVersionId")
            executor_key = task_data.get("ExecutorKey") or task_data.get("executorKey") or "blender"
            script_url = task_data.get("ScriptUrl") or task_data.get("scriptUrl")
            script_hash = task_data.get("ScriptHash") or task_data.get("scriptHash")
            entry_point = task_data.get("EntryPoint") or task_data.get("entryPoint") or "main.py"
            resource_file_path = task_data.get("ResourceFilePath") or task_data.get("resourceFilePath")

            logger.info(f"==> Nhận task kiểm tra Inspection ID: {inspection_id} (Executor: {executor_key})")

            # 1. Chuẩn bị Script (Cache hoặc Tải về)
            script_exec_path = self._ensure_script_cached(script_url, script_hash, entry_point)

            # 2. Tìm Executable Path phù hợp trên máy
            executable_path = self._resolve_executable_path(executor_key)

            # 3. Chạy Subprocess Headless
            status, summary_message, data_json = self._run_inspection_subprocess(
                executor_key=executor_key,
                executable_path=executable_path,
                script_path=script_exec_path,
                target_file=resource_file_path,
            )

            execution_time_ms = int((time.time() - start_time) * 1000)

            # 4. Phản hồi kết quả bằng message trực tiếp vào queue inspection_results
            self._publish_result(
                inspection_id=inspection_id,
                status=status,
                summary_message=summary_message,
                data_json=data_json,
                execution_time_ms=execution_time_ms,
            )

            ch.basic_ack(delivery_tag=method.delivery_tag)
            logger.info(f"<== Hoàn thành Inspection ID: {inspection_id} trong {execution_time_ms}ms (Status: {status})")

        except Exception as e:
            execution_time_ms = int((time.time() - start_time) * 1000)
            logger.exception(f"Lỗi khi xử lý inspection task: {e}")
            inspection_id = task_data.get("InspectionId") or task_data.get("inspectionId")
            if inspection_id:
                self._publish_result(
                    inspection_id=inspection_id,
                    status=4, # Failed
                    summary_message=f"Agent Worker Execution Error: {str(e)}",
                    data_json={"error": str(e)},
                    execution_time_ms=execution_time_ms,
                )
            ch.basic_ack(delivery_tag=method.delivery_tag)

    def _ensure_script_cached(self, script_url: str, script_hash: str, entry_point: str) -> str:
        """Kiểm tra cache script theo SHA-256 hash. Nếu chưa có thì tải về và giải nén."""
        if not script_hash:
            raise ValueError("ScriptHash không được để trống")

        cached_folder = os.path.join(self.cache_base_dir, script_hash)
        entry_file_path = os.path.join(cached_folder, entry_point)

        if os.path.exists(entry_file_path):
            logger.debug(f"Sử dụng script đã cache tại: {entry_file_path}")
            return entry_file_path

        os.makedirs(cached_folder, exist_ok=True)

        if script_url:
            import urllib.request
            temp_download_path = os.path.join(cached_folder, "downloaded_script")
            logger.info(f"Đang tải script từ {script_url}...")
            
            try:
                req = urllib.request.Request(
                    script_url,
                    headers={
                        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                    }
                )
                with urllib.request.urlopen(req, timeout=30) as response, open(temp_download_path, 'wb') as out_file:
                    shutil.copyfileobj(response, out_file)
            except Exception as dl_err:
                logger.warning(f"Không thể tải script trực tiếp từ R2 URL ({dl_err}). Đang tìm kiếm fallback cục bộ...")
                # Fallback tìm trong thư mục scripts của Agent
                local_candidates = [
                    os.path.join(os.path.dirname(__file__), "scripts", "inspectors", entry_point),
                    os.path.join(os.path.dirname(__file__), "scripts", "inspectors", "daz_test_inspector.py"),
                    os.path.join(os.path.dirname(os.path.dirname(__file__)), "worker", "scripts", "inspectors", entry_point),
                ]
                found = False
                for cand in local_candidates:
                    if os.path.exists(cand):
                        shutil.copy(cand, temp_download_path)
                        found = True
                        logger.info(f"Đã sử dụng script fallback tại: {cand}")
                        break
                if not found:
                    raise dl_err

            if zipfile.is_zipfile(temp_download_path):
                with zipfile.ZipFile(temp_download_path, 'r') as zip_ref:
                    zip_ref.extractall(cached_folder)
                try:
                    os.remove(temp_download_path)
                except Exception:
                    pass
            else:
                target_script = os.path.join(cached_folder, entry_point)
                if os.path.exists(target_script):
                    os.remove(target_script)
                shutil.move(temp_download_path, target_script)
        else:
            raise FileNotFoundError(f"Script chưa có trong cache và không có ScriptUrl để tải (Hash: {script_hash})")

        if not os.path.exists(entry_file_path):
            raise FileNotFoundError(f"Không tìm thấy entry point '{entry_point}' trong thư mục script: {cached_folder}")

        return entry_file_path

    def _resolve_executable_path(self, executor_key: str) -> str:
        """Tìm đường dẫn thực thi cho executor (ưu tiên cấu hình trong agent_config.json, sau đó auto-scan)."""
        executors_cfg = self.config.get("executors") or {}
        if executor_key in executors_cfg and os.path.exists(executors_cfg[executor_key].get("executable_path", "")):
            return executors_cfg[executor_key]["executable_path"]

        candidates = scan_all_executors(executor_key)
        if candidates and len(candidates) > 0:
            return candidates[0]["executable_path"]

        if executor_key.lower() == "python":
            return sys.executable

        raise FileNotFoundError(f"Không tìm thấy phần mềm thực thi phù hợp cho executorKey='{executor_key}' trên máy trạm")

    def _run_inspection_subprocess(self, executor_key: str, executable_path: str, script_path: str, target_file: str):
        """Chạy script trong môi trường headless và thu thập stdout/stderr dạng JSON."""
        cmd = []
        if executor_key.lower() == "blender":
            cmd = [
                executable_path,
                "--background",
                "--factory-startup",
            ]
            if target_file and os.path.exists(target_file) and target_file.endswith(".blend"):
                cmd.append(target_file)
            cmd.extend([
                "--python", script_path,
                "--",
                f"--target={target_file or ''}"
            ])
        else:
            cmd = [
                executable_path,
                script_path,
                f"--target={target_file or ''}"
            ]

        logger.info(f"Chạy lệnh kiểm tra: {' '.join(cmd)}")
        result = subprocess.run(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            timeout=300 # 5 phút
        )

        stdout = result.stdout
        stderr = result.stderr

        parsed_data = None
        for line in stdout.splitlines():
            line_str = line.strip()
            if line_str.startswith("{") and line_str.endswith("}"):
                try:
                    parsed_data = json.loads(line_str)
                    break
                except Exception:
                    continue

        if parsed_data is None:
            parsed_data = {
                "stdout": stdout,
                "stderr": stderr,
                "returncode": result.returncode,
            }

        if result.returncode == 0:
            status = 2 # Passed
            summary = "Inspection completed successfully."
            if isinstance(parsed_data, dict):
                if parsed_data.get("status") == "warning" or parsed_data.get("has_warnings"):
                    status = 3 # Warning
                    summary = parsed_data.get("summary") or "Inspection completed with warnings."
                elif parsed_data.get("passed") is False or parsed_data.get("status") == "failed":
                    status = 4 # Failed
                    summary = parsed_data.get("summary") or "Validation criteria failed."
                else:
                    summary = parsed_data.get("summary") or summary
        else:
            status = 4 # Failed
            summary = f"Script execution failed with returncode {result.returncode}."

        return status, summary, parsed_data

    def _publish_result(self, inspection_id: str, status: int, summary_message: str, data_json: dict, execution_time_ms: int):
        """Bắn message kết quả vào RabbitMQ queue 'inspection_results' để Wolverine tự động route."""
        agent_id = self.config.get("agent_id") or "local-agent"

        payload = {
            "InspectionId": inspection_id,
            "AgentId": agent_id,
            "Status": status,
            "SummaryMessage": summary_message,
            "Data": data_json,
            "ExecutionTimeMs": execution_time_ms,
        }

        body = json.dumps(payload, ensure_ascii=False).encode("utf-8")

        properties = pika.BasicProperties(
            delivery_mode=2,
            content_type="application/json",
            headers={"message-type": "inspection-result"}
        )

        self._channel.basic_publish(
            exchange="",
            routing_key=QUEUE_INSPECT_RESULTS,
            body=body,
            properties=properties
        )

        logger.info(f"[*] Đã publish kết quả Inspection '{inspection_id}' vào queue '{QUEUE_INSPECT_RESULTS}' (message-type: inspection-result)")
