namespace Atc.SourceGenerators.Tests.Generators;

/// <summary>
/// Shared infrastructure for generator tests: a fixed set of metadata references and a run that
/// compiles the generated code, so tests can't pass while the generator emits code that doesn't compile.
/// </summary>
internal static class GeneratorTestHarness
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> LazyReferences = new(CreateReferences);

    /// <summary>
    /// Gets the metadata references for test compilations: the platform and application assemblies of the
    /// test host (fixed per build), so results don't depend on which assemblies happen to be loaded already.
    /// </summary>
    public static ImmutableArray<MetadataReference> References
        => LazyReferences.Value;

    /// <summary>
    /// Creates a compilation of the source with the shared references.
    /// </summary>
    public static CSharpCompilation CreateCompilation(
        string source,
        string assemblyName = "TestAssembly",
        IEnumerable<MetadataReference>? additionalReferences = null)
        => CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            additionalReferences is null ? References : References.AddRange(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    /// <summary>
    /// Gets the compiler errors located in generated files (not in the test's own source, which may be
    /// intentionally invalid to test generator diagnostics).
    /// </summary>
    public static ImmutableArray<Diagnostic> GetGeneratedCodeErrors(
        Compilation inputCompilation,
        Compilation outputCompilation)
    {
        var inputTrees = new HashSet<SyntaxTree>(inputCompilation.SyntaxTrees);
        return outputCompilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error &&
                        d.Location.SourceTree is { } tree &&
                        !inputTrees.Contains(tree))
            .ToImmutableArray();
    }

    /// <summary>
    /// Asserts that the generated code compiles.
    /// </summary>
    public static void AssertGeneratedCodeCompiles(
        Compilation inputCompilation,
        Compilation outputCompilation)
    {
        var errors = GetGeneratedCodeErrors(inputCompilation, outputCompilation);
        var message = "The generated code does not compile:" + Environment.NewLine +
                      string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
        Assert.True(errors.IsEmpty, message);
    }

    private static ImmutableArray<MetadataReference> CreateReferences()
    {
        var trustedAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        return trustedAssemblies
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Cast<MetadataReference>()
            .ToImmutableArray();
    }
}