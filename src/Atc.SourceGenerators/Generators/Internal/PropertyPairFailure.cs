namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Why a source and target property with matching names can't be mapped.
/// </summary>
internal enum PropertyPairFailure
{
    None,

    /// <summary>The collection elements or dictionary values have no mapping.</summary>
    ElementHasNoMapping,

    /// <summary>No conversion exists between the property types.</summary>
    IncompatibleTypes,
}