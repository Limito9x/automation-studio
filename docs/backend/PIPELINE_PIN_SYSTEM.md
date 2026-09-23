# Pipeline Pin System & Catalogue Architecture

Tài liệu này chuẩn hóa hệ thống Chân cắm (Pin System) và API Danh mục (Pin Catalogue) của Module Pipeline Automation trong Backend.

---

## 1. Single Source of Truth (SSOT): Pin Catalogue API

Để đảm bảo tính nhất quán giữa Backend (Engine thực thi) và Frontend (Canvas/Inspector) mà không phải hardcode thông tin giao diện (màu sắc, nhãn, badge, icon), Backend cung cấp endpoint tập trung:

- **Endpoint:** `GET /api/pipelines/pin-catalogue`
- **Group:** `PinCatalogueGroup`
- **Response DTO:** `IReadOnlyList<PinTypeMetadataDto>`

Mỗi phần tử đại diện cho một loại dữ liệu chân cắm với đầy đủ metadata:
- `Code`: Tên mã chuẩn hóa (`String`, `Number`, `Boolean`, `Path`, `EntityRef`, `Asset`).
- `Label`: Tên hiển thị người dùng (`Text`, `Number`, `Boolean`, `File Path`, `Entity Reference`, `File Upload`).
- `Color`: Mã màu HEX chủ đạo dùng cho chân Handle trên Canvas (VD: `#0ea5e9` cho String, `#8b5cf6` cho Number).
- `BadgeStyle`: Lớp CSS Tailwind/shadcn dành cho Badge hiển thị trong Inspector.
- `DefaultControl`: Loại điều khiển mặc định gợi ý cho Dynamic Form Renderer (`input`, `number`, `switch`, `entity-select`, `file-upload`).

---

## 2. Chuẩn Hóa Kiểu Dữ Liệu Pin (Enums & Schema)

### 2.1. String Enum Serialization
Toàn bộ Enums liên quan đến Pin được lưu trữ và serialize dưới dạng **Chuỗi ký tự (String Enum)** trong cơ sở dữ liệu PostgreSQL và JSON DTO để tránh lỗi lệch chỉ mục số (Inverted Enum Bug):

- **`PinKind`:**
  - `"Data"`: Chân truyền nhận dữ liệu nghiệp vụ (String, Int, File, Asset...).
  - `"Exec"`: Chân điều hướng luồng thực thi (Execution Flow, màu trắng/xám).
- **`PinCardinality`:**
  - `"Single"`: Giá trị đơn nhất (scalar).
  - `"Array"`: Mảng danh sách các giá trị (`List<T>`).
  - `"Map"`: Bảng ánh xạ khóa-giá trị (`Dictionary<string, T>`).

### 2.2. Loại bỏ kiểu Pin "Variable" mơ hồ
- Kiểu dữ liệu chân cắm phản ánh **bản chất dữ liệu** (`String`, `Number`, `Boolean`, `Path`, `EntityRef`, `Asset`).
- **Không coi "Variable" là một kiểu dữ liệu nguyên thủy (Primitive Type).**
- Khi một chân cắm cần tham chiếu tới biến nội bộ của Pipeline (ví dụ node `GetVariable`, `SetVariable`), sử dụng `EntityTarget = "variable"` hoặc tên chân cắm chuẩn hóa (`VariableName`, `TargetVariable`).

---

## 3. Quy tắc Thao tác Graph & Granular APIs

1. **Granular APIs:** Tuyệt đối không thiết kế API monolithic lưu toàn bộ đồ thị (`SavePipelineGraph`). Mỗi thao tác (Thêm node, Sửa vị trí, Xóa node, Nối dây, Xóa dây) là một endpoint độc lập (`AddNode`, `UpdateNodePosition`, `DeleteNode`, `AddEdge`, `DeleteEdge`).
2. **Unloaded Collection Tracking:** Khi thêm/xóa entity con độc lập (`PipelineNode`, `PipelineEdge`), thao tác trực tiếp qua `DbSet` của entity con (`db.PipelineNodes.AddAsync`), tuyệt đối không nạp Entity cha vào ChangeTracker khi không `Include` toàn bộ collection để tránh lỗi `DbUpdateConcurrencyException`.
