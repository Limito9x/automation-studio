using System.Text.Json;
using System.Text.RegularExpressions;
using Automation.Tag.Contracts.Dtos;
using Automation.Workspace.Contracts.Dtos;

namespace Automation.Workspace.Contracts.Extensions;

public static class MetadataExtensions
{
    /// <summary>
    /// Lấy giá trị (dạng object/string) từ trường dữ liệu của Metadata theo TagId
    /// </summary>
    public static object? GetValueByTagId(this ResourceMetadataDetailDto detail, Guid tagId)
    {
        if (detail.Metadata == null)
            return null;

        // Tìm entry trong TagMap có chứa TagId cần tìm
        var matchedEntry = detail.TagMap.FirstOrDefault(kvp =>
            kvp.Value.Any(t => t.TagId == tagId)
        );

        if (matchedEntry.Value == null || string.IsNullOrWhiteSpace(matchedEntry.Key))
            return null;

        return ExtractJsonValue(detail.Metadata.RootElement, matchedEntry.Key);
    }

    /// <summary>
    /// Lấy danh sách tất cả các giá trị gắn với TagId (trường hợp tag gắn ở nhiều field)
    /// </summary>
    public static IReadOnlyList<object> GetAllValuesByTagId(this ResourceMetadataDetailDto detail, Guid tagId)
    {
        if (detail.Metadata == null)
            return Array.Empty<object>();

        var matchedPaths = detail.TagMap
            .Where(kvp => kvp.Value.Any(t => t.TagId == tagId) && !string.IsNullOrWhiteSpace(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        var values = new List<object>(matchedPaths.Count);
        foreach (var path in matchedPaths)
        {
            var val = ExtractJsonValue(detail.Metadata.RootElement, path);
            if (val is IEnumerable<object> list)
            {
                values.AddRange(list);
            }
            else if (val != null)
            {
                values.Add(val);
            }
        }

        return values;
    }

    /// <summary>
    /// Lấy danh sách tất cả các giá trị gắn với Tag (hỗ trợ cả TagId GUID, TagPath dạng 'Mesh.Body', hoặc TagName 'Body')
    /// </summary>
    public static IReadOnlyList<object> GetAllValuesByTag(this ResourceMetadataDetailDto detail, object? tagIdentifier)
    {
        if (detail.Metadata == null || tagIdentifier == null)
            return Array.Empty<object>();

        var raw = tagIdentifier.ToString()?.Trim();
        if (string.IsNullOrEmpty(raw))
            return Array.Empty<object>();

        Guid? tagGuid = null;
        if (Guid.TryParse(raw, out var parsedGuid))
        {
            tagGuid = parsedGuid;
        }
        else if (raw.Contains(':') && !raw.StartsWith('{'))
        {
            var lastColon = raw.LastIndexOf(':');
            if (lastColon >= 0 && Guid.TryParse(raw[(lastColon + 1)..].Trim(), out var suffixedGuid))
            {
                tagGuid = suffixedGuid;
            }
        }

        var matchedPaths = detail.TagMap
            .Where(kvp => kvp.Value.Any(t =>
                (tagGuid != null && t.TagId == tagGuid.Value) ||
                string.Equals(t.TagPath, raw, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.TagName, raw, StringComparison.OrdinalIgnoreCase) ||
                t.TagPath.EndsWith("." + raw, StringComparison.OrdinalIgnoreCase)
            ) && !string.IsNullOrWhiteSpace(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        var values = new List<object>(matchedPaths.Count);
        foreach (var path in matchedPaths)
        {
            var val = ExtractJsonValue(detail.Metadata.RootElement, path);
            if (val is IEnumerable<object> list)
            {
                values.AddRange(list);
            }
            else if (val != null)
            {
                values.Add(val);
            }
        }

        return values;
    }

    /// <summary>
    /// Lấy giá trị (dạng object/string) từ trường dữ liệu của Metadata theo TagId cho ResourceBatchItemDto
    /// </summary>
    public static object? GetValueByTagId(this ResourceBatchItemDto detail, Guid tagId)
    {
        if (detail.Metadata == null)
            return null;

        var matchedEntry = detail.TagMap.FirstOrDefault(kvp =>
            kvp.Value.Any(t => t.TagId == tagId)
        );

        if (matchedEntry.Value == null || string.IsNullOrWhiteSpace(matchedEntry.Key))
            return null;

        return ExtractJsonValue(detail.Metadata.RootElement, matchedEntry.Key);
    }

    /// <summary>
    /// Lấy danh sách tất cả các giá trị gắn với TagId cho ResourceBatchItemDto
    /// </summary>
    public static IReadOnlyList<object> GetAllValuesByTagId(this ResourceBatchItemDto detail, Guid tagId)
    {
        if (detail.Metadata == null)
            return Array.Empty<object>();

        var matchedPaths = detail.TagMap
            .Where(kvp => kvp.Value.Any(t => t.TagId == tagId) && !string.IsNullOrWhiteSpace(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        var values = new List<object>(matchedPaths.Count);
        foreach (var path in matchedPaths)
        {
            var val = ExtractJsonValue(detail.Metadata.RootElement, path);
            if (val is IEnumerable<object> list)
            {
                values.AddRange(list);
            }
            else if (val != null)
            {
                values.Add(val);
            }
        }

        return values;
    }


    /// <summary>
    /// Trích xuất giá trị từ JsonElement theo đường dẫn JSONPath (hỗ trợ dot notation, array indexing: "main_objects[0].name", "main_objects[*].name", "video.resolution", v.v.)
    /// </summary>
    public static object? ExtractJsonValue(this JsonElement root, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return GetElementValue(root);

        // Normalize path: convert "a[0].b" or "a[10][2]" -> "a.0.b" / "a.10.2"
        var normalizedPath = Regex.Replace(path.TrimStart('$', '.', '/'), @"\[(\d+|\*)\]", ".$1")
            .Replace("[]", ".*");

        var parts = normalizedPath.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return TraversePath(root, parts, 0);
    }

    private static object? TraversePath(JsonElement current, string[] parts, int index)
    {
        if (index >= parts.Length)
            return GetElementValue(current);

        var segment = parts[index];

        // 1. If segment is a wildcard "*" -> evaluate all array elements
        if (segment == "*")
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object>();
                foreach (var item in current.EnumerateArray())
                {
                    var childVal = TraversePath(item, parts, index + 1);
                    if (childVal is IEnumerable<object> subList)
                        list.AddRange(subList);
                    else if (childVal != null)
                        list.Add(childVal);
                }
                return list.Count > 0 ? list : null;
            }
            return null;
        }

        // 2. If segment is an integer index (e.g. "0", "1") and current is Array
        if (int.TryParse(segment, out var arrayIndex))
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                if (arrayIndex >= 0 && arrayIndex < current.GetArrayLength())
                {
                    return TraversePath(current[arrayIndex], parts, index + 1);
                }
                return null;
            }
        }

        // 3. If current is Object, look up property name
        if (current.ValueKind == JsonValueKind.Object)
        {
            if (current.TryGetProperty(segment, out var next))
            {
                return TraversePath(next, parts, index + 1);
            }

            // Case-insensitive fallback
            foreach (var prop in current.EnumerateObject())
            {
                if (string.Equals(prop.Name, segment, StringComparison.OrdinalIgnoreCase))
                {
                    return TraversePath(prop.Value, parts, index + 1);
                }
            }
            return null;
        }

        return null;
    }

    private static object? GetElementValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l)
                ? l
                : element.TryGetDouble(out var d)
                    ? d
                    : element.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(GetElementValue).Where(x => x != null).ToList()!,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.GetRawText()
        };
}

public static class TagMigrationHelper
{
    public static List<UpdatedTagLink> RealignTagPaths(
        JsonElement oldMetadata,
        JsonElement newMetadata,
        IReadOnlyList<TagLinkDetailDto> existingLinks)
    {
        var updatedLinks = new List<UpdatedTagLink>();

        foreach (var link in existingLinks)
        {
            var oldPath = !string.IsNullOrEmpty(link.TargetSubPath) ? link.TargetSubPath : ExtractPath(link.MetadataJson);
            if (string.IsNullOrWhiteSpace(oldPath))
                continue;

            // 1. Thuộc tính tĩnh không chứa index mảng -> Giữ nguyên
            if (!oldPath.Contains('['))
            {
                updatedLinks.Add(new(link.TagId, oldPath));
                continue;
            }

            // 2. Lấy giá trị cũ mà tag đang trỏ vào
            var capturedValue = oldMetadata.ExtractJsonValue(oldPath)?.ToString();
            if (string.IsNullOrEmpty(capturedValue))
                continue;

            // 3. Tách tên mảng và sub-path: "objects[1].name" -> array="objects", sub="name"
            var match = Regex.Match(oldPath, @"^([a-zA-Z0-9_]+)\[\d+\]\.(.*)$");
            if (!match.Success)
            {
                updatedLinks.Add(new(link.TagId, oldPath));
                continue;
            }

            var arrayPropName = match.Groups[1].Value;
            var subPath = match.Groups[2].Value;

            // 4. Dò tìm trong mảng mới xem phần tử nào có giá trị trùng khớp
            if (newMetadata.TryGetProperty(arrayPropName, out var newArray) && newArray.ValueKind == JsonValueKind.Array)
            {
                var newIndex = -1;
                for (int i = 0; i < newArray.GetArrayLength(); i++)
                {
                    var itemVal = newArray[i].ExtractJsonValue(subPath)?.ToString();
                    if (string.Equals(itemVal, capturedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        newIndex = i;
                        break;
                    }
                }

                if (newIndex != -1)
                {
                    var newPath = $"{arrayPropName}[{newIndex}].{subPath}";
                    updatedLinks.Add(new(link.TagId, newPath));
                }
            }
        }

        return updatedLinks;
    }

    public static string ExtractPath(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(metadataJson);
            if (
                doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("path", out var pathProp)
            )
            {
                return pathProp.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            return string.Empty;
        }

        return string.Empty;
    }
}
