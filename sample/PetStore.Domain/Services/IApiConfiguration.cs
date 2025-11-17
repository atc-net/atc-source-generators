namespace PetStore.Domain.Services;

/// <summary>
/// Represents API configuration for the PetStore service.
/// </summary>
public interface IApiConfiguration
{
    /// <summary>
    /// Gets the API version.
    /// </summary>
    string ApiVersion { get; }

    /// <summary>
    /// Gets the maximum page size for list operations.
    /// </summary>
    int MaxPageSize { get; }

    /// <summary>
    /// Gets a value indicating whether API documentation is enabled.
    /// </summary>
    bool EnableApiDocumentation { get; }

    /// <summary>
    /// Gets the API endpoint base URL.
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value or null if not found.</returns>
    string? GetConfigValue(string key);
}
