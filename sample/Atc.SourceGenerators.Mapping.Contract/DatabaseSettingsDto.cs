namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Represents database settings in snake_case format (typical for PostgreSQL/Python APIs).
/// </summary>
public class DatabaseSettingsDto
{
    /// <summary>
    /// Gets or sets the database host.
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable SA1300 // Element should begin with an uppercase letter
    public string database_host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database port.
    /// </summary>
    public int database_port { get; set; }

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string database_name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int connection_timeout { get; set; }

    /// <summary>
    /// Gets or sets whether SSL is enabled.
    /// </summary>
    public bool enable_ssl { get; set; }
#pragma warning restore SA1300 // Element should begin with an uppercase letter
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore IDE1006 // Naming Styles
}