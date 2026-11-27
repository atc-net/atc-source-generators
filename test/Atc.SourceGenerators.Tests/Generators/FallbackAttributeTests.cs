namespace Atc.SourceGenerators.Tests.Generators;

/// <summary>
/// The generators add their attributes to every project as internal, [Embedded] types, so a project that
/// references another project using the generators doesn't see a second copy (no CS0436 to suppress).
/// </summary>
public class FallbackAttributeTests
{
    private static IIncrementalGenerator[] AllGenerators
        => [
            new DependencyRegistrationGenerator(),
            new OptionsBindingGenerator(),
            new ObjectMappingGenerator(),
            new EnumMappingGenerator(),
            new MappingConfigurationGenerator(),
        ];

    [Fact]
    public void Referencing_Project_Should_Not_Report_CS0436_For_The_Generated_Attributes()
    {
        const string librarySource = """
                                     using Atc.DependencyInjection;
                                     using Atc.SourceGenerators.Annotations;

                                     namespace Library;

                                     public interface ILibraryService
                                     {
                                     }

                                     [Registration]
                                     public class LibraryService : ILibraryService
                                     {
                                     }

                                     public class ItemDto
                                     {
                                         public int Id { get; set; }
                                     }

                                     [MapTo(typeof(ItemDto))]
                                     public partial class Item
                                     {
                                         public int Id { get; set; }
                                     }
                                     """;

        const string appSource = """
                                 using Atc.DependencyInjection;
                                 using Atc.SourceGenerators.Annotations;

                                 namespace App;

                                 public interface IAppService
                                 {
                                 }

                                 [Registration(Lifetime.Scoped)]
                                 public class AppService : IAppService
                                 {
                                 }

                                 public class OrderDto
                                 {
                                     public int Id { get; set; }
                                 }

                                 [MapTo(typeof(OrderDto), PropertyNameStrategy = PropertyNameStrategy.PascalCase)]
                                 public partial class Order
                                 {
                                     public int Id { get; set; }
                                 }

                                 [OptionsBinding("App", Lifetime = OptionsLifetime.Monitor)]
                                 public partial class AppOptions
                                 {
                                     public string Name { get; set; } = string.Empty;
                                 }
                                 """;

        var libraryCompilation = GeneratorTestHarness.CreateCompilation(librarySource, "Library");
        CSharpGeneratorDriver
            .Create(AllGenerators)
            .RunGeneratorsAndUpdateCompilation(libraryCompilation, out var libraryOutput, out _, TestContext.Current.CancellationToken);

        using var libraryImage = new MemoryStream();
        var emitResult = libraryOutput.Emit(libraryImage, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

        var appCompilation = GeneratorTestHarness.CreateCompilation(
            appSource,
            "App",
            [MetadataReference.CreateFromImage(libraryImage.ToArray())]);
        CSharpGeneratorDriver
            .Create(AllGenerators)
            .RunGeneratorsAndUpdateCompilation(appCompilation, out var appOutput, out _, TestContext.Current.CancellationToken);

        var problems = appOutput
            .GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error || d.Id == "CS0436")
            .ToList();
        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }
}