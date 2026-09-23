import * as TagsApi from "@/gen/endpoints/tags/tags";
import type {
    GetTagsParams,
    GetTagTreeParams,
    GetTagLinksParams,
    TagItemDto,
    TagLinkDto,
    TagTreeNodeDto,
    CreateTagCommand,
    CreateTagLinkCommand,
    UpdateTagCommand,
    DeleteTagCommand,
} from "@/gen/model";
import { createMutationHook } from "@/lib/query-utils";
import { keepPreviousData } from "@tanstack/react-query";

export type {
    TagItemDto,
    TagLinkDto,
    TagTreeNodeDto,
    CreateTagCommand,
    CreateTagLinkCommand,
    UpdateTagCommand,
    DeleteTagCommand,
};

// 1. Queries
export const useTags = (
    params?: GetTagsParams,
    options?: { enabled?: boolean }
) => {
    return TagsApi.useGetTags(params, {
        query: {
            enabled: options?.enabled ?? true,
            placeholderData: keepPreviousData,
        },
    });
};

export const useTagTree = (
    params: GetTagTreeParams,
    options?: { enabled?: boolean }
) => {
    return TagsApi.useGetTagTree(params, {
        query: {
            enabled: options?.enabled ?? Boolean(params.projectId),
            placeholderData: keepPreviousData,
        },
    });
};

export const useTagLinks = (
    params: GetTagLinksParams,
    options?: { enabled?: boolean }
) => {
    return TagsApi.useGetTagLinks(params, {
        query: {
            enabled: options?.enabled ?? Boolean(params.entityType && params.entityId),
            placeholderData: keepPreviousData,
        },
    });
};

// 2. Mutations
export const useCreateTag = createMutationHook(TagsApi.useCreateTag, [
    TagsApi.getGetTagsQueryKey(),
    TagsApi.getGetTagTreeQueryKey(),
]);

export const useUpdateTag = createMutationHook(TagsApi.useUpdateTag, [
    TagsApi.getGetTagsQueryKey(),
    TagsApi.getGetTagTreeQueryKey(),
]);

export const useDeleteTag = createMutationHook(TagsApi.useDeleteTag, [
    TagsApi.getGetTagsQueryKey(),
    TagsApi.getGetTagTreeQueryKey(),
]);

export const useCreateTagLink = createMutationHook(TagsApi.useCreateTagLink, [
    TagsApi.getGetTagLinksQueryKey(),
    TagsApi.getGetTagsQueryKey(),
    ["/api/resources"],
    ["/api/resource-versions"],
]);

export const useDeleteTagLink = createMutationHook(TagsApi.useDeleteTagLink, [
    TagsApi.getGetTagLinksQueryKey(),
    ["/api/resources"],
    ["/api/resource-versions"],
]);
