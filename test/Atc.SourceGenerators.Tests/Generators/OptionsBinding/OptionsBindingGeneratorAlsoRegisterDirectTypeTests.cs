// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Register_Direct_Type_With_Singleton_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", Lifetime = OptionsLifetime.Singleton, AlsoRegisterDirectType = true)]
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
        Assert.Contains("Also register DatabaseOptions as direct type (for legacy code or third-party library compatibility)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptions<global::MyApp.Configuration.DatabaseOptions>>().Value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Direct_Type_With_Scoped_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Request", Lifetime = OptionsLifetime.Scoped, AlsoRegisterDirectType = true)]
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
        Assert.Contains("Also register RequestOptions as direct type (for legacy code or third-party library compatibility)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddScoped(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptionsSnapshot<global::MyApp.Configuration.RequestOptions>>().Value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Direct_Type_With_Monitor_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, AlsoRegisterDirectType = true)]
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
        Assert.Contains("Also register FeatureOptions as direct type (for legacy code or third-party library compatibility)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptionsMonitor<global::MyApp.Configuration.FeatureOptions>>().CurrentValue);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Register_Direct_Type_When_AlsoRegisterDirectType_Is_False()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Cache", AlsoRegisterDirectType = false)]
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
        Assert.DoesNotContain("Also register CacheOptions as direct type", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AddSingleton(sp => sp.GetRequiredService", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Register_Direct_Type_When_AlsoRegisterDirectType_Not_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

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
        Assert.DoesNotContain("Also register LoggingOptions as direct type", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AddSingleton(sp => sp.GetRequiredService", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Register_Direct_Type_For_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email:Primary", Name = "Primary", AlsoRegisterDirectType = true)]
                              [OptionsBinding("Email:Secondary", Name = "Secondary", AlsoRegisterDirectType = true)]
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

        // Named options should not generate direct type registration
        Assert.DoesNotContain("Also register EmailOptions as direct type", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AddSingleton(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptions", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AddScoped(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptionsSnapshot", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Direct_Type_With_Validation()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true, AlsoRegisterDirectType = true)]
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
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Also register DatabaseOptions as direct type (for legacy code or third-party library compatibility)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptions<global::MyApp.Configuration.DatabaseOptions>>().Value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Direct_Type_With_ErrorOnMissingKeys()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings", ErrorOnMissingKeys = true, AlsoRegisterDirectType = true)]
                              public partial class AppSettings
                              {
                                  public string AppName { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains(".Validate(options =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'AppSettings' is missing", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Also register AppSettings as direct type (for legacy code or third-party library compatibility)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptions<global::MyApp.Configuration.AppSettings>>().Value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Direct_Type_With_PostConfigure()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Storage", PostConfigure = nameof(NormalizePaths), AlsoRegisterDirectType = true)]
                              public partial class StorageOptions
                              {
                                  public string BasePath { get; set; } = string.Empty;

                                  internal static void NormalizePaths(StorageOptions options)
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
        Assert.Contains(".PostConfigure(options => global::MyApp.Configuration.StorageOptions.NormalizePaths(options))", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Also register StorageOptions as direct type (for legacy code or third-party library compatibility)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptions<global::MyApp.Configuration.StorageOptions>>().Value);", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Register_Direct_Type_For_Child_Sections()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email", ChildSections = new[] { "Primary", "Secondary" }, AlsoRegisterDirectType = true)]
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

        // ChildSections create named options, so no direct type registration
        Assert.DoesNotContain("Also register EmailOptions as direct type", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AddSingleton(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptions", generatedCode, StringComparison.Ordinal);
    }
}