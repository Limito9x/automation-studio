import { useState } from "react";
import type { AgentDto, AgentExecutorConfigDto, ExecutorCandidateDto } from "@/gen/model";
import { BaseDialog } from "@/components/custom-ui/overlays/dialog/BaseDialog";
import { useAgentExecutors } from "../hooks/useAgentExecutors";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
    Loader2,
    CheckCircle2,
    Cpu,
    Terminal,
    Gamepad2,
    Server,
    Layers,
    PlusCircle,
    Check,
    FolderSearch,
    Sparkles,
} from "lucide-react";
import { useTranslation } from "react-i18next";

type ExtendedAgentDto = AgentDto & {
    executorConfigs?: AgentExecutorConfigDto[] | null;
};

interface AgentExecutorsDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    agent: AgentDto | null;
}

export function AgentExecutorsDialog({ open, onOpenChange, agent }: AgentExecutorsDialogProps) {
    const { t } = useTranslation();
    const agentId = agent?.id ?? "";
    const { scanExecutors, isScanning, scanCandidates, configureExecutor, isConfiguring } = useAgentExecutors(agentId);

    // Manual custom path addition
    const [showCustomForm, setShowCustomForm] = useState(false);
    const [customKey, setCustomKey] = useState("blender");
    const [customVersion, setCustomVersion] = useState("");
    const [customPath, setCustomPath] = useState("");
    const [activeActionKey, setActiveActionKey] = useState<string | null>(null);

    if (!agent) return null;

    const extAgent = agent as ExtendedAgentDto;
    const currentConfigs: AgentExecutorConfigDto[] = extAgent.executorConfigs ?? [];

    const handleScanAll = async () => {
        try {
            await scanExecutors({});
        } catch {
            // Handled in hook
        }
    };

    const handleQuickActivate = async (candidate: ExecutorCandidateDto) => {
        if (!candidate.executorKey || !candidate.executablePath || !candidate.version) return;
        try {
            setActiveActionKey(candidate.executablePath);
            await configureExecutor({
                executorKey: candidate.executorKey,
                executablePath: candidate.executablePath,
                version: candidate.version,
            });
        } finally {
            setActiveActionKey(null);
        }
    };

    const handleSaveCustom = async () => {
        if (!customKey || !customVersion || !customPath) return;
        try {
            await configureExecutor({
                executorKey: customKey.trim().toLowerCase(),
                version: customVersion.trim(),
                executablePath: customPath.trim(),
            });
            setShowCustomForm(false);
            setCustomPath("");
            setCustomVersion("");
        } catch {
            // Handled in hook
        }
    };

    // Group candidates by Executor Key (e.g. "blender", "python")
    const groupedCandidates: Record<string, ExecutorCandidateDto[]> = {};
    if (scanCandidates) {
        for (const candidate of scanCandidates) {
            const key = (candidate.executorKey || "other").toLowerCase();
            if (!groupedCandidates[key]) {
                groupedCandidates[key] = [];
            }
            groupedCandidates[key].push(candidate);
        }
    }

    const renderExecutorIcon = (key: string) => {
        const k = key.toLowerCase();
        if (k.includes("blender")) return <Cpu className="w-4 h-4 text-orange-500" />;
        if (k.includes("python")) return <Terminal className="w-4 h-4 text-blue-500" />;
        if (k.includes("unreal")) return <Gamepad2 className="w-4 h-4 text-purple-500" />;
        return <Layers className="w-4 h-4 text-primary" />;
    };

    return (
        <BaseDialog
            open={open}
            onOpenChange={onOpenChange}
            title={t("agents.executorsDialogTitle", { defaultValue: `Runtime & Executors — ${agent.name}` })}
            description={t("agents.executorsDialogDesc", {
                defaultValue: "Manage active runtime software (Blender, Python, Unreal Engine) and scan installations on this workstation.",
            })}
            size="2xl"
        >
            <div className="space-y-6 py-1 max-h-[75vh] overflow-y-auto pr-1">
                {/* Agent Header Badge */}
                <div className="flex items-center justify-between p-3 bg-muted/40 rounded-lg border text-xs">
                    <div className="flex items-center gap-2">
                        <Server className="w-4 h-4 text-primary" />
                        <span className="font-semibold text-foreground">{agent.name}</span>
                        <span className="font-mono text-muted-foreground">({agent.machineKey})</span>
                    </div>
                    <Badge variant={agent.isActive ? "default" : "secondary"}>
                        {agent.isActive ? t("common.online", { defaultValue: "Connected" }) : t("common.offline", { defaultValue: "Offline" })}
                    </Badge>
                </div>

                {/* 1. CURRENT ACTIVE RUNTIMES */}
                <div className="space-y-3">
                    <div className="flex items-center justify-between">
                        <Label className="text-xs font-bold uppercase tracking-wider text-muted-foreground flex items-center gap-1.5">
                            <CheckCircle2 className="w-4 h-4 text-emerald-500" />
                            {t("agents.currentRuntimes", { defaultValue: "Active Configured Runtimes" })}
                        </Label>
                        <span className="text-[11px] text-muted-foreground">
                            {currentConfigs.length} {t("agents.runtimesConfigured", { defaultValue: "runtimes active" })}
                        </span>
                    </div>

                    {currentConfigs.length === 0 ? (
                        <div className="p-4 text-center text-xs text-muted-foreground bg-muted/20 rounded-lg border border-dashed">
                            {t("agents.noRuntimesConfigured", {
                                defaultValue: "No runtimes configured yet. Click 'Scan Machine Runtimes' below to auto-detect.",
                            })}
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 gap-2">
                            {currentConfigs.map((cfg: AgentExecutorConfigDto) => (
                                <div
                                    key={cfg.id}
                                    className="p-3 bg-card border rounded-lg flex items-center justify-between gap-3 shadow-xs"
                                >
                                    <div className="flex items-center gap-3 min-w-0">
                                        <div className="p-2 rounded-md bg-muted shrink-0">
                                            {renderExecutorIcon(cfg.executorKey)}
                                        </div>
                                        <div className="space-y-0.5 min-w-0">
                                            <div className="flex items-center gap-2">
                                                <span className="font-semibold text-xs capitalize text-foreground">
                                                    {cfg.executorKey}
                                                </span>
                                                <Badge variant="outline" className="font-mono text-[10px] px-1.5 py-0">
                                                    v{cfg.version || "unknown"}
                                                </Badge>
                                                <Badge variant="default" className="bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 text-[10px] font-medium border-emerald-500/30 px-1.5 py-0">
                                                    Active
                                                </Badge>
                                            </div>
                                            <p className="font-mono text-[11px] text-muted-foreground truncate" title={cfg.executablePath}>
                                                {cfg.executablePath}
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* 2. AUTO-SCAN SECTION */}
                <div className="p-4 rounded-xl border bg-gradient-to-b from-card/80 to-muted/20 space-y-4">
                    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                        <div>
                            <h4 className="text-xs font-bold text-foreground flex items-center gap-1.5">
                                <Sparkles className="w-4 h-4 text-primary" />
                                {t("agents.autoDetectTitle", { defaultValue: "Auto-Detect Installed Software" })}
                            </h4>
                            <p className="text-[11px] text-muted-foreground mt-0.5">
                                {t("agents.autoDetectDesc", { defaultValue: "Query workstation via gRPC to locate Blender and Python environments." })}
                            </p>
                        </div>

                        <Button
                            onPress={handleScanAll}
                            isDisabled={isScanning || !agent.isActive}
                            size="sm"
                            className="shrink-0"
                        >
                            {isScanning ? (
                                <>
                                    <Loader2 className="w-3.5 h-3.5 mr-1.5 animate-spin" />
                                    {t("agents.scanning", { defaultValue: "Scanning Machine..." })}
                                </>
                            ) : (
                                <>
                                    <FolderSearch className="w-3.5 h-3.5 mr-1.5" />
                                    {t("agents.scanMachine", { defaultValue: "Scan Machine Runtimes" })}
                                </>
                            )}
                        </Button>
                    </div>

                    {/* Scan Results by Category */}
                    {scanCandidates && (
                        <div className="space-y-4 pt-3 border-t">
                            {Object.keys(groupedCandidates).length === 0 ? (
                                <div className="p-4 text-center text-xs text-muted-foreground bg-muted/30 rounded-lg border border-dashed">
                                    {t("agents.noExecutablesDetected", { defaultValue: "No supported executables detected on this machine." })}
                                </div>
                            ) : (
                                Object.entries(groupedCandidates).map(([execKey, candidates]) => (
                                    <div key={execKey} className="space-y-2">
                                        <div className="flex items-center gap-2">
                                            {renderExecutorIcon(execKey)}
                                            <span className="text-xs font-bold uppercase tracking-wider text-foreground">
                                                {execKey} ({candidates.length} found)
                                            </span>
                                        </div>

                                        <div className="border rounded-lg overflow-hidden divide-y divide-border bg-background">
                                            {candidates.map((cand, idx) => {
                                                const isActive = currentConfigs.some(
                                                    (c: AgentExecutorConfigDto) =>
                                                        c.executorKey.toLowerCase() === cand.executorKey?.toLowerCase() &&
                                                        c.executablePath.toLowerCase() === cand.executablePath.toLowerCase()
                                                );
                                                const isActivatingThis = activeActionKey === cand.executablePath;

                                                return (
                                                    <div
                                                        key={idx}
                                                        className={`p-3 text-xs flex items-center justify-between gap-3 transition-colors ${
                                                            isActive ? "bg-emerald-500/5 font-medium" : "hover:bg-muted/40"
                                                        }`}
                                                    >
                                                        <div className="space-y-0.5 min-w-0 flex-1">
                                                            <div className="flex items-center gap-2">
                                                                <span className="font-semibold text-foreground">v{cand.version}</span>
                                                                {isActive && (
                                                                    <Badge variant="outline" className="text-[10px] text-emerald-600 dark:text-emerald-400 border-emerald-500/40 bg-emerald-500/10">
                                                                        Current Active
                                                                    </Badge>
                                                                )}
                                                            </div>
                                                            <p
                                                                className="font-mono text-[11px] text-muted-foreground truncate"
                                                                title={cand.executablePath}
                                                            >
                                                                {cand.executablePath}
                                                            </p>
                                                        </div>

                                                        {isActive ? (
                                                            <div className="flex items-center gap-1 text-emerald-600 text-xs font-medium shrink-0 px-2 py-1 bg-emerald-500/10 rounded-md">
                                                                <Check className="w-3.5 h-3.5" />
                                                                <span>In Use</span>
                                                            </div>
                                                        ) : (
                                                            <Button
                                                                size="sm"
                                                                variant="outline"
                                                                className="shrink-0 text-xs h-7 hover:border-primary"
                                                                isDisabled={isConfiguring}
                                                                onPress={() => handleQuickActivate(cand)}
                                                            >
                                                                {isActivatingThis ? (
                                                                    <Loader2 className="w-3 h-3 animate-spin" />
                                                                ) : (
                                                                    t("agents.setAsActive", { defaultValue: "Set as Active" })
                                                                )}
                                                            </Button>
                                                        )}
                                                    </div>
                                                );
                                            })}
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    )}
                </div>

                {/* 3. MANUAL PATH CONFIGURATION (COLLAPSIBLE) */}
                <div className="space-y-3 pt-1">
                    {!showCustomForm ? (
                        <Button
                            variant="ghost"
                            size="sm"
                            onPress={() => setShowCustomForm(true)}
                            className="text-xs text-muted-foreground hover:text-foreground w-full justify-center border border-dashed"
                        >
                            <PlusCircle className="w-3.5 h-3.5 mr-1.5" />
                            {t("agents.addCustomPath", { defaultValue: "Manually add custom executable path" })}
                        </Button>
                    ) : (
                        <div className="p-4 rounded-xl border bg-muted/20 space-y-3">
                            <div className="flex items-center justify-between">
                                <Label className="text-xs font-bold uppercase tracking-wider text-foreground">
                                    {t("agents.manualConfig", { defaultValue: "Manual Executable Configuration" })}
                                </Label>
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-6 text-xs text-muted-foreground"
                                    onPress={() => setShowCustomForm(false)}
                                >
                                    {t("common.close", { defaultValue: "Close" })}
                                </Button>
                            </div>

                            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                                <div>
                                    <Label htmlFor="customKey" className="text-xs">
                                        {t("agents.executorKey", { defaultValue: "Executor Key" })}
                                    </Label>
                                    <Input
                                        id="customKey"
                                        value={customKey}
                                        onChange={(e) => setCustomKey(e.target.value)}
                                        className="h-8 text-xs font-mono mt-1"
                                        placeholder="blender / python / unreal"
                                    />
                                </div>
                                <div className="sm:col-span-2">
                                    <Label htmlFor="customVersion" className="text-xs">
                                        {t("agents.version", { defaultValue: "Semantic Version" })}
                                    </Label>
                                    <Input
                                        id="customVersion"
                                        value={customVersion}
                                        onChange={(e) => setCustomVersion(e.target.value)}
                                        className="h-8 text-xs font-mono mt-1"
                                        placeholder="e.g. 5.8.0"
                                    />
                                </div>
                            </div>

                            <div>
                                <Label htmlFor="customPath" className="text-xs">
                                    {t("agents.executablePath", { defaultValue: "Executable Path on Workstation" })}
                                </Label>
                                <Input
                                    id="customPath"
                                    value={customPath}
                                    onChange={(e) => setCustomPath(e.target.value)}
                                    className="h-8 text-xs font-mono mt-1"
                                    placeholder="e.g. C:/Program Files/Epic Games/UE_5.8/Engine/Binaries/Win64/UnrealEditor-Cmd.exe"
                                />
                            </div>

                            <div className="flex justify-end gap-2 pt-1">
                                <Button
                                    size="sm"
                                    onPress={handleSaveCustom}
                                    isDisabled={isConfiguring || !customPath || !customVersion}
                                >
                                    {isConfiguring ? <Loader2 className="w-3 h-3 animate-spin mr-1" /> : <Check className="w-3 h-3 mr-1" />}
                                    {t("agents.saveManualPath", { defaultValue: "Save Runtime Path" })}
                                </Button>
                            </div>
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="flex justify-end pt-2 border-t">
                    <Button variant="outline" onPress={() => onOpenChange(false)}>
                        {t("common.done", { defaultValue: "Done" })}
                    </Button>
                </div>
            </div>
        </BaseDialog>
    );
}
