import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { FormInput, FormImageUpload, FormTagsInput } from "@/components/form-controls";
import { createPlatformSchema, type CreatePlatformInput, type CreatePlatformOutput } from "../schemas/platformSchema";
import { useTranslation } from "react-i18next";

interface CreatePlatformFormProps {
    onSubmit: (data: CreatePlatformOutput) => void;
}

export function CreatePlatformForm({ onSubmit }: CreatePlatformFormProps) {
    const { t } = useTranslation("common");
    const form = useForm<CreatePlatformInput, any, CreatePlatformOutput>({
        resolver: zodResolver(createPlatformSchema),
        defaultValues: {
            key: "",
            name: "",
            iconAssetId: undefined,
            extensions: [],
        }
    });

    return (
        <Form form={form} formId="create-platform-form" onSubmit={onSubmit}>
            <FormGrid cols={1}>
                <FormImageUpload
                    control={form.control}
                    name="iconAssetId"
                    label={t("fields.icon", { defaultValue: "Icon" })}
                    aspectRatio={1}
                    cropShape="rect"
                    objectFit="contain"
                    maxSizeMB={5}
                />
                <FormInput
                    control={form.control}
                    label={t("fields.key", { defaultValue: "Key" })}
                    name="key"
                    type="text"
                    placeholder="e.g. windows, macos, chrome"
                />
                <FormInput
                    control={form.control}
                    label={t("fields.name", { defaultValue: "Name" })}
                    name="name"
                    type="text"
                    placeholder="e.g. Windows OS"
                />
                <FormTagsInput
                    control={form.control}
                    label={t("fields.extensions", { defaultValue: "Supported Extensions (Batch)" })}
                    name="extensions"
                    placeholder="Type extensions e.g. .exe, .bat, .sh (press Enter)"
                />
            </FormGrid>
        </Form>
    );
}
