namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Represents filtering rules for excluding types from registration.
/// </summary>
internal sealed record FilterRules(
    ImmutableArray<string> ExcludedNamespaces,
    ImmutableArray<string> ExcludedPatterns,
    ImmutableArray<ITypeSymbol> ExcludedInterfaces)
{
    /// <summary>
    /// Gets an empty filter rules instance with no exclusions.
    /// </summary>
    public static FilterRules Empty { get; } = new(
        ImmutableArray<string>.Empty,
        ImmutableArray<string>.Empty,
        ImmutableArray<ITypeSymbol>.Empty);

    /// <summary>
    /// Determines whether a type should be excluded based on the filter rules.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <returns>True if the type should be excluded; otherwise, false.</returns>
    public bool ShouldExclude(INamedTypeSymbol typeSymbol)
    {
        // Check namespace exclusion
        var typeNamespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        foreach (var excludedNs in ExcludedNamespaces)
        {
            // Exact match or sub-namespace match
            if (typeNamespace == excludedNs ||
                typeNamespace.StartsWith($"{excludedNs}.", StringComparison.Ordinal))
            {
                return true;
            }
        }

        // Check pattern exclusion (wildcard matching)
        var typeName = typeSymbol.Name;
        var fullTypeName = typeSymbol.ToDisplayString();
        foreach (var pattern in ExcludedPatterns)
        {
            if (MatchesPattern(typeName, pattern) || MatchesPattern(fullTypeName, pattern))
            {
                return true;
            }
        }

        // Check interface exclusion
        if (!ExcludedInterfaces.IsEmpty)
        {
            var implementedInterfaces = typeSymbol.AllInterfaces;
            foreach (var excludedInterface in ExcludedInterfaces)
            {
                foreach (var implementedInterface in implementedInterfaces)
                {
                    // Use SymbolEqualityComparer to properly compare generic types
                    if (SymbolEqualityComparer.Default.Equals(
                        implementedInterface.OriginalDefinition,
                        excludedInterface.OriginalDefinition))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Matches a string against a wildcard pattern.
    /// Supports * (any characters) and ? (single character).
    /// </summary>
    private static bool MatchesPattern(
        string value,
        string pattern)
    {
        // Convert wildcard pattern to regex
        var escapedPattern = System.Text.RegularExpressions.Regex.Escape(pattern);
        var replacedStars = escapedPattern.Replace("\\*", ".*");
        var replacedQuestions = replacedStars.Replace("\\?", ".");
        var regexPattern = $"^{replacedQuestions}$";

        return System.Text.RegularExpressions.Regex.IsMatch(
            value,
            regexPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(1));
    }
}