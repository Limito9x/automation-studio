import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { FormInput } from "@/components/form-controls";
import { createProjectSchema, type CreateProjectInput, type CreateProjectOutput } from "../schemas/createProjectSchema";
import { useTranslation } from "react-i18next";

interface ProjectFormProps {
    onSubmit: (data: CreateProjectOutput) => void;
}

export function CreateProjectForm({ onSubmit }: ProjectFormProps) {
    const { t } = useTranslation("projects");
    const form = useForm<CreateProjectInput, any, CreateProjectOutput>({
        resolver: zodResolver(createProjectSchema),
        defaultValues: {
            name: "",
        }
    });

    return (
        <Form form={form} formId={"create-project-form"} onSubmit={onSubmit}>
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
