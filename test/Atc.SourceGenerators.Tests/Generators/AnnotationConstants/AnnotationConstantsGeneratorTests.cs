// ReSharper disable RedundantAssignment
namespace Atc.SourceGenerators.Tests.Generators.AnnotationConstants;

public partial class AnnotationConstantsGeneratorTests
{
    [SuppressMessage("", "S1854:Remove this useless assignment to local variable 'driver'", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput(
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Get references from loaded assemblies
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        // Explicitly add System.ComponentModel.DataAnnotations reference
        var dataAnnotationsAssembly = typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly;
        if (!string.IsNullOrWhiteSpace(dataAnnotationsAssembly.Location))
        {
            references.Add(MetadataReference.CreateFromFile(dataAnnotationsAssembly.Location));
        }

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AnnotationConstantsGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = outputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCANN", StringComparison.Ordinal))
            .ToImmutableArray();

        var output = string.Join(
            Constants.LineFeed,
            outputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, output);
    }
}