namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents database settings (demonstrates PropertyNameStrategy with snake_case).
/// </summary>
[MapTo(typeof(DatabaseSettingsDto), PropertyNameStrategy = PropertyNameStrategy.SnakeCase)]
public partial class DatabaseSettings
{
    /// <summary>
    /// Gets or sets the database host.
    /// </summary>
    public string DatabaseHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database port.
    /// </summary>
    public int DatabasePort { get; set; }

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeout { get; set; }

    /// <summary>
    /// Gets or sets whether SSL is enabled.
    /// </summary>
    public bool EnableSsl { get; set; }
}

