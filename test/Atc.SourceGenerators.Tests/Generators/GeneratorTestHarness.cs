namespace Atc.SourceGenerators.Tests.Generators;

/// <summary>
/// Shared infrastructure for generator tests: a fixed set of metadata references and a run that
/// compiles the generated code, so tests can't pass while the generator emits code that doesn't compile.
/// </summary>
internal static class GeneratorTestHarness
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> LazyReferences = new(CreateReferences);
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> DumpRunCounts = new(StringComparer.Ordinal);

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

    /// <summary>
    /// When the <c>ATC_SG_DUMP_DIR</c> environment variable is set, writes the generated files and the generator
    /// diagnostics of this run to <c>{dir}/{test name}/</c>. Dumps from two commits can be compared with
    /// <c>diff -r</c> to check that a refactoring doesn't change any generated output.
    /// </summary>
    public static void DumpIfRequested(
        Compilation inputCompilation,
        Compilation outputCompilation,
        ImmutableArray<Diagnostic> generatorDiagnostics)
    {
        var root = Environment.GetEnvironmentVariable("ATC_SG_DUMP_DIR");
        if (string.IsNullOrEmpty(root))
        {
            return;
        }

        var testName = TestContext.Current.Test?.TestDisplayName ?? "unknown";
        var runIndex = DumpRunCounts.AddOrUpdate(testName, 0, (_, count) => count + 1);
        var directory = Path.Combine(root, ToDirectoryName(testName), runIndex.ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(directory);

        var inputTrees = new HashSet<SyntaxTree>(inputCompilation.SyntaxTrees);
        foreach (var tree in outputCompilation.SyntaxTrees.Where(t => !inputTrees.Contains(t)))
        {
            File.WriteAllText(Path.Combine(directory, Path.GetFileName(tree.FilePath)), tree.ToString());
        }

        var diagnostics = generatorDiagnostics
            .Select(d => $"{d.Id} {d.Severity} {d.Location.GetLineSpan().Span}: {d.GetMessage(CultureInfo.InvariantCulture)}")
            .OrderBy(line => line, StringComparer.Ordinal);
        File.WriteAllLines(Path.Combine(directory, "_diagnostics.txt"), diagnostics);
    }

    private static string ToDirectoryName(string testName)
    {
        var name = new string(testName.Select(c => char.IsLetterOrDigit(c) || c is '_' or '.' ? c : '_').ToArray());
        if (name.Length <= 120)
        {
            return name;
        }

        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(testName)));
        return name[..100] + "_" + hash[..16];
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