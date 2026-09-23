import { useTranslation } from "react-i18next";
import { BaseTable } from "@/components/table/BaseTable";
import type { ColumnDef, Table } from "@tanstack/react-table";
import type { PlatformExtensionDto } from "@/gen/model";

export interface PlatformExtensionsTableProps {
    table: Table<PlatformExtensionDto>;
    columns: ColumnDef<PlatformExtensionDto>[];
    isLoading: boolean;
}

export function PlatformExtensionsTable({
    table,
    columns,
    isLoading,
}: PlatformExtensionsTableProps) {
    const { t } = useTranslation(["common"]);

    return (
        <BaseTable
            table={table}
            columns={columns}
            isLoading={isLoading}
            caption={t("table.caption", { defaultValue: "Extensions List" })}
        />
    );
}
