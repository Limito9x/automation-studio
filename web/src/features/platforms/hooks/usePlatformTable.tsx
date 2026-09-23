import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useDialogStore } from "@/stores/dialogStore";
import { useAuthStore } from "@/stores/authStore";
import { DataTableRowActions, type ActionItem } from "@/components/table/DataTableRowActions";
import type { BaseSearchParams, useResourceQuery } from "@/lib/useResourceQuery";
import type { ColumnDef } from "@tanstack/react-table";
import type { PlatformDto } from "@/gen/model";
import { EditIcon, TrashIcon, KeyIcon, TypeIcon, LayersIcon, ImageIcon } from "lucide-react";
import { useDataTable } from "@/lib/useDataTable";
import { Badge } from "@/components/ui/badge";

export interface UsePlatformTableOptions {
    data: PlatformDto[];
    totalCount: number;
    resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
}

export function usePlatformTable({ data, totalCount, resource }: UsePlatformTableOptions) {
    const { t } = useTranslation(["common"]);
    const openDialog = useDialogStore((state) => state.openDialog);
    const hasPermission = useAuthStore((state) => state.hasPermission);

    const columns = useMemo<ColumnDef<PlatformDto>[]>(
        () => [
            {
                accessorKey: "iconUrl",
                header: () => t("fields.icon", { defaultValue: "Icon" }),
                meta: { label: t("fields.icon", { defaultValue: "Icon" }), icon: ImageIcon },
                enableSorting: false,
                cell: ({ row }) => {
                    const iconUrl = row.original.iconUrl;
                    const name = row.original.name;
                    return (
                        <div className="flex items-center justify-center w-8 h-8 rounded-full bg-muted/30 overflow-hidden border border-border">
                            {iconUrl ? (
                                <img src={iconUrl} alt={name} className="w-full h-full object-cover" />
                            ) : (
                                <ImageIcon className="w-4 h-4 text-muted-foreground" />
                            )}
                        </div>
                    );
                },
            },
            {
                accessorKey: "key",
                header: () => t("fields.key", { defaultValue: "Key" }),
                meta: { label: t("fields.key", { defaultValue: "Key" }), icon: KeyIcon },
                cell: (info) => (
                    <span className="font-mono text-xs text-muted-foreground bg-muted px-2 py-0.5 rounded">
                        {info.getValue() as string}
                    </span>
                ),
            },
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
                accessorKey: "extensions",
                header: () => t("fields.extensions", { defaultValue: "Extensions" }),
                meta: { label: t("fields.extensions", { defaultValue: "Extensions" }), icon: LayersIcon },
                enableSorting: false,
                cell: ({ row }) => {
                    const exts = row.original.extensions || [];
                    if (exts.length === 0) {
                        return <span className="text-muted-foreground text-xs font-italic">None</span>;
                    }
                    return (
                        <div className="flex flex-wrap gap-1">
                            {exts.map((ext) => (
                                <Badge key={ext} variant="outline" className="text-xs">
                                    {ext}
                                </Badge>
                            ))}
                        </div>
                    );
                },
            },
            {
                id: "actions",
                enableSorting: false,
                cell: ({ row }) => {
                    const item = row.original;
                    const actions = [
                        hasPermission("platform:update") && {
                            label: t("edit", { defaultValue: "Edit" }),
                            icon: EditIcon,
                            onClick: () => openDialog("update-platform", { id: item.id! }),
                        },
                        hasPermission("platform:delete") && {
                            label: t("delete", { defaultValue: "Delete" }),
                            icon: TrashIcon,
                            onClick: () => openDialog("delete-platform", { id: item.id! }),
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
