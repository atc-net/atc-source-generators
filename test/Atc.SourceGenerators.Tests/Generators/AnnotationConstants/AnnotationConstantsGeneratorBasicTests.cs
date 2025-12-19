namespace Atc.SourceGenerators.Tests.Generators.AnnotationConstants;

public partial class AnnotationConstantsGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Constants_For_Class_With_Required_Attribute()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Required]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Product", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Name", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsRequired = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Constants_For_Record_With_Required_Attribute()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public record Product
                              {
                                  [Required]
                                  public string Name { get; init; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("AnnotationConstants", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Product", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Name", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsRequired = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_For_Class_Without_Annotations()
    {
        const string source = """
                              namespace TestNamespace;

                              public class Product
                              {
                                  public string Name { get; set; } = string.Empty;
                                  public decimal Price { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("AnnotationConstants", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_For_Properties_Without_Supported_Attributes()
    {
        const string source = """
                              using System;

                              namespace TestNamespace;

                              [AttributeUsage(AttributeTargets.Property)]
                              public class CustomAttribute : Attribute { }

                              public class Product
                              {
                                  [Custom]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("AnnotationConstants", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_For_Multiple_Properties()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Required]
                                  [StringLength(100)]
                                  public string Name { get; set; } = string.Empty;

                                  [Required]
                                  [Range(0.01, 999999.99)]
                                  public decimal Price { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public static partial class Name", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Price", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Correct_Namespace()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace MyCompany.MyProduct.Models;

                              public class Customer
                              {
                                  [Required]
                                  public string Email { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("namespace MyCompany.MyProduct.Models;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Multiple_Types_In_Same_Namespace()
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
                                  [Required]
                                  public string Email { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public static partial class Product", output, StringComparison.Ordinal);
        Assert.Contains("public static partial class Customer", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Include_Generated_Code_Attributes()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Required]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("[global::System.CodeDom.Compiler.GeneratedCode", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.ComponentModel.EditorBrowsable", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.Runtime.CompilerServices.CompilerGenerated]", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.Diagnostics.DebuggerNonUserCode]", output, StringComparison.Ordinal);
        Assert.Contains("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Cross_Namespace_Types()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace Domain.Models
                              {
                                  public class Product
                                  {
                                      [Required]
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }

                              namespace Api.Contracts
                              {
                                  public class ProductDto
                                  {
                                      [Required]
                                      public string Name { get; set; } = string.Empty;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("namespace Domain.Models;", output, StringComparison.Ordinal);
        Assert.Contains("namespace Api.Contracts;", output, StringComparison.Ordinal);
    }
}