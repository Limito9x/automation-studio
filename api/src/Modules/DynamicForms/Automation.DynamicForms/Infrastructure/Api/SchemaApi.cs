using System.Text.Json;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Domain.Entities;
using Automation.DynamicForms.Infrastructure.Persistence;
using Automation.DynamicForms.Services;
using Microsoft.EntityFrameworkCore;

namespace Automation.DynamicForms.Infrastructure.Api;

public class SchemaApi(DynamicFormsDbContext db, IEnumerable<RegisteredDynamicSchema> registeredSchemas, IDynamicFormEngine engine) : ISchemaApi
{
    public async Task<Result<SchemaVersionDto>> GetActiveVersionAsync(string ownerType, string ownerId, CancellationToken ct = default)
    {
        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.OwnerType == ownerType && s.OwnerId == ownerId, ct);

        if (schema == null)
            return Result.Fail(new NotFoundError($"Schema for {ownerType} {ownerId} not found"));

        var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
        if (activeVersion == null)
            return Result.Fail(new NotFoundError($"Active schema version for {ownerType} {ownerId} not found"));

        return new SchemaVersionDto
        {
            Id = activeVersion.Id,
            SchemaDefinitionId = activeVersion.SchemaDefinitionId,
            Fields = activeVersion.Fields,
            Version = activeVersion.Version,
            IsActive = activeVersion.IsActive,
            DependencySchemaIds = activeVersion.DependencySchemaIds
        };
    }

    public async Task<Result<SchemaWithDependenciesDto>> GetActiveVersionWithDependenciesAsync(Guid schemaDefinitionId, CancellationToken ct = default)
    {
        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schemaDefinitionId, ct);

        if (schema == null)
            return Result.Fail(new NotFoundError($"Schema with id '{schemaDefinitionId}' not found"));

        return await BuildSchemaWithDependenciesResponseAsync(schema, ct);
    }

    public async Task<Result<SchemaWithDependenciesDto>> GetActiveVersionWithDependenciesAsync(string ownerType, string ownerId, CancellationToken ct = default)
    {
        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OwnerType == ownerType && s.OwnerId == ownerId, ct);

        if (schema == null)
            return Result.Fail(new NotFoundError($"Schema for {ownerType} {ownerId} not found"));

        return await BuildSchemaWithDependenciesResponseAsync(schema, ct);
    }

    private async Task<Result<SchemaWithDependenciesDto>> BuildSchemaWithDependenciesResponseAsync(SchemaDefinition schema, CancellationToken ct)
    {
        var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
        if (activeVersion == null)
            return Result.Fail(new NotFoundError($"Active schema version for schema '{schema.Id}' not found"));

        var dependenciesDict = new Dictionary<Guid, SchemaVersionDto>();

        if (activeVersion.DependencySchemaIds.Count > 0)
        {
            var depSchemas = await db.SchemaDefinitions
                .Include(s => s.Versions.Where(v => v.IsActive))
                .AsNoTracking()
                .Where(s => activeVersion.DependencySchemaIds.Contains(s.Id))
                .ToListAsync(ct);

            foreach (var dep in depSchemas)
            {
                var depActiveVersion = dep.Versions.FirstOrDefault(v => v.IsActive);
                if (depActiveVersion != null)
                {
                    dependenciesDict[dep.Id] = new SchemaVersionDto
                    {
                        Id = depActiveVersion.Id,
                        SchemaDefinitionId = dep.Id,
                        Fields = depActiveVersion.Fields,
                        Version = depActiveVersion.Version,
                        IsActive = depActiveVersion.IsActive,
                        DependencySchemaIds = depActiveVersion.DependencySchemaIds
                    };
                }
            }
        }

        return Result.Ok(new SchemaWithDependenciesDto
        {
            SchemaDefinitionId = schema.Id,
            Name = schema.Name,
            OwnerType = schema.OwnerType,
            OwnerId = schema.OwnerId,
            ActiveVersion = new SchemaVersionDto
            {
                Id = activeVersion.Id,
                SchemaDefinitionId = schema.Id,
                Fields = activeVersion.Fields,
                Version = activeVersion.Version,
                IsActive = activeVersion.IsActive,
                DependencySchemaIds = activeVersion.DependencySchemaIds
            },
            Dependencies = dependenciesDict
        });
    }

    public async Task<Result<bool>> CanDeleteSchemaAsync(Guid schemaDefinitionId, CancellationToken ct = default)
    {
        var isReferenced = await db.SchemaVersions
            .AnyAsync(v => v.IsActive && v.DependencySchemaIds.Contains(schemaDefinitionId), ct);

        return Result.Ok(!isReferenced);
    }

    public async Task<Result<IReadOnlyList<SchemaReferencingUsageDto>>> GetReferencingSchemasAsync(Guid schemaDefinitionId, CancellationToken ct = default)
    {
        var referencingVersions = await db.SchemaVersions
            .Include(v => v.SchemaDefinition)
            .AsNoTracking()
            .Where(v => v.IsActive && v.DependencySchemaIds.Contains(schemaDefinitionId))
            .Select(v => new SchemaReferencingUsageDto
            {
                SchemaDefinitionId = v.SchemaDefinition.Id,
                Name = v.SchemaDefinition.Name,
                OwnerType = v.SchemaDefinition.OwnerType,
                OwnerId = v.SchemaDefinition.OwnerId
            })
            .Distinct()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<SchemaReferencingUsageDto>>(referencingVersions);
    }

    public async Task<Result<SchemaDataDto>> SaveDataAsync(string ownerType, string ownerId, string clientId, string clientType, JsonDocument values, CancellationToken ct = default)
    {
        // 1. Check if ownerType is registered
        if (!registeredSchemas.Any(r => r.OwnerType == ownerType))
            return Result.Fail(new Error($"OwnerType '{ownerType}' is not registered to use DynamicForms."));

        // 2. Get active schema version
        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.OwnerType == ownerType && s.OwnerId == ownerId, ct);

        if (schema == null)
            return Result.Fail(new NotFoundError($"Schema for {ownerType} {ownerId} not found"));

        var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
        if (activeVersion == null)
            return Result.Fail(new NotFoundError($"Active schema version for {ownerType} {ownerId} not found"));

        // Preload dependency schemas if any
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null;
        if (activeVersion.DependencySchemaIds.Count > 0)
        {
            preloadedSchemas = await db.SchemaVersions
                .AsNoTracking()
                .Where(v => activeVersion.DependencySchemaIds.Contains(v.SchemaDefinitionId) && v.IsActive)
                .ToDictionaryAsync(v => v.SchemaDefinitionId, v => v.Fields, ct);
        }

        // 3. Validation Engine
        var validationResult = engine.ValidateValues(activeVersion.Fields, values, preloadedSchemas);
        if (validationResult.IsFailed)
            return validationResult;

        // 4. Auto-migration / Data normalization
        var normalizedValues = engine.NormalizeValues(activeVersion.Fields, values, preloadedSchemas);

        // 5. Save or Update
        var existingData = await db.SchemaData
            .FirstOrDefaultAsync(d => d.ClientId == clientId && d.ClientType == clientType, ct);

        if (existingData != null)
        {
            existingData.UpdateValues(normalizedValues, activeVersion.Id);
        }
        else
        {
            existingData = new SchemaData(activeVersion.Id, normalizedValues, clientId, clientType);
            db.SchemaData.Add(existingData);
        }

        await db.SaveChangesAsync(ct);

        // 6. Link file fields (if any) to asset links
        await engine.LinkFileFieldsAsync(existingData.Id.ToString(), activeVersion.Fields, normalizedValues, ct, preloadedSchemas);

        // 7. Resolve data for response
        var resolvedDataResult = await engine.ResolveDataAsync(existingData.Id.ToString(), activeVersion.Fields, existingData.Values, ct, preloadedSchemas);

        return new SchemaDataDto
        {
            Id = existingData.Id,
            SchemaVersion = activeVersion.Fields,
            Values = existingData.Values,
            ResolvedData = resolvedDataResult.IsSuccess ? resolvedDataResult.Value : existingData.Values,
            ClientId = existingData.ClientId,
            ClientType = existingData.ClientType
        };
    }

    public async Task<Result<SchemaDataDto>> GetDataAsync(string clientId, string clientType, CancellationToken ct = default)
    {
        var data = await db.SchemaData
            .Include(d => d.SchemaVersion)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ClientId == clientId && (d.ClientType == clientType || clientType == "ContentItem" || string.IsNullOrEmpty(clientType)), ct);

        if (data == null)
            return Result.Fail(new NotFoundError($"Schema data for {clientType} {clientId} not found"));

        IDictionary<Guid, JsonDocument>? preloadedSchemas = null;
        if (data.SchemaVersion.DependencySchemaIds.Count > 0)
        {
            preloadedSchemas = await db.SchemaVersions
                .AsNoTracking()
                .Where(v => data.SchemaVersion.DependencySchemaIds.Contains(v.SchemaDefinitionId) && v.IsActive)
                .ToDictionaryAsync(v => v.SchemaDefinitionId, v => v.Fields, ct);
        }

        var resolvedDataResult = await engine.ResolveDataAsync(data.Id.ToString(), data.SchemaVersion.Fields, data.Values, ct, preloadedSchemas);

        return new SchemaDataDto
        {
            Id = data.Id,
            SchemaVersion = data.SchemaVersion.Fields,
            Values = data.Values,
            ResolvedData = resolvedDataResult.IsSuccess ? resolvedDataResult.Value : data.Values,
            ClientId = data.ClientId,
            ClientType = data.ClientType
        };
    }

    public async Task<Result<IEnumerable<SchemaDataDto>>> GetMultipleDataAsync(IEnumerable<string> clientIds, string clientType, CancellationToken ct = default)
    {
        var clientIdsList = clientIds.ToList();
        
        var data = await db.SchemaData
            .Include(d => d.SchemaVersion)
            .AsNoTracking()
            .Where(d => clientIdsList.Contains(d.ClientId) && (d.ClientType == clientType || clientType == "ContentItem" || string.IsNullOrEmpty(clientType)))
            .ToListAsync(ct);

        var dtos = new List<SchemaDataDto>();
        foreach (var d in data)
        {
            IDictionary<Guid, JsonDocument>? preloadedSchemas = null;
            if (d.SchemaVersion.DependencySchemaIds.Count > 0)
            {
                preloadedSchemas = await db.SchemaVersions
                    .AsNoTracking()
                    .Where(v => d.SchemaVersion.DependencySchemaIds.Contains(v.SchemaDefinitionId) && v.IsActive)
                    .ToDictionaryAsync(v => v.SchemaDefinitionId, v => v.Fields, ct);
            }

            var resolvedDataResult = await engine.ResolveDataAsync(d.Id.ToString(), d.SchemaVersion.Fields, d.Values, ct, preloadedSchemas);
            dtos.Add(new SchemaDataDto
            {
                Id = d.Id,
                SchemaVersion = d.SchemaVersion.Fields,
                Values = d.Values,
                ResolvedData = resolvedDataResult.IsSuccess ? resolvedDataResult.Value : d.Values,
                ClientId = d.ClientId,
                ClientType = d.ClientType
            });
        }

        return Result.Ok<IEnumerable<SchemaDataDto>>(dtos);
    }

    public async Task<Result> UpsertSchemaAsync(string ownerType, string ownerId, string schemaName, JsonDocument fields, CancellationToken ct = default)
    {
        // Check if ownerType is registered
        if (!registeredSchemas.Any(r => r.OwnerType == ownerType))
            return Result.Fail(new Error($"OwnerType '{ownerType}' is not registered to use DynamicForms."));

        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.OwnerType == ownerType && s.OwnerId == ownerId, ct);

        // 1. Extract direct dependencies and compute transitive dependencies
        var directStructIds = StructDependencyHelper.ExtractDirectStructIds(fields);
        List<Guid> transitiveDependencies = [];

        if (directStructIds.Count > 0)
        {
            var targetSchemaId = schema?.Id ?? Guid.Empty;

            async Task<IEnumerable<Guid>?> GetChildDependenciesFunc(Guid childId, CancellationToken token)
            {
                var childVersion = await db.SchemaVersions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.SchemaDefinitionId == childId && v.IsActive, token);
                return childVersion?.DependencySchemaIds;
            }

            var depResult = await StructDependencyHelper.ComputeTransitiveDependenciesAsync(targetSchemaId, directStructIds, GetChildDependenciesFunc, ct);
            if (depResult.IsFailed)
                return depResult.ToResult();

            transitiveDependencies = depResult.Value;
        }

        if (schema == null)
        {
            schema = new SchemaDefinition(schemaName, ownerId, ownerType);
            db.SchemaDefinitions.Add(schema);
            
            var initialVersion = new SchemaVersion(schema.Id, fields, 1, true)
            {
                DependencySchemaIds = transitiveDependencies
            };
            db.SchemaVersions.Add(initialVersion);
        }
        else
        {
            var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
            
            // Check if fields actually changed to avoid creating unnecessary versions
            if (activeVersion != null && JsonSerializer.Serialize(activeVersion.Fields) == JsonSerializer.Serialize(fields))
            {
                // If name changed, update it
                if (schema.Name != schemaName)
                {
                    schema.Name = schemaName;
                    await db.SaveChangesAsync(ct);
                }
                return Result.Ok();
            }

            if (activeVersion != null)
            {
                activeVersion.Deactivate();
            }

            var nextVersionNumber = schema.Versions.Any() ? schema.Versions.Max(v => v.Version) + 1 : 1;
            var newVersion = new SchemaVersion(schema.Id, fields, nextVersionNumber, true)
            {
                DependencySchemaIds = transitiveDependencies
            };
            schema.Name = schemaName;
            db.SchemaVersions.Add(newVersion);
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<IReadOnlyList<SchemaWithDependenciesDto>>> GetSchemasByOwnerAsync(string ownerType, string ownerId, CancellationToken ct = default)
    {
        var schemas = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .AsNoTracking()
            .Where(s => s.OwnerType == ownerType && s.OwnerId == ownerId)
            .ToListAsync(ct);

        var list = new List<SchemaWithDependenciesDto>();
        foreach (var s in schemas)
        {
            var res = await BuildSchemaWithDependenciesResponseAsync(s, ct);
            if (res.IsSuccess)
            {
                list.Add(res.Value);
            }
        }

        return Result.Ok<IReadOnlyList<SchemaWithDependenciesDto>>(list);
    }
}
