// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Simple_Mapping()
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
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.Source source)", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Nested_Object_And_Enum_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum SourceStatus { Active = 0, Inactive = 1 }
                              public enum TargetStatus { Active = 0, Inactive = 1 }

                              public class TargetAddress
                              {
                                  public string Street { get; set; } = string.Empty;
                                  public string City { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public TargetStatus Status { get; set; }
                                  public TargetAddress? Address { get; set; }
                              }

                              [MapTo(typeof(TargetAddress))]
                              public partial class SourceAddress
                              {
                                  public string Street { get; set; } = string.Empty;
                                  public string City { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public SourceStatus Status { get; set; }
                                  public SourceAddress? Address { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetAddress", output, StringComparison.Ordinal);
        Assert.Contains("(TestNamespace.TargetStatus)source.Status", output, StringComparison.Ordinal);
        Assert.Contains("source.Address?.MapToTargetAddress()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Class_Is_Not_Partial()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public class Source
                              {
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Generate_Bidirectional_Mapping_When_Bidirectional_Is_True()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto), Bidirectional = true)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: Source.MapToTargetDto()
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.Source source)", output, StringComparison.Ordinal);

        // Reverse mapping: TargetDto.MapToSource()
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.Source MapToSource(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.TargetDto source)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Reverse_Mapping_When_Bidirectional_Is_False()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto), Bidirectional = false)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: Source.MapToTargetDto() - should exist
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Reverse mapping: TargetDto.MapToSource() - should NOT exist
        Assert.DoesNotContain("MapToSource", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Enum_Mapping_Method_When_Enum_Has_MapTo_Attribute()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus { Active = 0, Inactive = 1 }

                              public enum TargetStatus { Active = 0, Inactive = 1 }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public TargetStatus Status { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public SourceStatus Status { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should use enum mapping method instead of cast
        Assert.Contains("Status = source.Status.MapToTargetStatus()", output, StringComparison.Ordinal);
        Assert.DoesNotContain("(TestNamespace.TargetStatus)source.Status", output, StringComparison.Ordinal);
    }
}