using System.Text.Json;
using Automation.Pipeline.Engine;
using Automation.Pipeline.Engine.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Automation.Pipeline.Infrastructure.Redis;

public class RedisExecutionStateStore : IExecutionStateStore
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisExecutionStateStore> _logger;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(48);

    public RedisExecutionStateStore(
        ILogger<RedisExecutionStateStore> logger,
        IConnectionMultiplexer? redis = null
    )
    {
        _logger = logger;
        _redis = redis;
    }

    private IDatabase? GetDatabase()
    {
        try
        {
            return _redis?.IsConnected == true ? _redis.GetDatabase() : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get Redis database, proceeding without Redis.");
            return null;
        }
    }

    private static string StartInputKey(Guid execId, string key) =>
        $"exec:{execId}:input:{key}";

    private static string NodeOutputKey(Guid execId, Guid nodeId, string pinKey) =>
        $"exec:{execId}:node:{nodeId}:out:{pinKey}";

    private static string NodeStatusKey(Guid execId, Guid nodeId) =>
        $"exec:{execId}:node:{nodeId}:status";

    private static string FullStateKey(Guid execId) =>
        $"exec:{execId}:state";

    public async Task SetStartInputAsync(Guid execId, string key, object? value, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return;

        var redisKey = StartInputKey(execId, key);
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(redisKey, json, DefaultTtl);

        // Also track key in start inputs set for quick enumeration
        await db.SetAddAsync($"exec:{execId}:inputs", key);
    }

    public async Task<object?> GetStartInputAsync(Guid execId, string key, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return null;

        var redisKey = StartInputKey(execId, key);
        var val = await db.StringGetAsync(redisKey);
        if (!val.HasValue) return null;

        return DeserializeValue(val!);
    }

    public async Task<Dictionary<string, object?>> GetAllStartInputsAsync(Guid execId, CancellationToken ct = default)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var db = GetDatabase();
        if (db == null) return result;

        var inputKeys = await db.SetMembersAsync($"exec:{execId}:inputs");
        foreach (var k in inputKeys)
        {
            var keyStr = k.ToString();
            var val = await GetStartInputAsync(execId, keyStr, ct);
            result[keyStr] = val;
        }

        return result;
    }

    public async Task SetNodeOutputAsync(Guid execId, Guid nodeId, string pinKey, object? value, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return;

        var redisKey = NodeOutputKey(execId, nodeId, pinKey);
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(redisKey, json, DefaultTtl);

        await db.SetAddAsync($"exec:{execId}:node:{nodeId}:pins", pinKey);
        await db.SetAddAsync($"exec:{execId}:nodes", nodeId.ToString());
    }

    public async Task<object?> GetNodeOutputAsync(Guid execId, Guid nodeId, string pinKey, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return null;

        var redisKey = NodeOutputKey(execId, nodeId, pinKey);
        var val = await db.StringGetAsync(redisKey);
        if (!val.HasValue) return null;

        return DeserializeValue(val!);
    }

    public async Task<Dictionary<string, object?>> GetNodeAllOutputsAsync(Guid execId, Guid nodeId, CancellationToken ct = default)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var db = GetDatabase();
        if (db == null) return result;

        var pinKeys = await db.SetMembersAsync($"exec:{execId}:node:{nodeId}:pins");
        foreach (var pin in pinKeys)
        {
            var pinStr = pin.ToString();
            var val = await GetNodeOutputAsync(execId, nodeId, pinStr, ct);
            result[pinStr] = val;
        }

        return result;
    }

    public async Task SetNodeOutputsAsync(Guid execId, Guid nodeId, Dictionary<string, object?> outputs, CancellationToken ct = default)
    {
        foreach (var (k, v) in outputs)
        {
            await SetNodeOutputAsync(execId, nodeId, k, v, ct);
        }
    }

    public async Task SetNodeStatusAsync(Guid execId, Guid nodeId, string status, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return;

        var redisKey = NodeStatusKey(execId, nodeId);
        await db.StringSetAsync(redisKey, status, DefaultTtl);
    }

    public async Task<string?> GetNodeStatusAsync(Guid execId, Guid nodeId, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return null;

        var redisKey = NodeStatusKey(execId, nodeId);
        var val = await db.StringGetAsync(redisKey);
        return val.HasValue ? val.ToString() : null;
    }

    public async Task<PipelineExecutionState> GetFullStateAsync(Guid execId, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return new PipelineExecutionState();

        // 1. Check if snapshot state exists
        var stateJson = await db.StringGetAsync(FullStateKey(execId));
        if (stateJson.HasValue)
        {
            try
            {
                var doc = JsonDocument.Parse(stateJson.ToString());
                return PipelineExecutionState.FromJsonDocument(doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse state snapshot from Redis for execution {ExecutionId}", execId);
            }
        }

        // 2. Build state dynamically from individual Redis keys
        var state = new PipelineExecutionState();
        var startInputs = await GetAllStartInputsAsync(execId, ct);
        foreach (var (k, v) in startInputs)
        {
            state.RuntimeInputs[k] = v;
        }

        var nodeIds = await db.SetMembersAsync($"exec:{execId}:nodes");
        foreach (var nIdRedis in nodeIds)
        {
            if (Guid.TryParse(nIdRedis.ToString(), out var nGuid))
            {
                var nodeOutputs = await GetNodeAllOutputsAsync(execId, nGuid, ct);
                state.SetNodeOutputs(nGuid, nodeOutputs);
            }
        }

        return state;
    }

    public async Task SaveFullStateAsync(Guid execId, PipelineExecutionState state, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return;

        // Persist snapshot
        var json = JsonSerializer.Serialize(state);
        await db.StringSetAsync(FullStateKey(execId), json, DefaultTtl);

        // Also sync runtime inputs & node outputs
        foreach (var (k, v) in state.RuntimeInputs)
        {
            await SetStartInputAsync(execId, k, v, ct);
        }

        foreach (var (nIdStr, outputs) in state.NodeOutputs)
        {
            if (Guid.TryParse(nIdStr, out var nGuid))
            {
                foreach (var (pin, val) in outputs)
                {
                    await SetNodeOutputAsync(execId, nGuid, pin, val, ct);
                }
            }
        }
    }

    public async Task ExpireExecutionAsync(Guid execId, TimeSpan ttl, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return;

        await db.KeyExpireAsync(FullStateKey(execId), ttl);
        await db.KeyExpireAsync($"exec:{execId}:inputs", ttl);
        await db.KeyExpireAsync($"exec:{execId}:nodes", ttl);
    }

    private static object? DeserializeValue(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind switch
            {
                JsonValueKind.String => doc.RootElement.GetString(),
                JsonValueKind.Number => doc.RootElement.TryGetInt64(out var l) ? l : doc.RootElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => json
            };
        }
        catch
        {
            return json;
        }
    }
}
