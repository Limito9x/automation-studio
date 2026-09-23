---
name: create_form_control
description: "Hướng dẫn quy trình chuẩn để tạo một Form Control động (Dynamic Form Control) theo chuẩn VSA và Auto-Scan Registry."
---

# Hướng dẫn tạo Form Control cho Dynamic Form

Khi cần tạo một Form Control mới (ví dụ: `AddressPicker`, `UserSelect`, `RichTextEditor`), **BẮT BUỘC** phải tuân thủ luồng các bước sau để đảm bảo Form Control hoạt động đúng với kiến trúc Dynamic Form (`FormRenderer`) và cơ chế Auto-Scan `GlobalFieldRegistry`.

## Bước 1: Tạo Component
Tạo file Component tại thư mục phù hợp (ví dụ: `src/components/form-controls/FormAddress.tsx` hoặc `src/features/users/components/UserSelect.tsx`).

## Bước 2: Kế thừa Props Chuẩn
Props của Component phải kế thừa từ `BaseFormControlProps<T>` và `OmitFormProps<...>` để tương thích với `BaseFormField` và loại bỏ xung đột Type với HTML Element.

```tsx
import type { BaseFormControlProps, OmitFormProps } from "@/components/form-controls/type";
import type { FieldValues } from "react-hook-form";

// 1. Định nghĩa Props
export interface FormAddressProps<T extends FieldValues>
    extends BaseFormControlProps<T>,
    OmitFormProps<React.ComponentPropsWithoutRef<"input">> {
    // Thêm các props ĐẶC THÙ của Component ở đây (ví dụ: limit, apiEndpoint)
    country?: string; 
}
```

## Bước 3: Wrap Component với `BaseFormField`
Sử dụng `BaseFormField` để xử lý React Hook Form controller, Label, và Error Message tự động. Gom tất cả props đặc thù và native HTML props vào `...rest` để truyền xuống `BaseFormField`.

```tsx
import { BaseFormField } from "@/components/form-controls/BaseFormField";

// 2. Component chính
export function FormAddress<T extends FieldValues>({
    country = "VN",
    disabled,
    ...rest
}: FormAddressProps<T>) {
    return (
        <BaseFormField
            {...rest}
            render={(field) => (
                <input
                    {...field}
                    id={field.field_id}
                    name={field.input_name}
                    disabled={disabled}
                    // Truyền custom prop nếu cần
                    data-country={country}
                />
            )}
        />
    );
}
```

## Bước 4: Đăng Ký Auto-Scan Registry (QUAN TRỌNG NHẤT)
Để `FormRenderer` nhận diện được Type mới dưới dạng config JSON, bạn **BẮT BUỘC** phải gắn đoạn Module Augmentation và gọi `registerField` ở ngay cuối file Component vừa tạo.

```tsx
import { registerField, type ExtractConfig } from "@/lib/field-registry";

// 3. Module Augmentation & Registration
declare module "@/lib/field-registry" {
    interface GlobalFieldRegistry {
        "address": ExtractConfig<FormAddressProps<any>>
    }
}
registerField({
    type: "address",
    component: FormAddress
});
```

> **Lưu ý:** Bước này cho phép Type `address` tự động được suy luận vào Type của `config` một cách Typesafe 100%, đồng thời component tự động đăng ký với Registry mà không cần sửa file core (`field-registry.ts`).
