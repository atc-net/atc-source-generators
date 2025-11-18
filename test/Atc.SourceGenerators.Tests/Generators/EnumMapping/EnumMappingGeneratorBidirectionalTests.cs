namespace Atc.SourceGenerators.Tests.Generators.EnumMapping;

public partial class EnumMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Bidirectional_Mapping_When_Bidirectional_Is_True()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Available,
                                  Pending,
                              }

                              [MapTo(typeof(TargetStatus), Bidirectional = true)]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: SourceStatus.MapToTargetStatus()
        Assert.Contains("MapToTargetStatus", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetStatus MapToTargetStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.SourceStatus source)", output, StringComparison.Ordinal);
        Assert.Contains("=> source switch", output, StringComparison.Ordinal);

        // Reverse mapping: TargetStatus.MapToSourceStatus()
        Assert.Contains("MapToSourceStatus", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.SourceStatus MapToSourceStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.TargetStatus source)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Reverse_Mapping_When_Bidirectional_Is_False()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Available,
                                  Pending,
                              }

                              [MapTo(typeof(TargetStatus), Bidirectional = false)]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: SourceStatus.MapToTargetStatus() - should exist
        Assert.Contains("MapToTargetStatus", output, StringComparison.Ordinal);

        // Reverse mapping: TargetStatus.MapToSourceStatus() - should NOT exist
        Assert.DoesNotContain("MapToSourceStatus", output, StringComparison.Ordinal);
    }
}