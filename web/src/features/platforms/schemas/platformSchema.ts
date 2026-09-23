import { z } from "zod";

export const createPlatformSchema = z.object({
    key: z.string().min(1, "Key is required").max(100),
    name: z.string().min(1, "Name is required").max(100),
    iconAssetId: z.string().nullable().optional(),
    extensions: z.array(z.string()).optional(),
});

export type CreatePlatformInput = z.input<typeof createPlatformSchema>;
export type CreatePlatformOutput = z.output<typeof createPlatformSchema>;

export const updatePlatformSchema = z.object({
    name: z.string().min(1, "Name is required").max(100),
    iconAssetId: z.string().nullable().optional(),
    extensions: z.array(z.string()).optional(),
});

export type UpdatePlatformInput = z.input<typeof updatePlatformSchema>;
export type UpdatePlatformOutput = z.output<typeof updatePlatformSchema>;

export const createExtensionSchema = z.object({
    extension: z.string().min(1, "Extension name is required").max(100),
});

export type CreateExtensionInput = z.input<typeof createExtensionSchema>;
export type CreateExtensionOutput = z.output<typeof createExtensionSchema>;
