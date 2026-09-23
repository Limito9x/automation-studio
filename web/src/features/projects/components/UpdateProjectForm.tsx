import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { FormInput } from "@/components/form-controls";
import { updateProjectSchema, type UpdateProjectInput, type UpdateProjectOutput } from "../schemas/updateProjectSchema";
import { useTranslation } from "react-i18next";
// import { useGetProjectById } from "../hooks/useProjects";

interface ProjectFormProps {
    id: string;
    onSubmit: (data: UpdateProjectOutput) => void;
}

export function UpdateProjectForm({ id, onSubmit }: ProjectFormProps) {
    const { t } = useTranslation("projects");
    
    // TODO: Bỏ comment khi API get by id khả dụng
    // const { data: project } = useGetProjectById(id);
    const project: any = null;

    const form = useForm<UpdateProjectInput, any, UpdateProjectOutput>({
        resolver: zodResolver(updateProjectSchema),
        values: project ? {
            name: project.name || "",
        } : undefined,
        defaultValues: {
            name: "",
        }
    });

    return (
        <Form form={form} formId={`update-project-form-${id}`} onSubmit={onSubmit}>
            <FormGrid cols={1}>
                <FormInput
                    control={form.control}
                    label={t("fields.name", { defaultValue: "Name" })}
                    name="name"
                    type="text"
                    placeholder="Enter name"
                />
            </FormGrid>
        </Form>
    );
}
