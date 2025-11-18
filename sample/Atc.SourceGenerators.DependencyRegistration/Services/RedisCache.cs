namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Redis cache implementation - registered only when Features:UseRedisCache is true.
/// </summary>
[Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
public class RedisCache : ICache
{
    private readonly Dictionary<string, string> cache = new(StringComparer.Ordinal);

    public string ProviderName => "Redis Cache";

    public string? Get(string key)
    {
        cache.TryGetValue(key, out var value);
        Console.WriteLine($"  [Redis] Get('{key}') = {value ?? "null"}");
        return value;
    }

    public void Set(
        string key,
        string value)
    {
        cache[key] = value;
        Console.WriteLine($"  [Redis] Set('{key}', '{value}')");
    }

    public void Clear()
    {
        var count = cache.Count;
        cache.Clear();
        Console.WriteLine($"  [Redis] Cleared {count} entries");
    }
}
