// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
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

        // Run both ObjectMappingGenerator and EnumMappingGenerator
        // (EnumMappingGenerator generates the MapToXxx extension methods for enums)
        var objectMappingGenerator = new ObjectMappingGenerator();
        var enumMappingGenerator = new EnumMappingGenerator();
        var driver = CSharpGeneratorDriver.Create(objectMappingGenerator, enumMappingGenerator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = outputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCMAP", StringComparison.Ordinal))
            .ToImmutableArray();

        var output = string.Join(
            Constants.LineFeed,
            outputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, output);
    }

    private static int CountOccurrences(
        string text,
        string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }
}