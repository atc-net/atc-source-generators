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

    [Fact]
    public void Config_Should_Handle_Numeric_Type_Width_Conversion_In_Constructor()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceProject
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public uint Version { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public sealed record TargetProject(string Name, int Version);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetProject MapToTargetProject(this ExternalLib.SourceProject source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetProject(", output, StringComparison.Ordinal);
        Assert.Contains("(int)source.Version", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Multiple_Numeric_Conversions_In_Constructor()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceDevice
                                  {
                                      public string Id { get; set; } = string.Empty;
                                      public ushort Address { get; set; }
                                      public uint Port { get; set; }
                                      public long Timestamp { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public sealed record TargetDevice(string Id, int Address, int Port, int Timestamp);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetDevice MapToTargetDevice(this ExternalLib.SourceDevice source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetDevice(", output, StringComparison.Ordinal);
        Assert.Contains("(int)source.Address", output, StringComparison.Ordinal);
        Assert.Contains("(int)source.Port", output, StringComparison.Ordinal);
        Assert.Contains("(int)source.Timestamp", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Numeric_Conversion_With_Object_Initializer()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceData
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public uint Count { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetData
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public int Count { get; set; }
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetData MapToTargetData(this ExternalLib.SourceData source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Count = (int)source.Count", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Use_Constructor_For_Record_With_Collection_Of_Different_Element_Types()
    {
        const string source = """
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceHeader
                                  {
                                      public string Title { get; set; } = string.Empty;
                                  }

                                  public class SourceProject
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public IList<SourceHeader> Headers { get; set; } = new List<SourceHeader>();
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetHeader
                                  {
                                      public string Title { get; set; } = string.Empty;
                                  }

                                  public sealed record TargetProject(string Name, List<TargetHeader> Headers);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetHeader MapToTargetHeader(this ExternalLib.SourceHeader source);
                                      public static partial MyApp.TargetProject MapToTargetProject(this ExternalLib.SourceProject source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetProject(", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetHeader", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Use_Constructor_For_Record_With_Mixed_Collections_And_Numeric_Conversions()
    {
        const string source = """
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceItem
                                  {
                                      public string Id { get; set; } = string.Empty;
                                  }

                                  public class SourceContainer
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public uint Version { get; set; }
                                      public IList<SourceItem> Items { get; set; } = new List<SourceItem>();
                                  }
                              }

                              namespace MyApp
                              {
                                  public class TargetItem
                                  {
                                      public string Id { get; set; } = string.Empty;
                                  }

                                  public sealed record TargetContainer(string Name, int Version, List<TargetItem> Items);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetItem MapToTargetItem(this ExternalLib.SourceItem source);
                                      public static partial MyApp.TargetContainer MapToTargetContainer(this ExternalLib.SourceContainer source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetContainer(", output, StringComparison.Ordinal);
        Assert.Contains("(int)source.Version", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetItem", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Emit_All_Constructor_Args_When_Source_Has_Default_Values()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class SourceGridSettings
                                  {
                                      public bool Show { get; set; }
                                      public double DefaultThickness { get; set; }
                                  }

                                  public sealed record SourceDesignerSettings(
                                      SourceGridSettings GridLines,
                                      int SizeHorizontal = 300,
                                      int SizeVertical = 300,
                                      int GridCountHorizontal = 6,
                                      int GridCountVertical = 6,
                                      string BackgroundColorInHex = "#FF333333");
                              }

                              namespace MyApp
                              {
                                  public class TargetGridSettings
                                  {
                                      public bool Show { get; set; }
                                      public double DefaultThickness { get; set; }
                                  }

                                  public record TargetDesignerSettings(
                                      TargetGridSettings GridLines,
                                      int SizeHorizontal,
                                      int SizeVertical,
                                      int GridCountHorizontal,
                                      int GridCountVertical,
                                      string BackgroundColorInHex);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetGridSettings MapToTargetGridSettings(this ExternalLib.SourceGridSettings source);
                                      public static partial MyApp.TargetDesignerSettings MapToTargetDesignerSettings(this ExternalLib.SourceDesignerSettings source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new MyApp.TargetDesignerSettings(", output, StringComparison.Ordinal);
        Assert.Contains("source.SizeHorizontal", output, StringComparison.Ordinal);
        Assert.Contains("source.SizeVertical", output, StringComparison.Ordinal);
        Assert.Contains("source.GridCountHorizontal", output, StringComparison.Ordinal);
        Assert.Contains("source.GridCountVertical", output, StringComparison.Ordinal);
        Assert.Contains("source.BackgroundColorInHex", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Not_Use_Null_Conditional_On_Value_Type_Nested_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public struct SourcePoint
                                  {
                                      public double X { get; set; }
                                      public double Y { get; set; }
                                  }

                                  public class SourceShape
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public SourcePoint Location { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public struct TargetPoint
                                  {
                                      public double X { get; set; }
                                      public double Y { get; set; }
                                  }

                                  public class TargetShape
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public TargetPoint Location { get; set; }
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.TargetPoint MapToTargetPoint(this ExternalLib.SourcePoint source);
                                      public static partial MyApp.TargetShape MapToTargetShape(this ExternalLib.SourceShape source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("source.Location.MapToTargetPoint()", output, StringComparison.Ordinal);
        Assert.DoesNotContain("source.Location?.MapToTargetPoint()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Bug6_Source_With_Defaults_To_Target_Without_Defaults()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Source
                              {
                                  public sealed record SettingsWithDefaults(
                                      string Name,
                                      int Width = 100,
                                      int Height = 200,
                                      string Color = "#FFF");
                              }

                              namespace Target
                              {
                                  public record SettingsNoDefaults(
                                      string Name,
                                      int Width,
                                      int Height,
                                      string Color);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target.SettingsNoDefaults MapToSettingsNoDefaults(
                                          this Source.SettingsWithDefaults source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new Target.SettingsNoDefaults(", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
        Assert.Contains("source.Width", output, StringComparison.Ordinal);
        Assert.Contains("source.Height", output, StringComparison.Ordinal);
        Assert.Contains("source.Color", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Bug8a_Ref_To_Value_Type_Nested_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Source
                              {
                                  public sealed record PositionDto(double X, double Y);
                                  public sealed record ItemDto(string Name, PositionDto Location);
                              }

                              namespace Target
                              {
                                  public readonly struct Position
                                  {
                                      public Position(double x, double y) { X = x; Y = y; }
                                      public double X { get; }
                                      public double Y { get; }
                                  }

                                  public record Item(string Name, Position Location);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target.Position MapToPosition(this Source.PositionDto source);
                                      public static partial Target.Item MapToItem(this Source.ItemDto source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        // Should NOT use ?. when target type is value type
        Assert.DoesNotContain("?.MapToPosition()!", output, StringComparison.Ordinal);
        // Should use ternary or direct call pattern
        Assert.Contains("MapToPosition()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Bug8b_No_Null_Guard_For_Struct_Source()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Source
                              {
                                  public readonly struct Coordinate
                                  {
                                      public Coordinate(int x, int y) { X = x; Y = y; }
                                      public int X { get; }
                                      public int Y { get; }
                                  }
                              }

                              namespace Target
                              {
                                  public sealed record CoordinateDto(int X, int Y);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target.CoordinateDto MapToCoordinateDto(
                                          this Source.Coordinate source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new Target.CoordinateDto(", output, StringComparison.Ordinal);
        // Should NOT have null guard for struct input
        Assert.DoesNotContain("if (source is null)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Bug8c_MapConfigProperty_With_Constructor_Uses_Named_Args()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Source
                              {
                                  public readonly struct GridPosition
                                  {
                                      public GridPosition(int x, int y) { X = x; Y = y; }
                                      public int X { get; }
                                      public int Y { get; }
                                  }
                              }

                              namespace Target
                              {
                                  public sealed record GridPositionDto(int Row, int Column);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigProperty("X", "Column")]
                                      [MapConfigProperty("Y", "Row")]
                                      public static partial Target.GridPositionDto MapToGridPositionDto(
                                          this Source.GridPosition source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        // Should use constructor syntax, not object initializer
        Assert.Contains("new Target.GridPositionDto(", output, StringComparison.Ordinal);
        // Should NOT use object initializer
        Assert.DoesNotContain("new Target.GridPositionDto\n", output, StringComparison.Ordinal);
    }
}