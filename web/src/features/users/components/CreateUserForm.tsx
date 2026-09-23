import { Form, FormGrid, zodResolver, useForm } from "@/components/form";
import { FormApiCombobox, FormInput } from "@/components/form-controls";
import { useCreateUserSchema, type CreateUserInput, type CreateUserOutput } from "@/features/users/schemas/createUserSchema";
import { useRoleOptions } from "@/features/roles/hooks/useRoles";
import { useTranslation } from "react-i18next";

interface UserFormProps {
    onSubmit: (data: CreateUserOutput) => void;
}

export function CreateUserForm({ onSubmit }: UserFormProps) {
    const { t } = useTranslation("users");
    const createUserSchema = useCreateUserSchema();
    const form = useForm<CreateUserInput, any, CreateUserOutput>({
        resolver: zodResolver(createUserSchema),
        defaultValues: {
            fullName: "",
            email: "",
            roleId: ""
        }
    });

    return (
        <Form form={form} formId={"create-user-form"} onSubmit={onSubmit}>
            <FormGrid cols={2}>
                <FormInput
                    control={form.control}
                    label={t("fields.fullName", { defaultValue: "Full Name" })}
                    name="fullName"
                    type="text"
                    placeholder={t("placeholders.fullName", { defaultValue: "John Doe" })}
                />
                <FormInput
                    control={form.control}
                    label={t("fields.email", { defaultValue: "Email" })}
                    name="email"
                    type="email"
                    placeholder={t("placeholders.email", { defaultValue: "email@example.com" })}
                />
            </FormGrid>
            <FormApiCombobox
                control={form.control}
                label={t("fields.roleId", { defaultValue: "Role" })}
                name="roleId"
                placeholder={t("placeholders.roleId", { defaultValue: "Select a role" })}
                useOptions={useRoleOptions}
                getItemValue={(item) => item.value}
            />
        </Form>
    )
}