// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Register_Custom_Validator_When_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using Microsoft.Extensions.Options;

                              namespace MyApp.Configuration;

                              [OptionsBinding("ConnectionPool", ValidateDataAnnotations = true, Validator = typeof(ConnectionPoolOptionsValidator))]
                              public partial class ConnectionPoolOptions
                              {
                                  public int MinConnections { get; set; } = 1;
                                  public int MaxConnections { get; set; } = 10;
                              }

                              public class ConnectionPoolOptionsValidator : IValidateOptions<ConnectionPoolOptions>
                              {
                                  public ValidateOptionsResult Validate(string? name, ConnectionPoolOptions options)
                                  {
                                      if (options.MaxConnections <= options.MinConnections)
                                      {
                                          return ValidateOptionsResult.Fail("MaxConnections must be greater than MinConnections");
                                      }

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

        // Verify that the validator registration is generated
        Assert.Contains("services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Configuration.ConnectionPoolOptions>, global::MyApp.Configuration.ConnectionPoolOptionsValidator>();", generatedCode, StringComparison.Ordinal);

        // Verify that ValidateDataAnnotations is still present
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Register_Validator_When_Not_Specified()
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

        // Verify that no validator registration is generated
        Assert.DoesNotContain("IValidateOptions", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Validator_With_ValidateOnStart()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using Microsoft.Extensions.Options;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Storage", ValidateOnStart = true, Validator = typeof(StorageOptionsValidator))]
                              public partial class StorageOptions
                              {
                                  public string BasePath { get; set; } = string.Empty;
                                  public int MaxFileSize { get; set; }
                              }

                              public class StorageOptionsValidator : IValidateOptions<StorageOptions>
                              {
                                  public ValidateOptionsResult Validate(string? name, StorageOptions options)
                                  {
                                      if (string.IsNullOrWhiteSpace(options.BasePath))
                                      {
                                          return ValidateOptionsResult.Fail("BasePath is required");
                                      }

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

        // Verify validator registration appears after ValidateOnStart
        Assert.Contains(".ValidateOnStart();", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Configuration.StorageOptions>, global::MyApp.Configuration.StorageOptionsValidator>();", generatedCode, StringComparison.Ordinal);

        // Ensure validator is registered on a separate line after the semicolon
        var lines = generatedCode.Split('\n');
        var validateOnStartIndex = Array.FindIndex(lines, l => l.Contains(".ValidateOnStart();", StringComparison.Ordinal));
        var validatorIndex = Array.FindIndex(lines, l => l.Contains("IValidateOptions", StringComparison.Ordinal));

        Assert.True(validateOnStartIndex >= 0, "ValidateOnStart line not found");
        Assert.True(validatorIndex >= 0, "Validator registration line not found");
        Assert.True(validatorIndex > validateOnStartIndex, "Validator registration should be after ValidateOnStart");
    }

    [Fact]
    public void Generator_Should_Register_Multiple_Validators_For_Different_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using Microsoft.Extensions.Options;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", Validator = typeof(DatabaseOptionsValidator))]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }

                              [OptionsBinding("Cache", Validator = typeof(CacheOptionsValidator))]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }
                              }

                              public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
                              {
                                  public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
                                  {
                                      return ValidateOptionsResult.Success;
                                  }
                              }

                              public class CacheOptionsValidator : IValidateOptions<CacheOptions>
                              {
                                  public ValidateOptionsResult Validate(string? name, CacheOptions options)
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

        // Verify both validators are registered
        Assert.Contains("services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Configuration.DatabaseOptions>, global::MyApp.Configuration.DatabaseOptionsValidator>();", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Configuration.CacheOptions>, global::MyApp.Configuration.CacheOptionsValidator>();", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Validator_With_All_Validation_Options()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using Microsoft.Extensions.Options;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Email", ValidateDataAnnotations = true, ValidateOnStart = true, Validator = typeof(EmailOptionsValidator))]
                              public partial class EmailOptions
                              {
                                  public string SmtpServer { get; set; } = string.Empty;
                                  public int Port { get; set; }
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

        // Verify all validation methods are present
        Assert.Contains(".ValidateDataAnnotations()", generatedCode, StringComparison.Ordinal);
        Assert.Contains(".ValidateOnStart();", generatedCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Configuration.EmailOptions>, global::MyApp.Configuration.EmailOptionsValidator>();", generatedCode, StringComparison.Ordinal);

        // Verify the order: Bind → ValidateDataAnnotations → ValidateOnStart ; → Validator
        var bindIndex = generatedCode.IndexOf(".Bind(", StringComparison.Ordinal);
        var dataAnnotationsIndex = generatedCode.IndexOf(".ValidateDataAnnotations()", StringComparison.Ordinal);
        var onStartIndex = generatedCode.IndexOf(".ValidateOnStart();", StringComparison.Ordinal);
        var validatorIndex = generatedCode.IndexOf("IValidateOptions<global::MyApp.Configuration.EmailOptions>", StringComparison.Ordinal);

        Assert.True(bindIndex < dataAnnotationsIndex, "Bind should come before ValidateDataAnnotations");
        Assert.True(dataAnnotationsIndex < onStartIndex, "ValidateDataAnnotations should come before ValidateOnStart");
        Assert.True(onStartIndex < validatorIndex, "ValidateOnStart should come before validator registration");
    }

    [Fact]
    public void Generator_Should_Use_Fully_Qualified_Type_Name_For_Validator()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using Microsoft.Extensions.Options;

                              namespace MyApp.Configuration
                              {
                                  [OptionsBinding("Api", Validator = typeof(Validators.ApiOptionsValidator))]
                                  public partial class ApiOptions
                                  {
                                      public string BaseUrl { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Configuration.Validators
                              {
                                  public class ApiOptionsValidator : IValidateOptions<MyApp.Configuration.ApiOptions>
                                  {
                                      public ValidateOptionsResult Validate(string? name, MyApp.Configuration.ApiOptions options)
                                      {
                                          return ValidateOptionsResult.Success;
                                      }
                                  }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Verify fully qualified validator type name is used
        Assert.Contains("global::MyApp.Configuration.Validators.ApiOptionsValidator", generatedCode, StringComparison.Ordinal);
    }
}
