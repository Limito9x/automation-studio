import { useState } from "react";
import { BaseDialog } from "@/components/custom-ui/overlays/dialog/BaseDialog";
import { Button } from "@/components/ui/button";
import { useGenerateSetupToken } from "../hooks/useAgents";
import { Copy, Check, Key, Terminal, RefreshCw } from "lucide-react";
import { toast } from "sonner";
import { useTranslation } from "react-i18next";

interface GenerateSetupTokenDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function GenerateSetupTokenDialog({ open, onOpenChange }: GenerateSetupTokenDialogProps) {
    const { t } = useTranslation();
    const generateTokenMutation = useGenerateSetupToken();
    const [tokenData, setTokenData] = useState<{ token: string; expiresAt: string } | null>(null);
    const [copied, setCopied] = useState(false);

    const handleGenerate = () => {
        generateTokenMutation.mutate(undefined, {
            onSuccess: (data: any) => {
                setTokenData(data);
                toast.success(t("agents.tokenGenerated", { defaultValue: "Setup token generated successfully!" }));
            },
            onError: () => {
                toast.error(t("agents.tokenFailed", { defaultValue: "Failed to generate setup token." }));
            }
        });
    };

    const handleCopy = () => {
        if (!tokenData?.token) return;
        navigator.clipboard.writeText(tokenData.token);
        setCopied(true);
        toast.success(t("common.copied", { defaultValue: "Copied to clipboard!" }));
        setTimeout(() => setCopied(false), 2000);
    };

    const handleClose = (isOpen: boolean) => {
        if (!isOpen) {
            setTokenData(null);
        }
        onOpenChange(isOpen);
    };

    return (
        <BaseDialog
            open={open}
            onOpenChange={handleClose}
            title={t("agents.generateTokenTitle", { defaultValue: "Agent Setup Token" })}
            description={t("agents.generateTokenDesc", { defaultValue: "Generate a temporary token to register a new agent on a remote machine." })}
            size="md"
        >
            <div className="space-y-4 py-2">
                {!tokenData ? (
                    <div className="flex flex-col items-center justify-center p-6 text-center space-y-4 border border-dashed rounded-lg bg-muted/30">
                        <div className="p-3 bg-primary/10 rounded-full text-primary">
                            <Key className="w-8 h-8" />
                        </div>
                        <div className="space-y-1">
                            <h4 className="font-semibold text-sm">
                                {t("agents.readyToGenerate", { defaultValue: "Ready to Register Agent" })}
                            </h4>
                            <p className="text-xs text-muted-foreground max-w-xs">
                                {t("agents.generateInfo", { defaultValue: "Setup tokens are single-use and expire in 30 minutes for security." })}
                            </p>
                        </div>
                        <Button
                            onClick={handleGenerate}
                            isDisabled={generateTokenMutation.isPending}
                            className="mt-2"
                        >
                            {generateTokenMutation.isPending ? (
                                <>
                                    <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
                                    {t("common.generating", { defaultValue: "Generating..." })}
                                </>
                            ) : (
                                <>
                                    <Key className="mr-2 h-4 w-4" />
                                    {t("agents.generateBtn", { defaultValue: "Generate Token" })}
                                </>
                            )}
                        </Button>
                    </div>
                ) : (
                    <div className="space-y-4">
                        <div className="p-4 bg-card border rounded-lg space-y-2">
                            <label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
                                {t("agents.setupTokenLabel", { defaultValue: "Setup Token (Single Use)" })}
                            </label>
                            <div className="flex items-center gap-2">
                                <code className="flex-1 p-2.5 bg-muted font-mono text-base font-bold rounded tracking-wide select-all text-primary">
                                    {tokenData.token}
                                </code>
                                <Button size="icon" variant="outline" onClick={handleCopy}>
                                    {copied ? <Check className="h-4 w-4 text-emerald-500" /> : <Copy className="h-4 w-4" />}
                                </Button>
                            </div>
                        </div>

                        <div className="p-3 bg-muted/50 rounded-lg space-y-2">
                            <div className="flex items-center text-xs font-semibold text-muted-foreground gap-1.5">
                                <Terminal className="w-4 h-4 text-primary" />
                                <span>{t("agents.cliInstructions", { defaultValue: "CLI Agent Activation Command:" })}</span>
                            </div>
                            <pre className="p-2 bg-background border rounded text-xs font-mono overflow-x-auto text-foreground">
                                python tools/register_agent.py
                            </pre>
                            <p className="text-[11px] text-muted-foreground">
                                {t("agents.cliHelp", { defaultValue: "Run the command above on the agent machine and enter the Setup Token when prompted." })}
                            </p>
                        </div>

                        <div className="flex justify-end">
                            <Button variant="secondary" onClick={() => handleClose(false)}>
                                {t("common.done", { defaultValue: "Done" })}
                            </Button>
                        </div>
                    </div>
                )}
            </div>
        </BaseDialog>
    );
}
