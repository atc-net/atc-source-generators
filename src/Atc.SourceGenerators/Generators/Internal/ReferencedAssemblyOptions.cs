namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The options of one referenced assembly.
/// </summary>
/// <param name="AssemblyName">The assembly name.</param>
/// <param name="Options">The options of the assembly.</param>
internal sealed record ReferencedAssemblyOptions(
    string AssemblyName,
    EquatableArray<OptionsInfo> Options);