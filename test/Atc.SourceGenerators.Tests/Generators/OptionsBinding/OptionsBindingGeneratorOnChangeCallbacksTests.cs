// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_OnChange_Callback_With_Monitor_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
                              public partial class FeaturesOptions
                              {
                                  public bool EnableNewUI { get; set; }
                                  public bool EnableBetaFeatures { get; set; }

                                  private static void OnFeaturesChanged(FeaturesOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify Monitor lifetime registration
        Assert.Contains("services.AddOptions<global::MyApp.Configuration.FeaturesOptions>()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Features\"))", generatedCode, StringComparison.Ordinal);

        // Verify hosted service registration
        Assert.Contains("services.AddHostedService<FeaturesOptionsChangeListener>();", generatedCode, StringComparison.Ordinal);

        // Verify hosted service class generation
        Assert.Contains("internal sealed class FeaturesOptionsChangeListener : global::Microsoft.Extensions.Hosting.IHostedService", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private readonly global::Microsoft.Extensions.Options.IOptionsMonitor<global::MyApp.Configuration.FeaturesOptions> _monitor;", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private global::System.IDisposable? _changeToken;", generatedCode, StringComparison.Ordinal);
        Assert.Contains("_changeToken = _monitor.OnChange((options, name) =>", generatedCode, StringComparison.Ordinal);
        Assert.Contains("global::MyApp.Configuration.FeaturesOptions.OnFeaturesChanged(options, name));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("_changeToken?.Dispose();", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Used_Without_Monitor_Lifetime()
    {
        // Arrange - Using Singleton lifetime with OnChange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", Lifetime = OptionsLifetime.Singleton, OnChange = nameof(OnDatabaseChanged))]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;

                                  private static void OnDatabaseChanged(DatabaseOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("OnChange callback 'OnDatabaseChanged' can only be used when Lifetime = OptionsLifetime.Monitor", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Used_With_Scoped_Lifetime()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Api", Lifetime = OptionsLifetime.Scoped, OnChange = nameof(OnApiChanged))]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;

                                  private static void OnApiChanged(ApiOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Used_With_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email:Primary", Name = "Primary", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnEmailChanged))]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;

                                  private static void OnEmailChanged(EmailOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT005", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("OnChange callback 'OnEmailChanged' cannot be used with named options (Name = 'Primary')", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Callback_Method_Not_Found()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Cache", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnCacheChanged))]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }

                                  // Method name is wrong - should be OnCacheChanged
                                  private static void OnChanged(CacheOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT006", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("OnChange callback method 'OnCacheChanged' not found in class 'CacheOptions'", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Callback_Is_Not_Static()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Logging", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnLoggingChanged))]
                              public partial class LoggingOptions
                              {
                                  public string Level { get; set; } = "Information";

                                  // Not static - should be static
                                  private void OnLoggingChanged(LoggingOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT007", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("OnChange callback method 'OnLoggingChanged' must have signature: static void OnLoggingChanged(LoggingOptions options, string? name)", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Callback_Returns_Non_Void()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Security", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnSecurityChanged))]
                              public partial class SecurityOptions
                              {
                                  public bool EnableSsl { get; set; }

                                  // Returns bool instead of void
                                  private static bool OnSecurityChanged(SecurityOptions options, string? name)
                                  {
                                      return true;
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT007", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Callback_Has_Wrong_Parameter_Count()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("App", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnAppChanged))]
                              public partial class AppOptions
                              {
                                  public string Version { get; set; } = "1.0";

                                  // Only one parameter instead of two
                                  private static void OnAppChanged(AppOptions options)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT007", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Callback_Has_Wrong_First_Parameter_Type()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Storage", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnStorageChanged))]
                              public partial class StorageOptions
                              {
                                  public string Path { get; set; } = "/data";

                                  // First parameter is string instead of StorageOptions
                                  private static void OnStorageChanged(string options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT007", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_OnChange_Callback_Has_Wrong_Second_Parameter_Type()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Metrics", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnMetricsChanged))]
                              public partial class MetricsOptions
                              {
                                  public bool Enabled { get; set; }

                                  // Second parameter is int instead of string?
                                  private static void OnMetricsChanged(MetricsOptions options, int name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT007", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Combine_OnChange_With_ValidateDataAnnotations()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", Lifetime = OptionsLifetime.Monitor, ValidateDataAnnotations = true, OnChange = nameof(OnDatabaseChanged))]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;

                                  private static void OnDatabaseChanged(DatabaseOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both features are present
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddHostedService<DatabaseOptionsChangeListener>();", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Combine_OnChange_With_ValidateOnStart()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Api", Lifetime = OptionsLifetime.Monitor, ValidateOnStart = true, OnChange = nameof(OnApiChanged))]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;

                                  private static void OnApiChanged(ApiOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both features are present
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddHostedService<ApiOptionsChangeListener>();", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Combine_OnChange_With_All_Validation_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, ValidateDataAnnotations = true, ValidateOnStart = true, OnChange = nameof(OnFeaturesChanged))]
                              public partial class FeaturesOptions
                              {
                                  public bool EnableNewUI { get; set; }

                                  private static void OnFeaturesChanged(FeaturesOptions options, string? name)
                                  {
                                      // Callback implementation
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
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddHostedService<FeaturesOptionsChangeListener>();", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Correct_Callback_Method_Name_In_Generated_Code()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("CustomApp", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(HandleConfigurationChange))]
                              public partial class CustomAppOptions
                              {
                                  public string Setting { get; set; } = string.Empty;

                                  private static void HandleConfigurationChange(CustomAppOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify the correct method name is used
        Assert.Contains("global::MyApp.Configuration.CustomAppOptions.HandleConfigurationChange(options, name)", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Unique_Listener_Class_Names_For_Multiple_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnDatabaseChanged))]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;

                                  private static void OnDatabaseChanged(DatabaseOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }

                              [OptionsBinding("Cache", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnCacheChanged))]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }

                                  private static void OnCacheChanged(CacheOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify unique listener class names
        Assert.Contains("internal sealed class DatabaseOptionsChangeListener : global::Microsoft.Extensions.Hosting.IHostedService", generatedCode, StringComparison.Ordinal);
        Assert.Contains("internal sealed class CacheOptionsChangeListener : global::Microsoft.Extensions.Hosting.IHostedService", generatedCode, StringComparison.Ordinal);

        // Verify both hosted services are registered
        Assert.Contains("services.AddHostedService<DatabaseOptionsChangeListener>();", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddHostedService<CacheOptionsChangeListener>();", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Listener_When_OnChange_Is_Null()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Simple", Lifetime = OptionsLifetime.Monitor)]
                              public partial class SimpleOptions
                              {
                                  public string Value { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify no listener is generated
        Assert.DoesNotContain("ChangeListener", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHostedService", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Listener_When_OnChange_Is_Empty()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Empty", Lifetime = OptionsLifetime.Monitor, OnChange = "")]
                              public partial class EmptyOptions
                              {
                                  public string Value { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify no listener is generated
        Assert.DoesNotContain("ChangeListener", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHostedService", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Correct_Section_Name_In_Monitor_Binding()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("App:Features:Advanced", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnAdvancedFeaturesChanged))]
                              public partial class AdvancedFeaturesOptions
                              {
                                  public bool Enabled { get; set; }

                                  private static void OnAdvancedFeaturesChanged(AdvancedFeaturesOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify correct section path
        Assert.Contains(".Bind(configuration.GetSection(\"App:Features:Advanced\"))", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Const_SectionName_With_OnChange()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding(Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnRuntimeChanged))]
                              public partial class RuntimeOptions
                              {
                                  public const string SectionName = "Runtime:Settings";

                                  public int Timeout { get; set; }

                                  private static void OnRuntimeChanged(RuntimeOptions options, string? name)
                                  {
                                      // Callback implementation
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify const SectionName is used
        Assert.Contains(".Bind(configuration.GetSection(\"Runtime:Settings\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddHostedService<RuntimeOptionsChangeListener>();", generatedCode, StringComparison.Ordinal);
    }
}