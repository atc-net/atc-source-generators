// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_Map_Nullable_Guid_And_DateTimeOffset_To_NonNullable()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record Source(Guid? Id, DateTimeOffset? CreatedOn);

                                  public sealed class Target
                                  {
                                      public Guid Id { get; set; }

                                      public DateTimeOffset CreatedOn { get; set; }
                                  }

                                  [MappingConfiguration]
                                  public static partial class Config
                                  {
                                      public static partial Target MapToTarget(this Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToGuid", output, StringComparison.Ordinal);
        Assert.DoesNotContain("MapToDateTimeOffset", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id ?? default", output, StringComparison.Ordinal);
        Assert.Contains("CreatedOn = source.CreatedOn ?? default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Guid_To_Nullable_Guid()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed class Source
                                  {
                                      public Guid Id { get; set; }
                                  }

                                  public sealed class Target
                                  {
                                      public Guid? Id { get; set; }
                                  }

                                  [MappingConfiguration]
                                  public static partial class Config
                                  {
                                      public static partial Target MapToTarget(this Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToGuid", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DateTimeOffset")]
    [InlineData("TimeSpan")]
    [InlineData("DateOnly")]
    [InlineData("TimeOnly")]
    [InlineData("DateTime")]
    [InlineData("int")]
    public void Config_Should_Map_Nullable_Struct_To_NonNullable(
        string typeName)
    {
        var source = $$"""
                       using System;
                       using Atc.SourceGenerators.Annotations;

                       namespace TestNamespace
                       {
                           public sealed class Source
                           {
                               public {{typeName}}? Value { get; set; }
                           }

                           public sealed class Target
                           {
                               public {{typeName}} Value { get; set; }
                           }

                           [MappingConfiguration]
                           public static partial class Config
                           {
                               public static partial Target MapToTarget(this Source source);
                           }
                       }
                       """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain($"MapTo{typeName}(", output, StringComparison.Ordinal);
        Assert.Contains("Value = source.Value ?? default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Nullable_Guid_To_NonNullable_In_RecordConstructor()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed record Source(Guid? Id, string Name);

                                  public sealed record Target(Guid Id, string Name);

                                  [MappingConfiguration]
                                  public static partial class Config
                                  {
                                      public static partial Target MapToTarget(this Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToGuid", output, StringComparison.Ordinal);
        Assert.Contains("source.Id ?? default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Pass_Optional_Constructor_Parameter_From_Source_Property()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed class Source
                                  {
                                      public double X { get; set; }

                                      public double Y { get; set; }
                                  }

                                  public sealed record Point2D(double X, double? Y = null);

                                  [MappingConfiguration]
                                  public static partial class Config
                                  {
                                      public static partial Point2D MapToPoint2D(this Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("source.X", output, StringComparison.Ordinal);
        Assert.Contains("source.Y", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Map_Nullable_Guid_Bidirectional()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public sealed class Source
                                  {
                                      public Guid? Id { get; set; }
                                  }

                                  public sealed class Target
                                  {
                                      public Guid Id { get; set; }
                                  }

                                  [MappingConfiguration]
                                  public static partial class Config
                                  {
                                      [MapConfigOptions(Bidirectional = true)]
                                      public static partial Target MapToTarget(this Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToGuid", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id ?? default", output, StringComparison.Ordinal);
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
    }
}