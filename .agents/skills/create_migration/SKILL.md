---
name: create_migration
description: Hướng dẫn cách tạo và áp dụng Entity Framework Core Migration cho một module cụ thể.
---

# Hướng Dẫn Tạo Và Áp Dụng Migration

Dự án Modular Monolith sử dụng nhiều DbContext khác nhau cho mỗi Module. Do đó, khi tạo migration, bạn phải chỉ định rõ project và DbContext của module đó.

### 1. Lệnh Tạo Migration

Sử dụng lệnh `dotnet ef migrations add` với các tham số bắt buộc sau:
- `--project`: Trỏ tới project của Module đó (ví dụ: `src/Modules/Identity/Automation.Identity/Automation.Identity.csproj`)
- `--startup-project`: Trỏ tới `Automation.Api` (nơi chứa chuỗi kết nối và cài đặt DI).
- `--context`: Tên của lớp DbContext của module (ví dụ: `IdentityDbContext`, `BillingDbContext`).
- `--output-dir`: (Tuỳ chọn) Trỏ vào thư mục `Infrastructure/Persistence/Migrations` trong module đó để mã nguồn gọn gàng.

**Ví dụ lệnh (chạy tại thư mục gốc của giải pháp):**
```bash
dotnet ef migrations add <TênMigration> \
  --project src/Modules/<TênModule>/Automation.<TênModule>/Automation.<TênModule>.csproj \
  --startup-project src/Automation.Api/Automation.Api.csproj \
  --context <TênModule>DbContext \
  --output-dir Infrastructure/Persistence/Migrations
```

Ví dụ thực tế cho module `Billing`:
```bash
dotnet ef migrations add InitialBillingSchema \
  --project src/Modules/Billing/Automation.Billing/Automation.Billing.csproj \
  --startup-project src/Automation.Api/Automation.Api.csproj \
  --context BillingDbContext \
  --output-dir Infrastructure/Persistence/Migrations
```

### 2. Cập Nhật Database (Áp Dụng Migration)
Tương tự như khi tạo migration, lệnh update database cũng cần chỉ rõ `project`, `startup-project` và `context`:

```bash
dotnet ef database update \
  --project src/Modules/<TênModule>/Automation.<TênModule>/Automation.<TênModule>.csproj \
  --startup-project src/Automation.Api/Automation.Api.csproj \
  --context <TênModule>DbContext
```

### Lưu ý:
- Nếu bạn gặp lỗi không tìm thấy `dotnet ef`, hãy đảm bảo bạn đã cài đặt tool EF Core bằng lệnh: `dotnet tool install --global dotnet-ef`.
- Luôn đảm bảo bạn đã build thành công dự án (`dotnet build`) trước khi chạy lệnh tạo migration.


