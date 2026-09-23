using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Tag.Constants;

public class TagPermissions
{
    public static TagFeature Tag { get; } = new();
    public static TagLinkFeature TagLink { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Tag", Tag.All },
        { "TagLink", TagLink.All },
    };

    public class TagFeature() : BaseCrudPermission("tag") { }
    public class TagLinkFeature() : BaseCrudPermission("tag-link") { }
}