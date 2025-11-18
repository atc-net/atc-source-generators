namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record MappingInfo(
    INamedTypeSymbol SourceType,
    INamedTypeSymbol TargetType,
    List<PropertyMapping> PropertyMappings,
    bool Bidirectional,
    bool EnableFlattening,
    IMethodSymbol? Constructor,
    List<string> ConstructorParameterNames,
    List<DerivedTypeMapping> DerivedTypeMappings,
    string? BeforeMap,
    string? AfterMap,
    string? Factory,
    bool UpdateTarget,
    bool GenerateProjection,
    bool IsGeneric,
    bool IncludePrivateMembers);