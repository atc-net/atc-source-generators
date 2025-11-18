namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Common cache interface for conditional registration example.
/// </summary>
public interface ICache
{
    /// <summary>
    /// Gets the cache provider name.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    string? Get(string key);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    void Set(
        string key,
        string value);

    /// <summary>
    /// Clears all cache entries.
    /// </summary>
    void Clear();
}
