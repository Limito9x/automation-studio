import { MoreHorizontal } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import React from "react";

export interface ActionItem {
    label: string;
    icon?: React.ElementType;
    onClick: () => void;
    destructive?: boolean;
    disabled?: boolean;
    separatorBefore?: boolean;
}

interface DataTableRowActionsProps {
    actions: ActionItem[];
}

export function DataTableRowActions({ actions }: DataTableRowActionsProps) {
    if (!actions || actions.length === 0) return null;

    return (
        <DropdownMenuTrigger>
            <Button variant="ghost" className="flex h-8 w-8 p-0 data-[state=open]:bg-muted">
                <MoreHorizontal className="h-4 w-4" />
                <span className="sr-only">Open menu</span>
            </Button>
            <DropdownMenu placement="bottom end" className="w-[160px]">
                {actions.map((action, index) => (
                    <React.Fragment key={index}>
                        {action.separatorBefore && <DropdownMenuSeparator />}
                        <DropdownMenuItem
                            onAction={action.onClick}
                            isDisabled={action.disabled}
                            variant={action.destructive ? "destructive" : "default"}
                        >
                            {action.icon && <action.icon className="mr-2 h-4 w-4" />}
                            {action.label}
                        </DropdownMenuItem>
                    </React.Fragment>
                ))}
            </DropdownMenu>
        </DropdownMenuTrigger>
    );
}
