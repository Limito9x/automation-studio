import { useState, useMemo } from "react";
import type { FieldValues } from "react-hook-form";
import { BaseFormField } from "@/components/form-controls/BaseFormField";
import type { BaseFormControlProps } from "@/components/form-controls/type";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useTagTree, type TagTreeNodeDto } from "@/features/tags/hooks/useTags";
import { usePipelineFormScope } from "../form-scope/PipelineFormScope";
import {
  ChevronRight,
  ChevronDown,
  Tag as TagIcon,
  Search,
  CheckSquare,
  Square,
} from "lucide-react";
import { cn } from "@/lib/utils";

export interface FormPinTagTreeSelectProps<T extends FieldValues>
  extends BaseFormControlProps<T> {
  placeholder?: string;
  disabled?: boolean;
}

export function FormPinTagTreeSelect<T extends FieldValues>({
  placeholder = "Filter tags...",
  disabled,
  ...rest
}: FormPinTagTreeSelectProps<T>) {
  const { projectId = "" } = usePipelineFormScope();
  const { data: treeData = [], isLoading } = useTagTree(
    { projectId },
    { enabled: Boolean(projectId) }
  );

  const [searchQuery, setSearchQuery] = useState("");
  const [expandedPaths, setExpandedPaths] = useState<Set<string>>(new Set());

  const toggleExpand = (path: string) => {
    setExpandedPaths((prev) => {
      const next = new Set(prev);
      if (next.has(path)) next.delete(path);
      else next.add(path);
      return next;
    });
  };

  // Helper collect all descendant paths
  const getAllDescendantPaths = (node: TagTreeNodeDto): string[] => {
    const paths = [node.path];
    if (node.children) {
      for (const child of node.children) {
        paths.push(...getAllDescendantPaths(child));
      }
    }
    return paths;
  };

  // Filter tree recursively based on search
  const filteredTree = useMemo(() => {
    if (!searchQuery.trim()) return treeData;
    const query = searchQuery.toLowerCase();

    function filterNodes(nodes: TagTreeNodeDto[]): TagTreeNodeDto[] {
      const result: TagTreeNodeDto[] = [];
      for (const node of nodes) {
        const matchesSelf =
          node.name.toLowerCase().includes(query) ||
          node.path.toLowerCase().includes(query);
        const filteredChildren = filterNodes(node.children || []);
        if (matchesSelf || filteredChildren.length > 0) {
          result.push({
            ...node,
            children: filteredChildren,
          });
        }
      }
      return result;
    }

    return filterNodes(treeData);
  }, [treeData, searchQuery]);

  return (
    <BaseFormField
      {...rest}
      render={(field) => {
        const selectedPaths: string[] = Array.isArray(field.value)
          ? field.value.map(String)
          : field.value
          ? [String(field.value)]
          : [];

        const isSelected = (path: string) => selectedPaths.includes(path);

        const handleNodeToggle = (node: TagTreeNodeDto, checked: boolean) => {
          const targetPaths = getAllDescendantPaths(node);
          let newSelected: string[];

          if (checked) {
            // Add node and all its descendants
            newSelected = Array.from(new Set([...selectedPaths, ...targetPaths]));
          } else {
            // Remove node and all its descendants
            const targetsSet = new Set(targetPaths);
            newSelected = selectedPaths.filter((p) => !targetsSet.has(p));
          }

          field.onChange(newSelected);
        };

        const handleSelectAll = () => {
          const allPaths: string[] = [];
          function collect(nodes: TagTreeNodeDto[]) {
            for (const n of nodes) {
              allPaths.push(n.path);
              if (n.children) collect(n.children);
            }
          }
          collect(treeData);
          field.onChange(Array.from(new Set(allPaths)));
        };

        const handleClearAll = () => {
          field.onChange([]);
        };

        const renderTreeNode = (node: TagTreeNodeDto, depth = 0) => {
          const hasChildren = Boolean(node.children && node.children.length > 0);
          const isExpanded = expandedPaths.has(node.path) || Boolean(searchQuery.trim());
          const checked = isSelected(node.path);
          const tagColor = node.color || "#3b82f6";

          return (
            <div key={node.path} className="select-none">
              <div
                className={cn(
                  "flex items-center gap-1.5 py-1 px-1.5 rounded-md hover:bg-muted/50 transition-colors text-xs",
                  checked && "bg-primary/5"
                )}
                style={{ paddingLeft: `${depth * 14 + 6}px` }}
              >
                {/* Chevron expand/collapse */}
                {hasChildren ? (
                  <button
                    type="button"
                    onClick={() => toggleExpand(node.path)}
                    className="p-0.5 text-muted-foreground hover:text-foreground rounded cursor-pointer"
                    aria-label={isExpanded ? "Collapse" : "Expand"}
                  >
                    {isExpanded ? (
                      <ChevronDown className="size-3.5" />
                    ) : (
                      <ChevronRight className="size-3.5" />
                    )}
                  </button>
                ) : (
                  <span className="w-4.5" />
                )}

                {/* Checkbox */}
                <Checkbox
                  isSelected={checked}
                  onChange={(val) => handleNodeToggle(node, val)}
                  aria-label={`Select ${node.name}`}
                  className="cursor-pointer"
                />

                {/* Tag pill / label */}
                <div
                  className="flex items-center gap-1.5 min-w-0 flex-1 cursor-pointer"
                  onClick={() => handleNodeToggle(node, !checked)}
                >
                  <TagIcon
                    className="size-3 shrink-0"
                    style={{ color: tagColor }}
                  />
                  <span
                    className={cn(
                      "truncate font-medium",
                      checked ? "text-foreground font-semibold" : "text-muted-foreground"
                    )}
                  >
                    {node.name}
                  </span>
                  <span className="text-[10px] text-muted-foreground/60 font-mono truncate ml-auto">
                    {node.path}
                  </span>
                </div>
              </div>

              {/* Render children */}
              {hasChildren && isExpanded && (
                <div>
                  {node.children!.map((child) => renderTreeNode(child, depth + 1))}
                </div>
              )}
            </div>
          );
        };

        return (
          <div className="space-y-2 border rounded-lg p-2.5 bg-card/60">
            {/* Header controls: Search & Quick actions */}
            <div className="flex items-center gap-2">
              <div className="relative flex-1">
                <Search className="absolute left-2.5 top-2.5 size-3.5 text-muted-foreground" />
                <Input
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder={placeholder}
                  className="h-8 pl-8 text-xs"
                  disabled={disabled}
                />
              </div>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={handleSelectAll}
                className="size-8 shrink-0 cursor-pointer"
                aria-label="Select All"
              >
                <CheckSquare className="size-3.5" />
              </Button>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={handleClearAll}
                className="size-8 shrink-0 cursor-pointer"
                aria-label="Clear All"
              >
                <Square className="size-3.5" />
              </Button>

            </div>

            {/* Selected count info */}
            <div className="flex items-center justify-between text-[11px] text-muted-foreground px-1">
              <span>
                {selectedPaths.length} {selectedPaths.length === 1 ? "tag" : "tags"} selected
              </span>
              {selectedPaths.length > 0 && (
                <button
                  type="button"
                  onClick={handleClearAll}
                  className="text-primary hover:underline cursor-pointer"
                >
                  Clear
                </button>
              )}
            </div>

            {/* Tree nodes view */}
            <div className="max-h-48 overflow-y-auto border rounded-md p-1 bg-background/50 space-y-0.5">
              {isLoading ? (
                <div className="p-3 text-center text-xs text-muted-foreground animate-pulse">
                  Loading tag hierarchy...
                </div>
              ) : filteredTree.length === 0 ? (
                <div className="p-3 text-center text-xs text-muted-foreground">
                  {searchQuery ? "No matching tags found." : "No tags available in project."}
                </div>
              ) : (
                filteredTree.map((node) => renderTreeNode(node))
              )}
            </div>
          </div>
        );
      }}
    />
  );
}
