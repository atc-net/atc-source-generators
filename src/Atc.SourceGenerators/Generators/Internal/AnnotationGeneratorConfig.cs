namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Configuration options for the AnnotationConstantsGenerator.
/// Must implement proper equality for incremental generation caching.
/// </summary>
internal readonly record struct AnnotationGeneratorConfig(
    bool IncludeUnannotatedProperties);