# Filter Panel & Field Adapters Architecture

Tài liệu này mô tả kiến trúc của hệ thống Filter Panel và cơ chế biến đổi dữ liệu giữa UI Form và API Query bằng pattern `FieldAdapter`.

## Tổng quan

Hệ thống Filter Panel được thiết kế để tách biệt hoàn toàn giữa:
1. **Dữ liệu trên UI (Form Values)**: Các giá trị nguyên thủy hoặc object phức tạp sinh ra từ các component (ví dụ: `Temporal.PlainDate`, object `{ from, to }`, mảng `string[]`).
2. **Dữ liệu gửi API (FilterField[])**: Mảng chuẩn mực của API theo định dạng `[{ field, operator, value }]`.

Việc biến đổi qua lại giữa 2 dạng này được quản lý hoàn toàn thông qua **Field Adapters**, giúp loại bỏ các câu lệnh `if/else` hardcode bên trong core của filter array builder.

## Cấu trúc thư mục

```text
src/components/filter-panel/
├── filter-types.ts         # Khai báo type (FilterFieldDef, FieldAdapter)
├── filter-adapters.ts      # Các adapter built-in (default, exactMatch, dateRange, ...)
├── filter-config.ts        # Helper define config
├── build-filter-array.ts   # Core engine gọi adapter (serialize/parse)
├── FilterPanel.tsx         # Component wrapper quản lý React Hook Form
└── AdvancedFilters.tsx     # Render UI dựa vào Dynamic Form Builder
```

## Kiến trúc `FieldAdapter`

Mỗi field trong cấu hình filter có thể gán một adapter. Một adapter bao gồm 2 hàm:

```typescript
export interface FieldAdapter<TValue = any> {
  /** Form Value -> FilterField[] để gửi lên API */
  serialize: (fieldName: string, value: TValue) => FilterField[]
  
  /** FilterField[] từ URL/API -> Form Value để điền lại vào Form */
  parse: (fields: FilterField[]) => TValue
}
```

### Các Built-in Adapters

1. **`defaultAdapter`**: Dùng cho các text input thông thường.
   - **Serialize**: Sử dụng operator `Contains` mặc định.
   - **Parse**: Lấy value từ mảng `FilterField` đầu tiên tìm thấy.

2. **`exactMatchAdapter`**: Dùng cho các lựa chọn chính xác (Select, Quick Filters).
   - **Serialize**: Sử dụng operator `Equal`.
   - **Parse**: Tương tự `defaultAdapter`.

3. **`dateRangeAdapter`**: Dùng cho component DateRangePicker (cung cấp giá trị kiểu `{ from?: Temporal.PlainDate, to?: Temporal.PlainDate }`).
   - **Serialize**: Chuyển `from` thành object có operator `GreaterThanOrEqual` và `to` thành `LessThanOrEqual`. Gắn cứng thời gian `T00:00:00Z` cho startOfDay và `T23:59:59.999Z` cho endOfDay.
   - **Parse**: Gom 2 filter field `GreaterThanOrEqual` và `LessThanOrEqual` lại thành một object có chứa `Temporal.PlainDate` để truyền ngược lại vào DateRangePicker.

## Cách sử dụng

### 1. Khai báo Schema (Zod)
Đảm bảo kiểu dữ liệu trong Zod schema trùng khớp với dữ liệu sinh ra bởi form control (Đặc biệt lưu ý `Temporal.PlainDate` khi dùng DatePicker).

```typescript
export const filterSchema = z.object({
  userName: z.string().optional(),
  createdAt: z.object({ 
    from: z.instanceof(Temporal.PlainDate).optional(), 
    to: z.instanceof(Temporal.PlainDate).optional() 
  }).optional(),
})
```

### 2. Định nghĩa UI Config
Áp dụng adapter tương ứng cho từng field.

```typescript
export const filterUIConfig = defineFilterConfig(filterSchema, {
  fields: {
    userName:  { label: "Username", fieldType: "text" }, // Tự dùng defaultAdapter
    createdAt: { label: "Created",  fieldType: "dateRange", adapter: dateRangeAdapter },
  }
})
```

## Lợi ích
- **Tính mở rộng (Scalability)**: Dễ dàng thêm các loại filter phức tạp (như mảng đa lựa chọn, number range) bằng cách viết thêm adapter thay vì sửa hàm `buildFilterArray`.
- **Khử Hardcode (Decoupling)**: Hàm `buildFilterArray` không còn phải chứa logic nhận biết từng kiểu dữ liệu cụ thể (`if (value.from) ...`). Mọi thứ được ủy quyền (delegate) qua adapter.
- **Single Source of Truth**: URL vẫn giữ trạng thái chuẩn API (`FilterField[]`). Form UI tự động nội suy qua hàm `parse` của adapter.
