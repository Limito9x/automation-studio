using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Projects.Constants;

public class ProjectsPermissions
{
    // 1. Khai báo instance
    public static ProjectFeature Project { get; } = new();

    // 2. Thêm vào GetPermissions dictionary
    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Project", Project.All }
    };

    // 3. Khai báo cấu trúc quyền
    public class ProjectFeature() : BaseCrudPermission("projects") { }
}

