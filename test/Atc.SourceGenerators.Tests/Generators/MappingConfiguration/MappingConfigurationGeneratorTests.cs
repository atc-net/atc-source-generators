// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [SuppressMessage("", "S1854:Remove this useless assignment to local variable 'driver'", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput(
        string source,
        string? diagnosticPrefix = null)
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

        // Run MappingConfigurationGenerator, ObjectMappingGenerator, and EnumMappingGenerator
        var mappingConfigGenerator = new MappingConfigurationGenerator();
        var objectMappingGenerator = new ObjectMappingGenerator();
        var enumMappingGenerator = new EnumMappingGenerator();
        var driver = CSharpGeneratorDriver.Create(mappingConfigGenerator, objectMappingGenerator, enumMappingGenerator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var prefix = diagnosticPrefix ?? "ATCM";
        var allDiagnostics = outputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Info &&
                        d.Id.StartsWith(prefix, StringComparison.Ordinal))
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