namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Represents application configuration settings.
/// </summary>
public interface IAppConfiguration
{
    /// <summary>
    /// Gets the application name.
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// Gets the environment name (Development, Staging, Production).
    /// </summary>
    string Environment { get; }

    /// <summary>
    /// Gets the maximum allowed connections.
    /// </summary>
    int MaxConnections { get; }

    /// <summary>
    /// Gets a value indicating whether debug mode is enabled.
    /// </summary>
    bool IsDebugMode { get; }

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value or null if not found.</returns>
    string? GetValue(string key);
}
