import { z } from "zod";

export const createWorkspaceSchema = z.object({
  name: z.string().min(1, "Workspace name is required").max(100),
  platformIds: z.array(z.string()).optional(),
});

export type CreateWorkspaceInput = z.input<typeof createWorkspaceSchema>;
export type CreateWorkspaceOutput = z.output<typeof createWorkspaceSchema>;

export const attachAgentSchema = z.object({
  agentId: z.string().min(1, "Please select an Agent"),
  rootPath: z.string().min(1, "Root path is required"),
});

export type AttachAgentInput = z.input<typeof attachAgentSchema>;
export type AttachAgentOutput = z.output<typeof attachAgentSchema>;
