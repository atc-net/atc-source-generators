// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Attribute_Definition()
    {
        // Arrange
        const string source = """
                              namespace TestNamespace;

                              public class TestClass
                              {
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);
        Assert.Contains("OptionsBindingAttribute.g.cs", output.Keys, StringComparer.Ordinal);
        Assert.Contains("class OptionsBindingAttribute", output["OptionsBindingAttribute.g.cs"], StringComparison.Ordinal);
        Assert.Contains("enum OptionsLifetime", output["OptionsBindingAttribute.g.cs"], StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Extension_Method_With_Inferred_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                                  public int MaxRetries { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("AddOptionsFromTestAssembly", generatedCode, StringComparison.Ordinal);
        Assert.Contains("AddOptions<global::MyApp.Configuration.DatabaseOptions>()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"DatabaseOptions\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Extension_Method_With_Explicit_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("App:Database:Settings")]
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
        Assert.Contains(".Bind(configuration.GetSection(\"App:Database:Settings\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Multiple_Options_Classes()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database")]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }

                              [OptionsBinding("Api")]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;
                              }

                              [OptionsBinding("Logging")]
                              public partial class LoggingOptions
                              {
                                  public string Level { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("Configure DatabaseOptions", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configure ApiOptions", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configure LoggingOptions", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Database\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Api\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Logging\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_When_No_OptionsBinding_Attribute()
    {
        // Arrange
        const string source = """
                              namespace MyApp.Configuration;

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
        Assert.Null(generatedCode);
    }

    [Fact]
    public void Generator_Should_Use_Atc_DependencyInjection_Namespace_For_Extension_Method()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyCompany.MyApp.Configuration;

                              [OptionsBinding("Database")]
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
        Assert.Contains("namespace Atc.DependencyInjection", generatedCode, StringComparison.Ordinal);
    }
}