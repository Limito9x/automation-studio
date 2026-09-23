# Frontend Rules & Guidelines (`web/`)

Quy tắc áp dụng bắt buộc khi xây dựng hoặc sửa đổi mã nguồn trong thư mục `web/`.

---

## 1. Package Manager & Script Execution
- Luôn sử dụng **`pnpm`** cho toàn bộ thao tác trong thư mục `web/` (`pnpm add`, `pnpm dlx`, `pnpm run`).
- **TUYỆT ĐỐI KHÔNG** dùng `npm` hoặc `npx`.

---

## 2. API Auto-Generation với Orval
- Thư mục `web/src/gen` được tự động sinh bởi `orval` (`pnpm run gen:api`).
- **TUYỆT ĐỐI KHÔNG** viết, sửa hoặc thêm code thủ công bên trong `src/gen`.
- Mọi logic custom API, interceptors, error handling phải đặt bên ngoài (ví dụ: `src/lib/api-client.ts`).

---

## 3. Kiến Trúc Ràng Buộc Cốt Lõi
- **Styling**: Bắt buộc dùng CSS tokens của Shadcn (ví dụ: `bg-background`, `text-muted-foreground`). KHÔNG hardcode mã màu.
- **State**: Bộ lọc (filter), tìm kiếm, phân trang (pagination) BẮT BUỘC lưu trên URL thông qua TanStack Router search params. KHÔNG dùng `useState` cho URL state.
- **Forms**: Bắt buộc dùng `react-hook-form` kết hợp validation schema bằng `zod`.
- **Dates**: Bắt buộc dùng `Temporal API` (`@/lib/temporal`). CẤM dùng native JavaScript `Date` trừ khi tương tác ở ranh giới UI nguyên thủy.
- **Error Handling**: Xử lý tập trung trong `api-client.ts`. KHÔNG viết `try-catch` tùy tiện bên trong React components.

---

## 4. Kiến Trúc Custom Hooks (MANDATORY)
- Tất cả custom hooks của 1 feature phải đặt tại: `src/features/<feature>/hooks/use<Feature>.ts`.
- **TUYỆT ĐỐI KHÔNG** gọi trực tiếp `customInstance` hoặc viết raw axios thủ công khi Orval đã sinh sẵn API trong `src/gen/endpoints/`.
- **Namespace Import:** Luôn import API tự sinh theo namespace:
  ```typescript
  import * as Api from "@/gen/endpoints/<feature>/<feature>";
  ```
- **Re-export Types:** Re-export toàn bộ DTO request/response từ `@/gen/model` ở đầu file hook để các components chỉ cần import tập trung từ `hooks/use<Feature>`.
- **Query Hooks:** Bọc `useGet...` tự sinh, cấu hình `placeholderData: keepPreviousData` (cho list query) và `enabled: !!id` (cho get by id query).
- **Mutation Hooks:**
  - Bắt buộc dùng factory: `createMutationHook(Api.useMutationName, [queryKeysToInvalidate])()`.
  - Tự động invalidate cache khi mutation thành công, không gọi `queryClient.invalidateQueries` thủ công rải rác trong Dialog/Component.

---

## 5. React Aria Components & Thư Viện UI
- **Custom Shadcn Stack:** Dự án sử dụng bộ Shadcn đặc biệt xây dựng trên nền **React Aria Components**, **KHÔNG PHẢI Radix UI**.
- **Component Props Constraints:** Các thuộc tính quen thuộc của Radix như `asChild` thường **KHÔNG TỒN TẠI** hoặc hành vi khác biệt. Trước khi sử dụng props, BẮT BUỘC đọc file type definition trong `src/components/ui/`.
- **Tái Sử Dụng Base Components:** Ưu tiên tái sử dụng các component có sẵn trong `src/components/custom-ui/` và `src/components/layout/` (`BaseDialog`, `BaseFormDialog`, `ResourcePageShell`, `FormSubmitButton`). Không tự chế lại dialog hay page shell từ đầu.

---

## 6. Table & Dialog Architecture
- **Data Tables**: Table phải là "dumb" presentation component. Không truyền các action callbacks (`onEdit`, `onDelete`) từ Page xuống Table. Không đặt header hay nút "Add" bên trong Table component.
- **Action Columns**: Các cột thao tác mở trực tiếp dialog qua store: `useDialogStore(state => state.openDialog)`.
- **Dialog Extraction**: Toàn bộ dialog (Create, Update, Delete) phải là standalone component đặt trong `features/*/dialogs/` và đăng ký tập trung qua registry (`index.ts`). Không viết inline dialog state (`useState`) trong Page.

---

## 7. Dynamic Form & Canvas Form Architecture
- **Phân biệt Thuật Ngữ**: `FormRenderer` (vẽ Form cho End-User từ JSON) vs `FormBuilder` (Giao diện cấu hình cho Admin).
- **BaseFormField**: Mọi Form Control phải bọc qua `BaseFormField` và dùng `OmitFormProps` để tránh xung đột Type với React Hook Form.
- **Kiến trúc Sàn (Scoped Registry):** Khi phát triển Form Controls cho domain đặc thù (Pipeline Canvas), dùng `ScopedFieldRegistry` kế thừa từ `baseRegistry` (như `pipelineRegistry`).
- **Scope Context Injection:** Cung cấp context (`pipelineId`, `projectId`, `variables`, `edges`, `nodes`) qua Context Provider ở cấp Canvas/Page (`PipelineFormScopeProvider`). Các Form Controls bên dưới đọc qua hook `usePipelineFormScope()`.

---

## 8. Kiểm Tra Mã Nguồn (Code Verification)
- Sau khi hoàn thành code frontend, LUÔN LUÔN chạy lệnh kiểm tra lỗi type TypeScript:
  ```bash
  pnpm tsc --project tsconfig.app.json --noEmit
  # hoặc
  pnpm tsc -b
  ```
- Bất cứ lỗi TypeScript nào xuất hiện cũng phải được sửa triệt để trước khi báo cáo hoàn thành.
