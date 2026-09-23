using System.Text.Json;
using System.Text.Json.Nodes;
using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Domain.Entities;
using Automation.Files.Contracts;

namespace Automation.DynamicForms.Services.Processors;

public class FileFieldProcessor(IAssetApi assetApi) : IFieldTypeProcessor
{
    public string FieldType => SchemaType.File;

    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Result Validate(FieldDefinitionContext field, JsonElement value, FieldValidationContext ctx)
    {
        bool isNullOrEmpty = value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
                             (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString())) ||
                             (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() == 0);

        if (field.IsRequired && isNullOrEmpty)
        {
            return Result.Fail(new Error(field.RequiredMessage).WithMetadata("Field", field.Name));
        }

        return Result.Ok();
    }

    public JsonNode? Normalize(FieldDefinitionContext field, JsonElement? rawValue)
    {
        if (rawValue.HasValue && rawValue.Value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
        {
            return JsonNode.Parse(rawValue.Value.GetRawText());
        }

        return null;
    }

    public async Task<Result> BeforeSaveAsync(FieldDefinitionContext field, JsonElement value, FieldSaveContext ctx, CancellationToken ct = default)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return Result.Ok();
        }

        var assetUpsertDtos = new List<AssetUpsertDto>();
        ExtractAssetUpserts(value, assetUpsertDtos);

        if (assetUpsertDtos.Count > 0)
        {
            return await assetApi.UpsertMultipleAsync(
                nameof(SchemaData),
                ctx.SchemaDataId,
                DynamicFormAssets.SchemaDataAsset,
                assetUpsertDtos,
                ct);
        }

        return Result.Ok();
    }

    public async Task<JsonNode?> ResolveAsync(FieldDefinitionContext field, JsonElement value, FieldResolveContext ctx, CancellationToken ct = default)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        var referencedAssetIds = new List<string>();
        ExtractAssetIdStrings(value, referencedAssetIds);

        var assetMap = new Dictionary<string, AssetDto>(StringComparer.OrdinalIgnoreCase);
        if (referencedAssetIds.Count > 0)
        {
            var assetsResult = await assetApi.GetAssetsByIdsAsync(
                nameof(SchemaData),
                ctx.SchemaDataId,
                DynamicFormAssets.SchemaDataAsset,
                referencedAssetIds.Distinct(),
                ct);

            if (assetsResult.IsSuccess && assetsResult.Value != null)
            {
                foreach (var asset in assetsResult.Value)
                {
                    assetMap[asset.Id.ToString()] = asset;
                }
            }
        }

        var resolvedAssetList = ResolveAssetDtosForField(value, assetMap);
        return JsonNode.Parse(JsonSerializer.Serialize(resolvedAssetList, CamelCaseOptions));
    }

    private static void ExtractAssetUpserts(JsonElement element, List<AssetUpsertDto> dtos)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractSingleAssetUpsert(item, dtos);
            }
        }
        else
        {
            ExtractSingleAssetUpsert(element, dtos);
        }
    }

    private static void ExtractSingleAssetUpsert(JsonElement item, List<AssetUpsertDto> dtos)
    {
        if (item.ValueKind == JsonValueKind.Object)
        {
            Guid assetId = Guid.Empty;
            if (item.TryGetProperty("assetId", out var idProp) && idProp.ValueKind == JsonValueKind.String)
            {
                Guid.TryParse(idProp.GetString(), out assetId);
            }
            else if (item.TryGetProperty("id", out var idProp2) && idProp2.ValueKind == JsonValueKind.String)
            {
                Guid.TryParse(idProp2.GetString(), out assetId);
            }

            if (assetId != Guid.Empty)
            {
                string name = string.Empty;
                if (item.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                {
                    name = nameProp.GetString() ?? string.Empty;
                }
                else if (item.TryGetProperty("originalName", out var nameProp2) && nameProp2.ValueKind == JsonValueKind.String)
                {
                    name = nameProp2.GetString() ?? string.Empty;
                }

                dtos.Add(new AssetUpsertDto { AssetId = assetId, Name = name });
            }
        }
        else if (item.ValueKind == JsonValueKind.String)
        {
            if (Guid.TryParse(item.GetString(), out var assetId))
            {
                dtos.Add(new AssetUpsertDto { AssetId = assetId, Name = string.Empty });
            }
        }
    }

    private static void ExtractAssetIdStrings(JsonElement element, List<string> ids)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractSingleAssetIdString(item, ids);
            }
        }
        else
        {
            ExtractSingleAssetIdString(element, ids);
        }
    }

    private static void ExtractSingleAssetIdString(JsonElement item, List<string> ids)
    {
        if (item.ValueKind == JsonValueKind.Object)
        {
            if (item.TryGetProperty("assetId", out var idProp) && idProp.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idProp.GetString()))
            {
                ids.Add(idProp.GetString()!);
            }
            else if (item.TryGetProperty("id", out var idProp2) && idProp2.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idProp2.GetString()))
            {
                ids.Add(idProp2.GetString()!);
            }
        }
        else if (item.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(item.GetString()))
        {
            ids.Add(item.GetString()!);
        }
    }

    private static List<AssetDto> ResolveAssetDtosForField(JsonElement element, Dictionary<string, AssetDto> assetMap)
    {
        var result = new List<AssetDto>();

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var dto = ResolveSingleAssetDto(item, assetMap);
                if (dto != null) result.Add(dto);
            }
        }
        else
        {
            var dto = ResolveSingleAssetDto(element, assetMap);
            if (dto != null) result.Add(dto);
        }

        return result;
    }

    private static AssetDto? ResolveSingleAssetDto(JsonElement item, Dictionary<string, AssetDto> assetMap)
    {
        string? assetIdStr = null;
        string? customName = null;

        if (item.ValueKind == JsonValueKind.Object)
        {
            if (item.TryGetProperty("assetId", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                assetIdStr = idProp.GetString();
            else if (item.TryGetProperty("id", out var idProp2) && idProp2.ValueKind == JsonValueKind.String)
                assetIdStr = idProp2.GetString();

            if (item.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                customName = nameProp.GetString();
            else if (item.TryGetProperty("originalName", out var nameProp2) && nameProp2.ValueKind == JsonValueKind.String)
                customName = nameProp2.GetString();
        }
        else if (item.ValueKind == JsonValueKind.String)
        {
            assetIdStr = item.GetString();
        }

        if (assetIdStr != null && assetMap.TryGetValue(assetIdStr, out var assetDto))
        {
            if (!string.IsNullOrEmpty(customName))
            {
                return assetDto with { Name = customName };
            }
            return assetDto;
        }

        return null;
    }
}
