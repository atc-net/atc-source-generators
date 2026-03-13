// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Report_Error_When_Class_Not_Partial()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto))]
                              public class NonPartialSource
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Target_Is_Interface()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public interface ITarget
                              {
                                  int Id { get; set; }
                              }

                              [MapTo(typeof(ITarget))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP002");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Target_Is_Enum()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetEnum { A, B, C }

                              [MapTo(typeof(TargetEnum))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP002");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_MapProperty_References_Nonexistent_Target_Property()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }

                                  [MapProperty("NonExistentProperty")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => d.Id == "ATCMAP003");
    }

    [Fact]
    public void Generator_Should_Not_Generate_Mapping_When_Class_Not_Partial()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public class NonPartialSource
                              {
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCMAP001");
        Assert.DoesNotContain("MapToTargetDto", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Mapping_When_Target_Is_Interface()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public interface INotAClass
                              {
                                  int Id { get; set; }
                              }

                              [MapTo(typeof(INotAClass))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCMAP002");
        Assert.DoesNotContain("MapToINotAClass", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_For_Each_Invalid_MapTo_Target()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public interface ITarget1
                              {
                                  int Id { get; set; }
                              }

                              public interface ITarget2
                              {
                                  string Name { get; set; }
                              }

                              [MapTo(typeof(ITarget1))]
                              [MapTo(typeof(ITarget2))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        var targetErrors = diagnostics.Where(d => d.Id == "ATCMAP002").ToList();
        Assert.True(targetErrors.Count >= 2, $"Expected at least 2 ATCMAP002 errors but found {targetErrors.Count}");
    }
}