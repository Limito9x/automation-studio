import { useTranslation } from "react-i18next";
import { BaseTable } from "@/components/table/BaseTable";
import type { ColumnDef, Table } from "@tanstack/react-table";
import type { PlatformDto } from "@/gen/model";

export interface PlatformsTableProps {
    table: Table<PlatformDto>;
    columns: ColumnDef<PlatformDto>[];
    isLoading: boolean;
}

export function PlatformsTable({
    table,
    columns,
    isLoading,
}: PlatformsTableProps) {
    const { t } = useTranslation(["common"]);

    return (
        <BaseTable
            table={table}
            columns={columns}
            isLoading={isLoading}
            caption={t("table.caption", { defaultValue: "Platforms List" })}
        />
    );
}
