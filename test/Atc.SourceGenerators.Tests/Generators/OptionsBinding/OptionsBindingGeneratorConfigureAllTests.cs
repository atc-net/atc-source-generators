// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_ConfigureAll_For_Multiple_Named_Options()
    {
        // Arrange - Multiple named instances with ConfigureAll
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Email:Secondary", Name = "Secondary")]
                              [OptionsBinding("Email:Fallback", Name = "Fallback")]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; }
                                  public int TimeoutSeconds { get; set; }

                                  internal static void SetDefaults(EmailOptions options)
                                  {
                                      options.TimeoutSeconds = 30; // Default for all instances
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify ConfigureAll is generated BEFORE individual Configure calls
        Assert.Contains("services.ConfigureAll<global::MyApp.Configuration.EmailOptions>(options => global::MyApp.Configuration.EmailOptions.SetDefaults(options));", generatedCode, StringComparison.Ordinal);

        // Verify individual Configure calls
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\", configuration.GetSection(\"Email:Primary\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Secondary\", configuration.GetSection(\"Email:Secondary\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Fallback\", configuration.GetSection(\"Email:Fallback\"));", generatedCode, StringComparison.Ordinal);

        // Verify ConfigureAll comes before Configure calls
        var configureAllIndex = generatedCode.IndexOf("services.ConfigureAll<", StringComparison.Ordinal);
        var firstConfigureIndex = generatedCode.IndexOf("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\"", StringComparison.Ordinal);
        Assert.True(configureAllIndex < firstConfigureIndex, "ConfigureAll should come before individual Configure calls");
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Used_With_Single_Named_Instance()
    {
        // Arrange - Only one named instance
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;

                                  internal static void SetDefaults(EmailOptions options)
                                  {
                                      // Set defaults
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT011", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("ConfigureAll callback 'SetDefaults' can only be used when the class has multiple named instances", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Used_With_Unnamed_Instance()
    {
        // Arrange - Single unnamed instance
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ConfigureAll = nameof(SetDefaults))]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;

                                  internal static void SetDefaults(DatabaseOptions options)
                                  {
                                      // Set defaults
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT011", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Method_Not_Found()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Api:Primary", Name = "Primary", ConfigureAll = nameof(NonExistentMethod))]
                              [OptionsBinding("Api:Secondary", Name = "Secondary")]
                              public partial class ApiOptions
                              {
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT012", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("ConfigureAll callback method 'NonExistentMethod' not found in class 'ApiOptions'", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Method_Has_Wrong_Parameter_Count()
    {
        // Arrange - Method has no parameters
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Db:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Db:Secondary", Name = "Secondary")]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;

                                  internal static void SetDefaults()
                                  {
                                      // No parameters - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT013", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("ConfigureAll callback method 'SetDefaults' must have signature: static void SetDefaults(DatabaseOptions options)", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Method_Has_Too_Many_Parameters()
    {
        // Arrange - Method has two parameters instead of one
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Cache:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Cache:Secondary", Name = "Secondary")]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }

                                  internal static void SetDefaults(CacheOptions options, string name)
                                  {
                                      // Two parameters - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT013", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Method_Has_Wrong_Parameter_Type()
    {
        // Arrange - Parameter type doesn't match options class
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Storage:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Storage:Secondary", Name = "Secondary")]
                              public partial class StorageOptions
                              {
                                  public string BasePath { get; set; } = string.Empty;

                                  internal static void SetDefaults(string wrongType)
                                  {
                                      // Wrong parameter type - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT013", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Method_Is_Not_Static()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Queue:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Queue:Secondary", Name = "Secondary")]
                              public partial class QueueOptions
                              {
                                  public int MaxMessages { get; set; }

                                  internal void SetDefaults(QueueOptions options)
                                  {
                                      // Not static - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT013", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ConfigureAll_Method_Returns_NonVoid()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Messaging:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Messaging:Secondary", Name = "Secondary")]
                              public partial class MessagingOptions
                              {
                                  public string Endpoint { get; set; } = string.Empty;

                                  internal static bool SetDefaults(MessagingOptions options)
                                  {
                                      return true; // Non-void return - invalid!
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT013", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Support_ConfigureAll_On_Any_Attribute()
    {
        // Arrange - ConfigureAll on second attribute instead of first
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Server:Primary", Name = "Primary")]
                              [OptionsBinding("Server:Secondary", Name = "Secondary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Server:Tertiary", Name = "Tertiary")]
                              public partial class ServerOptions
                              {
                                  public string Host { get; set; } = string.Empty;
                                  public int MaxConnections { get; set; }

                                  internal static void SetDefaults(ServerOptions options)
                                  {
                                      options.MaxConnections = 100; // Default for all
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("services.ConfigureAll<global::MyApp.Configuration.ServerOptions>(options => global::MyApp.Configuration.ServerOptions.SetDefaults(options));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_ConfigureAll_Only_Once_When_Multiple_Attributes_Have_It()
    {
        // Arrange - ConfigureAll on multiple attributes (should use first found)
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Db:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Db:Secondary", Name = "Secondary", ConfigureAll = nameof(SetDefaults))]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;

                                  internal static void SetDefaults(DatabaseOptions options)
                                  {
                                      // Set defaults
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Count occurrences of ConfigureAll - should be exactly 1
        var count = generatedCode.Split(new[] { "services.ConfigureAll<" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(1, count);
    }

    [Fact]
    public void Generator_Should_Support_Public_ConfigureAll_Method()
    {
        // Arrange - Test with public method instead of internal
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Endpoint:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Endpoint:Secondary", Name = "Secondary")]
                              public partial class EndpointOptions
                              {
                                  public string Url { get; set; } = string.Empty;
                                  public int Timeout { get; set; }

                                  public static void SetDefaults(EndpointOptions options)
                                  {
                                      options.Timeout = 60; // Public method works too
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("services.ConfigureAll<global::MyApp.Configuration.EndpointOptions>(options => global::MyApp.Configuration.EndpointOptions.SetDefaults(options));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Private_ConfigureAll_Method()
    {
        // Arrange - Test with private method
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Logger:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Logger:Secondary", Name = "Secondary")]
                              public partial class LoggerOptions
                              {
                                  public string Level { get; set; } = string.Empty;

                                  private static void SetDefaults(LoggerOptions options)
                                  {
                                      options.Level = "Information"; // Private method works too
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);
        Assert.Contains("services.ConfigureAll<global::MyApp.Configuration.LoggerOptions>(options => global::MyApp.Configuration.LoggerOptions.SetDefaults(options));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Work_With_Many_Named_Instances()
    {
        // Arrange - Test with many named instances
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Region:US-East", Name = "USEast", ConfigureAll = nameof(SetDefaults))]
                              [OptionsBinding("Region:US-West", Name = "USWest")]
                              [OptionsBinding("Region:EU-West", Name = "EUWest")]
                              [OptionsBinding("Region:AP-South", Name = "APSouth")]
                              [OptionsBinding("Region:AP-North", Name = "APNorth")]
                              public partial class RegionOptions
                              {
                                  public string Endpoint { get; set; } = string.Empty;
                                  public int MaxRetries { get; set; }

                                  internal static void SetDefaults(RegionOptions options)
                                  {
                                      options.MaxRetries = 5; // Default for all regions
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify ConfigureAll is generated
        Assert.Contains("services.ConfigureAll<global::MyApp.Configuration.RegionOptions>(options => global::MyApp.Configuration.RegionOptions.SetDefaults(options));", generatedCode, StringComparison.Ordinal);

        // Verify all named instances are configured
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"USEast\"", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"USWest\"", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"EUWest\"", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"APSouth\"", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"APNorth\"", generatedCode, StringComparison.Ordinal);
    }
}