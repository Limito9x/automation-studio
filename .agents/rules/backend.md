# Backend Rules & Guidelines (`api/`)

Quy tắc áp dụng bắt buộc khi xây dựng hoặc sửa đổi mã nguồn trong thư mục `api/`.

---

## 1. Cấu trúc Module và Vertical Slice Architecture (VSA)

1. **Cấu trúc lồng Project và Đặt tên (Nesting & Naming):**
   - Đặt folder ngoài cùng mang tên Module (vd: `api/src/Modules/Orders`).
   - Bên trong folder đó, tạo thư mục con mang tiền tố project: `Automation.Orders`.
   - File project (.csproj) và Root Namespace mang tên: `Automation.Orders.csproj` và `Automation.Orders`.
   - Ưu tiên gói gọn trong 1 project này (Domain, Infrastructure, API, Application), KHÔNG chia nhỏ thành các tầng layer khác nhau (như `Orders.Domain.csproj`). Trừ khi cần tách riêng giao tiếp (Contracts), có thể tạo project `Automation.Orders.Contracts` nằm ngang hàng.

2. **Cấu trúc thư mục của Module:**
   - `Domain/`: Entities, Value Objects, Domain Events, Interfaces của riêng module.
   - `Infrastructure/`: EF Core Configurations, DbContext (nếu có riêng), Repositories, External integrations.
   - `Features/`: Các tính năng tổ chức theo chiều dọc (Vertical Slices).
   - `Shared/`: DTOs dùng chung nội bộ module.

3. **Kiến trúc tính năng theo chiều dọc (Vertical Slice - VSA):**
   - Đặt tính năng trong thư mục: `Features/<FeatureGroup>/<FeatureName>/` (VD: `Features/Orders/CreateOrder/`).
   - Tại `Features/<FeatureGroup>/`, bắt buộc có một file Endpoint Group (VD: `OrdersGroup.cs`) kế thừa `Group` của FastEndpoints. Luôn thêm `.WithTags("<Tên_Group>")` để Swagger hiển thị nhóm.
   - Một slice tính năng hoàn chỉnh nằm CÙNG một thư mục:
     - `*Command.cs` hoặc `*Query.cs`: Request model đầu vào.
     - `*Endpoint.cs`: API Endpoint kế thừa từ `Endpoint` của FastEndpoints, khai báo `Group<FeatureGroup>()`.
     - `*Handler.cs`: Xử lý nghiệp vụ chính bằng Wolverine handler.
     - `*Validator.cs`: Khai báo FluentValidation cho request.

---

## 2. Quy Chuẩn Code & Thư Viện

4. **Sử dụng CLI để tạo Module và CRUD:**
   - TUYỆT ĐỐI KHÔNG tự tạo tay các thư mục và file cho Module mới hoặc tính năng CRUD.
   - Luôn sử dụng CLI tại `api/tools/Automation.Cli`:
     - Tạo Module: `dotnet run --project api/tools/Automation.Cli -- add-module <TênModule>`
     - Tạo CRUD: `dotnet run --project api/tools/Automation.Cli -- add-crud <TênModule> <TênEntity>`

5. **Global Using Check:**
   - Trước khi viết code, LUÔN kiểm tra `GlobalUsing.cs` ở module (vd: `api/src/Modules/<ModuleName>/GlobalUsing.cs`) để xem những namespace đã import sẵn. Tránh `using` thừa.

6. **Tôn trọng Triết Lý Kiến Trúc:**
   - Đọc và tuân thủ tài liệu [docs/backend/ARCHITECTURE.md](file:///d:/FullStack/Automation/docs/backend/ARCHITECTURE.md). Tránh rườm rà (Ceremony), đề cao Use Case (VSA).

7. **Sử dụng Mapster cho Data Mapping:**
   - LUÔN dùng Mapster (`.Adapt<TDto>()`, `.ProjectToType<TDto>()`) để ánh xạ Entities và DTOs. Không map tay từng trường trừ khi bắt buộc.

8. **Quy định về Access Modifiers (Sử dụng public):**
   - Mọi class, interface, enum, struct trong Backend (Wolverine Handlers, Endpoints, DbContexts, Configurations, Validators, DTOs, Commands, Queries, Entities) BẮT BUỘC để ở mức `public` để tránh lỗi Assembly Scanning và DI registration.

9. **Tổ Chức Cấu Hình Liên Module:**
   - Khi cấu hình giao tiếp giữa các module, không viết rời rạc trong `ConfigureServices` của `*Module.cs`. Tạo Extension Methods riêng trong thư mục `Extensions/` của module (vd: `IdentityAssetExtensions.cs`).

10. **Chuẩn Hóa Phân Quyền (Permissions):**
    - Định nghĩa quyền tập trung trong thư mục `Constants` qua `<Module>Permissions.cs`.
    - Inner class kế thừa `BaseCrudPermission` hoặc `BasePermission`, bộc lộ static properties.
    - Module class implement `IPermissionModule` và trả về `new Constants.<Module>Permissions().GetPermissions();`.
    - Endpoint sử dụng `.Permissions(P.<FeatureName>.<Action>);` thay vì `AllowAnonymous()`.

11. **Quy tắc Kiến Trúc Shared Kernel:**
    - Tuân thủ sự phân tách Abstraction (interfaces, base classes, models) và Infrastructure (implementations, service registration).
    - Các extension cho WebApp/Services phải đặt riêng trong `Automation.SharedKernel.Extensions`.

12. **Endpoint Return Types (Không bọc Result<T>):**
    - Kiểu trả về `TResponse` của `Endpoint<TRequest, TResponse>` tuyệt đối KHÔNG ĐƯỢC bọc trong `Result` hay `Result<T>`. Trả trực tiếp kiểu dữ liệu thô (ví dụ: `CursorPage<NotificationDto>`, `RoleDto`) để Orval sinh TypeScript model chính xác ở Frontend.
    - Logic lỗi unwrapped thông qua `.SendResultAsync()`.

13. **Transaction Control trong Wolverine Handlers:**
    - Handlers ghi dữ liệu (Write/Mutation) hoặc gọi chéo module: BẮT BUỘC khai báo `[Transactional(typeof(<ModuleName>DbContext))]` trỏ tới DbContext của CHÍNH MODULE ĐÓ. Tuyệt đối không reference DbContext của module khác.
    - Handlers truy vấn (Query / Read-only): BẮT BUỘC khai báo `[NonTransactional]` để tránh mở transaction thừa.

14. **Khai báo Route trong FastEndpoints Group:**
    - Khi `Group` đã định nghĩa route prefix (vd: `Configure("pipelines")`), route trong endpoint con CHỈ ĐƯỢC CHỨA đường dẫn tương đối (vd: `Post("{PipelineId:guid}/nodes")`). TUYỆT ĐỐI KHÔNG lặp lại tiền tố `/api/pipelines/...`.

15. **API cho Canvas / Đồ thị Tương tác:**
    - TUYỆT ĐỐI KHÔNG thiết kế 1 API monolithic nhận toàn bộ đồ thị để lưu mỗi khi có thay đổi nhỏ.
    - Phân rã thành Granular APIs (AddNode, UpdateNodePosition, ConnectEdge, DeleteEdge, UpdatePinValue...).
