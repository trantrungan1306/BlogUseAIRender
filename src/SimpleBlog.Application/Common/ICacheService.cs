namespace SimpleBlog.Application.Common;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
    Task<long> GetVersionAsync(string scope, CancellationToken ct = default);
    Task BumpVersionAsync(string scope, CancellationToken ct = default);
}
