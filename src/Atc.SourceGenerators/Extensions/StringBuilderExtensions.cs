// ReSharper disable once CheckNamespace
namespace Atc.SourceGenerators;

/// <summary>
/// Extension methods for StringBuilder to ensure consistent line endings across platforms.
/// </summary>
internal static class StringBuilderExtensions
{
    /// <summary>
    /// Appends a string followed by a Unix-style line feed (\n) to ensure consistent
    /// line endings in generated code across all platforms (Windows, macOS, Linux).
    /// </summary>
    /// <param name="builder">The StringBuilder instance.</param>
    /// <param name="value">The string value to append. If null or empty, only the line feed is appended.</param>
    /// <returns>The same StringBuilder instance for method chaining.</returns>
    public static StringBuilder AppendLineLf(
        this StringBuilder builder,
        string? value = null)
    {
        if (!string.IsNullOrEmpty(value))
        {
            builder.Append(value);
        }

        builder.Append(Constants.LineFeed);
        return builder;
    }
}