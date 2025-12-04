// ReSharper disable CommentTypo
namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Logging configuration options.
/// Explicitly binds to "Logging" section in appsettings.json.
/// Demonstrates configuration change callbacks with Monitor lifetime.
/// </summary>
[OptionsBinding("Logging", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnLoggingChanged))]
public partial class LoggingOptions
{
    public string Level { get; set; } = "Information";

    public bool EnableConsole { get; set; } = true;

    public bool EnableFile { get; set; }

    public string? FilePath { get; set; }

    /// <summary>
    /// Called automatically when the Logging configuration section changes.
    /// Requires appsettings.json to have reloadOnChange: true.
    /// </summary>
    internal static void OnLoggingChanged(
        LoggingOptions options,
        string? name)
    {
        Console.WriteLine("[OnChange Callback] Logging configuration changed:");
        Console.WriteLine($"  Level: {options.Level}");
        Console.WriteLine($"  EnableConsole: {options.EnableConsole}");
        Console.WriteLine($"  EnableFile: {options.EnableFile}");
        Console.WriteLine($"  FilePath: {options.FilePath ?? "(not set)"}");
        Console.WriteLine();
    }
}