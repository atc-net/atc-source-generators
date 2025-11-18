// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Comment_For_Singleton_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", Lifetime = OptionsLifetime.Singleton)]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure DatabaseOptions - Inject using IOptions<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Comment_For_Scoped_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Request", Lifetime = OptionsLifetime.Scoped)]
                              public partial class RequestOptions
                              {
                                  public string ClientId { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure RequestOptions - Inject using IOptionsSnapshot<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Comment_For_Monitor_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor)]
                              public partial class FeatureOptions
                              {
                                  public bool EnableNewFeature { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure FeatureOptions - Inject using IOptionsMonitor<T>", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Default_To_Singleton_When_Lifetime_Not_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Cache")]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure CacheOptions - Inject using IOptions<T>", generatedCode, StringComparison.Ordinal);
    }
}