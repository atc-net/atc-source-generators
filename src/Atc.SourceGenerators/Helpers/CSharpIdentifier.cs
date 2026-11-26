namespace Atc.SourceGenerators.Helpers;

/// <summary>
/// Formats names as C# identifiers for generated code.
/// </summary>
internal static class CSharpIdentifier
{
    /// <summary>
    /// Escapes a name that is a reserved C# keyword with '@' (e.g. "class" becomes "@class"),
    /// so it can be used as an identifier in generated code.
    /// </summary>
    /// <param name="name">The name, as reported by Roslyn (without '@').</param>
    /// <returns>The name, prefixed with '@' when it is a reserved keyword.</returns>
    public static string Escape(string name)
        => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
            ? "@" + name
            : name;
}