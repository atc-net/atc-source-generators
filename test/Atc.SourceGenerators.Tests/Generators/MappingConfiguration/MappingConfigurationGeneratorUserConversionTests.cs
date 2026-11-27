// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Use_User_Conversion_Method_For_Property_Types()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public sealed record Money(decimal Amount);

                                  public class Source
                                  {
                                      public int Id { get; set; }

                                      public decimal Price { get; set; }
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }

                                      public Money Price { get; set; } = new(0);
                                  }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target MapToTarget(this Source source);

                                      private static Money ToMoney(decimal amount) => new(amount);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Price = ToMoney(source.Price)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Prefer_User_Conversion_Over_Built_In_Conversion()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public string Created { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public DateTime Created { get; set; }
                                  }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target MapToTarget(this Source source);

                                      internal static DateTime ParseDanishDate(string value)
                                          => DateTime.ParseExact(value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Created = ParseDanishDate(source.Created)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Use_User_Conversion_Method_For_Constructor_Parameters()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public sealed record CustomerId(string Value);

                                  public sealed record Source(string Id, string Name);

                                  public sealed record Target(CustomerId Id, string Name);

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target MapToTarget(this Source source);

                                      private static CustomerId ToCustomerId(string value) => new(value);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("ToCustomerId(source.Id)", output, StringComparison.Ordinal);
        Assert.Contains("return new global::MyApp.Target(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Use_User_Conversion_Method_In_Nested_Mappers()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public sealed record Money(decimal Amount);

                                  public class Line { public decimal Price { get; set; } }

                                  public class LineDto { public Money Price { get; set; } = new(0); }

                                  public class Order { public Line Line { get; set; } = new(); }

                                  public class OrderDto { public LineDto Line { get; set; } = new(); }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial OrderDto MapToOrderDto(this Order source);

                                      private static Money ToMoney(decimal amount) => new(amount);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Price = ToMoney(source.Price)", output, StringComparison.Ordinal);
    }
}