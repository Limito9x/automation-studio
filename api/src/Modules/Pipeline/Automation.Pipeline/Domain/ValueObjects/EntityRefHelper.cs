using System.Text.Json;

namespace Automation.Pipeline.Domain.ValueObjects;

/// <summary>
/// Helper xử lý chuẩn hóa và giải mã đối tượng EntityReference dạng { "$type": "Resource", "$ref": "guid" }
/// </summary>
public static class EntityRefHelper
{
    public static (string Type, Guid Id, bool IsValid) Parse(object? input)
    {
        if (input == null)
            return (string.Empty, Guid.Empty, false);

        // 1. Direct Guid
        if (input is Guid directGuid && directGuid != Guid.Empty)
        {
            return (string.Empty, directGuid, true);
        }

        // 2. String representation (raw GUID, Type:Guid, or JSON string)
        var str = input.ToString()?.Trim();
        if (string.IsNullOrEmpty(str))
            return (string.Empty, Guid.Empty, false);

        if (Guid.TryParse(str, out var parsedGuid))
        {
            return (string.Empty, parsedGuid, true);
        }

        // Support "Type:Guid" and "urn:type:guid" formats (e.g. "Resource:e94ae6c6-...", "urn:resource:...")
        if (str.Contains(':') && !str.StartsWith('{'))
        {
            var lastColon = str.LastIndexOf(':');
            if (lastColon >= 0 && Guid.TryParse(str[(lastColon + 1)..].Trim(), out var suffixedGuid))
            {
                var typePart = str[..lastColon].Trim();
                return (typePart, suffixedGuid, true);
            }
        }

        // 3. Dictionary / KeyValuePair format
        if (input is System.Collections.IDictionary dict)
        {
            string? type = null;
            string? refVal = null;

            foreach (var key in dict.Keys)
            {
                if (key == null) continue;
                var kStr = key.ToString();
                if (string.Equals(kStr, "$type", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(kStr, "type", StringComparison.OrdinalIgnoreCase))
                {
                    type = dict[key]?.ToString();
                }
                else if (string.Equals(kStr, "$ref", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(kStr, "id", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(kStr, "$id", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(kStr, "resourceVersionId", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(kStr, "resourceId", StringComparison.OrdinalIgnoreCase))
                {
                    refVal = dict[key]?.ToString();
                }
            }

            if (Guid.TryParse(refVal, out var dictGuid))
            {
                return (type ?? string.Empty, dictGuid, true);
            }
        }

        // 4. JsonElement object { "$type": "...", "$ref": "..." }
        if (input is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            string? type = null;
            string? refStr = null;

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.NameEquals("$type") || string.Equals(prop.Name, "type", StringComparison.OrdinalIgnoreCase))
                {
                    type = prop.Value.GetString();
                }
                else if (prop.NameEquals("$ref") ||
                         string.Equals(prop.Name, "id", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(prop.Name, "$id", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(prop.Name, "resourceVersionId", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(prop.Name, "resourceId", StringComparison.OrdinalIgnoreCase))
                {
                    refStr = prop.Value.GetString();
                }
            }

            if (!string.IsNullOrEmpty(refStr) && Guid.TryParse(refStr, out var elGuid))
            {
                return (type ?? string.Empty, elGuid, true);
            }
        }

        // 5. JSON serialized string fallback
        if (str.StartsWith('{') && str.EndsWith('}'))
        {
            try
            {
                using var doc = JsonDocument.Parse(str);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    string? type = null;
                    string? refStr = null;

                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        if (prop.NameEquals("$type") || string.Equals(prop.Name, "type", StringComparison.OrdinalIgnoreCase))
                        {
                            type = prop.Value.GetString();
                        }
                        else if (prop.NameEquals("$ref") ||
                                 string.Equals(prop.Name, "id", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(prop.Name, "$id", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(prop.Name, "resourceVersionId", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(prop.Name, "resourceId", StringComparison.OrdinalIgnoreCase))
                        {
                            refStr = prop.Value.GetString();
                        }
                    }

                    if (!string.IsNullOrEmpty(refStr) && Guid.TryParse(refStr, out var sGuid))
                    {
                        return (type ?? string.Empty, sGuid, true);
                    }
                }
            }
            catch
            {
                // Not a valid JSON object
            }
        }

        return (string.Empty, Guid.Empty, false);
    }

    public static Guid? ExtractRefId(object? input)
    {
        var (_, id, isValid) = Parse(input);
        return isValid && id != Guid.Empty ? id : null;
    }

    public static Dictionary<string, object?> Create(string type, Guid id)
    {
        return new Dictionary<string, object?>
        {
            ["$type"] = type,
            ["$ref"] = id.ToString()
        };
    }
}
