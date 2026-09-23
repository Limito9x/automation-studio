import { useState, useRef } from "react";
import { Upload, FileCode, Loader2, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { uploadAssetFlow } from "@/lib/upload-utils";
import { cn } from "@/lib/utils";

interface AssetPinUploadProps {
  value?: string;
  onChange: (assetId: string) => void;
  accept?: string;
  placeholder?: string;
  disabled?: boolean;
}

export function AssetPinUpload({
  value,
  onChange,
  accept,
  placeholder = "Upload file (Preset / Script)",
  disabled = false,
}: AssetPinUploadProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [fileName, setFileName] = useState<string>("");
  const [isUploading, setIsUploading] = useState<boolean>(false);
  const [uploadError, setUploadError] = useState<string | null>(null);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setIsUploading(true);
    setUploadError(null);
    setFileName(file.name);

    try {
      const assetId = await uploadAssetFlow(file);
      onChange(assetId);
    } catch (err: any) {
      setUploadError(err?.message || "Failed to upload file");
      setFileName("");
      onChange("");
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    setFileName("");
    setUploadError(null);
    onChange("");
  };

  return (
    <div className="w-full space-y-1.5">
      <input
        ref={fileInputRef}
        type="file"
        accept={accept || undefined}
        onChange={handleFileChange}
        disabled={disabled || isUploading}
        className="hidden"
      />

      {value || fileName ? (
        <div className="flex items-center justify-between gap-2 px-3 py-2 rounded-lg border border-primary/30 bg-primary/5 text-xs">
          <div className="flex items-center gap-2 min-w-0">
            <FileCode className="h-4 w-4 text-primary shrink-0" />
            <div className="min-w-0">
              <span className="block font-medium text-foreground truncate">
                {fileName || `Asset: ${value}`}
              </span>
              <span className="block text-[10px] text-emerald-600 dark:text-emerald-400 font-mono">
                Uploaded & Ready
              </span>
            </div>
          </div>

          <div className="flex items-center gap-1 shrink-0">
            <Button
              variant="ghost"
              size="icon-xs"
              onPress={() => fileInputRef.current?.click()}
              isDisabled={disabled || isUploading}
              className="text-muted-foreground hover:text-foreground"
            >
              <Upload className="h-3 w-3" />
            </Button>
            <Button
              variant="ghost"
              size="icon-xs"
              onClick={handleClear}
              isDisabled={disabled || isUploading}
              className="text-muted-foreground hover:text-destructive"
            >
              <X className="h-3 w-3" />
            </Button>
          </div>
        </div>
      ) : (
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled || isUploading}
          className={cn(
            "w-full flex items-center justify-center gap-2 px-3 py-2.5 rounded-lg border border-dashed text-xs transition-colors",
            "border-muted-foreground/30 hover:border-primary/50 hover:bg-muted/50 text-muted-foreground hover:text-foreground",
            isUploading && "pointer-events-none opacity-60",
            uploadError && "border-destructive/50 text-destructive"
          )}
        >
          {isUploading ? (
            <>
              <Loader2 className="h-3.5 w-3.5 animate-spin text-primary" />
              <span>Uploading to cloud storage...</span>
            </>
          ) : (
            <>
              <Upload className="h-3.5 w-3.5" />
              <span>{uploadError || placeholder}</span>
            </>
          )}
        </button>
      )}
    </div>
  );
}
