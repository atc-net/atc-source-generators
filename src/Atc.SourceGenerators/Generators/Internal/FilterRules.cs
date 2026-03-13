namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Represents filtering rules for excluding types from registration.
/// </summary>
internal sealed class FilterRules
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Text.RegularExpressions.Regex> PatternCache = new(StringComparer.Ordinal);

    public ImmutableArray<string> ExcludedNamespaces { get; }

    public ImmutableArray<string> ExcludedPatterns { get; }

    public ImmutableArray<ITypeSymbol> ExcludedInterfaces { get; }

    private readonly HashSet<ISymbol> excludedInterfaceSet;

    public FilterRules(
        ImmutableArray<string> excludedNamespaces,
        ImmutableArray<string> excludedPatterns,
        ImmutableArray<ITypeSymbol> excludedInterfaces)
    {
        ExcludedNamespaces = excludedNamespaces;
        ExcludedPatterns = excludedPatterns;
        ExcludedInterfaces = excludedInterfaces;

        // Pre-build HashSet for O(1) interface lookups instead of O(n×m) nested loops
        excludedInterfaceSet = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var iface in excludedInterfaces)
        {
            excludedInterfaceSet.Add(iface.OriginalDefinition);
        }
    }

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
                (typeNamespace.StartsWith(excludedNs, StringComparison.Ordinal) &&
                 typeNamespace.Length > excludedNs.Length &&
                 typeNamespace[excludedNs.Length] == '.'))
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

        // Check interface exclusion using HashSet for O(1) lookup
        if (excludedInterfaceSet.Count > 0)
        {
            foreach (var implementedInterface in typeSymbol.AllInterfaces)
            {
                if (excludedInterfaceSet.Contains(implementedInterface.OriginalDefinition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Matches a string against a wildcard pattern.
    /// Supports * (any characters) and ? (single character).
    /// Uses a static cache to avoid recompiling regex patterns.
    /// </summary>
    private static bool MatchesPattern(
        string value,
        string pattern)
    {
        var regex = PatternCache.GetOrAdd(pattern, static p =>
        {
            var escapedPattern = System.Text.RegularExpressions.Regex.Escape(p);
            var replacedStars = escapedPattern.Replace("\\*", ".*");
            var replacedQuestions = replacedStars.Replace("\\?", ".");
            var regexPattern = $"^{replacedQuestions}$";

            return new System.Text.RegularExpressions.Regex(
                regexPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled,
                TimeSpan.FromSeconds(1));
        });

        return regex.IsMatch(value);
    }
}