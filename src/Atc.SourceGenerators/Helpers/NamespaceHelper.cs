namespace Atc.SourceGenerators.Helpers;

/// <summary>
/// Provides methods for emitting namespaces in generated code, including types declared
/// in the global namespace (whose display string is "&lt;global namespace&gt;").
/// </summary>
internal static class NamespaceHelper
{
    /// <summary>
    /// Gets the namespace name to emit in a <c>using</c> or <c>namespace</c> declaration,
    /// or <see langword="null"/> when the namespace is the global namespace (or missing).
    /// </summary>
    /// <param name="namespaceSymbol">The namespace symbol.</param>
    /// <returns>The namespace name, or <see langword="null"/> for the global namespace.</returns>
    public static string? GetNamespaceName(INamespaceSymbol? namespaceSymbol)
        => namespaceSymbol is null || namespaceSymbol.IsGlobalNamespace
            ? null
            : namespaceSymbol.ToDisplayString();

    /// <summary>
    /// Gets the qualifier to put in front of a type name declared in the namespace:
    /// "My.Namespace." for a regular namespace, or an empty string for the global namespace.
    /// </summary>
    /// <param name="namespaceSymbol">The namespace symbol.</param>
    /// <returns>The qualifier including the trailing dot, or an empty string.</returns>
    public static string GetQualifier(INamespaceSymbol? namespaceSymbol)
        => GetNamespaceName(namespaceSymbol) is { } name
            ? name + "."
            : string.Empty;
}