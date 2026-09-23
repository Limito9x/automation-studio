import { UpdatePlatformForm } from "../components/UpdatePlatformForm";
import { BaseFormDialog } from "@/components/custom-ui/overlays/dialog/BaseFormDialog";
import type { DialogProps } from "@/lib/dialog-registry";
import { usePlatform, useUpdatePlatform } from "../hooks/usePlatforms";

export function UpdatePlatformDialog({ open, onOpenChange, data }: DialogProps<{ id: string }>) {
    const { data: platform, isLoading } = usePlatform(data?.id || "");
    const updatePlatform = useUpdatePlatform();

    if (!data?.id || isLoading || !platform) {
        return null;
    }

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="Edit Platform"
            formId="update-platform-form"
            isPending={updatePlatform.isPending}
            size="md"
        >
            <UpdatePlatformForm
                platform={platform}
                onSubmit={(values) => {
                    updatePlatform.mutate(
                        { id: data.id, data: values },
                        {
                            onSuccess: () => onOpenChange(false)
                        }
                    );
                }}
            />
        </BaseFormDialog>
    );
}
