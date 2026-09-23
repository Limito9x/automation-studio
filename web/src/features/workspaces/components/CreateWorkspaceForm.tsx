import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { FormInput, FormStaticCombobox } from "@/components/form-controls";
import {
  createWorkspaceSchema,
  type CreateWorkspaceInput,
  type CreateWorkspaceOutput,
} from "../schemas/workspaceSchema";
import { usePlatforms } from "@/features/platforms/hooks/usePlatforms";

interface CreateWorkspaceFormProps {
  onSubmit: (values: CreateWorkspaceOutput) => void;
  defaultValues?: Partial<CreateWorkspaceInput>;
}

export function CreateWorkspaceForm({ onSubmit, defaultValues }: CreateWorkspaceFormProps) {
  const { data: platforms, isLoading: isLoadingPlatforms } = usePlatforms();

  const platformOptions = (platforms || []).map((p) => ({
    label: p.name,
    value: p.id,
  }));

  const form = useForm<CreateWorkspaceInput, any, CreateWorkspaceOutput>({
    resolver: zodResolver(createWorkspaceSchema),
    defaultValues: {
      name: defaultValues?.name || "",
      platformIds: defaultValues?.platformIds || [],
    },
  });

  return (
    <Form form={form} formId="workspace-form" onSubmit={onSubmit}>
      <FormGrid cols={1} className="gap-4">
        <FormInput
          control={form.control}
          label="Workspace Name"
          name="name"
          type="text"
          placeholder="e.g. Production Workspace"
          isRequired
        />

        <FormStaticCombobox
          control={form.control}
          name="platformIds"
          label="Supported Platforms"
          placeholder="Select platforms (e.g. Unreal, Blender, Daz3D)..."
          options={platformOptions}
          isLoading={isLoadingPlatforms}
          multiple
        />
      </FormGrid>
    </Form>
  );
}
