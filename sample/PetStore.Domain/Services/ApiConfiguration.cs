namespace PetStore.Domain.Services;

/// <summary>
/// API configuration for the PetStore service.
/// Demonstrates instance registration using a static property.
/// </summary>
[Registration(As = typeof(IApiConfiguration), Instance = nameof(DefaultInstance))]
public class ApiConfiguration : IApiConfiguration
{
    private readonly Dictionary<string, string> configValues = new(StringComparer.Ordinal)
    {
        ["RateLimitPerMinute"] = "60",
        ["CacheDurationSeconds"] = "300",
        ["EnableLogging"] = "true",
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiConfiguration"/> class.
    /// </summary>
    private ApiConfiguration()
    {
    }

    /// <inheritdoc />
    public string ApiVersion { get; init; } = string.Empty;

    /// <inheritdoc />
    public int MaxPageSize { get; init; }

    /// <inheritdoc />
    public bool EnableApiDocumentation { get; init; }

    /// <inheritdoc />
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default instance of the API configuration.
    /// This instance is registered as a singleton via the Instance parameter.
    /// </summary>
    public static ApiConfiguration DefaultInstance { get; } = new()
    {
        ApiVersion = "v1",
        MaxPageSize = 100,
        EnableApiDocumentation = true,
        BaseUrl = "https://localhost:42616",
    };

    /// <inheritdoc />
    public string? GetConfigValue(string key)
        => configValues.TryGetValue(key, out var value) ? value : null;
}
