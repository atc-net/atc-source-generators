// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [SuppressMessage("", "S1854:Remove this useless assignment to local variable 'driver'", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput(
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new DependencyRegistrationGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = outputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCDIR", StringComparison.Ordinal))
            .ToImmutableArray();

        var output = string.Join(
            "\n",
            outputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, output);
    }

    [SuppressMessage("", "S1854:Remove this useless assignment to local variable", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, string ReferencedOutput, string CurrentOutput) GetGeneratedOutputWithReferencedAssembly(
        string referencedSource,
        string referencedAssemblyName,
        string currentSource,
        string currentAssemblyName)
    {
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        // Step 1: Compile referenced assembly
        var referencedSyntaxTree = CSharpSyntaxTree.ParseText(referencedSource);
        var referencedCompilation = CSharpCompilation.Create(
            referencedAssemblyName,
            [referencedSyntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var referencedGenerator = new DependencyRegistrationGenerator();
        var referencedDriver = CSharpGeneratorDriver.Create(referencedGenerator);
        referencedDriver = (CSharpGeneratorDriver)referencedDriver.RunGeneratorsAndUpdateCompilation(
            referencedCompilation,
            out var referencedOutputCompilation,
            out _);

        // Get referenced assembly output
        var referencedOutput = string.Join(
            "\n",
            referencedOutputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        // Step 2: Compile current assembly with reference to the first
        var currentSyntaxTree = CSharpSyntaxTree.ParseText(currentSource);

        // Create an in-memory reference to the referenced compilation
        var referencedAssemblyReference = referencedOutputCompilation.ToMetadataReference();
        var currentReferences = references
            .Concat([referencedAssemblyReference])
            .ToList();

        var currentCompilation = CSharpCompilation.Create(
            currentAssemblyName,
            [currentSyntaxTree],
            currentReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var currentGenerator = new DependencyRegistrationGenerator();
        var currentDriver = CSharpGeneratorDriver.Create(currentGenerator);
        currentDriver = (CSharpGeneratorDriver)currentDriver.RunGeneratorsAndUpdateCompilation(
            currentCompilation,
            out var currentOutputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = currentOutputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCDIR", StringComparison.Ordinal))
            .ToImmutableArray();

        var currentOutput = string.Join(
            "\n",
            currentOutputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, referencedOutput, currentOutput);
    }

    [SuppressMessage("", "S1854:Remove this useless assignment to local variable", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, Dictionary<string, string> Outputs) GetGeneratedOutputWithMultipleReferencedAssemblies(
        List<(string Source, string AssemblyName)> assemblies)
    {
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilations = new Dictionary<string, Compilation>(StringComparer.Ordinal);
        var outputs = new Dictionary<string, string>(StringComparer.Ordinal);
        var allDiagnostics = new List<Diagnostic>();

        // Compile assemblies in order, adding each to references for the next
        foreach (var (source, assemblyName) in assemblies)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(
                assemblyName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new DependencyRegistrationGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var generatorDiagnostics);

            allDiagnostics.AddRange(
                outputCompilation
                    .GetDiagnostics()
                    .Concat(generatorDiagnostics)
                    .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                                d.Id.StartsWith("ATCDIR", StringComparison.Ordinal)));

            var output = string.Join(
                "\n",
                outputCompilation
                    .SyntaxTrees
                    .Skip(1)
                    .Select(tree => tree.ToString()));

            outputs[assemblyName] = output;
            compilations[assemblyName] = outputCompilation;

            // Add this compilation as a reference for subsequent compilations
            references.Add(outputCompilation.ToMetadataReference());
        }

        return ([..allDiagnostics], outputs);
    }
}