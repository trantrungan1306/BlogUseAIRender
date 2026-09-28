using System.Collections.Concurrent;
using SimpleBlog.Application.Common;

namespace SimpleBlog.Tests.TestDoubles;

// In-memory cache stand-in so PostService can run without Redis in tests.
public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object?> _store = new();
    private readonly ConcurrentDictionary<string, long> _versions = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_store.TryGetValue(key, out var value) && value is T typed)
            return Task.FromResult<T?>(typed);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task<long> GetVersionAsync(string scope, CancellationToken ct = default)
        => Task.FromResult(_versions.GetValueOrDefault(scope));

    public Task BumpVersionAsync(string scope, CancellationToken ct = default)
    {
        _versions.AddOrUpdate(scope, 1, (_, current) => current + 1);
        return Task.CompletedTask;
    }
}
