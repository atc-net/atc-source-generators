namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record PropertyMapping(
    IPropertySymbol SourceProperty,
    IPropertySymbol TargetProperty,
    bool RequiresConversion,
    bool IsNested,
    bool HasEnumMapping,
    bool IsCollection,
    ITypeSymbol? CollectionElementType,
    string? CollectionTargetType,
    bool IsFlattened,
    IPropertySymbol? FlattenedNestedProperty);