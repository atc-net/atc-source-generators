// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

/// <summary>
/// Tests for Feature #6: Bind Configuration Subsections to Properties.
/// Verifies that nested configuration sections are automatically bound to complex property types.
/// </summary>
public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Bind_Simple_Nested_Object()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Email")]
            public partial class EmailOptions
            {
                public string From { get; set; } = string.Empty;

                // Nested object - should automatically bind "Email:Smtp" section
                public SmtpSettings Smtp { get; set; } = new();
            }

            public class SmtpSettings
            {
                public string Host { get; set; } = string.Empty;
                public int Port { get; set; }
                public bool UseSsl { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify the generated code uses .Bind() which handles nested objects automatically
        Assert.Contains("configuration.GetSection(\"Email\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Multiple_Nested_Objects()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Email")]
            public partial class EmailOptions
            {
                public string From { get; set; } = string.Empty;

                public SmtpSettings Smtp { get; set; } = new();

                public EmailTemplates Templates { get; set; } = new();
            }

            public class SmtpSettings
            {
                public string Host { get; set; } = string.Empty;
                public int Port { get; set; }
                public bool UseSsl { get; set; }
            }

            public class EmailTemplates
            {
                public string Welcome { get; set; } = string.Empty;
                public string ResetPassword { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"Email\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Deeply_Nested_Objects()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("App")]
            public partial class AppOptions
            {
                public DatabaseSettings Database { get; set; } = new();
            }

            public class DatabaseSettings
            {
                public string ConnectionString { get; set; } = string.Empty;

                // Nested within nested
                public RetryPolicy Retry { get; set; } = new();
            }

            public class RetryPolicy
            {
                public int MaxRetries { get; set; }
                public int DelayMilliseconds { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"App\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Nested_Collections()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;
            using System.Collections.Generic;

            namespace MyApp.Configuration;

            [OptionsBinding("Services")]
            public partial class ServicesOptions
            {
                // Collection of nested objects
                public List<ApiEndpoint> Endpoints { get; set; } = new();
            }

            public class ApiEndpoint
            {
                public string Name { get; set; } = string.Empty;
                public string Url { get; set; } = string.Empty;
                public int Timeout { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"Services\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Mixed_Simple_And_Complex_Properties()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;
            using System.ComponentModel.DataAnnotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Application", ValidateDataAnnotations = true)]
            public partial class ApplicationOptions
            {
                // Simple properties
                [Required]
                public string Name { get; set; } = string.Empty;

                public int Version { get; set; }

                public bool IsProduction { get; set; }

                // Complex property (nested object)
                public LoggingConfiguration Logging { get; set; } = new();

                // Another complex property
                public SecuritySettings Security { get; set; } = new();
            }

            public class LoggingConfiguration
            {
                public string Level { get; set; } = "Information";
                public bool EnableConsole { get; set; } = true;
            }

            public class SecuritySettings
            {
                public bool RequireHttps { get; set; } = true;
                public int TokenExpirationMinutes { get; set; } = 60;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"Application\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Nested_Objects_With_Validation()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;
            using System.ComponentModel.DataAnnotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Email", ValidateDataAnnotations = true, ValidateOnStart = true)]
            public partial class EmailOptions
            {
                [Required]
                [EmailAddress]
                public string From { get; set; } = string.Empty;

                // Nested object with validation - .Bind() will validate this too
                public SmtpSettings Smtp { get; set; } = new();
            }

            public class SmtpSettings
            {
                [Required]
                public string Host { get; set; } = string.Empty;

                [Range(1, 65535)]
                public int Port { get; set; }

                public bool UseSsl { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"Email\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Nested_Objects_With_Monitor_Lifetime()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace MyApp.Configuration;

            [OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor)]
            public partial class FeaturesOptions
            {
                public bool EnableNewUI { get; set; }

                // Nested feature flags
                public ExperimentalFeatures Experimental { get; set; } = new();
            }

            public class ExperimentalFeatures
            {
                public bool EnableBetaFeatures { get; set; }
                public bool EnableAlphaFeatures { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"Features\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddOptions<", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Nested_Dictionary()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;
            using System.Collections.Generic;

            namespace MyApp.Configuration;

            [OptionsBinding("App")]
            public partial class AppOptions
            {
                // Dictionary of settings
                public Dictionary<string, string> Settings { get; set; } = new();

                // Nested object with dictionary
                public ConnectionStrings Connections { get; set; } = new();
            }

            public class ConnectionStrings
            {
                public Dictionary<string, string> Databases { get; set; } = new();
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"App\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Bind_Complex_Real_World_Example()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;
            using System.Collections.Generic;
            using System.ComponentModel.DataAnnotations;

            namespace MyApp.Configuration;

            [OptionsBinding("CloudStorage", ValidateDataAnnotations = true, ValidateOnStart = true)]
            public partial class CloudStorageOptions
            {
                [Required]
                public string Provider { get; set; } = string.Empty;

                public AzureStorageSettings Azure { get; set; } = new();

                public AwsS3Settings Aws { get; set; } = new();

                public RetryPolicy RetryPolicy { get; set; } = new();
            }

            public class AzureStorageSettings
            {
                [Required]
                public string ConnectionString { get; set; } = string.Empty;

                public string ContainerName { get; set; } = string.Empty;

                public BlobSettings Blob { get; set; } = new();
            }

            public class BlobSettings
            {
                public int MaxBlockSize { get; set; } = 4194304;

                public int ParallelOperations { get; set; } = 8;
            }

            public class AwsS3Settings
            {
                [Required]
                public string AccessKey { get; set; } = string.Empty;

                [Required]
                public string SecretKey { get; set; } = string.Empty;

                public string Region { get; set; } = "us-east-1";

                public string BucketName { get; set; } = string.Empty;
            }

            public class RetryPolicy
            {
                [Range(0, 10)]
                public int MaxRetries { get; set; } = 3;

                [Range(100, 60000)]
                public int DelayMilliseconds { get; set; } = 1000;

                public bool UseExponentialBackoff { get; set; } = true;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        Assert.Contains("configuration.GetSection(\"CloudStorage\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }
}