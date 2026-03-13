// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.EnumMapping;

public partial class EnumMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Handle_Special_Case_None_To_Default()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Default,
                                  Active,
                                  Closed,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  None,
                                  Active,
                                  Closed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceStatus.None => TestNamespace.TargetStatus.Default,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Active => TestNamespace.TargetStatus.Active,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Closed => TestNamespace.TargetStatus.Closed,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Special_Case_Unknown_To_None()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  None,
                                  Active,
                                  Closed,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Unknown,
                                  Active,
                                  Closed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceStatus.Unknown => TestNamespace.TargetStatus.None,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Active => TestNamespace.TargetStatus.Active,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Closed => TestNamespace.TargetStatus.Closed,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Special_Case_Unknown_To_Default()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Default,
                                  Active,
                                  Closed,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Unknown,
                                  Active,
                                  Closed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceStatus.Unknown => TestNamespace.TargetStatus.Default,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Special_Case_Default_To_None()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  None,
                                  Active,
                                  Closed,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Default,
                                  Active,
                                  Closed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceStatus.Default => TestNamespace.TargetStatus.None,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Special_Case_Default_To_Unknown()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Unknown,
                                  Active,
                                  Closed,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Default,
                                  Active,
                                  Closed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceStatus.Default => TestNamespace.TargetStatus.Unknown,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Warn_For_Unmapped_Values_Without_Special_Case()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetColor
                              {
                                  Red,
                                  Green,
                              }

                              [MapTo(typeof(TargetColor))]
                              public enum SourceColor
                              {
                                  Red,
                                  Green,
                                  Blue,
                                  Yellow,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);

        var blueWarning = Assert.Single(
            diagnostics,
            d => d.Id == "ATCENUM002" &&
                 d
                     .GetMessage(CultureInfo.InvariantCulture)
                     .Contains("Blue", StringComparison.Ordinal));
        Assert.Equal(DiagnosticSeverity.Warning, blueWarning.Severity);

        var yellowWarning = Assert.Single(
            diagnostics,
            d => d.Id == "ATCENUM002" &&
                 d
                     .GetMessage(CultureInfo.InvariantCulture)
                     .Contains("Yellow", StringComparison.Ordinal));
        Assert.Equal(DiagnosticSeverity.Warning, yellowWarning.Severity);

        // Mapped values should be present in output
        Assert.Contains("TestNamespace.SourceColor.Red => TestNamespace.TargetColor.Red,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceColor.Green => TestNamespace.TargetColor.Green,", output, StringComparison.Ordinal);

        // Unmapped values should NOT be present in switch
        Assert.DoesNotContain("SourceColor.Blue =>", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourceColor.Yellow =>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_ATCENUM002_With_Correct_Message_Format()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetKind
                              {
                                  TypeA,
                              }

                              [MapTo(typeof(TargetKind))]
                              public enum SourceKind
                              {
                                  TypeA,
                                  TypeB,
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        var warning = Assert.Single(diagnostics, d => d.Id == "ATCENUM002");
        var message = warning.GetMessage(CultureInfo.InvariantCulture);
        Assert.Contains("SourceKind", message, StringComparison.Ordinal);
        Assert.Contains("TypeB", message, StringComparison.Ordinal);
        Assert.Contains("TargetKind", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Target_Is_A_Struct()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public struct TargetStruct
                              {
                                  public int Value;
                              }

                              [MapTo(typeof(TargetStruct))]
                              public enum SourceStatus
                              {
                                  Active,
                                  Inactive,
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCENUM001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("must be an enum type", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Target_Is_An_Interface()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public interface ITarget
                              {
                                  int Value { get; }
                              }

                              [MapTo(typeof(ITarget))]
                              public enum SourceStatus
                              {
                                  Active,
                                  Inactive,
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCENUM001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Handle_Enum_With_Many_Values()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetMonth
                              {
                                  January,
                                  February,
                                  March,
                                  April,
                                  May,
                                  June,
                                  July,
                                  August,
                                  September,
                                  October,
                                  November,
                                  December,
                              }

                              [MapTo(typeof(TargetMonth))]
                              public enum SourceMonth
                              {
                                  January,
                                  February,
                                  March,
                                  April,
                                  May,
                                  June,
                                  July,
                                  August,
                                  September,
                                  October,
                                  November,
                                  December,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetMonth", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceMonth.January => TestNamespace.TargetMonth.January,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceMonth.June => TestNamespace.TargetMonth.June,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceMonth.December => TestNamespace.TargetMonth.December,", output, StringComparison.Ordinal);

        // Verify all 12 months are present
        Assert.Contains("SourceMonth.February =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.March =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.April =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.May =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.July =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.August =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.September =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.October =>", output, StringComparison.Ordinal);
        Assert.Contains("SourceMonth.November =>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Source_With_More_Values_Than_Target()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetPriority
                              {
                                  Low,
                                  High,
                              }

                              [MapTo(typeof(TargetPriority))]
                              public enum SourcePriority
                              {
                                  Low,
                                  Medium,
                                  High,
                                  Critical,
                                  Urgent,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Should have warnings for Medium, Critical, Urgent
        Assert.Equal(3, diagnostics.Length);
        Assert.All(diagnostics, d => Assert.Equal("ATCENUM002", d.Id));
        Assert.All(diagnostics, d => Assert.Equal(DiagnosticSeverity.Warning, d.Severity));

        // Mapped values should be present
        Assert.Contains("TestNamespace.SourcePriority.Low => TestNamespace.TargetPriority.Low,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourcePriority.High => TestNamespace.TargetPriority.High,", output, StringComparison.Ordinal);

        // Unmapped values should NOT be present
        Assert.DoesNotContain("SourcePriority.Medium =>", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourcePriority.Critical =>", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourcePriority.Urgent =>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Target_With_More_Values_Than_Source()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetPriority
                              {
                                  Low,
                                  Medium,
                                  High,
                                  Critical,
                                  Urgent,
                              }

                              [MapTo(typeof(TargetPriority))]
                              public enum SourcePriority
                              {
                                  Low,
                                  High,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // No warnings - all source values have targets
        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourcePriority.Low => TestNamespace.TargetPriority.Low,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourcePriority.High => TestNamespace.TargetPriority.High,", output, StringComparison.Ordinal);

        // Extra target values should not appear as source in switch arms
        Assert.DoesNotContain("SourcePriority.Medium", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourcePriority.Critical", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourcePriority.Urgent", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Multiple_MapTo_On_Same_Enum()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetA
                              {
                                  Active,
                                  Inactive,
                              }

                              public enum TargetB
                              {
                                  Active,
                                  Inactive,
                              }

                              [MapTo(typeof(TargetA))]
                              [MapTo(typeof(TargetB))]
                              public enum SourceStatus
                              {
                                  Active,
                                  Inactive,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Both mappings should be generated
        Assert.Contains("MapToTargetA", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetB", output, StringComparison.Ordinal);

        Assert.Contains("public static TestNamespace.TargetA MapToTargetA(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.SourceStatus source)", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetB MapToTargetB(", output, StringComparison.Ordinal);

        Assert.Contains("TestNamespace.SourceStatus.Active => TestNamespace.TargetA.Active,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Active => TestNamespace.TargetB.Active,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Bidirectional_With_Special_Case_None_Unknown()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Unknown,
                                  Active,
                                  Closed,
                              }

                              [MapTo(typeof(TargetStatus), Bidirectional = true)]
                              public enum SourceStatus
                              {
                                  None,
                                  Active,
                                  Closed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward: None -> Unknown
        Assert.Contains("TestNamespace.SourceStatus.None => TestNamespace.TargetStatus.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Active => TestNamespace.TargetStatus.Active,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Closed => TestNamespace.TargetStatus.Closed,", output, StringComparison.Ordinal);

        // Reverse: Unknown -> None
        Assert.Contains("TestNamespace.TargetStatus.Unknown => TestNamespace.SourceStatus.None,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.TargetStatus.Active => TestNamespace.SourceStatus.Active,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.TargetStatus.Closed => TestNamespace.SourceStatus.Closed,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Bidirectional_With_Special_Case_Default_None()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  None,
                                  Open,
                                  Resolved,
                              }

                              [MapTo(typeof(TargetStatus), Bidirectional = true)]
                              public enum SourceStatus
                              {
                                  Default,
                                  Open,
                                  Resolved,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward: Default -> None
        Assert.Contains("TestNamespace.SourceStatus.Default => TestNamespace.TargetStatus.None,", output, StringComparison.Ordinal);

        // Reverse: None -> Default
        Assert.Contains("TestNamespace.TargetStatus.None => TestNamespace.SourceStatus.Default,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Case_Insensitive_UPPER_To_Lower()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetCase
                              {
                                  active,
                                  inactive,
                                  deleted,
                              }

                              [MapTo(typeof(TargetCase))]
                              public enum SourceCase
                              {
                                  ACTIVE,
                                  INACTIVE,
                                  DELETED,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceCase.ACTIVE => TestNamespace.TargetCase.active,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceCase.INACTIVE => TestNamespace.TargetCase.inactive,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceCase.DELETED => TestNamespace.TargetCase.deleted,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Case_Insensitive_PascalCase_To_CamelCase()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetCasing
                              {
                                  InProgress,
                                  OnHold,
                                  Completed,
                              }

                              [MapTo(typeof(TargetCasing))]
                              public enum SourceCasing
                              {
                                  inProgress,
                                  onHold,
                                  completed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceCasing.inProgress => TestNamespace.TargetCasing.InProgress,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceCasing.onHold => TestNamespace.TargetCasing.OnHold,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceCasing.completed => TestNamespace.TargetCasing.Completed,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Case_Insensitive_Mixed_Casing_Patterns()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetMixed
                              {
                                  ACTIVE,
                                  pending,
                                  Resolved,
                              }

                              [MapTo(typeof(TargetMixed))]
                              public enum SourceMixed
                              {
                                  active,
                                  PENDING,
                                  resolved,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceMixed.active => TestNamespace.TargetMixed.ACTIVE,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceMixed.PENDING => TestNamespace.TargetMixed.pending,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceMixed.resolved => TestNamespace.TargetMixed.Resolved,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Prefer_Exact_Case_Match_Over_Case_Insensitive()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetExact
                              {
                                  Active,
                                  Pending,
                              }

                              [MapTo(typeof(TargetExact))]
                              public enum SourceExact
                              {
                                  Active,
                                  Pending,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Exact case match should be used
        Assert.Contains("TestNamespace.SourceExact.Active => TestNamespace.TargetExact.Active,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceExact.Pending => TestNamespace.TargetExact.Pending,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_None_To_Unknown_Bidirectional_Round_Trip()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum DtoStatus
                              {
                                  Unknown,
                                  Open,
                                  Closed,
                              }

                              [MapTo(typeof(DtoStatus), Bidirectional = true)]
                              public enum EntityStatus
                              {
                                  None,
                                  Open,
                                  Closed,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward: EntityStatus.None -> DtoStatus.Unknown
        Assert.Contains("public static TestNamespace.DtoStatus MapToDtoStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.EntityStatus source)", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.EntityStatus.None => TestNamespace.DtoStatus.Unknown,", output, StringComparison.Ordinal);

        // Reverse: DtoStatus.Unknown -> EntityStatus.None
        Assert.Contains("public static TestNamespace.EntityStatus MapToEntityStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.DtoStatus source)", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.DtoStatus.Unknown => TestNamespace.EntityStatus.None,", output, StringComparison.Ordinal);

        // Other values should map normally in both directions
        Assert.Contains("TestNamespace.EntityStatus.Open => TestNamespace.DtoStatus.Open,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.DtoStatus.Open => TestNamespace.EntityStatus.Open,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Single_Value_Enum()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetSingle
                              {
                                  Only,
                              }

                              [MapTo(typeof(TargetSingle))]
                              public enum SourceSingle
                              {
                                  Only,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceSingle.Only => TestNamespace.TargetSingle.Only,", output, StringComparison.Ordinal);
        Assert.Contains("_ => throw new global::System.ArgumentOutOfRangeException(nameof(source), source, \"Unmapped enum value\"),", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_All_Values_Unmapped()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetShape
                              {
                                  Circle,
                                  Square,
                              }

                              [MapTo(typeof(TargetShape))]
                              public enum SourceShape
                              {
                                  Triangle,
                                  Hexagon,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // All values should have warnings
        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, d => Assert.Equal("ATCENUM002", d.Id));

        // No source enum values should appear in switch arms
        Assert.DoesNotContain("SourceShape.Triangle =>", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourceShape.Hexagon =>", output, StringComparison.Ordinal);

        // Default case should still be present
        Assert.Contains("_ => throw new global::System.ArgumentOutOfRangeException(nameof(source), source, \"Unmapped enum value\"),", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Method_In_Atc_Mapping_Namespace()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace MyApp.Models;

                              public enum TargetRole
                              {
                                  Admin,
                                  User,
                              }

                              [MapTo(typeof(TargetRole))]
                              public enum SourceRole
                              {
                                  Admin,
                                  User,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("namespace Atc.Mapping;", output, StringComparison.Ordinal);
        Assert.Contains("public static class EnumMappingExtensions", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_Best_Practice_Attributes_On_Generated_Class()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetLevel
                              {
                                  Low,
                                  High,
                              }

                              [MapTo(typeof(TargetLevel))]
                              public enum SourceLevel
                              {
                                  Low,
                                  High,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("[global::System.CodeDom.Compiler.GeneratedCode(\"Atc.SourceGenerators.EnumMapping\", \"1.0.0\")]", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.Runtime.CompilerServices.CompilerGenerated]", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.Diagnostics.DebuggerNonUserCode]", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Multiple_MapTo_With_Different_Bidirectional_Settings()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetX
                              {
                                  On,
                                  Off,
                              }

                              public enum TargetY
                              {
                                  On,
                                  Off,
                              }

                              [MapTo(typeof(TargetX), Bidirectional = true)]
                              [MapTo(typeof(TargetY), Bidirectional = false)]
                              public enum SourceSwitch
                              {
                                  On,
                                  Off,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mappings for both targets
        Assert.Contains("MapToTargetX", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetY", output, StringComparison.Ordinal);

        // Reverse mapping only for TargetX (Bidirectional = true)
        Assert.Contains("MapToSourceSwitch", output, StringComparison.Ordinal);

        // Verify reverse mapping is from TargetX
        Assert.Contains("public static TestNamespace.SourceSwitch MapToSourceSwitch(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.TargetX source)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Cross_Namespace_With_Special_Case()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Api.Models
                              {
                                  public enum UserState
                                  {
                                      Unknown,
                                      Active,
                                      Suspended,
                                  }
                              }

                              namespace Data.Entities
                              {
                                  [MapTo(typeof(Api.Models.UserState))]
                                  public enum UserStateEntity
                                  {
                                      None,
                                      Active,
                                      Suspended,
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Data.Entities.UserStateEntity.None => Api.Models.UserState.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("Data.Entities.UserStateEntity.Active => Api.Models.UserState.Active,", output, StringComparison.Ordinal);
        Assert.Contains("Data.Entities.UserStateEntity.Suspended => Api.Models.UserState.Suspended,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Using_Directives_For_All_Namespaces()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace First.Namespace
                              {
                                  public enum TargetEnum
                                  {
                                      Value1,
                                  }
                              }

                              namespace Second.Namespace
                              {
                                  [MapTo(typeof(First.Namespace.TargetEnum))]
                                  public enum SourceEnum
                                  {
                                      Value1,
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("using First.Namespace;", output, StringComparison.Ordinal);
        Assert.Contains("using Second.Namespace;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Bidirectional_Where_Reverse_Has_Unmapped_Extra_Values()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetAccess
                              {
                                  Read,
                                  Write,
                                  Admin,
                              }

                              [MapTo(typeof(TargetAccess), Bidirectional = true)]
                              public enum SourceAccess
                              {
                                  Read,
                                  Write,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // No ATCENUM002 warnings for forward direction (all source values match)
        Assert.Empty(diagnostics);

        // Forward: all source values map
        Assert.Contains("TestNamespace.SourceAccess.Read => TestNamespace.TargetAccess.Read,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceAccess.Write => TestNamespace.TargetAccess.Write,", output, StringComparison.Ordinal);

        // Reverse: Read and Write map, Admin has no match in source but
        // reverse mapping warnings are not reported as ATCENUM002 (only forward is diagnosed)
        Assert.Contains("TestNamespace.TargetAccess.Read => TestNamespace.SourceAccess.Read,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.TargetAccess.Write => TestNamespace.SourceAccess.Write,", output, StringComparison.Ordinal);

        // Admin should NOT appear as a mapped value in reverse (it's unmapped)
        Assert.DoesNotContain("TargetAccess.Admin => TestNamespace.SourceAccess", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Special_Case_Priority_None_Unknown_Default()
    {
        // When both Unknown and Default exist in target, None should map to Unknown first
        // (Unknown is first in the SpecialCaseMappings array for "None")
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetWithBoth
                              {
                                  Unknown,
                                  Default,
                                  Active,
                              }

                              [MapTo(typeof(TargetWithBoth))]
                              public enum SourceWithNone
                              {
                                  None,
                                  Active,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // None should map to Unknown (first in special case list)
        Assert.Contains("TestNamespace.SourceWithNone.None => TestNamespace.TargetWithBoth.Unknown,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Use_Special_Case_When_Exact_Match_Exists()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetWithNone
                              {
                                  None,
                                  Unknown,
                                  Active,
                              }

                              [MapTo(typeof(TargetWithNone))]
                              public enum SourceWithNone
                              {
                                  None,
                                  Active,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // None should map to None (exact match), not Unknown (special case)
        Assert.Contains("TestNamespace.SourceWithNone.None => TestNamespace.TargetWithNone.None,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_Xml_Doc_Comments_On_Method()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetDoc
                              {
                                  Value1,
                              }

                              [MapTo(typeof(TargetDoc))]
                              public enum SourceDoc
                              {
                                  Value1,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("/// <summary>", output, StringComparison.Ordinal);
        Assert.Contains("/// Maps <see cref=\"TestNamespace.SourceDoc\"/> to <see cref=\"TestNamespace.TargetDoc\"/>.", output, StringComparison.Ordinal);
        Assert.Contains("/// </summary>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Enum_With_Explicit_Integer_Values()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetFlag
                              {
                                  Read = 1,
                                  Write = 2,
                                  Execute = 4,
                              }

                              [MapTo(typeof(TargetFlag))]
                              public enum SourceFlag
                              {
                                  Read = 10,
                                  Write = 20,
                                  Execute = 40,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Mapping should work by name regardless of integer values
        Assert.Contains("TestNamespace.SourceFlag.Read => TestNamespace.TargetFlag.Read,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceFlag.Write => TestNamespace.TargetFlag.Write,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceFlag.Execute => TestNamespace.TargetFlag.Execute,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Warn_For_Flags_Enum_Source()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetPermission
                              {
                                  Read,
                                  Write,
                                  Execute,
                              }

                              [Flags]
                              [MapTo(typeof(TargetPermission))]
                              public enum SourcePermission
                              {
                                  None = 0,
                                  Read = 1,
                                  Write = 2,
                                  Execute = 4,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCENUM006");
        Assert.NotEmpty(output);
    }

    [Fact]
    public void Generator_Should_Warn_For_Flags_Enum_Target()
    {
        const string source = """
                              using System;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [Flags]
                              public enum TargetPermission
                              {
                                  None = 0,
                                  Read = 1,
                                  Write = 2,
                                  Execute = 4,
                              }

                              [MapTo(typeof(TargetPermission))]
                              public enum SourcePermission
                              {
                                  Read,
                                  Write,
                                  Execute,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Contains(diagnostics, d => d.Id == "ATCENUM006");
        Assert.NotEmpty(output);
    }
}