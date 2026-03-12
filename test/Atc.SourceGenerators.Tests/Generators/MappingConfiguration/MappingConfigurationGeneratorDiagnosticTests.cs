// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Error_When_Class_Not_Static()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Target { public int Id { get; set; } }
                              }

                              namespace ExternalLib
                              {
                                  public class Source { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public partial class Mappings
                                  {
                                      public partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF001");
    }

    [Fact]
    public void Config_Should_Error_When_Class_Not_Partial()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Target { public int Id { get; set; } }
                              }

                              namespace ExternalLib
                              {
                                  public class Source { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static class Mappings
                                  {
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF002");
    }

    [Fact]
    public void Config_Should_Error_When_Method_Not_Extension()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Target { public int Id { get; set; } }
                              }

                              namespace ExternalLib
                              {
                                  public class Source { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Target MapToTarget(ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF004");
    }

    [Fact]
    public void Config_Should_Warn_When_Ignored_Property_Not_Found()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source { public int Id { get; set; } }
                              }

                              namespace MyApp
                              {
                                  public class Target { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigIgnore("NonExistentProperty")]
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF005");
    }

    [Fact]
    public void Config_Should_Error_When_Renamed_Source_Not_Found()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source { public int Id { get; set; } }
                              }

                              namespace MyApp
                              {
                                  public class Target
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
                                      [MapConfigProperty("NonExistent", "Name")]
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF006");
    }

    [Fact]
    public void Config_Should_Error_When_Renamed_Target_Not_Found()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Target { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigProperty("Name", "NonExistent")]
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF007");
    }

    [Fact]
    public void Config_Should_Info_When_Config_Class_Empty()
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

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF008");
    }

    [Fact]
    public void Config_Should_Error_When_Return_Type_Invalid()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public class Source { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial void MapToNothing(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF010");
    }
}