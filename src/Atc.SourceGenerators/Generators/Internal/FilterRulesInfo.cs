namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The [RegistrationFilter] rules of the current assembly as equatable data.
/// </summary>
/// <param name="ExcludedNamespaces">Namespaces (and their sub-namespaces) to exclude.</param>
/// <param name="ExcludedPatterns">Wildcard patterns for the type name or full type name.</param>
/// <param name="ExcludedInterfaces">The original definitions of excluded interfaces (display names).</param>
internal sealed record FilterRulesInfo(
    EquatableArray<string> ExcludedNamespaces,
    EquatableArray<string> ExcludedPatterns,
    EquatableArray<string> ExcludedInterfaces)
{
    public bool ShouldExclude(ServiceRegistrationInfo service)
    {
        foreach (var excludedNs in ExcludedNamespaces)
        {
            // Exact match or sub-namespace match
            if (service.ClassNamespace == excludedNs ||
                (service.ClassNamespace.StartsWith(excludedNs, StringComparison.Ordinal) &&
                 service.ClassNamespace.Length > excludedNs.Length &&
                 service.ClassNamespace[excludedNs.Length] == '.'))
            {
                return true;
            }
        }

        foreach (var pattern in ExcludedPatterns)
        {
            if (FilterRules.MatchesPattern(service.ClassName, pattern) ||
                FilterRules.MatchesPattern(service.ClassFullName, pattern))
            {
                return true;
            }
        }

        return ExcludedInterfaces.Count > 0 &&
               service.InterfaceDefinitionNames.Any(i => ExcludedInterfaces.Contains(i, StringComparer.Ordinal));
    }
}