namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record MappingInfo(
    INamedTypeSymbol SourceType,
    INamedTypeSymbol TargetType,
    List<PropertyMapping> PropertyMappings,
    bool Bidirectional);