namespace Atc.SourceGenerators.Helpers;

/// <summary>
/// Provides methods for sanitizing type names for use in generated C# identifiers.
/// </summary>
internal static class TypeNameSanitizer
{
    /// <summary>
    /// Sanitizes a type name so it can be used as part of a valid C# identifier.
    /// Strips generic arity markers (backtick + digits, e.g., "Nullable`1" → "Nullable")
    /// and removes any other characters that are not valid in C# identifiers.
    /// </summary>
    /// <param name="typeName">The type name to sanitize (typically from ITypeSymbol.Name).</param>
    /// <returns>A sanitized string safe for use in method names and other C# identifiers.</returns>
    public static string SanitizeForIdentifier(string typeName)
    {
        // Strip generic arity marker (e.g., "Nullable`1" → "Nullable")
        var backtickIndex = typeName.IndexOf('`');
        if (backtickIndex >= 0)
        {
            typeName = typeName.Substring(0, backtickIndex);
        }

        // Remove any remaining characters that are not valid in C# identifiers
        if (typeName.Length == 0)
        {
            return typeName;
        }

        var sb = new System.Text.StringBuilder(typeName.Length);
        foreach (var ch in typeName)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }
}