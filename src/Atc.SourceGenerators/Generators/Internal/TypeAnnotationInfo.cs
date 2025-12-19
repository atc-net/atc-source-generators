namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Represents a class/record with annotated properties.
/// </summary>
internal sealed record TypeAnnotationInfo(
    string TypeName,
    string Namespace,
    string AssemblyName,
    ImmutableArray<PropertyAnnotationInfo> Properties);