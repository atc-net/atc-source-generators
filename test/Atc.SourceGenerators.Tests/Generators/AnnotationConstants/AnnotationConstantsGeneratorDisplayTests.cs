namespace Atc.SourceGenerators.Tests.Generators.AnnotationConstants;

public partial class AnnotationConstantsGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Display_Name_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(Name = "Product Name")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string DisplayName = \"Product Name\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Display_Description_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(Description = "The name of the product")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string Description = \"The name of the product\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Display_ShortName_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(ShortName = "Name")]
                                  public string ProductName { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string ShortName = \"Name\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Display_GroupName_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(GroupName = "Basic Info")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string GroupName = \"Basic Info\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Display_Prompt_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(Prompt = "Enter product name")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string Prompt = \"Enter product name\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Display_Order_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(Order = 1)]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const int Order = 1;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_All_Display_Properties()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(
                                      Name = "Product Name",
                                      Description = "The product name",
                                      ShortName = "Name",
                                      GroupName = "Basic",
                                      Prompt = "Enter name",
                                      Order = 1)]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string DisplayName = \"Product Name\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string Description = \"The product name\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string ShortName = \"Name\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string GroupName = \"Basic\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string Prompt = \"Enter name\";", output, StringComparison.Ordinal);
        Assert.Contains("public const int Order = 1;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Escape_Special_Characters_In_Display_Values()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(Name = "Product \"Name\"", Description = "Line1\nLine2")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string DisplayName = \"Product \\\"Name\\\"\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string Description = \"Line1\\nLine2\";", output, StringComparison.Ordinal);
    }
}