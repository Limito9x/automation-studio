import { z } from "zod";

export const projectExecutorConfigSchema = z.object({
    agentId: z.string().min(1, "Please select an Agent"),
    executorKey: z.string().min(1, "Please select an Executor"),
    fullPathProject: z.string().min(1, "Project file path is required"),
    engineVersion: z.string().optional(),
}).transform((data) => ({
    agentId: data.agentId,
    executorKey: data.executorKey,
    settings: {
        fullPathProject: data.fullPathProject,
        ...(data.engineVersion ? { engineVersion: data.engineVersion } : {}),
    },
}));

export type ProjectExecutorConfigInput = z.input<typeof projectExecutorConfigSchema>;
export type ProjectExecutorConfigOutput = z.output<typeof projectExecutorConfigSchema>;
