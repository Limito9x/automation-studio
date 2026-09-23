import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { FormInput, FormImageUpload, FormTagsInput } from "@/components/form-controls";
import { updatePlatformSchema, type UpdatePlatformInput, type UpdatePlatformOutput } from "../schemas/platformSchema";
import type { PlatformDto } from "@/gen/model";
import { useTranslation } from "react-i18next";

interface UpdatePlatformFormProps {
    platform: PlatformDto;
    onSubmit: (data: UpdatePlatformOutput) => void;
}

export function UpdatePlatformForm({ platform, onSubmit }: UpdatePlatformFormProps) {
    const { t } = useTranslation("common");
    const form = useForm<UpdatePlatformInput, any, UpdatePlatformOutput>({
        resolver: zodResolver(updatePlatformSchema),
        defaultValues: {
            name: platform.name,
            iconAssetId: platform.iconAssetId || undefined,
            extensions: platform.extensions || [],
        }
    });

    return (
        <Form form={form} formId="update-platform-form" onSubmit={onSubmit}>
            <FormGrid cols={1}>
                <FormImageUpload
                    control={form.control}
                    name="iconAssetId"
                    label={t("fields.icon", { defaultValue: "Icon" })}
                    defaultPreviewUrl={platform.iconUrl || undefined}
                    aspectRatio={1}
                    cropShape="round"
                    objectFit="contain"
                    maxSizeMB={5}
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
