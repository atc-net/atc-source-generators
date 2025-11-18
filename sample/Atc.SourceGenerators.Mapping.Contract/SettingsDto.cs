namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Represents application settings in the API contract.
/// </summary>
public class SettingsDto
{
    /// <summary>
    /// Gets or sets the settings unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the application theme.
    /// </summary>
    public string Theme { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification preference.
    /// </summary>
    public bool EnableNotifications { get; set; }

    /// <summary>
    /// Gets or sets the language preference.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the settings were last modified.
    /// </summary>
    public DateTimeOffset LastModified { get; set; }
}