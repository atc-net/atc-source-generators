// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedVariable
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Tests.Generators.OptionsBinding;

public partial class OptionsBindingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_All_Four_Overloads_For_Transitive_Registration()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Options;

                              [OptionsBinding("App")]
                              public partial class AppOptions
                              {
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics);

        var generatedCode = GetGeneratedExtensionMethod(output);
        Assert.NotNull(generatedCode);

        // Check that all 4 overloads exist
        var overload1Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration\s*\)", RegexOptions.Multiline).Count;
        var overload2Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration,\s*bool includeReferencedAssemblies\s*\)", RegexOptions.Multiline).Count;
        var overload3Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration,\s*string referencedAssemblyName\s*\)", RegexOptions.Multiline).Count;
        var overload4Count = Regex.Matches(generatedCode, @"public static IServiceCollection AddOptionsFromTestAssembly\s*\(\s*this IServiceCollection services,\s*IConfiguration configuration,\s*params string\[\] referencedAssemblyNames\s*\)", RegexOptions.Multiline).Count;

        Assert.Equal(1, overload1Count);
        Assert.Equal(1, overload2Count);
        Assert.Equal(1, overload3Count);
        Assert.Equal(1, overload4Count);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Empty_If_Statement_When_No_Referenced_Assemblies()
    {
        // Arrange
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestApp.Options;

                              [OptionsBinding("TestSection")]
                              public partial class TestOptions
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

        // Verify that the overload with includeReferencedAssemblies parameter exists
        Assert.Contains("bool includeReferencedAssemblies", generatedCode, StringComparison.Ordinal);

        // Verify that there is NO empty if-statement in the generated code
        // The pattern we're looking for is an if-statement with only whitespace between the braces
        var emptyIfPattern = new Regex(@"if\s*\(\s*includeReferencedAssemblies\s*\)\s*\{\s*\}", RegexOptions.Multiline);
        Assert.False(emptyIfPattern.IsMatch(generatedCode), "Generated code should not contain an empty if-statement when there are no referenced assemblies");
    }
}