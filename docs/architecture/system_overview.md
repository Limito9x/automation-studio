# Automation Studio - System Architecture Overview

## 1. Giới thiệu Tổng quan (Monorepo Overview)

Hệ thống **Automation Studio** được cấu trúc dưới dạng **Monorepo** với 3 dự án thành phần chính:

```
                    ┌────────────────────────┐
                    │      web/ (Tauri)      │
                    │  React + TanStack +    │
                    │  React Aria Shadcn     │
                    └───────────┬────────────┘
                                │ (HTTP REST / WebSocket)
                                ▼
                    ┌────────────────────────┐
                    │      api/ (.NET 10)    │
                    │   Modular Monolith     │
                    │ FastEndpoints +        │
                    │ Wolverine + EF Core    │
                    └─────┬────────────▲─────┘
                          │            │
            RabbitMQ      │            │ gRPC Stream /
            Stage Tasks   │            │ JIT Resolution
                          ▼            │ (packages/proto)
                    ┌────────────────────────┐
                    │   workers/ (Python)    │
                    │   Pipeline Consumer    │
                    │   Subprocess Executors │
                    │ (Blender / UE / Python)│
                    └────────────────────────┘
```

---

## 2. Các Thành Phần Chính

### 2.1. `web/` - Desktop & Web UI Application

- **Công nghệ:** Tauri v2, React 19, TypeScript, Vite, Tailwind CSS, Shadcn (xây dựng trên nền **React Aria Components**), TanStack Router, TanStack Query, React Hook Form + Zod, Temporal API.
- **Vai trò:**
  - Cung cấp giao diện người dùng Desktop / Web: Quản lý Assets, Pipeline Canvas, Node Inspector, Dynamic Forms, Settings.
  - Tự động sinh Client Code từ OpenAPI spec thông qua `orval` (`pnpm run gen:api` sinh vào thư mục `src/gen/`).
  - Giao tiếp thời gian thực với Backend qua WebSocket và REST API.

### 2.2. `api/` - Backend Core Engine

- **Công nghệ:** .NET 10, Vertical Slice Architecture (VSA), FastEndpoints, Wolverine (Command/Message Bus & Outbox), Entity Framework Core, PostgreSQL, RabbitMQ, gRPC Server.
- **Vai trò:**
  - Tổ chức theo kiến trúc **Modular Monolith** (`src/Modules/*`): Quản lý Identity, Assets, Pipelines, Dynamic Forms, Schemas, etc.
  - Điều phối Pipeline: Lưu trữ đồ thị Node/Edge, phân tích DAG (Directed Acyclic Graph), sinh `StageTaskMessage` và gửi vào RabbitMQ.
  - gRPC Server: Giữ kết nối 2 chiều (`Connect` stream) với các Agent Worker để duy trì heartbeat, command dispatching, và JIT data resolution.

### 2.3. `workers/` - Automation Pipeline Worker

- **Công nghệ:** Python 3.12+, Pika (RabbitMQ), gRPC, Subprocess Execution Engine.
- **Vai trò:**
  - Nhận và thực thi tác vụ Pipeline (`stage_tasks.{agent_id}`) từ RabbitMQ.
  - Phân luồng thực thi dựa trên `executor`:
    - **Blender**: Chạy tiến trình headless `blender.exe --background --python stage_runner.py` để xử lý 3D mesh, baking, export FBX.
    - **Unreal Engine**: Chạy tiến trình headless `UnrealEditor-Cmd.exe <uproject> -run=pythonscript -script=ue_stage_runner.py` để setup Material, import FBX, binding slots.
    - **Python**: Chạy script độc lập trong môi trường Python hệ thống.
  - Bắn sự kiện tiến độ (`step_progress`) và kết quả (`stage_results`) về RabbitMQ cho Backend.

---

## 3. Luồng Vận Hành Pipeline (End-to-End Execution Flow)

1. **Trigger từ UI (`web/`)**:
   - Người dùng nhấn "Run Pipeline" trên Canvas hoặc kích hoạt qua trigger.
   - Request gửi tới API endpoint `POST /api/pipelines/{id}/executions`.

2. **Backend Orchestration (`api/`)**:
   - Backend phân tích các node trong Pipeline thành các **Stages** và **Steps**.
   - Đóng gói thành `StageTaskMessage` kèm input ban đầu và lưu trạng thái vào database.
   - Publish message vào hàng đợi RabbitMQ `stage_tasks.{agent_id}`.

3. **Worker Processing (`workers/`)**:
   - `PipelineConsumer` nhận message từ RabbitMQ.
   - Lựa chọn Executor tương ứng (`BlenderExecutor`, `UnrealEngineExecutor`, hoặc `PythonExecutor`).
   - Spawn tiến trình con tương ứng, nạp bootstrap runner (`stage_runner.py` hoặc `ue_stage_runner.py`).
   - Runner trong tiến trình con thực thi tuần tự các step trong cùng một session RAM, phân giải `$ref` inputs, JIT pull qua gRPC nếu cần.
   - Báo cáo tiến độ thời gian thực về queue `step_progress`.

4. **Completion & Sync**:
   - Runner thu gom output của các step, dọn rác RAM (`purge_orphans` hoặc `collect_garbage`).
   - Executor bắt exit code và log từ stdout/stderr, đóng gói thành `StageResultMessage` gửi về queue `stage_results`.
   - Backend nhận kết quả, cập nhật database và phát thông báo qua WebSocket để UI cập nhật trạng thái các node trên Canvas.
