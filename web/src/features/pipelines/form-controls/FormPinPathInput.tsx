import type { FieldValues } from "react-hook-form";
import { BaseFormField } from "@/components/form-controls/BaseFormField";
import type { BaseFormControlProps, OmitFormProps } from "@/components/form-controls/type";
import { Input } from "@/components/ui/input";
import { Folder } from "lucide-react";
import { cn } from "@/lib/utils";

export interface FormPinPathInputProps<T extends FieldValues>
  extends BaseFormControlProps<T>,
    OmitFormProps<React.ComponentPropsWithoutRef<"input">> {}

export function FormPinPathInput<T extends FieldValues>({
  placeholder = "e.g. /workspace/models or D:\\Assets...",
  className,
  disabled,
  ...rest
}: FormPinPathInputProps<T>) {
  return (
    <BaseFormField
      {...rest}
      render={(field) => (
        <div className="relative flex items-center">
          <Folder className="absolute left-2.5 h-3.5 w-3.5 text-muted-foreground pointer-events-none" />
          <Input
            {...field}
            id={field.field_id}
            name={field.input_name}
            autoComplete={field.autoComplete}
            type="text"
            placeholder={placeholder}
            disabled={disabled}
            value={field.value ?? ""}
            className={cn("w-full pl-8 font-mono text-xs", className)}
          />
        </div>
      )}
    />
  );
}
