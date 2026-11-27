namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The referenced assemblies with [OptionsBinding] classes and their options, as equatable data.
/// </summary>
/// <param name="Assemblies">The referenced assemblies with options.</param>
/// <param name="Options">The options of each referenced assembly, by assembly name.</param>
internal sealed record ReferencedOptionsInfo(
    EquatableArray<ReferencedAssemblyInfo> Assemblies,
    EquatableArray<ReferencedAssemblyOptions> Options);