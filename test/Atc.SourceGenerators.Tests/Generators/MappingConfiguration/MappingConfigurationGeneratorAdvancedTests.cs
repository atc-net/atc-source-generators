// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Generate_Bidirectional_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigOptions(Bidirectional = true)]
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("MapToContact", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Constructor_Mapping_For_Records()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Transaction
                                  {
                                      public string Id { get; set; } = string.Empty;
                                      public decimal Amount { get; set; }
                                      public string Currency { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public record Payment(string Id, decimal Amount, string Currency);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Payment MapToPayment(this ExternalLib.Transaction source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.Payment(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_Collection_List_Mapping()
    {
        const string source = """
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceItem
                                  {
                                      public int Id { get; set; }
                                      public string Value { get; set; } = string.Empty;
                                  }

                                  public class SourceContainer
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public List<SourceItem> Items { get; set; } = new();
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetItem
                                  {
                                      public int Id { get; set; }
                                      public string Value { get; set; } = string.Empty;
                                  }

                                  public class TargetContainer
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public List<TargetItem> Items { get; set; } = new();
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetContainer MapToTargetContainer(this ExternalLib.SourceContainer source);
                                      public static partial MyApp.TargetItem MapToTargetItem(this ExternalLib.SourceItem source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetContainer", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetItem", output, StringComparison.Ordinal);
        Assert.Contains(".Select(x => x.MapToTargetItem()).ToList()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_Collection_Array_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceItem
                                  {
                                      public int Id { get; set; }
                                      public string Value { get; set; } = string.Empty;
                                  }

                                  public class SourceContainer
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public SourceItem[] Items { get; set; } = System.Array.Empty<SourceItem>();
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetItem
                                  {
                                      public int Id { get; set; }
                                      public string Value { get; set; } = string.Empty;
                                  }

                                  public class TargetContainer
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public TargetItem[] Items { get; set; } = System.Array.Empty<TargetItem>();
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetContainer MapToTargetContainer(this ExternalLib.SourceContainer source);
                                      public static partial MyApp.TargetItem MapToTargetItem(this ExternalLib.SourceItem source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetContainer", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetItem", output, StringComparison.Ordinal);
        Assert.Contains(".Select(x => x.MapToTargetItem()).ToArray()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Type_Conversions()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceRecord
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public bool IsActive { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetRecord
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public bool IsActive { get; set; }
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetRecord MapToTargetRecord(this ExternalLib.SourceRecord source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetRecord", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.Contains("IsActive = source.IsActive", output, StringComparison.Ordinal);
    }
}