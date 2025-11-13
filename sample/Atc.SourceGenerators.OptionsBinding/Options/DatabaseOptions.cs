namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Database configuration options with validation.
/// Explicitly binds to "Database" section in appsettings.json.
/// </summary>
[OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class DatabaseOptions
{
    [Required]
    [MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    public int TimeoutSeconds { get; set; } = 30;
}