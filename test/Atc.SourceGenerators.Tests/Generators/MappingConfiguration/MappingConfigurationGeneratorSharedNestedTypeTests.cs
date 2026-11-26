// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    private const string SharedNestedTypes = """
                                             public record DomainPoint(double X, double Y);
                                             public record ApiPoint(double X, double Y);

                                             public record DomainA(DomainPoint P);
                                             public record ApiA(ApiPoint P);
                                             public record DomainB(DomainPoint P);
                                             public record ApiB(ApiPoint P);
                                             """;

    [Fact]
    public void Config_TwoMappingClasses_With_Same_Nested_Type_Should_Compile()
    {
        const string source = $$"""
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace
                                {
                                    {{SharedNestedTypes}}

                                    [MappingConfiguration]
                                    public static partial class AMappings
                                    {
                                        public static partial ApiA MapToApiA(this DomainA source);
                                    }

                                    [MappingConfiguration]
                                    public static partial class BMappings
                                    {
                                        public static partial ApiB MapToApiB(this DomainB source);
                                    }
                                }
                                """;

        var (diagnostics, _) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Config_Nested_Mapper_Declared_In_One_Class_Should_Not_Break_Other_Class()
    {
        const string source = $$"""
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace
                                {
                                    {{SharedNestedTypes}}

                                    [MappingConfiguration]
                                    public static partial class AMappings
                                    {
                                        public static partial ApiPoint MapToApiPoint(this DomainPoint source);

                                        public static partial ApiA MapToApiA(this DomainA source);
                                    }

                                    [MappingConfiguration]
                                    public static partial class BMappings
                                    {
                                        public static partial ApiB MapToApiB(this DomainB source);
                                    }
                                }
                                """;

        var (diagnostics, _) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Config_Nested_Mapper_Declared_In_Shared_Class_Should_Not_Break_Other_Classes()
    {
        const string source = $$"""
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace
                                {
                                    {{SharedNestedTypes}}

                                    [MappingConfiguration]
                                    public static partial class SharedMappings
                                    {
                                        [MapConfigOptions(Bidirectional = true)]
                                        public static partial ApiPoint MapToApiPoint(this DomainPoint source);
                                    }

                                    [MappingConfiguration]
                                    public static partial class AMappings
                                    {
                                        public static partial ApiA MapToApiA(this DomainA source);
                                    }

                                    [MappingConfiguration]
                                    public static partial class BMappings
                                    {
                                        public static partial ApiB MapToApiB(this DomainB source);
                                    }
                                }
                                """;

        var (diagnostics, _) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Config_Collection_Of_Shared_Nested_Type_In_Two_Classes_Should_Compile()
    {
        const string source = """
                              using System.Collections.Generic;
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public record DomainPoint(double X, double Y);
                                  public record ApiPoint(double X, double Y);

                                  public class DomainA { public List<DomainPoint> Points { get; set; } = new(); }
                                  public class ApiA { public List<ApiPoint> Points { get; set; } = new(); }
                                  public class DomainB { public DomainPoint[] Points { get; set; } = []; }
                                  public class ApiB { public ApiPoint[] Points { get; set; } = []; }

                                  [MappingConfiguration]
                                  public static partial class AMappings
                                  {
                                      public static partial ApiPoint MapToApiPoint(this DomainPoint source);

                                      public static partial ApiA MapToApiA(this DomainA source);
                                  }

                                  [MappingConfiguration]
                                  public static partial class BMappings
                                  {
                                      public static partial ApiB MapToApiB(this DomainB source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Config_Nullable_Shared_Nested_Types_In_Two_Classes_Should_Compile()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public record DomainPoint(double X, double Y);
                                  public record ApiPoint(double X, double Y);
                                  public record struct DomainSize(double Width, double Height);
                                  public record struct ApiSize(double Width, double Height);

                                  public class DomainA
                                  {
                                      public DomainPoint? P { get; set; }
                                      public DomainSize? Size { get; set; }
                                      public DomainSize Required { get; set; }
                                      public DomainSize? Fallback { get; set; }
                                  }

                                  public class ApiA
                                  {
                                      public ApiPoint? P { get; set; }
                                      public ApiSize? Size { get; set; }
                                      public ApiSize Required { get; set; }
                                      public ApiSize Fallback { get; set; }
                                  }

                                  public class DomainB
                                  {
                                      public DomainPoint? P { get; set; }
                                      public DomainSize Required { get; set; }
                                  }

                                  public class ApiB
                                  {
                                      public ApiPoint? P { get; set; }
                                      public ApiSize Required { get; set; }
                                  }

                                  [MappingConfiguration]
                                  public static partial class AMappings
                                  {
                                      public static partial ApiPoint MapToApiPoint(this DomainPoint source);

                                      public static partial ApiSize MapToApiSize(this DomainSize source);

                                      public static partial ApiA MapToApiA(this DomainA source);
                                  }

                                  [MappingConfiguration]
                                  public static partial class BMappings
                                  {
                                      public static partial ApiB MapToApiB(this DomainB source);
                                  }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Config_Nested_Mapper_Call_Should_Use_Containing_Class_Method()
    {
        const string source = $$"""
                                using Atc.SourceGenerators.Annotations;

                                namespace TestNamespace
                                {
                                    {{SharedNestedTypes}}

                                    [MappingConfiguration]
                                    public static partial class AMappings
                                    {
                                        public static partial ApiA MapToApiA(this DomainA source);
                                    }
                                }
                                """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.DoesNotContain(".MapToApiPoint()", output, StringComparison.Ordinal);
        Assert.Contains("MapToApiPoint(__p)", output, StringComparison.Ordinal);
    }
}