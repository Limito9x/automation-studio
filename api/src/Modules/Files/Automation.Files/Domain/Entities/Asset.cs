namespace Automation.Files.Domain.Entities;

public class Asset : AuditableEntity
{
    public string StoragePath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    
    // Hash SHA-256 to deduplicate files
    public string HashSha256 { get; set; } = string.Empty;
    
    // Check if this file is confirmed to be on R2
    public bool IsConfirmed { get; set; }

    public Asset() { }

    public Asset(string storagePath, long sizeBytes, string contentType, string extension, string hashSha256)
    {
        StoragePath = storagePath;
        SizeBytes = sizeBytes;
        ContentType = contentType;
        Extension = extension;
        HashSha256 = hashSha256;
        IsConfirmed = false;
    }

    public void MarkAsConfirmed()
    {
        IsConfirmed = true;
    }
}



