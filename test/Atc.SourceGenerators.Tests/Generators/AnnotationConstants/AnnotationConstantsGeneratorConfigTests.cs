namespace Atc.SourceGenerators.Tests.Generators.AnnotationConstants;

/// <summary>
/// Tests for configuration behavior of AnnotationConstantsGenerator.
/// Note: The generator supports configuration via .editorconfig:
///   atc_annotation_constants.include_unannotated_properties = true|false
/// Default behavior (false): Only properties with DataAnnotation attributes are included.
/// </summary>
public partial class AnnotationConstantsGeneratorTests
{
    [Fact]
    public void Generator_Should_Only_Include_Annotated_Properties_By_Default()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Required]
                                  public string Name { get; set; } = string.Empty;

                                  public string Description { get; set; } = string.Empty;

                                  [Range(0.01, 999999.99)]
                                  public decimal Price { get; set; }

                                  public string Sku { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Annotated properties should be included
        Assert.Contains("public static partial class Name", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Price", output, StringComparison.Ordinal);

        // Unannotated properties should NOT be included by default
        Assert.DoesNotContain("public static partial class Description", output, StringComparison.Ordinal);
        Assert.DoesNotContain("public static partial class Sku", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_When_No_Properties_Have_Supported_Annotations()
    {
        const string source = """
                              using System;

                              namespace TestNamespace;

                              [AttributeUsage(AttributeTargets.Property)]
                              public class MyCustomAttribute : Attribute { }

                              public class Product
                              {
                                  [MyCustom]
                                  public string Name { get; set; } = string.Empty;

                                  public decimal Price { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("AnnotationConstants", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Ignore_Properties_With_Only_Non_DataAnnotation_Attributes()
    {
        const string source = """
                              using System;
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              [AttributeUsage(AttributeTargets.Property)]
                              public class CustomAttribute : Attribute { }

                              public class Product
                              {
                                  [Required]
                                  public string Name { get; set; } = string.Empty;

                                  [Custom]
                                  public string Description { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Name with Required should be included
        Assert.Contains("public static partial class Name", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsRequired = true;", output, StringComparison.Ordinal);

        // Description with only custom attribute should NOT be included
        Assert.DoesNotContain("public static partial class Description", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Separate_Files_Per_Type()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Required]
                                  public string Name { get; set; } = string.Empty;
                              }

                              public class Customer
                              {
                                  [EmailAddress]
                                  public string Email { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Both types should have their own AnnotationConstants class
        Assert.Contains("public static partial class Product", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Customer", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Partial_Class_Declaration()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public partial class Product
                              {
                                  [Required]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public static partial class AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Product", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Nested_Types()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Outer
                              {
                                  [Required]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public static partial class Outer", output, StringComparison.Ordinal);
    }
}