// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
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
}