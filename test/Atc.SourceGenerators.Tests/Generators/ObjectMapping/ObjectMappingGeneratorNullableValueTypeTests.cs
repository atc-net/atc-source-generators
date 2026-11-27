// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Theory]
    [InlineData("Guid")]
    [InlineData("DateTimeOffset")]
    [InlineData("TimeSpan")]
    [InlineData("DateOnly")]
    public void Generator_Should_Map_Nullable_Struct_To_NonNullable(
        string typeName)
    {
        var source = $$"""
                       using System;
                       using Atc.SourceGenerators.Annotations;

                       namespace TestNamespace;

                       public class TargetDto
                       {
                           public {{typeName}} Value { get; set; }
                       }

                       [MapTo(typeof(TargetDto))]
                       public partial class Source
                       {
                           public {{typeName}}? Value { get; set; }
                       }
                       """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain($"MapTo{typeName}(", output, StringComparison.Ordinal);
        Assert.Contains("Value = source.Value ?? default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Guid_To_Nullable_Guid()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public Guid? Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public Guid Id { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToGuid", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Nullable_Guid_To_Guid_In_Projection()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public Guid Id { get; set; }

                                  public int Count { get; set; }
                              }

                              [MapTo(typeof(TargetDto), GenerateProjection = true)]
                              public partial class Source
                              {
                                  public Guid? Id { get; set; }

                                  public int? Count { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("ProjectToTargetDto", output, StringComparison.Ordinal);
        Assert.DoesNotContain("MapToGuid", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id ?? default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Nullable_Guid_To_Guid_With_UpdateTarget_And_Bidirectional()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public partial class TargetDto
                              {
                                  public Guid Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto), Bidirectional = true, UpdateTarget = true)]
                              public partial class Source
                              {
                                  public Guid? Id { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToGuid", output, StringComparison.Ordinal);
        Assert.Contains("source.Id ?? default", output, StringComparison.Ordinal);
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Nullable_User_Struct_To_Same_Struct_Directly()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public struct Money
                              {
                                  public decimal Amount { get; set; }
                              }

                              public class TargetDto
                              {
                                  public Money Price { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public Money? Price { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToMoney", output, StringComparison.Ordinal);
        Assert.Contains("Price = source.Price ?? default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Nullable_Enum_To_Other_Nullable_Enum_With_MapTo()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum StatusDto { Active, Inactive }

                              [MapTo(typeof(StatusDto))]
                              public enum Status { Active, Inactive }

                              public class TargetDto
                              {
                                  public StatusDto? Status { get; set; }

                                  public StatusDto Required { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public Status? Status { get; set; }

                                  public Status? Required { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToNullable", output, StringComparison.Ordinal);
        Assert.Contains("Status = source.Status is { } __status ? __status.MapToStatusDto() : null", output, StringComparison.Ordinal);
        Assert.Contains("Required = source.Required is { } __required ? __required.MapToStatusDto() : default", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Cast_Nullable_Enum_To_Other_Nullable_Enum_Without_MapTo()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum KindDto { A, B }

                              public enum Kind { A, B }

                              public class TargetDto
                              {
                                  public KindDto? Kind { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public Kind? Kind { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain("MapToNullable", output, StringComparison.Ordinal);
        Assert.Contains("Kind = (global::TestNamespace.KindDto?)source.Kind", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DateTimeOffset?", "DateTime?", "Value = source.Value?.DateTime")]
    [InlineData("DateTimeOffset?", "DateTime", "Value = source.Value?.DateTime ?? default")]
    [InlineData("DateTime?", "DateTimeOffset?", "Value = source.Value is { } __value ? new global::System.DateTimeOffset(__value) : null")]
    [InlineData("DateTime?", "DateTimeOffset", "Value = source.Value is { } __value ? new global::System.DateTimeOffset(__value) : default")]
    public void Generator_Should_Convert_Nullable_DateTime_And_DateTimeOffset(
        string sourceType,
        string targetType,
        string expected)
    {
        var source = $$"""
                       #nullable enable
                       using System;
                       using Atc.SourceGenerators.Annotations;

                       namespace TestNamespace;

                       public class TargetDto
                       {
                           public {{targetType}} Value { get; set; }
                       }

                       [MapTo(typeof(TargetDto))]
                       public partial class Source
                       {
                           public {{sourceType}} Value { get; set; }
                       }
                       """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains(expected, output, StringComparison.Ordinal);
    }
}