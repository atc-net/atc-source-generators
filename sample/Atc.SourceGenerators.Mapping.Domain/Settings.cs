namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents application settings (demonstrates update target mapping for EF Core scenarios).
/// </summary>
[MapTo(typeof(SettingsDto), UpdateTarget = true)]
public partial class Settings
{
    /// <summary>
    /// Gets or sets the settings unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the application theme.
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// Gets or sets the notification preference.
    /// </summary>
    public bool EnableNotifications { get; set; } = true;

    /// <summary>
    /// Gets or sets the language preference.
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Gets or sets when the settings were last modified.
    /// </summary>
    public DateTimeOffset LastModified { get; set; }
}