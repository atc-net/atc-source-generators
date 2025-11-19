// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Add_ErrorOnMissingKeys_Validation_When_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ErrorOnMissingKeys = true)]
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
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("var section = configuration.GetSection(\"Database\");", generatedCode, StringComparison.Ordinal);
        Assert.Contains("if (!section.Exists())", generatedCode, StringComparison.Ordinal);
        Assert.Contains("throw new global::System.InvalidOperationException(", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'Database' is missing.", generatedCode, StringComparison.Ordinal);
        Assert.Contains("return true;", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Combine_ErrorOnMissingKeys_With_ValidateOnStart()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ErrorOnMissingKeys = true, ValidateOnStart = true)]
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
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Combine_ErrorOnMissingKeys_With_ValidateDataAnnotations()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ErrorOnMissingKeys = true, ValidateDataAnnotations = true)]
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
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Combine_ErrorOnMissingKeys_With_All_Validation_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ErrorOnMissingKeys = true, ValidateDataAnnotations = true, ValidateOnStart = true)]
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
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Correct_Section_Name_In_ErrorOnMissingKeys_Message()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("App:Api:Settings", ErrorOnMissingKeys = true)]
                              public partial class ApiSettings
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
        Assert.Contains("var section = configuration.GetSection(\"App:Api:Settings\");", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'App:Api:Settings' is missing.", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_SectionName_In_ErrorOnMissingKeys_Message()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding(ErrorOnMissingKeys = true)]
                              public partial class CacheOptions
                              {
                                  public const string SectionName = "Caching";

                                  public int MaxSize { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("var section = configuration.GetSection(\"Caching\");", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'Caching' is missing.", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Add_ErrorOnMissingKeys_Validation_When_Not_Specified()
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
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.DoesNotContain(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("section.Exists()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Add_ErrorOnMissingKeys_Validation_When_False()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ErrorOnMissingKeys = false)]
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
        Assert.DoesNotContain(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("section.Exists()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_ErrorOnMissingKeys_With_NamedOptions()
    {
        // Arrange - Named options now support ErrorOnMissingKeys via fluent API
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email:Primary", Name = "Primary", ErrorOnMissingKeys = true, ValidateOnStart = true)]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Named options with validation use AddOptions<T>("name") fluent API pattern
        Assert.Contains("services.AddOptions<global::MyApp.Configuration.EmailOptions>(\"Primary\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Email:Primary\"))", generatedCode, StringComparison.Ordinal);

        // ErrorOnMissingKeys validation should be present
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("var section = configuration.GetSection(\"Email:Primary\");", generatedCode, StringComparison.Ordinal);
        Assert.Contains("if (!section.Exists())", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'Email:Primary' is missing", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_ErrorOnMissingKeys_With_Auto_Inferred_Section_Name()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding(ErrorOnMissingKeys = true)]
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
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("var section = configuration.GetSection(\"DatabaseOptions\");", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'DatabaseOptions' is missing.", generatedCode, StringComparison.Ordinal);
    }
}