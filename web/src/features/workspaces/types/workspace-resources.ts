import type { BaseSearchParams } from "@/lib/useResourceQuery";

export interface WorkspaceResourceDto {
  id: string;
  projectId?: string;
  workspaceId: string;
  displayName?: string;
  name?: string;
  relativePath?: string | null;
  filePath?: string | null;
  platformExtensionId?: string | null;
  contentId?: string | null;
  contentName?: string | null;
  contentTypeName?: string | null;
  contentTypeColor?: string | null;
  contentTypeIcon?: string | null;
  versionCount: number;
  createdAt: string;
}

export interface WorkspaceAgentResourceDto {
  resourceId: string;
  resourceName: string;
  relativePath: string;
  versionNo: number;
  isOrigin: boolean;
  fileHash?: string | null;
  discoveredAt: string;
  contentId?: string | null;
  contentName?: string | null;
  contentTypeName?: string | null;
  contentTypeColor?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface WorkspaceResourceSearchParams extends BaseSearchParams {
  projectId?: string;
  tab?: "resources" | "changes";
  agentId?: string;
}
