using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SimpleBlog.Application.Common;

namespace SimpleBlog.Infrastructure.Caching;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes is null || bytes.Length == 0)
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
    }

    public async Task<long> GetVersionAsync(string scope, CancellationToken ct = default)
    {
        var raw = await _cache.GetStringAsync(VersionKey(scope), ct);
        return long.TryParse(raw, out var value) ? value : 0;
    }

    public async Task BumpVersionAsync(string scope, CancellationToken ct = default)
    {
        var current = await GetVersionAsync(scope, ct);
        await _cache.SetStringAsync(VersionKey(scope), (current + 1).ToString(), ct);
    }

    private static string VersionKey(string scope) => $"cacheversion:{scope}";
}
