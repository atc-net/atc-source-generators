namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record EnumMappingInfo(
    INamedTypeSymbol SourceEnum,
    INamedTypeSymbol TargetEnum,
    List<EnumMappingHelper.EnumValueMapping> ValueMappings,
    bool Bidirectional);