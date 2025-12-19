namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Represents extracted annotation data for a single property.
/// </summary>
internal sealed record PropertyAnnotationInfo(
    string PropertyName,
    DisplayAnnotationInfo? Display,
    ValidationAnnotationInfo? MicrosoftValidation,
    AtcAnnotationInfo? AtcAnnotation);