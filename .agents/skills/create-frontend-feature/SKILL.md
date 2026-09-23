---
name: create-frontend-feature
description: Hướng dẫn quy trình chuẩn để tạo một Feature CRUD mới trên Frontend sử dụng công cụ Plop (hỗ trợ cả Dialog và Page layout).
---

# Feature Creation Workflow

Khi được yêu cầu tạo mới một tính năng CRUD (Resource Management), BẮT BUỘC KHÔNG ĐƯỢC tạo file thủ công. Thay vào đó, hãy sử dụng công cụ sinh code tự động của dự án.

## Bước 1: Quyết định Layout

Dự án hỗ trợ 2 loại layout cho CRUD:
1. **Dialog Layout (`dialog`)**:
   - Sử dụng cho các Entity đơn giản, ít trường thông tin (ví dụ: Role, Category, Tag).
   - Thao tác Create/Update sẽ mở ra một popup `BaseFormDialog`.
2. **Page Layout (`page`)**:
   - Sử dụng cho các Entity phức tạp, nhiều trường thông tin hoặc cần không gian hiển thị rộng (ví dụ: User, Product, Order).
   - Thao tác Create/Update/Detail sẽ chuyển hướng tới các trang rời (`FormPageShell` và `SinglePageShell`).

## Bước 2: Chạy lệnh Plop

Sử dụng terminal để chạy lệnh sinh code (không tương tác):
```bash
# Thay <featureName> bằng tên số ít, camelCase (ví dụ: product, userRole)
# Thay <layoutType> bằng "dialog" hoặc "page"
pnpm plop feature --name <featureName> --layout <layoutType>
```

## Bước 3: Triển khai chi tiết

Sau khi Plop sinh code xong, bạn chỉ cần tập trung điền logic nghiệp vụ vào các file đã được sinh ra:
1. **Schemas (`schemas/`)**: Định nghĩa `zod` schema cho API payload.
2. **Filter (`components/*Filter.ts`)**: Cấu hình các trường tìm kiếm.
3. **Table Columns (`hooks/use*Table.tsx`)**: Định nghĩa các cột cho danh sách.
4. **Forms (`components/Create*Form.tsx`, `Update*Form.tsx`)**: Thiết kế giao diện nhập liệu.
5. **Hooks (`hooks/use*s.ts`)**: Gọi hook mutation/query tương ứng từ mã API được tự sinh (`@/gen/endpoints/`).

## Lưu ý quan trọng (Convention)
- Form BẮT BUỘC dùng `react-hook-form` + `zod`.
- Table BẮT BUỘC dùng Hook `useDataTable` và KHÔNG gọi API trực tiếp trong component.
- KHÔNG thay đổi các file tự sinh trong thư mục `@/gen/`.
