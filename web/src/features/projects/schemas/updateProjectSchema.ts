import { z } from "zod";

export const updateProjectSchema = z.object({
    name: z.string().min(1),
});

export type UpdateProjectInput = z.input<typeof updateProjectSchema>;
export type UpdateProjectOutput = z.output<typeof updateProjectSchema>;
