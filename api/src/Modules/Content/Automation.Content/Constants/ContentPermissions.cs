using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Content.Constants;

public class ContentPermissions
{
    // 1. Khai báo instance
    public static ContentTypeFeature ContentType { get; } = new();
    public static ContentItemFeature ContentItem { get; } = new();

    // 2. Thêm vào GetPermissions dictionary
    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "ContentType", ContentType.All },
        { "ContentItem", ContentItem.All }
    };

    // 3. Khai báo cấu trúc quyền
    public class ContentTypeFeature() : BaseCrudPermission("contenttypes") { }
    public class ContentItemFeature() : BaseCrudPermission("contentitems") { }
}

