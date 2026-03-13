// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Ignore_Properties_With_MapConfigIgnore()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string InternalId { get; set; } = string.Empty;
                                      public string PasswordHash { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string InternalId { get; set; } = string.Empty;
                                      public string PasswordHash { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigIgnore("InternalId")]
                                      [MapConfigIgnore("PasswordHash")]
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.DoesNotContain("InternalId = source.InternalId", output, StringComparison.Ordinal);
        Assert.DoesNotContain("PasswordHash = source.PasswordHash", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Rename_Properties_With_MapConfigProperty()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string FullName { get; set; } = string.Empty;
                                      public string EmailAddress { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string DisplayName { get; set; } = string.Empty;
                                      public string Email { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigProperty("FullName", "DisplayName")]
                                      [MapConfigProperty("EmailAddress", "Email")]
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("DisplayName = source.FullName", output, StringComparison.Ordinal);
        Assert.Contains("Email = source.EmailAddress", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Base_Class_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public abstract class BaseEntity
                                  {
                                      public int Id { get; set; }
                                      public System.DateTime CreatedAt { get; set; }
                                  }

                                  public class Contact : BaseEntity
                                  {
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public System.DateTime CreatedAt { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("CreatedAt = source.CreatedAt", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Apply_CamelCase_PropertyNameStrategy()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string FirstName { get; set; } = string.Empty;
                                      public string LastName { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int id { get; set; }
                                      public string firstName { get; set; } = string.Empty;
                                      public string lastName { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigOptions(PropertyNameStrategy = PropertyNameStrategy.CamelCase)]
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // CamelCase strategy converts source PascalCase names to camelCase for target matching
        Assert.Contains("id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("firstName = source.FirstName", output, StringComparison.Ordinal);
        Assert.Contains("lastName = source.LastName", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Collection_Property_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;
                              using System.Collections.Generic;

                              namespace ExternalLib
                              {
                                  public class OrderItem
                                  {
                                      public int Id { get; set; }
                                      public string ProductName { get; set; } = string.Empty;
                                  }

                                  public class Order
                                  {
                                      public int Id { get; set; }
                                      public List<OrderItem> Items { get; set; } = new();
                                  }
                              }

                              namespace MyApp
                              {
                                  public class OrderItemDto
                                  {
                                      public int Id { get; set; }
                                      public string ProductName { get; set; } = string.Empty;
                                  }

                                  public class OrderDto
                                  {
                                      public int Id { get; set; }
                                      public List<OrderItemDto> Items { get; set; } = new();
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.OrderItemDto MapToOrderItemDto(this ExternalLib.OrderItem source);
                                      public static partial MyApp.OrderDto MapToOrderDto(this ExternalLib.Order source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should generate collection mapping with Select and ToList
        Assert.Contains("MapToOrderDto", output, StringComparison.Ordinal);
        Assert.Contains("MapToOrderItemDto", output, StringComparison.Ordinal);
        Assert.Contains(".Select(x => x.MapToOrderItemDto()).ToList()!", output, StringComparison.Ordinal);
    }
}