import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useDialogStore } from "@/stores/dialogStore";
import { useAuthStore } from "@/stores/authStore";
import { DataTableRowActions, type ActionItem } from "@/components/table/DataTableRowActions";
import type { BaseSearchParams, useResourceQuery } from "@/lib/useResourceQuery";
import type { ColumnDef } from "@tanstack/react-table";
import type { PlatformExtensionDto } from "@/gen/model";
import { TrashIcon, LayersIcon } from "lucide-react";
import { useDataTable } from "@/lib/useDataTable";

export interface UseExtensionTableOptions {
    data: PlatformExtensionDto[];
    totalCount: number;
    resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
}

export function useExtensionTable({ data, totalCount, resource }: UseExtensionTableOptions) {
    const { t } = useTranslation(["common"]);
    const openDialog = useDialogStore((state) => state.openDialog);
    const hasPermission = useAuthStore((state) => state.hasPermission);

    const columns = useMemo<ColumnDef<PlatformExtensionDto>[]>(
        () => [
            {
                accessorKey: "extension",
                header: () => t("fields.extension", { defaultValue: "Extension Name" }),
                meta: { label: t("fields.extension", { defaultValue: "Extension Name" }), icon: LayersIcon },
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
                        hasPermission("platform_extension:delete") && {
                            label: t("delete", { defaultValue: "Delete" }),
                            icon: TrashIcon,
                            onClick: () => openDialog("delete-extension", { id: item.id! }),
                            destructive: true,
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
