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

    [Fact]
    public void Config_Should_Use_Constructor_For_Positional_Record_With_Different_Types()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public enum SourceStatus { None, Active, Inactive }

                                  public class SourceItem
                                  {
                                      public string Id { get; set; } = string.Empty;
                                      public string Name { get; set; } = string.Empty;
                                      public SourceStatus Status { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public enum TargetStatus { Unknown, Active, Inactive }

                                  public sealed record TargetItem(string Id, string Name, TargetStatus Status);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetItem MapToTargetItem(this ExternalLib.SourceItem source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetItem(", output, StringComparison.Ordinal);
        Assert.DoesNotContain("new MyApp.TargetItem\n", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Use_Constructor_For_Positional_Record_With_Nested_Types()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceAddress
                                  {
                                      public string Street { get; set; } = string.Empty;
                                      public string City { get; set; } = string.Empty;
                                  }

                                  public class SourcePerson
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public SourceAddress Address { get; set; } = new();
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetAddress
                                  {
                                      public string Street { get; set; } = string.Empty;
                                      public string City { get; set; } = string.Empty;
                                  }

                                  public sealed record TargetPerson(string Name, TargetAddress Address);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetAddress MapToTargetAddress(this ExternalLib.SourceAddress source);
                                      public static partial MyApp.TargetPerson MapToTargetPerson(this ExternalLib.SourcePerson source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetPerson(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_Enum_Mapper_For_Collection_Element_Types()
    {
        const string source = """
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public enum FeatureType { None, Basic, Premium, Enterprise }

                                  public class SourceProduct
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public List<FeatureType> Features { get; set; } = new();
                                  }
                              }

                              namespace MyApp
                              {
                                  public enum FeatureType { Unknown, Basic, Premium, Enterprise }

                                  public class TargetProduct
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public List<FeatureType> Features { get; set; } = new();
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetProduct MapToTargetProduct(this ExternalLib.SourceProduct source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("x.MapToFeatureType()", output, StringComparison.Ordinal);
        Assert.Contains("MapToFeatureType(", output, StringComparison.Ordinal);
        Assert.Contains("this ExternalLib.FeatureType source)", output, StringComparison.Ordinal);
        Assert.Contains("=> source switch", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Use_Partial_Constructor_Match_For_Positional_Record()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceProduct
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public decimal Price { get; set; }
                                      public string? Description { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public sealed record TargetProduct(
                                      string Name,
                                      decimal Price,
                                      string? Description,
                                      string? ImageUrl,
                                      int StockCount);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetProduct MapToTargetProduct(this ExternalLib.SourceProduct source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetProduct(", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
        Assert.Contains("source.Price", output, StringComparison.Ordinal);
        Assert.Contains("source.Description", output, StringComparison.Ordinal);
        Assert.Contains("null", output, StringComparison.Ordinal);
        Assert.Contains("default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Not_Use_Partial_Match_When_Parameterless_Constructor_Exists()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceItem
                                  {
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetItem
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public string Extra { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetItem MapToTargetItem(this ExternalLib.SourceItem source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        // Should use object initializer, not constructor (class has parameterless constructor)
        Assert.Contains("new MyApp.TargetItem", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }
}