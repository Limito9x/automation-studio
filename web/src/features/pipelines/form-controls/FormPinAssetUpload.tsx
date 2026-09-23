import type { FieldValues } from "react-hook-form";
import { BaseFormField } from "@/components/form-controls/BaseFormField";
import type { BaseFormControlProps } from "@/components/form-controls/type";
import { AssetPinUpload } from "../components/canvas/AssetPinUpload";

export interface FormPinAssetUploadProps<T extends FieldValues>
  extends BaseFormControlProps<T> {
  accept?: string;
  placeholder?: string;
  disabled?: boolean;
}

export function FormPinAssetUpload<T extends FieldValues>({
  accept,
  placeholder,
  disabled,
  ...rest
}: FormPinAssetUploadProps<T>) {
  return (
    <BaseFormField
      {...rest}
      render={(field) => (
        <AssetPinUpload
          value={field.value}
          onChange={field.onChange}
          accept={accept}
          placeholder={placeholder || "Upload asset file (Preset / Script)..."}
          disabled={disabled}
        />
      )}
    />
  );
}
