# Automation Frontend — Architecture & Design System

## 1. Tech Stack

| Role | Library | Ghi chú |
|---|---|---|
| Framework | React 19 + TypeScript + Vite | Tối ưu hóa hiệu năng & References Build |
| Styling | Tailwind CSS v4 + React Aria Components | Custom Shadcn Stack xây trên React Aria, không dùng Radix UI thuần |
| Routing | TanStack Router (file-based) | URL là Single Source of Truth (SSOT) cho search params / filters |
| Server State | TanStack Query v5 | Quản lý cache và polling dữ liệu realtime từ API |
| Client State | Zustand | Dùng cho Dialog Registry (`useDialogStore`) và Auth State (`useAuthStore`) |
| Form State | React Hook Form + Zod | Schema validation và Zod `.transform()` mapping |
| API Generation | Orval (`pnpm run gen:api`) | Sinh code TypeScript tự động từ OpenAPI Spec vào `src/gen/` |
| Date/Time | Temporal API + `@/lib/temporal` | Nghiêm cấm dùng native `Date` ngoại trừ UI calendar boundary |
| Icons | Lucide React | Hệ thống icon đồng bộ |

---

## 2. Project Structure

```
src/
├── gen/                        # Code tự sinh bởi Orval — TUYỆT ĐỐI KHÔNG SỬA TAY
│   ├── endpoints/              # Generated API mutation/query functions
│   └── model/                  # Generated TypeScript DTOs & Interfaces
│
├── components/                 # Component dùng chung toàn dự án
│   ├── ui/                     # React Aria Primitives (Button, Dialog, Input...)
│   ├── custom-ui/              # Custom UI nâng cao
│   │   ├── tables/             # BaseTable, JsonTreeTable (Dynamic JSON viewer)
│   │   └── overlays/           # BaseDialog, BaseFormDialog
│   └── layout/                 # AppShell, ProjectSidebar, AppHeader
│
├── lib/
│   ├── api-client.ts           # Axios client instance, auth interceptors & error toasts
│   ├── upload-utils.ts         # Direct-to-S3 upload flow với SHA-256 calculation
│   └── temporal.ts             # Temporal date manipulation helpers
│
├── features/                   # Tính năng tổ chức theo Vertical Slices
│   ├── agents/                 # Quản lý Agent, gRPC status, Scan & Active Executable Runtimes
│   ├── inspectors/             # Quản lý Project Inspectors, Script Versions (Upload .py/.zip), Rules
│   ├── inspections/            # Tab xem kết quả kiểm định Resource & JsonTreeTable Viewer
│   ├── workspaces/             # Quản lý Workspaces, Đồng bộ file & Diff Compare
│   ├── contentTypes/           # Dynamic Form Schemas & Builder
│   └── contents/               # Quản lý nội dung dữ liệu Content Items
│
└── routes/                     # TanStack Router File-Based Routing
    ├── _protected/             # Routes yêu cầu xác thực
    │   ├── _layout/            # Layout chung (GlobalSidebar)
    │   └── _project/           # Layout dự án (ProjectSidebar)
    │       └── projects/$projectId/
    │           ├── overview.tsx
    │           ├── workspaces.tsx
    │           ├── inspectors.tsx # Giao diện cấu hình Inspectors & Rules
    │           └── contents/
```

---

## 3. Data Flow & Core Patterns

```
OpenAPI Spec (Backend)
     ↓ pnpm run gen:api (Orval)
src/gen/endpoints/ & src/gen/model/
     ↓ Wrap bằng useQuery / useMutation
features/*/hooks/ (useInspectors, useAgentExecutors...)
     ↓ Cung cấp dữ liệu và actions
features/*/components/ & dialogs/ (InspectorsTable, AgentExecutorsDialog...)
     ↓ Render trên Route Pages
routes/_protected/...
```

### 3.1. Dialog Architecture & Store
- Mọi Dialog thêm/sửa/xóa đều tách thành component độc lập trong `features/*/dialogs/`.
- Không nhúng state `useState(open)` rải rác trong Page. Kích hoạt Dialog thông qua `useDialogStore` hoặc state nội bộ rõ ràng.

### 3.2. Direct-to-Storage Upload Flow (`uploadAssetFlow`)
1. User chọn file script (`.py` hoặc `.zip`) hoặc file tài nguyên.
2. `uploadAssetFlow(file)` tự động tính mã băm **SHA-256** bằng `crypto.subtle.digest`.
3. Gọi API xin Presigned Upload URL từ Module Files.
4. Gửi trực tiếp file qua HTTP `PUT` lên S3/R2 Cloud Storage.
5. Gọi `confirmUpload` và trả về `assetId` liên kết với version mới.

### 3.3. Dynamic JSON Inspection Viewer (`JsonTreeTable`)
- Tự động phân tích kết quả JSON trả về từ Agent máy trạm:
  - **Array of Objects**: Chuyển đổi thành **Sub-Table** có thể đọc và phân tích từng metric.
  - **Primitive Fields**: Hiển thị Key - Value kèm badges trạng thái (`PASSED`: Xanh, `WARNING`: Vàng, `FAILED`: Đỏ).
  - **Nested Objects**: Cung cấp cây cấu trúc thu gọn/mở rộng (*Collapsible Tree*) tích hợp ô tìm kiếm lọc dữ liệu realtime.