// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Inline_Should_Generate_Mapping_From_MappingBuilder_Configure()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map(typeof(Source), typeof(Target));
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTarget", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Support_Generic_Map_Syntax()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<Source, Target>();
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTarget", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Support_Bidirectional()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<Source, Target>(bidirectional: true);
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTarget", output, StringComparison.Ordinal);
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Support_PropertyMap()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string FullName { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string DisplayName { get; set; } = string.Empty;
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<Source, Target>(
                                                  propertyMap: new[] { "FullName:DisplayName" });
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("DisplayName = source.FullName", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Support_IgnoreSourceProperties()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string Secret { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public string Secret { get; set; } = string.Empty;
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<Source, Target>(
                                                  ignoreSourceProperties: new[] { "Secret" });
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Target properties without a source are reported as Info (ATCMCF016), nothing else
        Assert.All(diagnostics, d => Assert.Equal("ATCMCF016", d.Id));
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Secret = source.Secret", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Support_Fluent_Chaining()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class SourceA
                                  {
                                      public int Id { get; set; }
                                  }

                                  public class TargetA
                                  {
                                      public int Id { get; set; }
                                  }

                                  public class SourceB
                                  {
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class TargetB
                                  {
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<SourceA, TargetA>()
                                                 .Map<SourceB, TargetB>();
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetA", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetB", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Support_TypeOf_Map_Syntax()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map(typeof(Source), typeof(Target));
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTarget", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_AutoDetect_Enums()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public enum SourceStatus { None, Active, Inactive }
                                  public enum TargetStatus { Unknown, Active, Inactive }

                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public SourceStatus Status { get; set; }
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public TargetStatus Status { get; set; }
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<Source, Target>();
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains("MapToTargetStatus", output, StringComparison.Ordinal);
        Assert.Contains("global::MyApp.SourceStatus.None => global::MyApp.TargetStatus.Unknown", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Deduplicate_With_Assembly_MapTypes()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              [assembly: MapTypes(typeof(MyApp.Source), typeof(MyApp.Target))]

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<Source, Target>();
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Should report duplicate and keep only one
        Assert.Contains(diagnostics, d => d.Id == "ATCMAP006");
    }

    [Fact]
    public void Inline_Should_Error_On_Missing_Lambda()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(null!);
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCMCF013");
    }

    [Fact]
    public void Inline_Should_Error_On_Invalid_Map_Arguments()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                  }

                                  public interface ITarget
                                  {
                                      int Id { get; set; }
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map(typeof(Source), typeof(ITarget));
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCMAP010");
    }

    [Fact]
    public void Inline_Should_Deduplicate_With_Attribute_MapTo()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  [MapTo(typeof(Target))]
                                  public partial class Source
                                  {
                                      public int Id { get; set; }
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map<Source, Target>();
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Should report duplicate - attribute takes precedence
        Assert.Contains(diagnostics, d => d.Id == "ATCMAP005");
    }

    [Fact]
    public void Inline_Should_Ignore_Unrelated_Configure_Calls_With_Map()
    {
        // An ASP.NET-style Configure(app => app.Map("/health", ...)) is not a mapping configuration
        const string source = """
                              using System;

                              namespace TestNamespace
                              {
                                  public class App
                                  {
                                      public App Map(string path, Action<App> branch) => this;
                                  }

                                  public class HostBuilder
                                  {
                                      public HostBuilder Configure(Action<App> configure) => this;
                                  }

                                  public static class Program
                                  {
                                      public static void Run()
                                      {
                                          new HostBuilder().Configure(app => app.Map("/health", branch => { }));
                                          new HostBuilder().Configure(app =>
                                          {
                                              app.Map("/ready", branch => { });
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.DoesNotContain(diagnostics, d => d.Id.StartsWith("ATCMCF", StringComparison.Ordinal));
    }

    [Fact]
    public void Inline_Should_Generate_Mapping_From_Expression_Bodied_Configure()
    {
        // map => map.Map<Source, Target>() has the Map call as the lambda body itself (not a descendant of it)
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                  }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                  }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          MappingBuilder.Configure(map => map.Map<Source, Target>());
                                      }
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTarget", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Report_Error_For_Mapping_Method_With_More_Than_One_Parameter()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source { public int Id { get; set; } }
                                  public class Target { public int Id { get; set; } }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target MapToTarget(this Source source, int extra);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMCF009");
        Assert.Contains("MapToTarget", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Inline_Should_Report_Error_For_Map_Arguments_That_Are_Not_TypeOf()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp
                              {
                                  public class Source { public int Id { get; set; } }
                                  public class Target { public int Id { get; set; } }

                                  public static class Startup
                                  {
                                      public static void Configure()
                                      {
                                          var sourceType = typeof(Source);
                                          MappingBuilder.Configure(map =>
                                          {
                                              map.Map(sourceType, typeof(Target));
                                          });
                                      }
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.Single(diagnostics, d => d.Id == "ATCMCF012");
        Assert.DoesNotContain(diagnostics, d => d.Id == "ATCMCF011");
    }
}