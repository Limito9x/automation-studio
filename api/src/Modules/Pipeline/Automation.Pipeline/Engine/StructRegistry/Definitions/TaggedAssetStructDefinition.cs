using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Tools;

namespace Automation.Pipeline.Engine.StructRegistry.Definitions;

/// <summary>
/// Static Struct đại diện cho một Asset đã được gắn Tag (sản phẩm từ BuildTagMapFromResource).
/// Quản lý hoàn toàn in-memory trong C#, không lưu DB.
/// </summary>
public class TaggedAssetStructDefinition : IEntityStructDefinition
{
    public string StructType => "TaggedAsset";
    public string Label => "Tagged Asset";

    public IReadOnlyList<PinDefinition> OutputPins =>
    [
        new()
        {
            Id = "ResourceId",
            Label = "Resource ID",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "VersionId",
            Label = "Version ID",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "AssetName",
            Label = "Asset Name",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "RelativePath",
            Label = "Relative Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "FilePath",
            Label = "File Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "ResourceTags",
            Label = "Resource Tags",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Array
        },
        new()
        {
            Id = "TagMap",
            Label = "Tag Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map
        },
        new()
        {
            Id = "PathMap",
            Label = "Path Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map
        }
    ];

    public Task<Dictionary<string, object>> ResolveAsync(
        object targetInput,
        ToolExecutionContext context
    )
    {
        var result = new Dictionary<string, object>();

        if (targetInput is JsonElement je && je.ValueKind == JsonValueKind.Object)
        {
            result["ResourceId"] = GetJsonProp(je, "resource_id", "ResourceId");
            result["VersionId"] = GetJsonProp(je, "version_id", "VersionId");
            result["AssetName"] = GetJsonProp(je, "asset_name", "AssetName");
            result["RelativePath"] = GetJsonProp(je, "relative_path", "RelativePath");
            result["FilePath"] = GetJsonProp(je, "file_path", "FilePath");
            result["ResourceTags"] = ExtractArray(je, "resource_tags", "ResourceTags");
            result["TagMap"] = ExtractMap(je, "tag_map", "TagMap");
            result["PathMap"] = ExtractMap(je, "path_map", "PathMap");
            return Task.FromResult(result);
        }

        if (targetInput is IDictionary<string, object?> dict)
        {
            result["ResourceId"] = GetDictProp(dict, "resource_id", "ResourceId") ?? "";
            result["VersionId"] = GetDictProp(dict, "version_id", "VersionId") ?? "";
            result["AssetName"] = GetDictProp(dict, "asset_name", "AssetName") ?? "";
            result["RelativePath"] = GetDictProp(dict, "relative_path", "RelativePath") ?? "";
            result["FilePath"] = GetDictProp(dict, "file_path", "FilePath") ?? "";
            result["ResourceTags"] = GetDictProp(dict, "resource_tags", "ResourceTags") ?? Array.Empty<string>();
            result["TagMap"] = GetDictProp(dict, "tag_map", "TagMap") ?? new Dictionary<string, object>();
            result["PathMap"] = GetDictProp(dict, "path_map", "PathMap") ?? new Dictionary<string, object>();
            return Task.FromResult(result);
        }

        if (targetInput is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonStr);
                return ResolveAsync(doc.RootElement.Clone(), context);
            }
            catch
            {
                // Ignore parse errors and return empty result
            }
        }

        return Task.FromResult(result);
    }

    private static string GetJsonProp(JsonElement el, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (el.TryGetProperty(name, out var val))
            {
                return val.GetString() ?? val.ToString();
            }
        }
        return string.Empty;
    }

    private static object ExtractArray(JsonElement el, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (el.TryGetProperty(name, out var val) && val.ValueKind == JsonValueKind.Array)
            {
                return val.EnumerateArray().Select(x => x.GetString() ?? x.ToString()).ToArray();
            }
        }
        return Array.Empty<string>();
    }

    private static object ExtractMap(JsonElement el, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (el.TryGetProperty(name, out var val) && val.ValueKind == JsonValueKind.Object)
            {
                var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in val.EnumerateObject())
                {
                    map[prop.Name] = ConvertJsonElement(prop.Value);
                }
                return map;
            }
        }
        return new Dictionary<string, object?>();
    }

    private static object? ConvertJsonElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var l) ? l : el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => el.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value), StringComparer.OrdinalIgnoreCase),
            _ => el.GetRawText()
        };
    }

    private static object? GetDictProp(IDictionary<string, object?> dict, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            if (dict.TryGetValue(name, out var val) && val != null)
            {
                return val;
            }
        }
        return null;
    }
}
