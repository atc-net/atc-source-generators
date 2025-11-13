// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
// ReSharper disable LoopCanBeConvertedToQuery
namespace Atc.SourceGenerators.Helpers;

/// <summary>
/// Utility for generating intelligent enum-to-enum mappings with special case handling.
/// </summary>
internal static class EnumMappingHelper
{
    /// <summary>
    /// Common special case mappings for enum values (case-insensitive).
    /// Limited to "zero/empty/null" state equivalents to avoid unexpected mappings.
    /// </summary>
    private static readonly Dictionary<string, string[]> SpecialCaseMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "None", ["Unknown", "Default"] },
        { "Unknown", ["None", "Default"] },
        { "Default", ["None", "Unknown"] },
    };

    /// <summary>
    /// Gets the mapping information for enum values.
    /// </summary>
    public static List<EnumValueMapping> GetEnumValueMappings(
        INamedTypeSymbol sourceEnum,
        INamedTypeSymbol targetEnum)
    {
        var mappings = new List<EnumValueMapping>();
        var sourceValues = GetEnumValues(sourceEnum);
        var targetValues = GetEnumValues(targetEnum);

        foreach (var sourceValue in sourceValues)
        {
            var targetValue = FindTargetValue(sourceValue, targetValues);
            mappings.Add(new EnumValueMapping(
                SourceValue: sourceValue,
                TargetValue: targetValue,
                IsMapped: targetValue is not null));
        }

        return mappings;
    }

    /// <summary>
    /// Generates a switch expression for enum mapping.
    /// </summary>
    public static void GenerateSwitchExpression(
        StringBuilder sb,
        string sourceParameter,
        INamedTypeSymbol sourceEnum,
        INamedTypeSymbol targetEnum,
        List<EnumValueMapping> mappings,
        string indentation = "        ")
    {
        sb.AppendLineLf($"{indentation}{sourceParameter} switch");
        sb.AppendLineLf($"{indentation}{{");

        foreach (var mapping in mappings)
        {
            if (mapping.IsMapped && mapping.TargetValue is not null)
            {
                sb.AppendLineLf($"{indentation}    {sourceEnum.ToDisplayString()}.{mapping.SourceValue} => {targetEnum.ToDisplayString()}.{mapping.TargetValue},");
            }
        }

        sb.AppendLineLf($"{indentation}    _ => throw new global::System.ArgumentOutOfRangeException(nameof({sourceParameter}), {sourceParameter}, \"Unmapped enum value\"),");
        sb.AppendLineLf($"{indentation}}}");
    }

    private static List<string> GetEnumValues(INamedTypeSymbol enumSymbol)
    {
        var values = new List<string>();

        foreach (var member in enumSymbol.GetMembers())
        {
            if (member is IFieldSymbol { IsConst: true, HasConstantValue: true })
            {
                values.Add(member.Name);
            }
        }

        return values;
    }

    private static string? FindTargetValue(
        string sourceValue,
        List<string> targetValues)
    {
        // 1. Try exact match (case-sensitive)
        if (targetValues.Contains(sourceValue, StringComparer.Ordinal))
        {
            return sourceValue;
        }

        // 2. Try case-insensitive match
        var caseInsensitiveMatch = targetValues.FirstOrDefault(t =>
            t.Equals(sourceValue, StringComparison.OrdinalIgnoreCase));
        if (caseInsensitiveMatch is not null)
        {
            return caseInsensitiveMatch;
        }

        // 3. Try special case mappings
        if (SpecialCaseMappings.TryGetValue(sourceValue, out var specialCases))
        {
            foreach (var specialCase in specialCases)
            {
                var specialMatch = targetValues.FirstOrDefault(t =>
                    t.Equals(specialCase, StringComparison.OrdinalIgnoreCase));
                if (specialMatch is not null)
                {
                    return specialMatch;
                }
            }
        }

        // 4. No match found
        return null;
    }

    public sealed record EnumValueMapping(
        string SourceValue,
        string? TargetValue,
        bool IsMapped);
}