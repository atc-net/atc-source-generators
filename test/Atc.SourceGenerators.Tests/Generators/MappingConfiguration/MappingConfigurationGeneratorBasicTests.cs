// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Generate_ClassToClass_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string Email { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string Email { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ExternalMappings
                                  {
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomer", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.Contains("Email = source.Email", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_ClassToRecord_Mapping()
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
                                  public record CustomerDto(int Id, string Name);
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ExternalMappings
                                  {
                                      public static partial MyApp.CustomerDto MapToCustomerDto(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomerDto", output, StringComparison.Ordinal);

        // Should use constructor since CustomerDto is a record
        Assert.Contains("new MyApp.CustomerDto(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_Null_Check()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Target
                                  {
                                      public int Id { get; set; }
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
        Assert.Contains("if (source is null)", output, StringComparison.Ordinal);
        Assert.Contains("return default!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Generate_In_Correct_Namespace()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Target
                                  {
                                      public int Id { get; set; }
                                  }
                              }

                              namespace MyApp.Infrastructure.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class ThirdPartyMappings
                                  {
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("namespace MyApp.Infrastructure.Mappings;", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class ThirdPartyMappings", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Apply_GeneratedCode_Attributes()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Target
                                  {
                                      public int Id { get; set; }
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
        Assert.Contains("GeneratedCode", output, StringComparison.Ordinal);
        Assert.Contains("Atc.SourceGenerators.MappingConfiguration", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Skip_Unmatched_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string ExtraProperty { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string DifferentProperty { get; set; } = string.Empty;
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
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.DoesNotContain("ExtraProperty", output, StringComparison.Ordinal);
        Assert.DoesNotContain("DifferentProperty", output, StringComparison.Ordinal);
    }
}