---
name: build_frontend_form
description: Hướng dẫn tiêu chuẩn để xây dựng Form (Create/Update) và nhúng vào Dialog hoặc Page theo kiến trúc tách biệt.
---

# Build Frontend Form

Khi xây dựng Form thêm mới hoặc cập nhật dữ liệu, BẮT BUỘC tuân thủ mô hình sau:

## 1. Chọn nơi đặt Form (Dialog hay Page)
- **Dialog (`BaseFormDialog`)**: Ưu tiên sử dụng cho các thao tác Create / Update tiêu chuẩn. Giúp User không bị chuyển trang và giữ nguyên bối cảnh bảng dữ liệu bên dưới.
- **Page riêng biệt**: Chỉ dùng cho các form cực kỳ phức tạp, có nhiều tab, wizard nhiều bước hoặc có sub-resources.

## 2. Quy tắc cho Form Component (`{Feature}Form.tsx`)
- Đây là nơi chứa UI của Form, sử dụng `react-hook-form` và `zodResolver`.
- **Tuyệt đối KHÔNG** gọi các hàm mutate API (ví dụ `useCreateUser`, `useUpdateUser`) tại đây. Thay vào đó, component phải nhận một prop `onSubmit: (data: FormData) => void` để trả dữ liệu ra ngoài.
- Trả về component `<Form>` (từ `@/components/form`) với props: `form={form}`, `formId="id-của-form"`, và `onSubmit={onSubmit}`.
- Nếu là form **Update/Edit**, component này CÓ THỂ tự gọi query hook (như `useGetUserById`) để lấy dữ liệu chi tiết, truyền vào `values` (khi load xong) và cung cấp `defaultValues` tĩnh lúc khởi tạo.

## 3. Quy tắc cho Dialog Container (`{Feature}Dialog.tsx`)
- Sử dụng `<BaseFormDialog>` (từ `@/components/custom-ui/dialog/BaseFormDialog`).
- **Data Mutator**: Dialog đóng vai trò container, nó sẽ gọi các mutation hook (`useCreateUser`) và thực thi mutate trong hàm callback `onSubmit`.
- **Cơ chế Submit**: `<BaseFormDialog>` sẽ tự động hiển thị nút Submit ở footer. Để nút này kết nối được với Form bên trong, bạn BẮT BUỘC phải truyền prop `formId` vào `<BaseFormDialog>` và prop này **phải khớp chính xác** với prop `formId` bạn truyền vào `<{Feature}Form>`.
- **Loading State**: Truyền trạng thái `.isPending` từ mutation vào prop `isPending` của `<BaseFormDialog>` để hiển thị loading trên nút Submit.
