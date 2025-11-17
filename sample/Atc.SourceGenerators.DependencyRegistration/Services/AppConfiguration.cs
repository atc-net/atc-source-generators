namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Application configuration with a pre-created singleton instance.
/// Demonstrates instance registration using a static field.
/// </summary>
[Registration(As = typeof(IAppConfiguration), Instance = nameof(DefaultInstance))]
public class AppConfiguration : IAppConfiguration
{
    /// <summary>
    /// Gets the default instance of the application configuration.
    /// This instance is registered as a singleton via the Instance parameter.
    /// </summary>
    public static readonly AppConfiguration DefaultInstance = new()
    {
        ApplicationName = "Atc.SourceGenerators.DependencyRegistration",
        Environment = "Development",
        MaxConnections = 100,
        IsDebugMode = true,
    };

    private readonly Dictionary<string, string> configuration = new(StringComparer.Ordinal)
    {
        ["ConnectionString"] = "Server=localhost;Database=SampleDb",
        ["CacheTimeout"] = "300",
        ["EnableFeatureX"] = "true",
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AppConfiguration"/> class.
    /// </summary>
    private AppConfiguration()
    {
    }

    /// <inheritdoc />
    public string ApplicationName { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Environment { get; init; } = string.Empty;

    /// <inheritdoc />
    public int MaxConnections { get; init; }

    /// <inheritdoc />
    public bool IsDebugMode { get; init; }

    /// <inheritdoc />
    public string? GetValue(string key)
        => configuration.TryGetValue(key, out var value) ? value : null;
}
