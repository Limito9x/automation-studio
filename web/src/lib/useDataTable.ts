import { useReactTable, getCoreRowModel, type ColumnDef } from "@tanstack/react-table";
import type { BaseSearchParams, useResourceQuery } from "@/lib/useResourceQuery";

export interface UseDataTableOptions<TData> {
    data: TData[];
    columns: ColumnDef<TData>[];
    totalCount: number;
    resource: ReturnType<typeof useResourceQuery<BaseSearchParams>>;
}

export function useDataTable<TData>({
    data,
    columns,
    totalCount,
    resource,
}: UseDataTableOptions<TData>) {
    return useReactTable({
        data,
        columns,
        getCoreRowModel: getCoreRowModel(),
        manualPagination: true,
        manualSorting: true,
        rowCount: totalCount,
        state: {
            sorting: resource.sorting,
            pagination: resource.pagination,
        },
        onSortingChange: resource.onSortingChange,
        onPaginationChange: resource.onPaginationChange,
    });
}
