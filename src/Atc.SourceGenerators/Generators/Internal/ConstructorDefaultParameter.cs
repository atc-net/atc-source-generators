namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Represents a constructor parameter that has no matching source property
/// and should be filled with a default value.
/// </summary>
internal sealed record ConstructorDefaultParameter(
    string Name,
    ITypeSymbol Type,
    string DefaultExpression);