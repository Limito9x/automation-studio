import { useTranslation } from "react-i18next";
import { ResourcePageShell } from "@/components/layout/shells/ResourcePageShell";
import { PlatformExtensionsTable } from "./components/PlatformExtensionsTable";
import { useDialogStore } from "@/stores/dialogStore";
import { useResourceQuery } from "@/lib/useResourceQuery";
import { useExtensions } from "./hooks/usePlatforms";
import { useExtensionTable } from "./hooks/useExtensionTable";
import { DataTableViewOptions } from "@/components/table/DataTableViewOptions";

export interface ExtensionPageProps {
    useSearch: () => any;
    useNavigate: () => (options: any) => void;
}

export function ExtensionPage({ useSearch, useNavigate }: ExtensionPageProps) {
    const { t } = useTranslation(["common"]);
    const search = useSearch();
    const navigate = useNavigate();
    const resourceQuery = useResourceQuery(search, navigate);
    const { openDialog } = useDialogStore();

    const { data: extensions, isLoading, refetch, isFetching } = useExtensions();

    const dataList = extensions || [];
    const { table, columns } = useExtensionTable({
        data: dataList as any,
        totalCount: dataList.length,
        resource: resourceQuery,
    });

    return (
        <ResourcePageShell
            title={t("extensions.title", { defaultValue: "Platform Extensions" })}
            onAdd={() => openDialog("create-extension")}
            addLabel={t("createExtension", { defaultValue: "Add Extension" })}
            onRefresh={() => refetch()}
            isRefreshing={isFetching}
            resource={resourceQuery}
            searchPlaceholder={t("searchPlaceholder", { defaultValue: "Search..." })}
            renderViewOptions={<DataTableViewOptions table={table} />}
        >
            <PlatformExtensionsTable
                table={table}
                columns={columns}
                isLoading={isLoading}
            />
        </ResourcePageShell>
    );
}
