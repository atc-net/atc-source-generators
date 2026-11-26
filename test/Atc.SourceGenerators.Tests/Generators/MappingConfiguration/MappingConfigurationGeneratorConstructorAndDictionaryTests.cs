// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Constructor_With_Init_Properties_Should_Not_Emit_Trailing_Comma()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record Domain(string Name)
                                  {
                                      public IReadOnlyList<string> Tags { get; init; } = [];
                                  }

                                  public sealed record Api(string Name, List<string>? Tags = null);

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Domain MapToDomain(this Api source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("Tags = ", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Constructor_With_Several_Parameters_And_Init_Properties_Should_Compile()
    {
        const string source = """
                              #nullable enable
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record Domain(int Id, string Name)
                                  {
                                      public string? Description { get; init; }
                                      public int Order { get; init; }
                                  }

                                  public sealed class Api
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string? Description { get; set; }
                                      public int Order { get; set; }
                                  }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Domain MapToDomain(this Api source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Description = source.Description", output, StringComparison.Ordinal);
        Assert.Contains("Order = source.Order", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Dictionary_Property_Constructor_Parameter_Should_Map_Both_Ways()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record Domain(string Id, IReadOnlyDictionary<string, string>? Details);
                                  public sealed record Api(string Id, Dictionary<string, string>? Details = null);

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Api MapToApi(this Domain source);
                                      public static partial Domain MapToDomain(this Api source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("source.Details?.ToDictionary(x => x.Key, x => x.Value)", output, StringComparison.Ordinal);
        Assert.Contains("return new TestNamespace.Domain(", output, StringComparison.Ordinal);
        Assert.Contains("return new TestNamespace.Api(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Dictionary_Property_In_Object_Initializer_Should_Map()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed class Domain
                                  {
                                      public IDictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();
                                  }

                                  public sealed class Api
                                  {
                                      public IReadOnlyDictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();
                                  }

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
        Assert.Contains("Counts = source.Counts?.ToDictionary(x => x.Key, x => x.Value)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Same_Dictionary_Type_Should_Still_Map()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record Domain(string Id, Dictionary<string, string> Details);
                                  public sealed record Api(string Id, Dictionary<string, string> Details);

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Api MapToApi(this Domain source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("source.Details", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Incompatible_Property_Types_Should_Report_Unmapped_Property_Diagnostic()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed class Domain
                                  {
                                      public Dictionary<string, int> Details { get; set; } = new();
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

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMCF015");
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("Details", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}