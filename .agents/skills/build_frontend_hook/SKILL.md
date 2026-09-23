---
name: build_frontend_hook
description: Hướng dẫn tiêu chuẩn để xây dựng custom hook (Query và Mutation) kết nối các API được generate với giao diện cho một feature.
---

# Build Frontend Hook

Khi cần tạo custom hook cho một tính năng (ví dụ: `useUsers.ts`, `useRoles.ts`, `useSystemSettings.ts`), BẮT BUỘC tuân thủ các quy tắc sau:

## 1. Vị trí
Tất cả các hooks API của một feature phải được đặt chung trong một file: `src/features/{feature}/hooks/use{Feature}.ts`. 
KHÔNG import trực tiếp hook từ `src/gen/endpoints/` vào các file giao diện (Component/Dialog).

## 2. Các Query Hook (Get List)
- Phải import Zod schema QueryParams tương ứng từ file `.zod.ts` (ví dụ: `GetUsersQueryParams`).
- Infer type từ schema: `type FeatureQuery = z.infer<typeof GetFeatureQueryParams>`.
- Hook lấy danh sách phải nhận tham số `params: FeatureQuery`.
- Sử dụng cấu hình `query: { placeholderData: keepPreviousData }` (import `keepPreviousData` từ `@tanstack/react-query`) để hỗ trợ UX pagination mượt mà.

## 3. Các Query Hook (Get By Id)
- Tạo wrapper nhận `id: string`.
- Thêm cấu hình `query: { enabled: !!id }` để tránh gọi API rác khi ID chưa sẵn sàng.

## 4. Các Mutation Hook (Create, Update, Delete, v.v...)
- **Tuyệt đối KHÔNG** tự gọi `queryClient.invalidateQueries` bên trong Dialog/Component UI.
- Phải sử dụng utility `createMutationHook` từ `@/lib/query-utils`.
- Export mutation bằng cách bọc hook tự sinh: 
  `export const useCreateFeature = (scopeId?: string) => createMutationHook(Api.useCreateFeature, [Api.getGetFeaturesQueryKey(scopeId)])();`
- **Đối với Mutation có URL Param (như `id`, `pipelineId`, `nodeId`):**
  Bọc wrapper nhận param từ ngoài để Component gọi tiện dụng và chuẩn types:
  ```ts
  export const useUpdateItem = (parentId?: string) => {
    const queryKey = parentId ? Api.getGetParentQueryKey(parentId) : ["parents"];
    const mutation = createMutationHook(Api.useUpdateItem, [queryKey])();
    return {
      ...mutation,
      mutate: ({ itemId, data }: { itemId: string; data: UpdateItemRequest }, options?: any) =>
        mutation.mutate({ parentId: parentId!, itemId, data }, options),
      mutateAsync: ({ itemId, data }: { itemId: string; data: UpdateItemRequest }, options?: any) =>
        mutation.mutateAsync({ parentId: parentId!, itemId, data }, options),
    };
  };
  ```

## 5. Re-export Types
- BẮT BUỘC re-export toàn bộ Request/Response DTO types từ `@/gen/model` ở đầu file `use{Feature}.ts` để các components/dialogs chỉ cần import tập trung từ file hook mà không phải đi tìm trong `gen/model`.
