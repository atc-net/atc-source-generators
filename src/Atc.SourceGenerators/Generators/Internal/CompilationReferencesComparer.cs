namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Compares compilations by assembly name and metadata references only. A pipeline step that reads referenced
/// assemblies (transitive registration) then re-runs when the references change, not on every edit.
/// </summary>
internal sealed class CompilationReferencesComparer : IEqualityComparer<Compilation>
{
    public static readonly CompilationReferencesComparer Instance = new();

    public bool Equals(
        Compilation? x,
        Compilation? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return string.Equals(x.AssemblyName, y.AssemblyName, StringComparison.Ordinal) &&
               x.References.SequenceEqual(y.References);
    }

    public int GetHashCode(Compilation obj)
        => obj.AssemblyName is null ? 0 : StringComparer.Ordinal.GetHashCode(obj.AssemblyName);
}