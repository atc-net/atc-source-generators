// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Register_Single_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings:Email", Name = "Primary")]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify that named options use Configure<T>(name, section) pattern
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\", configuration.GetSection(\"AppSettings:Email\"));", generatedCode, StringComparison.Ordinal);

        // Verify that the comment indicates named instance
        Assert.Contains("Configure EmailOptions (Named: \"Primary\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Inject using IOptionsSnapshot<T>.Get(\"Primary\")", generatedCode, StringComparison.Ordinal);

        // Verify that AddOptions pattern is NOT used for named options
        Assert.DoesNotContain("services.AddOptions<global::MyApp.Configuration.EmailOptions>()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Multiple_Named_Options_On_Same_Class()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings:Email:Primary", Name = "Primary")]
                              [OptionsBinding("AppSettings:Email:Secondary", Name = "Secondary")]
                              [OptionsBinding("AppSettings:Email:Fallback", Name = "Fallback")]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                                  public bool UseSsl { get; set; } = true;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify all three named instances are registered
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\", configuration.GetSection(\"AppSettings:Email:Primary\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Secondary\", configuration.GetSection(\"AppSettings:Email:Secondary\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Fallback\", configuration.GetSection(\"AppSettings:Email:Fallback\"));", generatedCode, StringComparison.Ordinal);

        // Verify all three have appropriate comments
        Assert.Contains("Configure EmailOptions (Named: \"Primary\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configure EmailOptions (Named: \"Secondary\")", generatedCode, StringComparison.Ordinal);
        Assert.Contains("Configure EmailOptions (Named: \"Fallback\")", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Both_Named_And_Unnamed_Options_On_Same_Class()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings:Email")]
                              [OptionsBinding("AppSettings:Email:Backup", Name = "Backup")]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify unnamed options use AddOptions pattern
        Assert.Contains("services.AddOptions<global::MyApp.Configuration.EmailOptions>()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".Bind(configuration.GetSection(\"AppSettings:Email\"))", generatedCode, StringComparison.Ordinal);

        // Verify named options use Configure pattern
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Backup\", configuration.GetSection(\"AppSettings:Email:Backup\"));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Include_Validation_Chain_For_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings:Email", Name = "Primary", ValidateDataAnnotations = true, ValidateOnStart = true)]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify named options registration is present
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\", configuration.GetSection(\"AppSettings:Email\"));", generatedCode, StringComparison.Ordinal);

        // Verify that validation methods are NOT called for named options
        Assert.DoesNotContain("ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Register_Validator_For_Named_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using Microsoft.Extensions.Options;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings:Email", Name = "Primary", Validator = typeof(EmailOptionsValidator))]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                              }

                              public class EmailOptionsValidator : IValidateOptions<EmailOptions>
                              {
                                  public ValidateOptionsResult Validate(string? name, EmailOptions options)
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

        // Verify named options registration is present
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\", configuration.GetSection(\"AppSettings:Email\"));", generatedCode, StringComparison.Ordinal);

        // Verify that validator registration is NOT present for named options
        Assert.DoesNotContain("AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Named_Options_With_Const_SectionName()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding(Name = "Primary")]
                              [OptionsBinding(Name = "Secondary")]
                              public partial class EmailOptions
                              {
                                  public const string SectionName = "EmailConfiguration";

                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both named instances use the const SectionName
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\", configuration.GetSection(\"EmailConfiguration\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Secondary\", configuration.GetSection(\"EmailConfiguration\"));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Named_Options_With_Different_Lifetimes()
    {
        // Arrange - Note: Lifetime property doesn't affect named options registration pattern
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings:Email:Primary", Name = "Primary", Lifetime = OptionsLifetime.Singleton)]
                              [OptionsBinding("AppSettings:Email:Secondary", Name = "Secondary", Lifetime = OptionsLifetime.Monitor)]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify both named instances are registered (Lifetime doesn't change the registration pattern for named options)
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Primary\", configuration.GetSection(\"AppSettings:Email:Primary\"));", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Secondary\", configuration.GetSection(\"AppSettings:Email:Secondary\"));", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Mixed_Named_And_Unnamed_With_Different_Validation()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using Microsoft.Extensions.Options;

                              namespace MyApp.Configuration;

                              [OptionsBinding("AppSettings:Email", ValidateDataAnnotations = true, ValidateOnStart = true, Validator = typeof(EmailOptionsValidator))]
                              [OptionsBinding("AppSettings:Email:Backup", Name = "Backup")]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; } = 587;
                              }

                              public class EmailOptionsValidator : IValidateOptions<EmailOptions>
                              {
                                  public ValidateOptionsResult Validate(string? name, EmailOptions options)
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

        // Verify unnamed options have full validation chain
        Assert.Contains("services.AddOptions<global::MyApp.Configuration.EmailOptions>()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Configuration.EmailOptions>, global::MyApp.Configuration.EmailOptionsValidator>();", generatedCode, StringComparison.Ordinal);

        // Verify named options do NOT have validation
        Assert.Contains("services.Configure<global::MyApp.Configuration.EmailOptions>(\"Backup\", configuration.GetSection(\"AppSettings:Email:Backup\"));", generatedCode, StringComparison.Ordinal);

        // Count ValidateDataAnnotations calls - should only be 1 (for unnamed)
        var validateDataAnnotationsCount = System.Text.RegularExpressions.Regex.Matches(generatedCode, "ValidateDataAnnotations").Count;
        Assert.Equal(1, validateDataAnnotationsCount);
    }
}