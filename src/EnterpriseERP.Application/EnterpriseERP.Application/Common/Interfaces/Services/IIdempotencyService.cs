namespace EnterpriseERP.Application.Common.Interfaces.Services;

/// <summary>
/// Phase 9 §9.1.4 — Idempotency-Key enforcement.
/// Checks Redis for a cached response matching the given key.
/// If found, returns the original response; otherwise stores the new one.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Returns the cached JSON payload for <paramref name="key"/>, or null if not cached.
    /// </summary>
    Task<string?> GetCachedResponseAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Stores <paramref name="responseJson"/> under <paramref name="key"/> for 24 hours.
    /// </summary>
    Task StoreResponseAsync(string key, string responseJson, CancellationToken ct = default);
}
