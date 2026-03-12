// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void ClassLevel_Should_Generate_Mapping_From_MapTypes_On_Class()
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

                                  [MapTypes(typeof(ExternalLib.Contact), typeof(Customer))]
                                  static class Mappings;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("ConfiguredMappingExtensions", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ClassLevel_Should_Support_Bidirectional()
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

                                  [MapTypes(typeof(ExternalLib.Contact), typeof(Customer), Bidirectional = true)]
                                  static class Mappings;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("MapToContact", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ClassLevel_Should_Support_PropertyMap()
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

                                  [MapTypes(
                                      typeof(ExternalLib.Contact),
                                      typeof(Customer),
                                      PropertyMap = new[] { "FullName:DisplayName", "EmailAddress:Email" })]
                                  static class Mappings;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("DisplayName = source.FullName", output, StringComparison.Ordinal);
        Assert.Contains("Email = source.EmailAddress", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ClassLevel_Should_Support_Multiple_MapTypes_On_Same_Class()
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

                                  public class Address
                                  {
                                      public string Street { get; set; } = string.Empty;
                                      public string City { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class CustomerAddress
                                  {
                                      public string Street { get; set; } = string.Empty;
                                      public string City { get; set; } = string.Empty;
                                  }

                                  [MapTypes(typeof(ExternalLib.Contact), typeof(Customer))]
                                  [MapTypes(typeof(ExternalLib.Address), typeof(CustomerAddress))]
                                  static class Mappings;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("MapToCustomerAddress", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ClassLevel_Should_Mix_With_Assembly_Level()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(ExternalLib.Address), typeof(MyApp.CustomerAddress))]

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Address
                                  {
                                      public string Street { get; set; } = string.Empty;
                                      public string City { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class CustomerAddress
                                  {
                                      public string Street { get; set; } = string.Empty;
                                      public string City { get; set; } = string.Empty;
                                  }

                                  [MapTypes(typeof(ExternalLib.Contact), typeof(Customer))]
                                  static class Mappings;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("MapToCustomerAddress", output, StringComparison.Ordinal);
    }
}