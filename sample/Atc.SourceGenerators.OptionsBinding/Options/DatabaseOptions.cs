// ReSharper disable CommentTypo
namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Database configuration options with validation.
/// Explicitly binds to "Database" section in appsettings.json.
/// Demonstrates ErrorOnMissingKeys to fail fast if configuration is missing.
/// </summary>
[OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true, ErrorOnMissingKeys = true, Validator = typeof(Validators.DatabaseOptionsValidator))]
public partial class DatabaseOptions
{
    [Required]
    [MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    public int TimeoutSeconds { get; set; } = 30;
}