// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Assembly_MapTypes_Should_Generate_Basic_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

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

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("ConfiguredMappingExtensions", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Assembly_MapTypes_Should_Apply_PropertyMap()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(
                                  typeof(ExternalLib.Contact),
                                  typeof(MyApp.Customer),
                                  PropertyMap = new[] { "FullName:DisplayName", "EmailAddress:Email" })]

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
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("DisplayName = source.FullName", output, StringComparison.Ordinal);
        Assert.Contains("Email = source.EmailAddress", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Assembly_MapTypes_Should_Apply_IgnoreSourceProperties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(
                                  typeof(ExternalLib.Contact),
                                  typeof(MyApp.Customer),
                                  IgnoreSourceProperties = new[] { "InternalId" })]

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string InternalId { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string InternalId { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.DoesNotContain("InternalId = source.InternalId", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Assembly_MapTypes_Should_Generate_Bidirectional()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(
                                  typeof(ExternalLib.Contact),
                                  typeof(MyApp.Customer),
                                  Bidirectional = true)]

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

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("MapToContact", output, StringComparison.Ordinal);
    }
}