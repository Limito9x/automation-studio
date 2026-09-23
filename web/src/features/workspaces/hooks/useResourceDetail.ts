import { keepPreviousData } from "@tanstack/react-query";
import { useGetResourceById } from "@/gen/endpoints/resources/resources";
import type { ResourceItemDto, ResourceVersionDto } from "@/gen/model";

export type ResourceDetailDto = ResourceItemDto & {
    versions?: ResourceVersionDto[];
};

export const useResourceById = (resourceId: string, options?: { enabled?: boolean }) => {
    return useGetResourceById<ResourceDetailDto>(resourceId, {
        query: {
            enabled: !!resourceId && (options?.enabled ?? true),
            placeholderData: keepPreviousData,
        },
    });
};
