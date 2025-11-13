namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Logging configuration options.
/// Explicitly binds to "Logging" section in appsettings.json.
/// </summary>
[OptionsBinding("Logging")]
public partial class LoggingOptions
{
    public string Level { get; set; } = "Information";

    public bool EnableConsole { get; set; } = true;

    public bool EnableFile { get; set; }

    public string? FilePath { get; set; }
}