namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record ReferencedAssemblyInfo(
    string AssemblyName,
    string SanitizedName,
    string ShortName);