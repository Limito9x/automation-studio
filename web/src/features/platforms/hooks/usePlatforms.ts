import * as PlatformsApi from "@/gen/endpoints/platforms/platforms";
import * as ExtensionsApi from "@/gen/endpoints/platform-extensions/platform-extensions";
import { createMutationHook } from "@/lib/query-utils";

export const usePlatforms = () => {
    return PlatformsApi.useGetPlatforms({
        query: {
            placeholderData: (previousData) => previousData
        }
    });
};

export const usePlatform = (id: string) => {
    return PlatformsApi.useGetPlatformById(id, {
        query: {
            enabled: !!id,
        }
    });
};

export const useCreatePlatform = createMutationHook(PlatformsApi.useCreatePlatform, [PlatformsApi.getGetPlatformsQueryKey()]);
export const useUpdatePlatform = createMutationHook(PlatformsApi.useUpdatePlatform, [PlatformsApi.getGetPlatformsQueryKey()]);
export const useDeletePlatform = createMutationHook(PlatformsApi.useDeletePlatform, [PlatformsApi.getGetPlatformsQueryKey()]);

export const useExtensions = () => {
    return ExtensionsApi.useGetExtensions({
        query: {
            placeholderData: (previousData) => previousData
        }
    });
};

export const useCreateExtension = createMutationHook(ExtensionsApi.useCreateExtension, [ExtensionsApi.getGetExtensionsQueryKey()]);
export const useDeleteExtension = createMutationHook(ExtensionsApi.useDeleteExtension, [ExtensionsApi.getGetExtensionsQueryKey()]);
