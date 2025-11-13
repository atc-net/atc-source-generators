namespace Atc.SourceGenerators.OptionsBinding.Domain.Options;

/// <summary>
/// Cache configuration options.
/// Demonstrates auto-inference (uses full class name "CacheOptions").
/// </summary>
[OptionsBinding(ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class CacheOptions
{
    [Range(1, 10000)]
    public int MaxSize { get; set; } = 1000;

    [Range(1, 86400)]
    public int DefaultExpirationSeconds { get; set; } = 3600;

    public bool EnableDistributedCache { get; set; }

    public string? RedisConnectionString { get; set; }
}