// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Map_String_To_Uri()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public string Link { get; set; } = string.Empty;
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public Uri Link { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("new global::System.Uri(source.Link)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Uri_To_String()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public Uri Link { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public string Link { get; set; } = string.Empty;
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("source.Link.ToString()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Nullable_String_To_Nullable_Uri()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              #nullable enable

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public string? Link { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public Uri? Link { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("is not null ? new global::System.Uri(source.Link) : null", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Nullable_Uri_To_Nullable_String()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              #nullable enable

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public Uri? Link { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public string? Link { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("source.Link?.ToString()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_String_To_Enum()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public string Status { get; set; } = string.Empty;
                                  }
                              }

                              namespace TargetNs
                              {
                                  public enum StatusType
                                  {
                                      None,
                                      Active,
                                      Inactive,
                                  }

                                  public class Item
                                  {
                                      public StatusType Status { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Enum.TryParse<global::TargetNs.StatusType>(source.Status, out var __parsed)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Enum_To_String()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public enum StatusType
                                  {
                                      None,
                                      Active,
                                      Inactive,
                                  }

                                  public class Item
                                  {
                                      public StatusType Status { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public string Status { get; set; } = string.Empty;
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("source.Status.ToString()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Nullable_Enum_To_String()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public enum StatusType
                                  {
                                      None,
                                      Active,
                                      Inactive,
                                  }

                                  public class Item
                                  {
                                      public StatusType? Status { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public string Status { get; set; } = string.Empty;
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("source.Status?.ToString()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Decimal_To_Double()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public decimal Value { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public double Value { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("(double)source.Value", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Nullable_Decimal_To_Nullable_Double()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public decimal? Value { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Item
                                  {
                                      public double? Value { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("(double?)source.Value", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_SameShape_DifferentNamespace_Nested()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace NS1
                              {
                                  public class KeyValue
                                  {
                                      public string Key { get; set; } = string.Empty;
                                      public string Value { get; set; } = string.Empty;
                                  }

                                  public class Container
                                  {
                                      public KeyValue Item { get; set; }
                                  }
                              }

                              namespace NS2
                              {
                                  public class KeyValue
                                  {
                                      public string Key { get; set; } = string.Empty;
                                      public string Value { get; set; } = string.Empty;
                                  }

                                  public class Container
                                  {
                                      public KeyValue Item { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ContainerMappings
                                  {
                                      public static partial NS2.Container MapToContainer(this NS1.Container source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToKeyValue", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_SameShape_Collection()
    {
        const string source = """
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace NS1
                              {
                                  public class Item
                                  {
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Container
                                  {
                                      public List<Item> Items { get; set; } = new();
                                  }
                              }

                              namespace NS2
                              {
                                  public class Item
                                  {
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Container
                                  {
                                      public List<Item> Items { get; set; } = new();
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ContainerMappings
                                  {
                                      public static partial NS2.Container MapToContainer(this NS1.Container source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains(".Select(x => x.MapToItem()).ToList()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_Nested_Property_SubMethod()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Inner
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public int Count { get; set; }
                                  }

                                  public class Outer
                                  {
                                      public string Title { get; set; } = string.Empty;
                                      public Inner Detail { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Inner
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public int Count { get; set; }
                                  }

                                  public class Outer
                                  {
                                      public string Title { get; set; } = string.Empty;
                                      public Inner Detail { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class OuterMappings
                                  {
                                      public static partial TargetNs.Outer MapToOuter(this SourceNs.Outer source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should generate the top-level mapping
        Assert.Contains("MapToOuter", output, StringComparison.Ordinal);

        // Should generate a sub-method for the nested type
        Assert.Contains("MapToInner", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_DeeplyNested_SubMethods()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Price
                                  {
                                      public decimal Amount { get; set; }
                                      public string Currency { get; set; } = string.Empty;
                                  }

                                  public class PurchaseInfo
                                  {
                                      public string Vendor { get; set; } = string.Empty;
                                      public Price Cost { get; set; }
                                  }

                                  public class Order
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public PurchaseInfo Purchase { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public class Price
                                  {
                                      public decimal Amount { get; set; }
                                      public string Currency { get; set; } = string.Empty;
                                  }

                                  public class PurchaseInfo
                                  {
                                      public string Vendor { get; set; } = string.Empty;
                                      public Price Cost { get; set; }
                                  }

                                  public class Order
                                  {
                                      public string Name { get; set; } = string.Empty;
                                      public PurchaseInfo Purchase { get; set; }
                                  }
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class OrderMappings
                                  {
                                      public static partial TargetNs.Order MapToOrder(this SourceNs.Order source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should generate sub-methods for all levels
        Assert.Contains("MapToOrder", output, StringComparison.Ordinal);
        Assert.Contains("MapToPurchaseInfo", output, StringComparison.Ordinal);
        Assert.Contains("MapToPrice", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_String_To_Enum_In_RecordConstructor()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace SourceNs
                              {
                                  public class Item
                                  {
                                      public string Status { get; set; } = string.Empty;
                                      public decimal Value { get; set; }
                                  }
                              }

                              namespace TargetNs
                              {
                                  public enum StatusType
                                  {
                                      None,
                                      Active,
                                      Inactive,
                                  }

                                  public record Item(StatusType Status, double Value);
                              }

                              namespace Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ItemMappings
                                  {
                                      public static partial TargetNs.Item MapToItem(this SourceNs.Item source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should use constructor invocation with TryParse for enum and cast for decimal->double
        Assert.Contains("Enum.TryParse<global::TargetNs.StatusType>(source.Status, out var __parsed)", output, StringComparison.Ordinal);
        Assert.Contains("(double)source.Value", output, StringComparison.Ordinal);
    }
}