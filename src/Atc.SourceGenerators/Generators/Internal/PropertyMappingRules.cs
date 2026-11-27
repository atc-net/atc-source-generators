namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The generator-specific rules for <see cref="PropertyPairClassifier"/>.
/// </summary>
internal sealed class PropertyMappingRules
{
    /// <summary>
    /// Gets the user conversion method for a source and target type, or <c>null</c> (configuration mappings).
    /// </summary>
    public Func<ITypeSymbol, ITypeSymbol, string?> FindConversionMethod { get; init; } = static (_, _) => null;

    /// <summary>
    /// Gets whether a mapping exists between two different dictionary value types.
    /// </summary>
    public Func<ITypeSymbol, ITypeSymbol, bool> CanMapDictionaryValue { get; init; } = static (_, _) => false;

    /// <summary>
    /// Gets whether a dictionary whose values can't be mapped is checked as a collection or other conversion next
    /// (<c>true</c>), or reported as an element without mapping (<c>false</c>).
    /// </summary>
    public bool UnmappableDictionaryFallsThrough { get; init; }

    /// <summary>
    /// Gets whether a mapping exists between two collection element types.
    /// </summary>
    public Func<ITypeSymbol, ITypeSymbol, bool> CanMapCollectionElement { get; init; } = static (_, _) => true;

    /// <summary>
    /// Gets whether an enum mapping method exists (or is generated) between two enum types.
    /// </summary>
    public Func<ITypeSymbol, ITypeSymbol, bool> ResolveEnumMapping { get; init; } = MappingTypeAnalyzer.HasEnumMappingAttribute;

    /// <summary>
    /// Gets a value indicating whether numeric types are converted with a cast (int -> short, decimal -> double).
    /// </summary>
    public bool AllowNumericConversions { get; init; }
}