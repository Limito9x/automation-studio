# Automation Worker - Architecture & Execution Guide

## 1. Giới thiệu

Thư mục `workers/` chứa mã nguồn của **Automation Agent Worker**. Worker đóng vai trò là daemon client cài đặt trực tiếp trên máy trạm (local machine), kết nối với hệ thống Backend thông qua hai kênh chính:
- **RabbitMQ**: Nhận tác vụ Pipeline (`stage_tasks.{agent_id}`) và phản hồi tiến độ/kết quả (`step_progress`, `stage_results`).
- **gRPC Stream**: Duy trì heartbeat, đồng bộ cấu hình, duyệt thư mục máy trạm (`browse`), và hỗ trợ JIT resolve dữ liệu step qua `ExecutionStateClient`.

---

## 2. Kiến trúc Phân Tầng: Executor vs Stage Runner

Để đảm bảo hiệu năng cao, tránh tràn bộ nhớ (OOM) và cách ly crash từ các phần mềm đồ họa 3D (Blender, Unreal Engine), Worker áp dụng mô hình kiến trúc **Host - Subprocess** với 2 tầng rõ rệt:

```
┌────────────────────────────────────────────────────────┐
│ WORKER HOST (Process Python venv của Agent)            │
│ cli.py -> start.py -> PipelineConsumer                 │
│                                                        │
│  [ EXECUTORS ] (Quản lý tiến trình ngoài)              │
│  - BlenderExecutor                                     │
│  - PythonExecutor                                      │
│  - UnrealEngineExecutor                                │
└──────────────────────────┬─────────────────────────────┘
                           │ Subprocess Spawn (CLI flags, UTF-8 pipes)
                           ▼
┌────────────────────────────────────────────────────────┐
│ SUBPROCESS GUEST (Tiến trình Blender / Unreal / Python) │
│                                                        │
│  [ STAGE RUNNERS ] (Điều phối Step trong bộ nhớ RAM)  │
│  - stage_runner.py (Dùng cho Blender & Python thuần)  │
│  - ue_stage_runner.py (Dành riêng cho Unreal Engine)   │
└────────────────────────────────────────────────────────┘
```

### 2.1. Tầng Executor (Host Process Manager)
Nằm tại `workers/worker/executors/`:
- Kế thừa từ `BaseSubprocessExecutor` (`base.py`).
- **Nhiệm vụ:**
  - Định vị đường dẫn cài đặt của phần mềm (`blender.exe`, `UnrealEditor-Cmd.exe`, hoặc `python.exe`).
  - Xây dựng câu lệnh CLI chạy headless không splash screen (e.g. `--background`, `-unattended`, `-nosplash`).
  - Quản lý vòng đời tiến trình con: cấu hình mã hóa UTF-8, pipe buffer, timeout, kiểm tra mã thoát (exit code), thu gom log stdout/stderr.
  - Đóng gói kết quả cuối cùng gửi về queue `stage_results`.

### 2.2. Tầng Stage Runner (In-Engine Orchestrator)
Nằm tại `workers/worker/scripts/`:
- **`stage_runner.py`**:
  - Dùng chung cho cả **Blender** và **Python thuần**.
  - Kiểm tra động module `bpy`:
    ```python
    try:
        import bpy
        HAS_BPY = True
    except ImportError:
        HAS_BPY = False
    ```
  - Khi chạy dưới Blender (`HAS_BPY = True`), tự động dọn rác bộ nhớ (`bpy.ops.outliner.orphans_purge()`) sau mỗi step.
  - Nạp các module script qua `importlib`, gọi hàm `main(inputs)`, lưu output vào RAM và hỗ trợ liên kết `$ref` giữa các step liên tiếp.
- **`ue_stage_runner.py`**:
  - Dành riêng cho **Unreal Engine Headless** (`UnrealEditor-Cmd.exe`).
  - Đọc payload từ file tạm JSON qua biến môi trường `UE_STAGE_TASK_FILE` nhằm tránh lỗi pipe deadlock của Windows stdin.
  - Thu dọn bộ nhớ Unreal thông qua `unreal.SystemLibrary.collect_garbage()`.

---

## 3. Khởi Chạy và Vận Hành Worker

### 3.1. Các lệnh CLI chính
- Khởi động toàn bộ Agent (Background Worker + gRPC Stream):
  ```bash
  python cli.py start
  # hoặc chạy qua script tiện ích:
  agent.bat
  ```
- Chỉ khởi động riêng Pipeline Consumer:
  ```bash
  python cli.py worker
  ```
- Quét các phần mềm (Blender, Python) đã cài đặt trên máy:
  ```bash
  python cli.py detect-executors
  ```
- Đăng ký Agent với Backend qua Token:
  ```bash
  python cli.py register
  ```

---

## 4. Quy tắc Bất Di Bất Dịch khi Phát Triển Worker
1. **Tuyệt đối không xóa hoặc import trực tiếp từ bên ngoài vào `stage_runner.py` và `ue_stage_runner.py`**. Chúng là bootstrap script chạy độc lập bên trong engine khách.
2. **Không chặn luồng chính (Main Thread)**: Các consumer RabbitMQ phải chạy trong thread riêng biệt hoặc vòng lặp tiêu thụ an toàn để duy trì heartbeat gRPC.
3. **Mã hóa UTF-8 bắt buộc**: Mọi lệnh khởi chạy subprocess và luồng đọc stdin/stdout phải cấu hình `PYTHONUTF8=1` và `PYTHONIOENCODING=utf-8` để tránh lỗi font chữ/ký tự đặc biệt trên Windows.
