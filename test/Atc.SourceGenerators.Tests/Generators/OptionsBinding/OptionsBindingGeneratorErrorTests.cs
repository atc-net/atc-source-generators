// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Report_Error_When_Class_Is_Not_Partial()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding("Database")]
                              public class DatabaseOptions
                              {
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT001", diagnostic.Id);
        Assert.Contains("must be declared as partial", diagnostic.GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Const_SectionName_Is_Empty()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class CacheOptions
                              {
                                  public const string SectionName = "";
                                  public int Size { get; set; }
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT003", diagnostic.Id);
        Assert.Contains("CacheOptions", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("SectionName", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Const_Name_Is_Empty()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class DatabaseOptions
                              {
                                  public const string Name = "";
                                  public string ConnectionString { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT003", diagnostic.Id);
        Assert.Contains("DatabaseOptions", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("Name", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Const_NameTitle_Is_Empty()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Configuration;

                              [OptionsBinding]
                              public partial class ApiOptions
                              {
                                  public const string NameTitle = "";
                                  public string BaseUrl { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("ATCOPT003", diagnostic.Id);
        Assert.Contains("ApiOptions", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("NameTitle", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}