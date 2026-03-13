// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Error_When_Class_Not_Static_WithMethods()
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
                                  public partial class NonStaticMappings
                                  {
                                      public partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF001");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Config_Should_Error_When_Class_Not_Partial_WithMethods()
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
                                  public static class NonPartialMappings
                                  {
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF002");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF002");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Config_Should_Error_When_Method_Not_Extension_Method()
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

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF004");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF004");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Config_Should_Error_When_Return_Type_Is_Void()
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

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF010");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF010");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Config_Should_Error_When_Return_Type_Is_Primitive()
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
                                      public static partial int MapToInt(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF010");
    }

    [Fact]
    public void Config_Should_Error_When_Renamed_Source_Property_Does_Not_Exist()
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
                                      public string DisplayName { get; set; } = string.Empty;
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      [MapConfigProperty("DoesNotExist", "DisplayName")]
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF006");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF006");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Config_Should_Error_When_Renamed_Target_Property_Does_Not_Exist()
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
                                      [MapConfigProperty("Name", "DoesNotExist")]
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF007");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF007");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Config_Should_Warn_When_Ignored_Property_Does_Not_Exist_On_Source()
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
                                      [MapConfigIgnore("PropertyThatDoesNotExist")]
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF005");
    }

    [Fact]
    public void Config_Should_Report_Info_When_Class_Has_No_Methods()
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

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF008");
    }

    [Fact]
    public void Config_Should_Report_Multiple_Errors_For_Multiple_Invalid_Methods()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Target1 { public int Id { get; set; } }
                                  public class Target2 { public int Id { get; set; } }
                              }

                              namespace ExternalLib
                              {
                                  public class Source1 { public int Id { get; set; } }
                                  public class Source2 { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Target1 MapToTarget1(ExternalLib.Source1 source);
                                      public static partial MyApp.Target2 MapToTarget2(ExternalLib.Source2 source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        var extensionErrors = diagnostics.Where(d => d.Id == "ATCMCF004").ToList();
        Assert.True(extensionErrors.Count >= 2, $"Expected at least 2 ATCMCF004 errors but found {extensionErrors.Count}");
    }

    [Fact]
    public void Config_Should_Warn_ATCMCF014_When_Return_Type_Is_Unresolved()
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
                                      public static partial MyApp.UnresolvedTarget MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF014");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF014");
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMCF010");
    }

    [Fact]
    public void Config_Should_Warn_ATCMCF014_When_Source_Type_Is_Unresolved()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Target { public int Id { get; set; } }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.UnresolvedSource source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF014");
        var diagnostic = diagnostics.First(d => d.Id == "ATCMCF014");
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [Fact]
    public void Config_Should_Still_Error_ATCMCF010_When_Return_Type_Is_Void()
    {
        // Ensure existing ATCMCF010 behavior is preserved for void/primitive returns
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

        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMCF");

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF010");
        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMCF014");
    }
}