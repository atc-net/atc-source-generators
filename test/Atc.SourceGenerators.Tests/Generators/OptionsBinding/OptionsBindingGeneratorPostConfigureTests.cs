// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_PostConfigure_Callback()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Storage", PostConfigure = nameof(NormalizePaths))]
                              public partial class StorageOptions
                              {
                                  public string BasePath { get; set; } = string.Empty;
                                  public string CachePath { get; set; } = string.Empty;

                                  private static void NormalizePaths(StorageOptions options)
                                  {
                                      // Normalize paths implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify PostConfigure registration
        Assert.Contains("services.AddOptions<global::MyApp.Configuration.StorageOptions>()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Storage\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.StorageOptions.NormalizePaths(options))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_PostConfigure_Used_With_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database:Primary", Name = "Primary", PostConfigure = nameof(Normalize))]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;

                                  private static void Normalize(DatabaseOptions options)
                                  {
                                      // Normalize implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT008", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("PostConfigure callback 'Normalize' cannot be used with named options (Name = 'Primary')", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_PostConfigure_Method_Not_Found()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Api", PostConfigure = nameof(NonExistentMethod))]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT009", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("PostConfigure callback method 'NonExistentMethod' not found in class 'ApiOptions'", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_PostConfigure_Method_Has_Wrong_Parameter_Count()
    {
        // Arrange - Method has no parameters
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email", PostConfigure = nameof(Configure))]
                              public partial class EmailOptions
                              {
                                  public string SmtpHost { get; set; } = string.Empty;

                                  private static void Configure()
                                  {
                                      // No parameters - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT010", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("PostConfigure callback method 'Configure' must have signature: static void Configure(EmailOptions options)", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_PostConfigure_Method_Has_Too_Many_Parameters()
    {
        // Arrange - Method has two parameters instead of one
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Features", PostConfigure = nameof(Configure))]
                              public partial class FeaturesOptions
                              {
                                  public bool EnableNewUI { get; set; }

                                  private static void Configure(FeaturesOptions options, string? name)
                                  {
                                      // Too many parameters - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT010", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("PostConfigure callback method 'Configure' must have signature: static void Configure(FeaturesOptions options)", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_PostConfigure_Method_Has_Wrong_Parameter_Type()
    {
        // Arrange - Parameter type doesn't match options class
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Logging", PostConfigure = nameof(Configure))]
                              public partial class LoggingOptions
                              {
                                  public string Level { get; set; } = string.Empty;

                                  private static void Configure(string wrongType)
                                  {
                                      // Wrong parameter type - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT010", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("PostConfigure callback method 'Configure' must have signature: static void Configure(LoggingOptions options)", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_PostConfigure_Method_Is_Not_Static()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Cache", PostConfigure = nameof(Configure))]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }

                                  private void Configure(CacheOptions options)
                                  {
                                      // Not static - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT010", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("PostConfigure callback method 'Configure' must have signature: static void Configure(CacheOptions options)", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_PostConfigure_Method_Returns_NonVoid()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Queue", PostConfigure = nameof(Configure))]
                              public partial class QueueOptions
                              {
                                  public int MaxMessages { get; set; }

                                  private static bool Configure(QueueOptions options)
                                  {
                                      return true; // Non-void return - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT010", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("PostConfigure callback method 'Configure' must have signature: static void Configure(QueueOptions options)", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_PostConfigure_With_ValidateDataAnnotations()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using System.ComponentModel.DataAnnotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Storage", ValidateDataAnnotations = true, PostConfigure = nameof(NormalizePaths))]
                              public partial class StorageOptions
                              {
                                  [Required]
                                  public string BasePath { get; set; } = string.Empty;

                                  private static void NormalizePaths(StorageOptions options)
                                  {
                                      // Normalize paths
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both ValidateDataAnnotations and PostConfigure are present
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.StorageOptions.NormalizePaths(options))", generatedCode, StringComparison.Ordinal);

        // Verify PostConfigure comes after ValidateDataAnnotations but before ValidateOnStart (if present)
        var validateDataAnnotationsIndex = generatedCode.IndexOf(".ValidateDataAnnotations()", StringComparison.Ordinal);
        var postConfigureIndex = generatedCode.IndexOf(".PostConfigure(", StringComparison.Ordinal);
        Assert.True(postConfigureIndex > validateDataAnnotationsIndex, "PostConfigure should come after ValidateDataAnnotations");
    }

    [Fact]
    public void Generator_Should_Generate_PostConfigure_With_ValidateOnStart()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Paths", ValidateOnStart = true, PostConfigure = nameof(NormalizePaths))]
                              public partial class PathsOptions
                              {
                                  public string TempPath { get; set; } = string.Empty;

                                  private static void NormalizePaths(PathsOptions options)
                                  {
                                      // Normalize paths
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both PostConfigure and ValidateOnStart are present
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.PathsOptions.NormalizePaths(options))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);

        // Verify PostConfigure comes before ValidateOnStart
        var postConfigureIndex = generatedCode.IndexOf(".PostConfigure(", StringComparison.Ordinal);
        var validateOnStartIndex = generatedCode.IndexOf(".ValidateOnStart()", StringComparison.Ordinal);
        Assert.True(validateOnStartIndex > postConfigureIndex, "ValidateOnStart should come after PostConfigure");
    }

    [Fact]
    public void Generator_Should_Generate_PostConfigure_With_ErrorOnMissingKeys()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Urls", ErrorOnMissingKeys = true, PostConfigure = nameof(NormalizeUrls))]
                              public partial class UrlsOptions
                              {
                                  public string ApiUrl { get; set; } = string.Empty;

                                  private static void NormalizeUrls(UrlsOptions options)
                                  {
                                      // Normalize URLs to lowercase
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both ErrorOnMissingKeys validation and PostConfigure are present
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("if (!section.Exists())", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.UrlsOptions.NormalizeUrls(options))", generatedCode, StringComparison.Ordinal);

        // Verify PostConfigure comes after ErrorOnMissingKeys validation
        var validateIndex = generatedCode.IndexOf(".Validate(options =>", StringComparison.Ordinal);
        var postConfigureIndex = generatedCode.IndexOf(".PostConfigure(", StringComparison.Ordinal);
        Assert.True(postConfigureIndex > validateIndex, "PostConfigure should come after ErrorOnMissingKeys validation");
    }

    [Fact]
    public void Generator_Should_Generate_PostConfigure_With_All_Validation_Features()
    {
        // Arrange - Test with all validation features combined
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using System.ComponentModel.DataAnnotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("ComplexStorage",
                                  ValidateDataAnnotations = true,
                                  ErrorOnMissingKeys = true,
                                  ValidateOnStart = true,
                                  PostConfigure = nameof(NormalizePaths))]
                              public partial class ComplexStorageOptions
                              {
                                  [Required]
                                  public string BasePath { get; set; } = string.Empty;

                                  [Required]
                                  public string TempPath { get; set; } = string.Empty;

                                  private static void NormalizePaths(ComplexStorageOptions options)
                                  {
                                      // Normalize all paths
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify all features are present
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("if (!section.Exists())", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.ComplexStorageOptions.NormalizePaths(options))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);

        // Verify correct order: Bind -> ValidateDataAnnotations -> ErrorOnMissingKeys -> PostConfigure -> ValidateOnStart
        var bindIndex = generatedCode.IndexOf(".Bind(configuration.GetSection", StringComparison.Ordinal);
        var validateDataAnnotationsIndex = generatedCode.IndexOf(".ValidateDataAnnotations()", StringComparison.Ordinal);
        var validateIndex = generatedCode.IndexOf(".Validate(options =>", StringComparison.Ordinal);
        var postConfigureIndex = generatedCode.IndexOf(".PostConfigure(", StringComparison.Ordinal);
        var validateOnStartIndex = generatedCode.IndexOf(".ValidateOnStart()", StringComparison.Ordinal);

        Assert.True(validateDataAnnotationsIndex > bindIndex, "ValidateDataAnnotations should come after Bind");
        Assert.True(validateIndex > validateDataAnnotationsIndex, "ErrorOnMissingKeys validation should come after ValidateDataAnnotations");
        Assert.True(postConfigureIndex > validateIndex, "PostConfigure should come after ErrorOnMissingKeys validation");
        Assert.True(validateOnStartIndex > postConfigureIndex, "ValidateOnStart should come after PostConfigure");
    }

    [Fact]
    public void Generator_Should_Support_Public_PostConfigure_Method()
    {
        // Arrange - Test with public method instead of private
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Public", PostConfigure = nameof(Configure))]
                              public partial class PublicOptions
                              {
                                  public string Value { get; set; } = string.Empty;

                                  public static void Configure(PublicOptions options)
                                  {
                                      // Public method works too
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.PublicOptions.Configure(options))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Internal_PostConfigure_Method()
    {
        // Arrange - Test with internal method
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Internal", PostConfigure = nameof(Configure))]
                              public partial class InternalOptions
                              {
                                  public string Value { get; set; } = string.Empty;

                                  internal static void Configure(InternalOptions options)
                                  {
                                      // Internal method works too
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.InternalOptions.Configure(options))", generatedCode, StringComparison.Ordinal);
    }
}