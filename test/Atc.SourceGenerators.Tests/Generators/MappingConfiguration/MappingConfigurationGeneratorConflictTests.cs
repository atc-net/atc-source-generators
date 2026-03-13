// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Emit_Warning_When_Attribute_And_Config_Both_Define_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(MyApp.Source), typeof(MyApp.Target))]

                              namespace MyApp
                              {
                                  [MapTo(typeof(Target))]
                                  public partial class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCMAP005");
    }

    [Fact]
    public void Config_Should_Use_Attribute_Over_Config_When_Both_Exist()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(MyApp.Source), typeof(MyApp.Target))]

                              namespace MyApp
                              {
                                  [MapTo(typeof(Target))]
                                  public partial class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Mapping method should still be generated (attribute takes precedence)
        Assert.Contains("MapToTarget", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Emit_Warning_For_Duplicate_Config_Same_Pair()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(ExternalLib.Contact), typeof(MyApp.Customer))]
                              [assembly: MapTypes(typeof(ExternalLib.Contact), typeof(MyApp.Customer))]

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
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCMAP006");
    }

    [Fact]
    public void Config_Should_Allow_Different_Targets_For_Same_Source()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(ExternalLib.Contact), typeof(MyApp.Customer))]
                              [assembly: MapTypes(typeof(ExternalLib.Contact), typeof(MyApp.CustomerSummary))]

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

                                  public class CustomerSummary
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("MapToCustomerSummary", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Allow_Same_Target_From_Different_Sources()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(ExternalLib.Contact), typeof(MyApp.Customer))]
                              [assembly: MapTypes(typeof(ExternalLib.Lead), typeof(MyApp.Customer))]

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Lead
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
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Both extension methods should exist - one on Contact, one on Lead
        Assert.Contains("this ExternalLib.Contact source", output, StringComparison.Ordinal);
        Assert.Contains("this ExternalLib.Lead source", output, StringComparison.Ordinal);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
    }
}