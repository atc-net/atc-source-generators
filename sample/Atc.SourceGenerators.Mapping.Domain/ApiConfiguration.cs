namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents API configuration settings (demonstrates PropertyNameStrategy with camelCase).
/// </summary>
[MapTo(typeof(ApiConfigurationDto), PropertyNameStrategy = PropertyNameStrategy.CamelCase)]
public partial class ApiConfiguration
{
    /// <summary>
    /// Gets or sets the API endpoint URL.
    /// </summary>
    public string ApiEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets whether retry logic is enabled.
    /// </summary>
    public bool EnableRetry { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; }
}

