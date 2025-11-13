namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Singleton cache service - single instance for the entire application.
/// </summary>
[Registration]
public class CacheService
{
    private readonly Dictionary<string, object> cache = new(StringComparer.Ordinal);

    public void Set(
        string key,
        object value)
    {
        cache[key] = value;
        Console.WriteLine($"Cache set: {key}");
    }

    public object? Get(string key)
        => cache.GetValueOrDefault(key);
}