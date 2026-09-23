import { CreatePlatformForm } from "../components/CreatePlatformForm";
import { BaseFormDialog } from "@/components/custom-ui/overlays/dialog/BaseFormDialog";
import type { DialogProps } from "@/lib/dialog-registry";
import { useCreatePlatform } from "../hooks/usePlatforms";

export function CreatePlatformDialog({ open, onOpenChange }: DialogProps<undefined>) {
    const createPlatform = useCreatePlatform();

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="Create Platform"
            formId="create-platform-form"
            isPending={createPlatform.isPending}
            size="md"
        >
            <CreatePlatformForm
                onSubmit={(values) => {
                    createPlatform.mutate({ data: values }, {
                        onSuccess: () => onOpenChange(false)
                    });
                }}
            />
        </BaseFormDialog>
    );
}
