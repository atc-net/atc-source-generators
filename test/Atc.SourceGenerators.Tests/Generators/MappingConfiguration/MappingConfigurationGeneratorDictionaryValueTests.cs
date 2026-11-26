// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Dictionary_With_Nested_Value_Types_Should_Map_Values()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record DomainItem(string Name, int Count);
                                  public sealed record ApiItem(string Name, int Count);

                                  public sealed class Domain
                                  {
                                      public IReadOnlyDictionary<string, DomainItem> Items { get; set; } = new Dictionary<string, DomainItem>();
                                  }

                                  public sealed class Api
                                  {
                                      public Dictionary<string, ApiItem> Items { get; set; } = new();
                                  }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Api MapToApi(this Domain source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Items = source.Items?.ToDictionary(x => x.Key, x => MapToApiItem(x.Value))", output, StringComparison.Ordinal);
        Assert.Contains("private static global::TestNamespace.ApiItem MapToApiItem(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Dictionary_With_Nested_Value_Types_As_Constructor_Parameters_Should_Map_Both_Ways()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record DomainItem(string Name);
                                  public sealed record ApiItem(string Name);

                                  public sealed record Domain(string Id, IReadOnlyDictionary<string, DomainItem>? Items);
                                  public sealed record Api(string Id, Dictionary<string, ApiItem>? Items = null);

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigOptions(Bidirectional = true)]
                                      public static partial Api MapToApi(this Domain source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToApiItem(x.Value)", output, StringComparison.Ordinal);
        Assert.Contains("MapToDomainItem(x.Value)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Dictionary_With_Enum_Value_Types_Should_Map_Values()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public enum DomainStatus { None, Active, Inactive }
                                  public enum ApiStatus { Unknown, Active, Inactive }

                                  public sealed class Domain
                                  {
                                      public Dictionary<string, DomainStatus> Statuses { get; set; } = new();
                                  }

                                  public sealed class Api
                                  {
                                      public IDictionary<string, ApiStatus> Statuses { get; set; } = new Dictionary<string, ApiStatus>();
                                  }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Api MapToApi(this Domain source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Statuses = source.Statuses?.ToDictionary(x => x.Key, x => x.Value.MapToApiStatus())", output, StringComparison.Ordinal);
        Assert.Contains("MapToApiStatus(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Dictionary_With_Different_Key_Types_Should_Report_Unmapped_Property_Diagnostic()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed class Domain
                                  {
                                      public Dictionary<int, string> Details { get; set; } = new();
                                  }

                                  public sealed class Api
                                  {
                                      public Dictionary<string, string> Details { get; set; } = new();
                                  }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Api MapToApi(this Domain source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.Single(diagnostics, d => d.Id == "ATCMCF015");
    }

    [Fact]
    public void Config_Bidirectional_Collection_Of_Nested_Types_Should_Generate_Reverse_Element_Mapper()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record DomainItem(string Name);
                                  public sealed record ApiItem(string Name);

                                  public sealed record Domain(string Id, List<DomainItem> Items);
                                  public sealed record Api(string Id, List<ApiItem> Items);

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigOptions(Bidirectional = true)]
                                      public static partial Api MapToApi(this Domain source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToDomainItem(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Assembly_MapTypes_Should_Map_Types_In_A_Root_Mapping_Namespace()
    {
        // Assembly-level mappings are generated in namespace Atc.Mapping, so an unqualified
        // "Mapping.Models.X" would resolve to Atc.Mapping.Models.X
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(Mapping.Models.Source), typeof(Mapping.Models.TargetDto), Bidirectional = true)]

                              namespace Mapping.Models
                              {
                                  public enum StatusDto { Active }

                                  public enum Status { Active }

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
                                      public int Id { get; set; }

                                      public StatusDto Status { get; set; }

                                      public System.Collections.Generic.List<ItemDto> Items { get; set; } = new();
                                  }

                                  public class Source
                                  {
                                      public int Id { get; set; }

                                      public Status Status { get; set; }

                                      public System.Collections.Generic.List<Item> Items { get; set; } = new();
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }
}