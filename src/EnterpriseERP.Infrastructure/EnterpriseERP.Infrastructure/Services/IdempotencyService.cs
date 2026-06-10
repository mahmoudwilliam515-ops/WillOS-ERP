using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace EnterpriseERP.Infrastructure.Services;

/// <summary>
/// Phase 9 §9.1.4 — Redis-backed idempotency store.
/// Keys are stored with a 24-hour TTL per the spec.
/// </summary>
public class IdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    public IdempotencyService(IDistributedCache cache) => _cache = cache;

    public async Task<string?> GetCachedResponseAsync(string key, CancellationToken ct = default)
        => await _cache.GetStringAsync(IdempotencyKey(key), ct);

    public async Task StoreResponseAsync(string key, string responseJson, CancellationToken ct = default)
        => await _cache.SetStringAsync(
            IdempotencyKey(key),
            responseJson,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl },
            ct);

    private static string IdempotencyKey(string key) => $"idempotency:{key}";
}
