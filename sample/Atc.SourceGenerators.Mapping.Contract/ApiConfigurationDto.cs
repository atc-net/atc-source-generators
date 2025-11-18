namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Represents API configuration in JSON format (camelCase for JavaScript compatibility).
/// </summary>
public class ApiConfigurationDto
{
    /// <summary>
    /// Gets or sets the API endpoint URL.
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element should begin with an uppercase letter
    public string apiEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    public string apiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds.
    /// </summary>
    public int timeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets whether retry logic is enabled.
    /// </summary>
    public bool enableRetry { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int maxRetryAttempts { get; set; }
#pragma warning restore SA1300 // Element should begin with an uppercase letter
#pragma warning restore IDE1006 // Naming Styles
}

