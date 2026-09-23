---
name: create_module
description: Hướng dẫn cách tạo một module mới trong dự án sử dụng Automation.Cli
---

# Hướng Dẫn Tạo Module Mới

Thay vì phải tạo thủ công toàn bộ cấu trúc thư mục, các file `.csproj`, `GlobalUsing.cs`, hay `DbContext` cho một module mới, bạn bắt buộc phải sử dụng công cụ **Automation.Cli** có sẵn trong dự án.

### Các bước thực hiện:

1. **Mở terminal hoặc command prompt**.
2. **Chạy lệnh CLI**:
   Sử dụng lệnh sau từ thư mục gốc của giải pháp hoặc trỏ thẳng project CLI để sinh code:
   
   ```bash
   dotnet run --project tools/Automation.Cli -- add-module <TênModule>
   ```
   *Lưu ý:* `<TênModule>` nên được viết hoa chữ cái đầu (PascalCase), ví dụ: `Billing`, `Catalog`, `Orders`.

3. **Kiểm tra kết quả**:
   - Lệnh trên sẽ tự động tạo thư mục `src/Modules/<TênModule>/Automation.<TênModule>`.
   - File `.csproj` sẽ được thêm vào Solution (`.sln`) và tham chiếu (referenced) vào `Automation.Api`.
   - Lớp `ModuleRegistry` sẽ được tự động cập nhật để thêm `<TênModule>Module` mới vào.
   - Thư mục chứa `Infrastructure/Persistence/<TênModule>DbContext.cs` đã được sinh ra.

4. **Biên dịch thử (Build)**:
   Sau khi sinh xong, chạy lệnh `dotnet build` để đảm bảo không có lỗi tham chiếu hay cú pháp nào.


