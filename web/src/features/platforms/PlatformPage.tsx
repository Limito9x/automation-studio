import { useTranslation } from "react-i18next";
import { ResourcePageShell } from "@/components/layout/shells/ResourcePageShell";
import { PlatformsTable } from "./components/PlatformsTable";
import { useDialogStore } from "@/stores/dialogStore";
import { useResourceQuery } from "@/lib/useResourceQuery";
import { usePlatforms } from "./hooks/usePlatforms";
import { usePlatformTable } from "./hooks/usePlatformTable";
import { DataTableViewOptions } from "@/components/table/DataTableViewOptions";

export interface PlatformPageProps {
    useSearch: () => any;
    useNavigate: () => (options: any) => void;
}

export function PlatformPage({ useSearch, useNavigate }: PlatformPageProps) {
    const { t } = useTranslation(["common"]);
    const search = useSearch();
    const navigate = useNavigate();
    const resourceQuery = useResourceQuery(search, navigate);
    const { openDialog } = useDialogStore();

    const { data: platforms, isLoading, refetch, isFetching } = usePlatforms();

    const dataList = platforms || [];
    const { table, columns } = usePlatformTable({
        data: dataList as any,
        totalCount: dataList.length,
        resource: resourceQuery,
    });

    return (
        <ResourcePageShell
            title={t("platforms.title", { defaultValue: "Platforms" })}
            onAdd={() => openDialog("create-platform")}
            addLabel={t("createPlatform", { defaultValue: "Add Platform" })}
            onRefresh={() => refetch()}
            isRefreshing={isFetching}
            resource={resourceQuery}
            searchPlaceholder={t("searchPlaceholder", { defaultValue: "Search..." })}
            renderViewOptions={<DataTableViewOptions table={table} />}
        >
            <PlatformsTable
                table={table}
                columns={columns}
                isLoading={isLoading}
            />
        </ResourcePageShell>
    );
}
