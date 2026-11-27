namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The result of classifying a source and target property pair: the mapping, or why there is none.
/// </summary>
internal sealed record PropertyPairResult(
    PropertyMapping? Mapping,
    PropertyPairFailure Failure,
    ITypeSymbol? SourceElementType = null,
    ITypeSymbol? TargetElementType = null);