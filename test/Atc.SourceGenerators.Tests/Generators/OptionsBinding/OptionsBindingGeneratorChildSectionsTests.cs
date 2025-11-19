// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Multiple_Named_Instances_From_ChildSections()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary", "Secondary", "Fallback" })]
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

        // Verify each child section is configured as a named instance
        Assert.Contains("services.Configure<global::MyApp.Configuration.DatabaseOptions>(\"Primary\", configuration.GetSection(\"Database:Primary\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.DatabaseOptions>(\"Secondary\", configuration.GetSection(\"Database:Secondary\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.DatabaseOptions>(\"Fallback\", configuration.GetSection(\"Database:Fallback\"));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ChildSections_Used_With_Name()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", Name = "Primary", ChildSections = new[] { "A", "B" })]
            public partial class DatabaseOptions
            {
                public string ConnectionString { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, _) = GetGeneratedOutput(source);

        // Assert
        var error = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT014", error.Id);
        Assert.Contains("ChildSections cannot be used with Name property", error.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ChildSections_Has_Only_One_Item()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary" })]
            public partial class DatabaseOptions
            {
                public string ConnectionString { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, _) = GetGeneratedOutput(source);

        // Assert
        var error = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT015", error.Id);
        Assert.Contains("ChildSections requires at least 2 items", error.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("Found 1 item(s)", error.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ChildSections_Contains_Empty_String()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary", "", "Secondary" })]
            public partial class DatabaseOptions
            {
                public string ConnectionString { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, _) = GetGeneratedOutput(source);

        // Assert
        var error = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT016", error.Id);
        Assert.Contains("ChildSections array contains null or empty value", error.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("index 1", error.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_ChildSections_With_Validation()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary", "Secondary" }, ValidateDataAnnotations = true, ValidateOnStart = true)]
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

        // Verify both instances have validation
        Assert.Contains("services.AddOptions<global::MyApp.Configuration.DatabaseOptions>(\"Primary\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"Database:Primary\"))", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_ChildSections_With_ConfigureAll()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary", "Secondary", "Tertiary" }, ConfigureAll = nameof(DatabaseOptions.SetDefaults))]
            public partial class DatabaseOptions
            {
                public string ConnectionString { get; set; } = string.Empty;
                public int MaxRetries { get; set; }

                internal static void SetDefaults(DatabaseOptions options)
                {
                    options.MaxRetries = 3;
                }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify ConfigureAll is generated before individual configurations
        Assert.Contains("services.ConfigureAll<global::MyApp.Configuration.DatabaseOptions>(options => global::MyApp.Configuration.DatabaseOptions.SetDefaults(options));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.DatabaseOptions>(\"Primary\", configuration.GetSection(\"Database:Primary\"));", generatedCode, StringComparison.Ordinal);

        // Verify ConfigureAll appears before individual Configure calls
        var configureAllIndex = generatedCode.IndexOf("services.ConfigureAll<", StringComparison.Ordinal);
        var configurePrimaryIndex = generatedCode.IndexOf("services.Configure<global::MyApp.Configuration.DatabaseOptions>(\"Primary\"", StringComparison.Ordinal);
        Assert.True(configureAllIndex < configurePrimaryIndex, "ConfigureAll should appear before individual Configure calls");
    }

    [Fact]
    public void Generator_Should_Support_ChildSections_With_Nested_Path()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("App:Services:Cache", ChildSections = new[] { "Redis", "Memory" })]
            public partial class CacheOptions
            {
                public string Provider { get; set; } = string.Empty;
                public int ExpirationMinutes { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify nested paths are constructed correctly
        Assert.Contains("services.Configure<global::MyApp.Configuration.CacheOptions>(\"Redis\", configuration.GetSection(\"App:Services:Cache:Redis\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.CacheOptions>(\"Memory\", configuration.GetSection(\"App:Services:Cache:Memory\"));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Many_Child_Sections()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Regions", ChildSections = new[] { "USEast", "USWest", "EUWest", "EUNorth", "APSouth", "APNorth" })]
            public partial class RegionOptions
            {
                public string ApiUrl { get; set; } = string.Empty;
                public int Timeout { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify all 6 regions are configured
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"USEast\",", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"USWest\",", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"EUWest\",", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"EUNorth\",", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"APSouth\",", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.RegionOptions>(\"APNorth\",", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_ChildSections_With_Custom_Validator()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;
            using Microsoft.Extensions.Options;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary", "Secondary" }, ValidateDataAnnotations = true, Validator = typeof(DatabaseOptionsValidator))]
            public partial class DatabaseOptions
            {
                public string ConnectionString { get; set; } = string.Empty;
            }

            public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
            {
                public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
                {
                    return ValidateOptionsResult.Success;
                }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify validator is registered
        Assert.Contains("services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Configuration.DatabaseOptions>, global::MyApp.Configuration.DatabaseOptionsValidator>();", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_ChildSections_With_Scoped_Lifetime()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Tenant", ChildSections = new[] { "TenantA", "TenantB" }, Lifetime = OptionsLifetime.Scoped)]
            public partial class TenantOptions
            {
                public string Name { get; set; } = string.Empty;
                public int MaxUsers { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both instances are configured (Scoped/Singleton both use Configure)
        Assert.Contains("services.Configure<global::MyApp.Configuration.TenantOptions>(\"TenantA\", configuration.GetSection(\"Tenant:TenantA\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.TenantOptions>(\"TenantB\", configuration.GetSection(\"Tenant:TenantB\"));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_ChildSections_Has_Zero_Items()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new string[] { })]
            public partial class DatabaseOptions
            {
                public string ConnectionString { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, _) = GetGeneratedOutput(source);

        // Assert
        var error = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT015", error.Id);
        Assert.Contains("ChildSections requires at least 2 items", error.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("Found 0 item(s)", error.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_ChildSections_With_ErrorOnMissingKeys()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary", "Secondary" }, ErrorOnMissingKeys = true, ValidateOnStart = true)]
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

        // Verify ErrorOnMissingKeys validation for both sections
        Assert.Contains("var section = configuration.GetSection(\"Database:Primary\");", generatedCode, StringComparison.Ordinal);
        Assert.Contains("if (!section.Exists())", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configuration section 'Database:Primary' is missing", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Allow_Two_Child_Sections_Minimum()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Database", ChildSections = new[] { "Primary", "Secondary" })]
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

        // Verify both sections are configured
        Assert.Contains("services.Configure<global::MyApp.Configuration.DatabaseOptions>(\"Primary\"", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.DatabaseOptions>(\"Secondary\"", generatedCode, StringComparison.Ordinal);
    }
}