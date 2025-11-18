namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Memory cache implementation - registered only when Features:UseRedisCache is false.
/// </summary>
[Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
public class MemoryCache : ICache
{
    private readonly Dictionary<string, string> cache = new(StringComparer.Ordinal);

    public string ProviderName => "Memory Cache";

    public string? Get(string key)
    {
        cache.TryGetValue(key, out var value);
        Console.WriteLine($"  [Memory] Get('{key}') = {value ?? "null"}");
        return value;
    }

    public void Set(
        string key,
        string value)
    {
        cache[key] = value;
        Console.WriteLine($"  [Memory] Set('{key}', '{value}')");
    }

    public void Clear()
    {
        var count = cache.Count;
        cache.Clear();
        Console.WriteLine($"  [Memory] Cleared {count} entries");
    }
}
