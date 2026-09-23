---
name: build_frontend_table
description: Hướng dẫn tiêu chuẩn để xây dựng Data Table (Table Component + Custom Hook) theo đúng kiến trúc của dự án.
---

# Build Frontend Table

Khi được yêu cầu xây dựng một Data Table cho một Feature (ví dụ: Users, Roles, Settings), BẮT BUỘC phải tuân thủ nghiêm ngặt các quy tắc tách biệt Component và Hook như sau:

## 1. Tách biệt Hook và Component UI
Mọi Table phải bao gồm 2 file:
- `use{Feature}Table.tsx`: Chứa toàn bộ logic định nghĩa cột, kết nối store, gọi API.
- `{Feature}Table.tsx`: Component UI "câm" (dumb component) chỉ nhận props và render.

## 2. Quy tắc cho Table Component UI (`{Feature}Table.tsx`)
- **Bắt buộc** sử dụng `<BaseTable>` từ `@/components/table/BaseTable`.
- **KHÔNG** sử dụng `<DataTable>` thô hoặc viết thêm logic `useMemo`, `useState` ở đây.
- Component chỉ nhận các props: `table`, `columns`, `isLoading` (và tuỳ chọn `caption`).

## 3. Quy tắc cho Custom Hook (`use{Feature}Table.tsx`)
- Sử dụng `useMemo` để định nghĩa `columns` (kiểu `ColumnDef<T>[]`).
- **Meta & Icons:** Trong định nghĩa cột, luôn bổ sung `meta: { label: string, icon: LucideIcon }` để hỗ trợ hiển thị Column Toggle Header.
- **Actions Column:** KHÔNG tự render các nút Button rời rạc. Phải trả về `<DataTableRowActions actions={[...]} />`. Các action (Thêm, Sửa, Xóa) phải gọi `openDialog` từ `useDialogStore`.
- Sử dụng `useDataTable` (từ `@/lib/useDataTable` hoặc `@/hooks/useDataTable`) để khởi tạo đối tượng `table`. Hook này phải trả về `{ table, columns }`.
