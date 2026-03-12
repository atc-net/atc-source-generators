namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record AutoDetectedEnumMapping(
    INamedTypeSymbol SourceEnum,
    INamedTypeSymbol TargetEnum,
    List<EnumMappingHelper.EnumValueMapping> ValueMappings);