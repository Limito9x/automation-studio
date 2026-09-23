import { CreateExtensionForm } from "../components/CreateExtensionForm";
import { BaseFormDialog } from "@/components/custom-ui/overlays/dialog/BaseFormDialog";
import type { DialogProps } from "@/lib/dialog-registry";
import { useCreateExtension } from "../hooks/usePlatforms";
import { toast } from "sonner";

export function CreateExtensionDialog({ open, onOpenChange }: DialogProps<undefined>) {
    const createExtension = useCreateExtension();

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="Create Extension"
            formId="create-extension-form"
            isPending={createExtension.isPending}
            size="md"
        >
            <CreateExtensionForm
                onSubmit={(values) => {
                    createExtension.mutate(
                        { data: values },
                        {
                            onSuccess: () => {
                                toast.success("Extension created successfully");
                                onOpenChange(false);
                            },
                            onError: (err: any) => {
                                toast.error(err?.message || "Failed to create extension");
                            }
                        }
                    );
                }}
            />
        </BaseFormDialog>
    );
}
