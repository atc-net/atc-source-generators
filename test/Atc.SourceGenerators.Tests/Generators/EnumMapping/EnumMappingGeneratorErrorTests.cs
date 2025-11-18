namespace Atc.SourceGenerators.Tests.Generators.EnumMapping;

public partial class EnumMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Report_Warning_For_Unmapped_Values()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Available,
                                  Pending,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                                  Adopted,
                                  Deleted,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Should have warnings for unmapped values
        Assert.NotEmpty(diagnostics);
        var adoptedWarning = Assert.Single(
            diagnostics,
            d => d.Id == "ATCENUM002" &&
                 d
                     .GetMessage(CultureInfo.InvariantCulture)
                     .Contains("Adopted", StringComparison.Ordinal));
        Assert.Equal(DiagnosticSeverity.Warning, adoptedWarning.Severity);

        var deletedWarning = Assert.Single(
            diagnostics,
            d => d.Id == "ATCENUM002" &&
                 d
                     .GetMessage(CultureInfo.InvariantCulture)
                     .Contains("Deleted", StringComparison.Ordinal));
        Assert.Equal(DiagnosticSeverity.Warning, deletedWarning.Severity);

        // Generated code should still work for mapped values
        Assert.Contains("TestNamespace.SourceStatus.Available => TestNamespace.TargetStatus.Available,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Pending => TestNamespace.TargetStatus.Pending,", output, StringComparison.Ordinal);

        // Unmapped values should not appear in switch expression
        Assert.DoesNotContain("SourceStatus.Adopted =>", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourceStatus.Deleted =>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Target_Is_Not_Enum()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCENUM001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("must be an enum type", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}