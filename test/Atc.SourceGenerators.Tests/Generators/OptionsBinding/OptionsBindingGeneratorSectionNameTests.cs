// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Infer_Section_Name_From_Class_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"ApiOptions\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Full_Class_Name_For_Inference()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class LoggingSettings
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
        Assert.Contains(".Bind(configuration.GetSection(\"LoggingSettings\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_SectionName_For_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string SectionName = "CustomDatabaseSection";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"CustomDatabaseSection\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_Name_For_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string Name = "MyDatabaseConfig";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"MyDatabaseConfig\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_NameTitle_For_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public const string NameTitle = "CustomApiSection";
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"CustomApiSection\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_SectionName_Over_NameTitle()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string SectionName = "X1";
                                  public const string NameTitle = "X2";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"X1\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X2\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_SectionName_Over_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public const string SectionName = "X1";
                                  public const string Name = "X3";
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"X1\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X3\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_NameTitle_Over_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class LoggingOptions
                              {
                                  public const string NameTitle = "AppLogging";
                                  public const string Name = "Logging";
                                  public string Level { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"AppLogging\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"Logging\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Full_Priority_Order()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class LoggingOptions
                              {
                                  public const string SectionName = "X1";
                                  public const string NameTitle = "X2";
                                  public const string Name = "X3";
                                  public string Level { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"X1\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X2\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"X3\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_Explicit_SectionName_Over_Const_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("ExplicitSection")]
                              public partial class CacheOptions
                              {
                                  public const string Name = "Cache";
                                  public int Size { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"ExplicitSection\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"Cache\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_Const_Name_Over_Auto_Inference()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class EmailOptions
                              {
                                  public const string Name = "EmailConfig";
                                  public string SmtpServer { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"EmailConfig\"))", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain(".Bind(configuration.GetSection(\"EmailOptions\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Named_Parameters_Without_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding(ValidateDataAnnotations = true)]
                              public partial class DatabaseOptions
                              {
                                  public const string Name = "MyDatabase";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Bind(configuration.GetSection(\"MyDatabase\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
    }
}