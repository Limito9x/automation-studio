import { z } from "zod";

export const createProjectSchema = z.object({
    name: z.string().min(1),
});

export type CreateProjectInput = z.input<typeof createProjectSchema>;
export type CreateProjectOutput = z.output<typeof createProjectSchema>;
