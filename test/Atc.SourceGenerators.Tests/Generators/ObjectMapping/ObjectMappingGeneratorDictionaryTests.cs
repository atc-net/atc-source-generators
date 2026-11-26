// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Copy_Dictionary_With_Same_Key_And_Value_Types()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public Dictionary<string, string>? Details { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public IReadOnlyDictionary<string, string>? Details { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Details = source.Details?.ToDictionary(x => x.Key, x => x.Value)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Dictionary_Constructor_Parameter_Of_Record()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(string Id, IReadOnlyDictionary<string, string>? Details);

                              [MapTo(typeof(TargetDto))]
                              public partial record Source(string Id, Dictionary<string, string>? Details);
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("source.Details?.ToDictionary(x => x.Key, x => x.Value)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Dictionary_Values_With_MapTo()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ItemDto
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(ItemDto))]
                              public partial class Item
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public Dictionary<string, ItemDto> Items { get; set; } = new();
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public Dictionary<string, Item> Items { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Items = source.Items?.ToDictionary(x => x.Key, x => x.Value.MapToItemDto())", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Dictionary_Enum_Values()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum StatusDto { Active, Inactive }

                              [MapTo(typeof(StatusDto))]
                              public enum Status { Active, Inactive }

                              public enum KindDto { A, B }

                              public enum Kind { A, B }

                              public class TargetDto
                              {
                                  public Dictionary<string, StatusDto> Statuses { get; set; } = new();
                                  public Dictionary<string, KindDto> Kinds { get; set; } = new();
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public Dictionary<string, Status> Statuses { get; set; } = new();
                                  public Dictionary<string, Kind> Kinds { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Statuses = source.Statuses?.ToDictionary(x => x.Key, x => x.Value.MapToStatusDto())", output, StringComparison.Ordinal);
        Assert.Contains("Kinds = source.Kinds?.ToDictionary(x => x.Key, x => (global::TestNamespace.KindDto)x.Value)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Diagnostic_For_Dictionary_Values_Without_Mapping()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ItemDto
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              public class Item
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public Dictionary<string, ItemDto> Items { get; set; } = new();
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public Dictionary<string, Item> Items { get; set; } = new();
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.Single(diagnostics, d => d.Id == "ATCMAP011");
    }

    [Fact]
    public void Generator_Should_Exclude_Dictionary_From_Projection()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public Dictionary<string, string>? Details { get; set; }
                              }

                              [MapTo(typeof(TargetDto), GenerateProjection = true)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public IReadOnlyDictionary<string, string>? Details { get; set; }
                              }
                              """;

        // The test compilation references the assemblies loaded in the AppDomain; make sure the
        // projection's Expression<T> type is available even when this test runs on its own
        Assert.NotNull(typeof(System.Linq.Expressions.Expression).Assembly.Location);

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        // The regular mapping copies the dictionary; the projection (an expression tree for IQueryable) leaves it out
        Assert.Contains("Details = source.Details?.ToDictionary(x => x.Key, x => x.Value)", output, StringComparison.Ordinal);
        var projection = output.Substring(output.IndexOf("ProjectToTargetDto()", StringComparison.Ordinal));
        projection = projection.Substring(0, projection.IndexOf("};", StringComparison.Ordinal));
        Assert.Contains("Id = source.Id", projection, StringComparison.Ordinal);
        Assert.DoesNotContain("Details", projection, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Dictionary_In_UpdateTarget()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public Dictionary<string, string>? Details { get; set; }
                              }

                              [MapTo(typeof(TargetDto), UpdateTarget = true)]
                              public partial class Source
                              {
                                  public IReadOnlyDictionary<string, string>? Details { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("target.Details = source.Details?.ToDictionary(x => x.Key, x => x.Value)", output, StringComparison.Ordinal);
    }
}