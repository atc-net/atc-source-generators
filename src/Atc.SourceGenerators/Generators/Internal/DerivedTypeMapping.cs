namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record DerivedTypeMapping(
    INamedTypeSymbol SourceDerivedType,
    INamedTypeSymbol TargetDerivedType);