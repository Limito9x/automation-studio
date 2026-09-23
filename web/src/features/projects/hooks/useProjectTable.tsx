import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useDialogStore } from "@/stores/dialogStore";
import { useAuthStore } from "@/stores/authStore";
import { DataTableRowActions, type ActionItem } from "@/components/table/DataTableRowActions";
import type { BaseSearchParams, useResourceQuery } from "@/lib/useResourceQuery";
import type { ColumnDef } from "@tanstack/react-table";
import type { ProjectDto } from "@/gen/model";
import { EditIcon, TrashIcon, TypeIcon } from "lucide-react";
import { useDataTable } from "@/lib/useDataTable";

export interface UseProjectTableOptions {
    data: ProjectDto[];
    totalCount: number;
    resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
}

export function useProjectTable({ data, totalCount, resource }: UseProjectTableOptions) {
    const { t } = useTranslation(["projects", "common"]);
    const openDialog = useDialogStore((state) => state.openDialog);
    const hasPermission = useAuthStore((state) => state.hasPermission);

    const columns = useMemo<ColumnDef<ProjectDto>[]>(
        () => [
            {
                accessorKey: "name",
                header: () => t("fields.name", { defaultValue: "Name" }),
                meta: { label: t("fields.name", { defaultValue: "Name" }), icon: TypeIcon },
                cell: (info) => (
                    <span className="font-semibold text-foreground">
                        {info.getValue() as string}
                    </span>
                ),
            },
            {
                id: "actions",
                enableSorting: false,
                cell: ({ row }) => {
                    const item = row.original;
                    const actions = [
                        hasPermission("projects:update") && {
                            label: t("common:edit", { defaultValue: "Edit" }),
                            icon: EditIcon,
                            onClick: () => openDialog("update-project", { id: item.id! }),
                        },
                        hasPermission("projects:delete") && {
                            label: t("common:delete", { defaultValue: "Delete" }),
                            icon: TrashIcon,
                            onClick: () => openDialog("delete-project", { id: item.id! }),
                            destructive: true,
                            separatorBefore: true,
                        }
                    ].filter(Boolean) as ActionItem[];

                    return <DataTableRowActions actions={actions} />;
                },
            },
        ],
        [openDialog, hasPermission, t]
    );

    const table = useDataTable({ data, columns, totalCount, resource });

    return { table, columns };
}
