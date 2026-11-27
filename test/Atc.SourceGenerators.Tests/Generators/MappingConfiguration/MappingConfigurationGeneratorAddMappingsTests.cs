namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void AddMappings_Should_Not_Be_Ambiguous_When_A_Referenced_Project_Also_Generates_It()
    {
        // Two projects that both reference dependency injection each get the generated AddMappings extension.
        // The referencing project must still be able to call services.AddMappings(...) (no CS0121).
        const string librarySource = """
                                     namespace Library;

                                     public class LibraryService
                                     {
                                     }
                                     """;

        const string appSource = """
                                 using Microsoft.Extensions.DependencyInjection;

                                 namespace App;

                                 public class Source
                                 {
                                     public int Id { get; set; }
                                 }

                                 public class Target
                                 {
                                     public int Id { get; set; }
                                 }

                                 public static class Startup
                                 {
                                     public static void Configure(IServiceCollection services)
                                         => services.AddMappings(map => map.Map<Source, Target>());
                                 }
                                 """;

        var libraryCompilation = GeneratorTestHarness.CreateCompilation(librarySource, "Library");
        CSharpGeneratorDriver
            .Create(new MappingConfigurationGenerator(), new ObjectMappingGenerator(), new EnumMappingGenerator())
            .RunGeneratorsAndUpdateCompilation(libraryCompilation, out var libraryOutput, out _, TestContext.Current.CancellationToken);

        using var libraryImage = new MemoryStream();
        var emitResult = libraryOutput.Emit(libraryImage, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

        var appCompilation = GeneratorTestHarness.CreateCompilation(
            appSource,
            "App",
            [MetadataReference.CreateFromImage(libraryImage.ToArray())]);
        CSharpGeneratorDriver
            .Create(new MappingConfigurationGenerator(), new ObjectMappingGenerator(), new EnumMappingGenerator())
            .RunGeneratorsAndUpdateCompilation(appCompilation, out var appOutput, out _, TestContext.Current.CancellationToken);

        var errors = appOutput
            .GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }
}