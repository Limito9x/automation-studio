import queue
import time
import os
import string
import ctypes
from core.grpc_client import get_channel
from core.config import get_config
from core import agent_pb2
from core import agent_pb2_grpc
from commands.scan import scan_directory
from commands.detect_environment import scan_all_executors

IGNORED_NAMES = {
    ".git", "venv", "__pycache__", "node_modules", ".idea", ".vscode",
    "$recycle.bin", "system volume information", "config.msi", "recovery",
    "msocache", "perflogs"
}

def is_hidden_or_system(entry):
    name_lower = entry.name.lower()
    if name_lower in IGNORED_NAMES or entry.name.startswith((".", "$")):
        return True
    
    if os.name == 'nt':
        try:
            attrs = entry.stat(follow_symlinks=False).st_file_attributes
            # 0x02: FILE_ATTRIBUTE_HIDDEN, 0x04: FILE_ATTRIBUTE_SYSTEM
            if attrs & (0x02 | 0x04):
                return True
        except Exception:
            pass
    return False

def get_browse_directory_info(target_dir):
    """
    Chuẩn hóa và duyệt thư mục, trả về:
    (current_path, parent_path, can_navigate_up, items)
    """
    target_dir = (target_dir or "").strip()
    
    # 1. Nếu rỗng hoặc '.', '/', '\\', 'ROOT' -> Liệt kê danh sách Ổ đĩa hệ thống (Drives)
    if not target_dir or target_dir in ["/", "\\", "ROOT", "."]:
        items = []
        if os.name == 'nt':
            try:
                bitmask = ctypes.cdll.kernel32.GetLogicalDrives()
                for letter in string.ascii_uppercase:
                    if bitmask & 1:
                        drive_path = f"{letter}:/"
                        items.append(agent_pb2.BrowseItemMessage(
                            name=f"Drive ({letter}:)",
                            path=drive_path,
                            is_directory=True,
                            size_bytes=0
                        ))
                    bitmask >>= 1
            except Exception:
                for letter in ["C", "D", "E", "F"]:
                    d_path = f"{letter}:/"
                    if os.path.exists(d_path):
                        items.append(agent_pb2.BrowseItemMessage(
                            name=f"Drive ({letter}:)",
                            path=d_path,
                            is_directory=True,
                            size_bytes=0
                        ))
        else:
            items.append(agent_pb2.BrowseItemMessage(
                name="Root (/)",
                path="/",
                is_directory=True,
                size_bytes=0
            ))
        return "", "", False, items

    # 2. Chuẩn hóa đường dẫn ổ đĩa đơn thuần (vd: "D:" hoặc "d:" -> "D:/")
    if len(target_dir) == 2 and target_dir[1] == ':' and target_dir[0].isalpha():
        target_dir = f"{target_dir[0].upper()}:/"

    # Lấy đường dẫn tuyệt đối
    abs_dir = os.path.abspath(target_dir)
    normalized_path = abs_dir.replace("\\", "/")

    # Tính toán parent_path và can_navigate_up
    is_drive_root = False
    if os.name == 'nt':
        if len(normalized_path) == 3 and normalized_path[1] == ':' and normalized_path[2] == '/':
            is_drive_root = True
        elif len(normalized_path) == 2 and normalized_path[1] == ':':
            is_drive_root = True
            normalized_path += "/"
    else:
        if normalized_path == "/":
            is_drive_root = True

    if is_drive_root:
        # Nếu đang ở thư mục gốc ổ đĩa -> parent_path là rỗng (để back về danh sách ổ đĩa), can_navigate_up = True
        current_path = normalized_path
        parent_path = ""
        can_navigate_up = True
    else:
        current_path = normalized_path
        parent_abs = os.path.dirname(abs_dir)
        parent_normalized = parent_abs.replace("\\", "/")
        if os.name == 'nt' and len(parent_normalized) == 2 and parent_normalized[1] == ':':
            parent_normalized += "/"
        parent_path = parent_normalized
        can_navigate_up = True

    items = []
    if os.path.exists(abs_dir) and os.path.isdir(abs_dir):
        try:
            with os.scandir(abs_dir) as entries:
                for entry in entries:
                    if is_hidden_or_system(entry):
                        continue
                    try:
                        if not entry.is_dir():
                            continue
                        full_path = os.path.abspath(entry.path).replace("\\", "/")
                        items.append(agent_pb2.BrowseItemMessage(
                            name=entry.name,
                            path=full_path,
                            is_directory=True,
                            size_bytes=0
                        ))
                    except Exception:
                        continue
        except Exception as e:
            print(f"[WARNING] Không thể duyệt thư mục {abs_dir}: {e}")

    items.sort(key=lambda x: x.name.lower())
    return current_path, parent_path, can_navigate_up, items

def generate_messages(agent_id, msg_queue):
    # 1. Gửi message đầu tiên chứa agent_id để Backend nhận diện và lưu vào Registry
    yield agent_pb2.AgentMessage(agent_id=agent_id)
    
    # 2. Duy trì generator để gửi các message từ queue lên Backend
    while True:
        try:
            msg = msg_queue.get(timeout=1.0)
            if msg is None:
                break
            yield msg
        except queue.Empty:
            pass

def handle_server_message(agent_id, server_msg, msg_queue):
    payload_case = server_msg.WhichOneof('payload')
    
    # 1. Scan Sync (Đồng bộ tài nguyên file + hash - Tính năng riêng biệt)
    if payload_case == 'scan_command':
        scan_cmd = server_msg.scan_command
        cmd_id = scan_cmd.command_id
        target_dir = scan_cmd.directory_path or "."
        extensions = list(scan_cmd.extensions) if scan_cmd.extensions else None
        
        abs_directory = os.path.abspath(target_dir)
        print(f"\n[INFO] [SCAN SYNC] Scan tài nguyên thư mục '{abs_directory}' (Exts: {extensions})...")
        
        try:
            items = scan_directory(abs_directory, abs_directory, recursive=True, extensions=extensions)
            print(f"[OK] Scan Sync thành công {len(items)} items. Đang gửi phản hồi lên Backend (Command ID: {cmd_id})...")
            
            response = agent_pb2.CommandResponse(
                command_id=cmd_id,
                success=True,
                scan_result=agent_pb2.ScanCommandResult(items=items)
            )
            
            agent_msg = agent_pb2.AgentMessage(
                agent_id=agent_id,
                command_response=response
            )
            msg_queue.put(agent_msg)
            
        except Exception as e:
            print(f"[ERROR] Lỗi khi thực thi Scan Sync: {e}")
            response = agent_pb2.CommandResponse(
                command_id=cmd_id,
                success=False,
                error_message=str(e)
            )
            agent_msg = agent_pb2.AgentMessage(
                agent_id=agent_id,
                command_response=response
            )
            msg_queue.put(agent_msg)

    # 2. Browse Directory (Duyệt cây FOLDER tuyệt đối trên máy Agent)
    elif payload_case == 'browse_command':
        browse_cmd = server_msg.browse_command
        cmd_id = browse_cmd.command_id
        target_dir = (browse_cmd.directory_path or "").strip()
        
        try:
            current_path, parent_path, can_navigate_up, browse_items = get_browse_directory_info(target_dir)
            
            response = agent_pb2.CommandResponse(
                command_id=cmd_id,
                success=True,
                browse_result=agent_pb2.BrowseCommandResult(
                    current_path=current_path,
                    parent_path=parent_path,
                    can_navigate_up=can_navigate_up,
                    items=browse_items
                )
            )
            
            agent_msg = agent_pb2.AgentMessage(
                agent_id=agent_id,
                command_response=response
            )
            msg_queue.put(agent_msg)
            print(f"[OK] Browse Folder thành công '{current_path}' ({len(browse_items)} items, Parent: '{parent_path}', CanUp: {can_navigate_up}). Command ID: {cmd_id}.")
        except Exception as e:
            print(f"[ERROR] Lỗi khi thực thi Browse Folder: {e}")
            response = agent_pb2.CommandResponse(
                command_id=cmd_id,
                success=False,
                error_message=str(e)
            )
            agent_msg = agent_pb2.AgentMessage(
                agent_id=agent_id,
                command_response=response
            )
            msg_queue.put(agent_msg)

    # 3. Scan Executors (Dò tìm các bản cài đặt Blender/Python trên máy Agent)
    elif payload_case == 'scan_executors_command':
        scan_exec_cmd = server_msg.scan_executors_command
        cmd_id = scan_exec_cmd.command_id
        target_key = (scan_exec_cmd.executor_key or "").strip().lower()

        print(f"\n[INFO] [SCAN EXECUTORS] Đang quét các bản cài đặt Executor (Key: '{target_key or 'ALL'}')...")
        try:
            candidates = scan_all_executors(target_key if target_key else None)
            candidate_messages = [
                agent_pb2.ExecutorCandidateMessage(
                    executor_key=c["executor_key"],
                    executable_path=c["executable_path"],
                    version=c["version"]
                )
                for c in candidates
            ]

            response = agent_pb2.CommandResponse(
                command_id=cmd_id,
                success=True,
                scan_executors_result=agent_pb2.ScanExecutorsCommandResult(
                    items=candidate_messages
                )
            )

            agent_msg = agent_pb2.AgentMessage(
                agent_id=agent_id,
                command_response=response
            )
            msg_queue.put(agent_msg)
            print(f"[OK] Scan Executors thành công: tìm thấy {len(candidate_messages)} candidates. Command ID: {cmd_id}.")
        except Exception as e:
            print(f"[ERROR] Lỗi khi thực thi Scan Executors: {e}")
            response = agent_pb2.CommandResponse(
                command_id=cmd_id,
                success=False,
                error_message=str(e)
            )
            agent_msg = agent_pb2.AgentMessage(
                agent_id=agent_id,
                command_response=response
            )
            msg_queue.put(agent_msg)

def run(retry_interval=5):
    config = get_config()
    agent_id = config.get("agentId")
    if not agent_id:
        print("[ERROR] Lỗi: agentId không tồn tại trong agent_config.json. Vui lòng chạy 'agent register' trước.")
        return

    print(f"[INFO] Đang bắt đầu kết nối gRPC Stream 2 chiều tới Server cho Agent ID: {agent_id}...")

    while True:
        try:
            channel = get_channel()
            stub = agent_pb2_grpc.AgentServiceStub(channel)
            msg_queue = queue.Queue()

            # Gọi RPC Connect với stream request generator
            response_stream = stub.Connect(generate_messages(agent_id, msg_queue))
            print("[OK] Kết nối gRPC Stream THÀNH CÔNG! Đang lắng nghe message từ Backend...\n")
            
            # Lắng nghe các message được Backend push xuống
            for server_msg in response_stream:
                handle_server_message(agent_id, server_msg, msg_queue)
                
            print("[WARNING] Kết nối ngắt từ phía Server. Đang chuẩn bị thử lại...")
        except KeyboardInterrupt:
            print("\n[STOP] Đã ngắt kết nối gRPC Stream bởi người dùng.")
            break
        except Exception as e:
            print(f"[ERROR] Kết nối gRPC thất bại: {e}")
            
        print(f"[INFO] Đang thử kết nối lại sau {retry_interval} giây... (Bấm Ctrl+C để dừng)")
        try:
            time.sleep(retry_interval)
        except KeyboardInterrupt:
            print("\n[STOP] Đã ngắt kết nối gRPC Stream bởi người dùng.")
            break
