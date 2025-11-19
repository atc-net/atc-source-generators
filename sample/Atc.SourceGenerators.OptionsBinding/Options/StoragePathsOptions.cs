namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Storage paths configuration options.
/// Demonstrates Feature #7: Post-Configuration support for normalizing values after binding.
/// The PostConfigure callback ensures all paths end with a directory separator,
/// providing consistent path handling across the application.
/// </summary>
[OptionsBinding("StoragePaths", ValidateDataAnnotations = true, PostConfigure = nameof(NormalizePaths))]
public partial class StoragePathsOptions
{
    [Required]
    public string BasePath { get; set; } = string.Empty;

    [Required]
    public string CachePath { get; set; } = string.Empty;

    [Required]
    public string TempPath { get; set; } = string.Empty;

    public string LogPath { get; set; } = string.Empty;

    /// <summary>
    /// Post-configuration method to normalize all path values.
    /// This ensures all paths end with a directory separator for consistent usage.
    /// Signature: static void MethodName(TOptions options)
    /// </summary>
    internal static void NormalizePaths(StoragePathsOptions options)
    {
        // Normalize all paths to ensure they end with directory separator
        options.BasePath = EnsureTrailingDirectorySeparator(options.BasePath);
        options.CachePath = EnsureTrailingDirectorySeparator(options.CachePath);
        options.TempPath = EnsureTrailingDirectorySeparator(options.TempPath);
        options.LogPath = EnsureTrailingDirectorySeparator(options.LogPath);
    }

    /// <summary>
    /// Ensures a path ends with a directory separator character.
    /// </summary>
    private static string EnsureTrailingDirectorySeparator(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return path.EndsWith(Path.DirectorySeparatorChar) ||
               path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}