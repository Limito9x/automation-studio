using Automation.Workspace.Contracts;
using Automation.Workspace.Domain.Entities;
using Automation.Workspace.Features.WorkspaceAgents.CompareWorkspaceResources;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Automation.Workspace.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.WorkspaceAgents.SyncLocalChanges;

[Transactional(typeof(WorkspaceDbContext))]
public class SyncLocalChangesHandler(WorkspaceDbContext dbContext, IMessageBus bus)
{
    public async Task<Result<SyncLocalChangesResult>> HandleAsync(
        SyncLocalChangesCommand cmd,
        CancellationToken ct
    )
    {
        var compareCmd = new CompareWorkspaceResourcesCommand(cmd.WorkspaceId, cmd.AgentId);

        var diffResult = await bus.InvokeAsync<Result<DiffResult>>(compareCmd);
        if (diffResult.IsFailed)
            return Result.Fail(diffResult.Errors);

        var diff = diffResult.Value;

        var workspaceAgentId = diff.WorkspaceAgentId;

        var workspaceAgent = await dbContext.WorkspaceAgents.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == workspaceAgentId, ct);
        var rootPath = workspaceAgent?.RootPath;

        // Chuẩn hóa và chuyển đổi mọi target path thành relative path (hỗ trợ cả absolute path và slash variants)
        var normalizedLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var candidateRelativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in cmd.TargetPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            var rel = ToRelativePath(path, rootPath);
            normalizedLookup[path] = rel;

            candidateRelativePaths.Add(rel);
            candidateRelativePaths.Add(rel.Replace('\\', '/'));
            candidateRelativePaths.Add(rel.Replace('/', '\\'));

            candidateRelativePaths.Add(path);
            candidateRelativePaths.Add(path.Replace('\\', '/'));
            candidateRelativePaths.Add(path.Replace('/', '\\'));
        }

        bool IsTargetMatch(string itemRelativePath)
        {
            if (candidateRelativePaths.Count == 0) return false;

            var normSlash = itemRelativePath.Replace('\\', '/').Trim('/');
            var normBackslash = itemRelativePath.Replace('/', '\\').Trim('\\');

            return candidateRelativePaths.Contains(itemRelativePath)
                || candidateRelativePaths.Contains(normSlash)
                || candidateRelativePaths.Contains(normBackslash);
        }

        string? GetNewResourceName(string relativePath, string defaultName)
        {
            if (cmd.NewResourceNames == null || cmd.NewResourceNames.Count == 0) return defaultName;

            if (cmd.NewResourceNames.TryGetValue(relativePath, out var name)) return name;
            if (cmd.NewResourceNames.TryGetValue(relativePath.Replace('\\', '/'), out name)) return name;
            if (cmd.NewResourceNames.TryGetValue(relativePath.Replace('/', '\\'), out name)) return name;

            foreach (var (rawKey, mappedName) in cmd.NewResourceNames)
            {
                if (ToRelativePath(rawKey, rootPath).Equals(relativePath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))
                    return mappedName;
            }

            return defaultName;
        }

        // Do sử dụng Guid làm Id của entity -> khi khởi tạo đã có ngay id
        // Có thể đẩy vào mảng sau đó raise event
        var createdVersions = new List<ResourceVersionCreatedInfo>();

        var toAdd = diff
            .Added.Where(x => IsTargetMatch(x.RelativePath))
            .Select(x =>
                ResourceItem.Create(
                    cmd.WorkspaceId,
                    workspaceAgentId,
                    x.PlatformExtensionId,
                    GetNewResourceName(x.RelativePath, x.Name ?? Path.GetFileName(x.RelativePath) ?? "Unnamed"),
                    x.RelativePath,
                    x.LocalHash ?? "",
                    x.LocalFileSize ?? 0
                )
            )
            .ToList();

        if (toAdd.Count > 0)
        {
            createdVersions.AddRange(
                toAdd.Select(x => new ResourceVersionCreatedInfo(
                    x.LatestVersion!.Id,
                    x.PlatformExtensionId,
                    x.ContentId,
                    ResourcePathHelper.GetExtension(x.RelativePath),
                    x.RelativePath
                ))
            );
            dbContext.ResourceItems.AddRange(toAdd);
        }

        var pathsToFetch = diff
            .Modified.Concat(diff.Deleted)
            .Select(x => x.RelativePath)
            .Where(IsTargetMatch)
            .ToList();

        var existingItems = await dbContext
            .ResourceItems.Include(r => r.Versions)
                .ThenInclude(v => v.Locations)
            .Where(x => x.WorkspaceId == cmd.WorkspaceId && pathsToFetch.Contains(x.RelativePath))
            .ToDictionaryAsync(x => x.RelativePath, y => y, ct);

        var modCount = 0;
        foreach (var mod in diff.Modified.Where(x => IsTargetMatch(x.RelativePath)))
        {
            if (existingItems.TryGetValue(mod.RelativePath, out var item))
            {
                var newVersion = item.AddNewVersion(
                    workspaceAgentId,
                    mod.LocalHash ?? "",
                    mod.LocalFileSize ?? 0,
                    cmd.Notes
                );
                dbContext.ResourceVersions.Add(newVersion);
                modCount++;
                createdVersions.Add(new ResourceVersionCreatedInfo(
                    newVersion.Id,
                    item.PlatformExtensionId,
                    item.ContentId,
                    ResourcePathHelper.GetExtension(item.RelativePath),
                    item.RelativePath
                ));
            }
        }

        var locations = existingItems
            .Where(x => diff.Deleted.Any(y => y.RelativePath == x.Key))
            .SelectMany(x => x.Value.Versions)
            .SelectMany(v => v.Locations)
            .Where(loc => loc.WorkspaceAgentId == workspaceAgentId)
            .ToList();

        var locationRemove = 0;
        if (locations.Count > 0)
        {
            dbContext.ResourceVersionLocations.RemoveRange(locations);
            locationRemove = locations.Count;
        }

        await dbContext.SaveChangesAsync(ct);

        // Sau khi thành công, lấy ProjectId và raise event
        if (createdVersions.Count > 0)
        {
            var projectId = await dbContext.Workspaces
                .AsNoTracking()
                .Where(w => w.Id == cmd.WorkspaceId)
                .Select(w => w.ProjectId)
                .FirstOrDefaultAsync(ct);

            await bus.PublishAsync(
                new ResourcesCreatedEvent(
                    projectId,
                    cmd.WorkspaceId,
                    cmd.AgentId,
                    createdVersions
                )
            );
        }

        var syncedResources = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        void RegisterSynced(string? path, Guid vId)
        {
            if (string.IsNullOrWhiteSpace(path) || vId == Guid.Empty) return;

            syncedResources[path] = vId;
            syncedResources[path.Replace('\\', '/')] = vId;
            syncedResources[path.Replace('/', '\\')] = vId;

            if (!string.IsNullOrWhiteSpace(rootPath))
            {
                var isRooted = Path.IsPathRooted(path) || (path.Length > 2 && path[1] == ':');
                if (!isRooted)
                {
                    var cleanRel = path.TrimStart('/', '\\');
                    var fullLocalPath = Path.Combine(rootPath, cleanRel);
                    syncedResources[fullLocalPath] = vId;
                    syncedResources[fullLocalPath.Replace('\\', '/')] = vId;
                    syncedResources[fullLocalPath.Replace('/', '\\')] = vId;
                }
            }
        }

        // 1. Luôn nạp từ createdVersions (các version vừa mới tạo hoặc cập nhật trong đợt sync này)
        foreach (var cv in createdVersions)
        {
            RegisterSynced(cv.RelativePath, cv.ResourceVersionId);
        }

        // 2. Nạp từ toAdd (các item mới thêm)
        foreach (var item in toAdd)
        {
            var vId = item.LatestVersion?.Id ?? Guid.Empty;
            if (vId != Guid.Empty)
            {
                RegisterSynced(item.RelativePath, vId);
            }
        }

        // 3. Nạp từ existingItems (các item đã có trong workspace)
        foreach (var item in existingItems.Values)
        {
            var vId = item.LatestVersion?.Id ?? Guid.Empty;
            if (vId != Guid.Empty)
            {
                RegisterSynced(item.RelativePath, vId);
            }
        }

        // 4. Query DB bổ sung
        var searchRelPaths = candidateRelativePaths.ToList();
        var targetItems = await dbContext.ResourceItems
            .AsNoTracking()
            .Where(x => x.WorkspaceId == cmd.WorkspaceId && searchRelPaths.Contains(x.RelativePath))
            .Select(x => new
            {
                x.RelativePath,
                VersionId = x.Versions.OrderByDescending(v => v.VersionNo).Select(v => v.Id).FirstOrDefault()
            })
            .ToListAsync(ct);

        foreach (var item in targetItems.Where(x => x.VersionId != Guid.Empty))
        {
            RegisterSynced(item.RelativePath, item.VersionId);
        }

        // 5. Map lại các TargetPaths ban đầu
        foreach (var rawPath in cmd.TargetPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            if (normalizedLookup.TryGetValue(rawPath, out var relPath))
            {
                if (syncedResources.TryGetValue(relPath, out var vId))
                {
                    RegisterSynced(rawPath, vId);
                }
            }

            // Nếu chỉ có 1 file được tạo và rawPath chưa được map, gán trực tiếp
            if (!syncedResources.ContainsKey(rawPath) && createdVersions.Count == 1)
            {
                RegisterSynced(rawPath, createdVersions[0].ResourceVersionId);
            }
        }

        var versionIds = createdVersions.Select(x => x.ResourceVersionId).ToList();
        if (versionIds.Count == 0 && syncedResources.Count > 0)
        {
            versionIds = syncedResources.Values.Distinct().ToList();
        }

        return Result.Ok(
            new SyncLocalChangesResult(
                cmd.WorkspaceId,
                workspaceAgentId,
                toAdd.Count,
                modCount,
                locationRemove,
                versionIds,
                syncedResources
            )
        );
    }

    private static string ToRelativePath(string path, string? rootPath)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;

        var trimmed = path.Trim();
        if (!string.IsNullOrWhiteSpace(rootPath))
        {
            var normPath = trimmed.Replace('\\', '/').TrimEnd('/');
            var normRoot = rootPath.Replace('\\', '/').TrimEnd('/');

            if (normPath.StartsWith(normRoot, StringComparison.OrdinalIgnoreCase))
            {
                var sub = normPath.Substring(normRoot.Length).TrimStart('/');
                return sub;
            }

            try
            {
                if (Path.IsPathRooted(trimmed))
                {
                    var rel = Path.GetRelativePath(rootPath, trimmed).Replace('\\', '/').Trim('/');
                    if (!rel.StartsWith("../") && !rel.StartsWith("..\\") && rel != "..")
                    {
                        return rel;
                    }
                }
            }
            catch
            {
                // Fallback nếu Path API gặp lỗi định dạng
            }
        }

        return trimmed.Replace('\\', '/').TrimStart('/');
    }
}
