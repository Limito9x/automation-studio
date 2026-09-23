import { keepPreviousData, useQueryClient } from "@tanstack/react-query";
import * as ContentItemsApi from "@/gen/endpoints/content-items/content-items";
import { GetContentItemsQueryParams } from "@/gen/endpoints/content-items/content-items.zod";
import type { LookupContentItemsParams } from "@/gen/model";
import { z } from "zod";

type contentItemQuery = z.infer<typeof GetContentItemsQueryParams>;

export const useContentItems = (params: contentItemQuery, { projectId, contentTypeKey}: {
    projectId: string
    contentTypeKey: string
}) => {
    return ContentItemsApi.useGetContentItems(projectId, contentTypeKey, params, {
        query: {
            enabled: !!projectId && !!contentTypeKey,
            placeholderData: keepPreviousData,
        }
    });
};

export const useGetContentItemById = (id: string) => {
    return ContentItemsApi.useGetContentItemById( id, {
        query: {
            enabled: !!id,
        }
    });
};


export const useCreateContentItem = (params?: { projectId?: string; contentTypeKey?: string }) => {
    const queryClient = useQueryClient();
    return ContentItemsApi.useCreateContentItem({
        mutation: {
            onSuccess: () => {
                if (params?.projectId && params?.contentTypeKey) {
                    queryClient.invalidateQueries({
                        queryKey: ContentItemsApi.getGetContentItemsQueryKey(params.projectId, params.contentTypeKey)
                    });
                }
                queryClient.invalidateQueries({
                    predicate: (query) => typeof query.queryKey[0] === "string" && query.queryKey[0].includes("/contents")
                });
            }
        }
    });
};

export const useUpdateContentItem = (params?: { projectId?: string; contentTypeKey?: string }) => {
    const queryClient = useQueryClient();
    return ContentItemsApi.useUpdateContentItem({
        mutation: {
            onSuccess: (_, variables) => {
                if (params?.projectId && params?.contentTypeKey) {
                    queryClient.invalidateQueries({
                        queryKey: ContentItemsApi.getGetContentItemsQueryKey(params.projectId, params.contentTypeKey)
                    });
                }
                queryClient.invalidateQueries({
                    predicate: (query) => typeof query.queryKey[0] === "string" && query.queryKey[0].includes("/contents")
                });
                queryClient.invalidateQueries({
                    queryKey: ContentItemsApi.getGetContentItemByIdQueryKey(variables.id)
                });
            }
        }
    });
};

export const useDeleteContentItem = (params?: { projectId?: string; contentTypeKey?: string }) => {
    const queryClient = useQueryClient();
    return ContentItemsApi.useDeleteContentItem({
        mutation: {
            onSuccess: () => {
                if (params?.projectId && params?.contentTypeKey) {
                    queryClient.invalidateQueries({
                        queryKey: ContentItemsApi.getGetContentItemsQueryKey(params.projectId, params.contentTypeKey)
                    });
                }
                queryClient.invalidateQueries({
                    predicate: (query) => typeof query.queryKey[0] === "string" && query.queryKey[0].includes("/contents")
                });
            }
        }
    });
};

export const useLookupContentItems = (
    projectId: string,
    params?: LookupContentItemsParams,
    options?: { enabled?: boolean }
) => {
    return ContentItemsApi.useLookupContentItems(projectId, params, {
        query: {
            enabled: !!projectId && (options?.enabled ?? true),
        }
    });
};

