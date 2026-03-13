namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record ConfiguredMappingInfo(
    INamedTypeSymbol SourceType,
    INamedTypeSymbol TargetType,
    IMethodSymbol? Method,
    INamedTypeSymbol? ContainingClass,
    List<PropertyMapping> PropertyMappings,
    bool Bidirectional,
    bool EnableFlattening,
    IMethodSymbol? Constructor,
    List<string> ConstructorParameterNames,
    List<ConstructorDefaultParameter> ConstructorDefaultParameters,
    PropertyNameStrategy PropertyNameStrategy,
    List<string> IgnoredProperties,
    List<(string Source, string Target)> PropertyRenames);