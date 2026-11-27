// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    private static (ImmutableArray<Diagnostic> Diagnostics, Dictionary<string, string> GeneratedSources) GetGeneratedOutput(
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = GeneratorTestHarness.References;

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new OptionsBindingGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out _);
        GeneratorTestHarness.AssertGeneratedCodeCompiles(compilation, outputCompilation);

        var runResult = driver.GetRunResult();

        var generatedSources = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var result in runResult.Results)
        {
            foreach (var generatedSource in result.GeneratedSources)
            {
                generatedSources[generatedSource.HintName] = generatedSource.SourceText.ToString();
            }
        }

        return (runResult.Diagnostics, generatedSources);
    }

    private static string? GetGeneratedExtensionMethod(
        Dictionary<string, string> output)
    {
        var extensionMethodFile = output.Keys.FirstOrDefault(k => k.StartsWith("OptionsBindingExtensions.", StringComparison.Ordinal));
        return extensionMethodFile != null ? output[extensionMethodFile] : null;
    }
}