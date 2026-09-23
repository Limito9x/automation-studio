import { useEffect, useRef } from "react";
import { ArrowRightCircle, ArrowDownCircle, Variable } from "lucide-react";

interface VariableDropMenuProps {
  varName: string;
  position: { x: number; y: number } | null;
  onSelect: (action: "Get" | "Set") => void;
  onClose: () => void;
}

export function VariableDropMenu({
  varName,
  position,
  onSelect,
  onClose,
}: VariableDropMenuProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        onClose();
      }
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        onClose();
      }
    }

    if (position) {
      document.addEventListener("mousedown", handleClickOutside);
      document.addEventListener("keydown", handleKeyDown);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [position, onClose]);

  if (!position) return null;

  return (
    <div
      ref={containerRef}
      style={{ top: position.y, left: position.x }}
      className="fixed z-50 w-48 rounded-xl border border-border/80 bg-popover/95 backdrop-blur-md p-1.5 shadow-2xl animate-in fade-in zoom-in-95 duration-100 font-sans"
    >
      <div className="px-2 py-1 mb-1 border-b border-border/50 flex items-center gap-1.5 text-[10px] font-semibold text-muted-foreground uppercase tracking-wider">
        <Variable className="h-3 w-3 text-cyan-500" />
        <span className="truncate font-mono">{varName}</span>
      </div>

      <button
        type="button"
        onClick={() => onSelect("Get")}
        className="flex w-full items-center gap-2 rounded-lg px-2.5 py-1.5 text-xs text-foreground hover:bg-emerald-500/10 hover:text-emerald-500 transition-colors cursor-pointer"
      >
        <ArrowRightCircle className="h-4 w-4 text-emerald-500 shrink-0" />
        <span className="font-medium">Get {varName}</span>
      </button>

      <button
        type="button"
        onClick={() => onSelect("Set")}
        className="flex w-full items-center gap-2 rounded-lg px-2.5 py-1.5 text-xs text-foreground hover:bg-sky-500/10 hover:text-sky-500 transition-colors cursor-pointer"
      >
        <ArrowDownCircle className="h-4 w-4 text-sky-500 shrink-0" />
        <span className="font-medium">Set {varName}</span>
      </button>
    </div>
  );
}
