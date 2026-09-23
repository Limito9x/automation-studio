import { keepPreviousData, useQueryClient } from "@tanstack/react-query";
import * as ContentTypesApi from "@/gen/endpoints/content-types/content-types";
import { GetContentTypesQueryParams } from "@/gen/endpoints/content-types/content-types.zod";
import { z } from "zod";

type contentTypeQuery = z.infer<typeof GetContentTypesQueryParams>;

export const useContentTypes = (
    params: contentTypeQuery,
    projectId?: string,
    options?: { enabled?: boolean }
) => {
    return ContentTypesApi.useGetContentTypes(projectId || "", params, {
        query: {
            placeholderData: keepPreviousData,
            enabled: Boolean(projectId) && (options?.enabled ?? true),
        }
    });
};

// Lấy content type theo id hay key đều đc
export const useGetContentType = (projectId: string, key: string) => {
    return ContentTypesApi.useGetContentType(projectId, key, {
        query: {
            enabled: Boolean(key) && Boolean(projectId),
        }
    });
};

export const useCreateContentType = ({ projectId }: { projectId: string }) => {
    const queryClient = useQueryClient();
    return ContentTypesApi.useCreateContentType({
        mutation: {
            onSuccess: () => {
                queryClient.invalidateQueries({
                    queryKey: ContentTypesApi.getGetContentTypesQueryKey(projectId),
                });
            }
        }
    });
};

export const useUpdateContentType = ({ projectId }: { projectId: string }) => {
    const queryClient = useQueryClient();
    return ContentTypesApi.useUpdateContentType({
        mutation: {
            onSuccess: () => {
                queryClient.invalidateQueries({
                    queryKey: ContentTypesApi.getGetContentTypesQueryKey(projectId),
                });
            }
        }
    });
};

export const useDeleteContentType = ({ projectId }: { projectId: string }) => {
    const queryClient = useQueryClient();
    return ContentTypesApi.useDeleteContentType({
        mutation: {
            onSuccess: () => {
                queryClient.invalidateQueries({
                    queryKey: ContentTypesApi.getGetContentTypesQueryKey(projectId),
                });
            }
        }
    });
};

export const useUpdateContentTypeSchema = ({ projectId }: { projectId: string }) => {
    const queryClient = useQueryClient();
    return ContentTypesApi.useUpdateContentTypeSchema({
        mutation: {
            onSuccess: () => {
                queryClient.invalidateQueries({
                    queryKey: ContentTypesApi.getGetContentTypesQueryKey(projectId),
                });
            }
        }
    });
};
