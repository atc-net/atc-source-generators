// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Add_ValidateDataAnnotations_When_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateDataAnnotations = true)]
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
    }

    [Fact]
    public void Generator_Should_Add_ValidateOnStart_When_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateOnStart = true)]
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
        Assert.Contains(".ValidateOnStart()", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Add_Both_Validation_Methods_When_Both_Specified()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
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
    }

    [Fact]
    public void Generator_Should_Place_Semicolon_On_Same_Line_As_Last_Method_Call()
    {
        // Arrange - Test with validation methods
        const string sourceWithValidation = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
                              public partial class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Arrange - Test without validation methods
        const string sourceWithoutValidation = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Cache")]
                              public partial class CacheOptions
                              {
                                  public int MaxSize { get; set; }
                              }
                              """;

        // Act
        var (diagnostics1, output1) = GetGeneratedOutput(sourceWithValidation);
        var (diagnostics2, output2) = GetGeneratedOutput(sourceWithoutValidation);

        // Assert
        Assert.Empty(diagnostics1);
        Assert.Empty(diagnostics2);

        var generatedCode1 = GetGeneratedExtensionMethod(output1);
        var generatedCode2 = GetGeneratedExtensionMethod(output2);

        Assert.NotNull(generatedCode1);
        Assert.NotNull(generatedCode2);

        // Verify semicolon is on the same line as the last method call (ValidateOnStart)
        Assert.Contains(".ValidateOnStart();", generatedCode1, StringComparison.Ordinal);

        // Verify semicolon is on the same line as PostConfigure (added for cache population)
        // Note: All unnamed options now get a PostConfigure for early access cache population
        Assert.Contains("            });", generatedCode2, StringComparison.Ordinal);

        // Verify there are NO standalone semicolons on a separate line
        Assert.DoesNotContain("            ;", generatedCode1, StringComparison.Ordinal);
        Assert.DoesNotContain("            ;", generatedCode2, StringComparison.Ordinal);
    }
}