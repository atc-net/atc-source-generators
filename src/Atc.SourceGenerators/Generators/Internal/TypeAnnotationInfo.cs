namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Represents a class/record with annotated properties.
/// </summary>
/// <param name="TypeName">The simple type name.</param>
/// <param name="Namespace">The containing namespace.</param>
/// <param name="AssemblyName">The containing assembly name.</param>
/// <param name="Properties">The annotated properties.</param>
/// <param name="FullName">The fully qualified type name, unique per type (used to de-duplicate partial declarations).</param>
/// <param name="ContainingTypeNames">The names of the containing types, outermost first (empty for a top-level type).</param>
internal sealed record TypeAnnotationInfo(
    string TypeName,
    string Namespace,
    string AssemblyName,
    EquatableArray<PropertyAnnotationInfo> Properties,
    string FullName,
    EquatableArray<string> ContainingTypeNames);