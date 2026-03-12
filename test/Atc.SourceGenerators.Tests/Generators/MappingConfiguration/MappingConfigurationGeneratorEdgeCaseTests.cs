// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Handle_Empty_Configuration_Class()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class EmptyMappings
                                  {
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        // Should emit info diagnostic
        Assert.Contains(diagnostics, d => d.Id == "ATCMCF008");
    }

    [Fact]
    public void Config_Should_Handle_Multiple_Config_Classes()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Contact { public int Id { get; set; } }
                                  public class Order { public int Id { get; set; } }
                              }

                              namespace MyApp
                              {
                                  public class Customer { public int Id { get; set; } }
                                  public class OrderDto { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class CrmMappings
                                  {
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }

                                  [MappingConfiguration]
                                  public static partial class OrderMappings
                                  {
                                      public static partial MyApp.OrderDto MapToOrderDto(this ExternalLib.Order source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("MapToOrderDto", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Source_And_Target_In_Different_Namespaces()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Vendor.SDK.Models
                              {
                                  public class ApiResponse
                                  {
                                      public int StatusCode { get; set; }
                                      public string Message { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Domain.Models
                              {
                                  public class ServiceResult
                                  {
                                      public int StatusCode { get; set; }
                                      public string Message { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Infrastructure
                              {
                                  [MappingConfiguration]
                                  public static partial class VendorMappings
                                  {
                                      public static partial MyApp.Domain.Models.ServiceResult MapToServiceResult(
                                          this Vendor.SDK.Models.ApiResponse source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("using MyApp.Domain.Models;", output, StringComparison.Ordinal);
        Assert.Contains("using Vendor.SDK.Models;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Types_With_No_Matching_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source
                                  {
                                      public int Foo { get; set; }
                                      public string Bar { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Target
                                  {
                                      public int Baz { get; set; }
                                      public string Qux { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should still generate the method, but with empty initializer
        Assert.Contains("MapToTarget", output, StringComparison.Ordinal);
        Assert.Contains("new MyApp.Target", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Not_Generate_When_Attribute_Missing()
    {
        const string source = """
                              namespace MyApp.Mappings
                              {
                                  public static partial class NotAMappingClass
                                  {
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("NotAMappingClass", output, StringComparison.Ordinal);
    }
}