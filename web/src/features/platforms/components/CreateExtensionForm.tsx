import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { FormInput } from "@/components/form-controls";
import { createExtensionSchema, type CreateExtensionInput, type CreateExtensionOutput } from "../schemas/platformSchema";
import { useTranslation } from "react-i18next";

interface CreateExtensionFormProps {
    onSubmit: (data: CreateExtensionOutput) => void;
}

export function CreateExtensionForm({ onSubmit }: CreateExtensionFormProps) {
    const { t } = useTranslation("common");
    const form = useForm<CreateExtensionInput, any, CreateExtensionOutput>({
        resolver: zodResolver(createExtensionSchema),
        defaultValues: {
            extension: "",
        }
    });

    return (
        <Form form={form} formId="create-extension-form" onSubmit={onSubmit}>
            <FormGrid cols={1}>
                <FormInput
                    control={form.control}
                    label={t("fields.extension", { defaultValue: "Extension Name" })}
                    name="extension"
                    type="text"
                    placeholder="e.g. .crx, .xpi"
                />
            </FormGrid>
        </Form>
    );
}
