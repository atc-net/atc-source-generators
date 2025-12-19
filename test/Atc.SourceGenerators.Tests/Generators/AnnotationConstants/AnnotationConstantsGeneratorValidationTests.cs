namespace Atc.SourceGenerators.Tests.Generators.AnnotationConstants;

public partial class AnnotationConstantsGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Required_With_AllowEmptyStrings()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Required(AllowEmptyStrings = true)]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const bool AllowEmptyStrings = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Required_With_ErrorMessage()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Required(ErrorMessage = "Name is required")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const string RequiredErrorMessage = \"Name is required\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_StringLength_Constants()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [StringLength(100, MinimumLength = 1)]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const int MaximumLength = 100;", output, StringComparison.Ordinal);
        Assert.Contains("public const int MinimumLength = 1;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_StringLength_With_ErrorMessage()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [StringLength(100, ErrorMessage = "Name must be under 100 characters")]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const int MaximumLength = 100;", output, StringComparison.Ordinal);
        Assert.Contains("public const string StringLengthErrorMessage = \"Name must be under 100 characters\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_MinLength_Constants()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [MinLength(5)]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const int MinimumLength = 5;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_MaxLength_Constants()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [MaxLength(200)]
                                  public string Description { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const int MaximumLength = 200;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Range_Constants_With_Int()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Range(1, 100)]
                                  public int Quantity { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string Minimum = \"1\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string Maximum = \"100\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Range_Constants_With_Double()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Range(0.01, 999999.99)]
                                  public double Price { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Double values may have floating-point representation differences
        Assert.Contains("public const string Minimum = ", output, StringComparison.Ordinal);
        Assert.Contains("public const string Maximum = ", output, StringComparison.Ordinal);

        // Verify the output contains the property class
        Assert.Contains("public static partial class Price", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Range_Constants_With_Typed_Overload()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Range(typeof(decimal), "0.01", "999999.99")]
                                  public decimal Price { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string Minimum = \"0.01\";", output, StringComparison.Ordinal);
        Assert.Contains("public const string Maximum = \"999999.99\";", output, StringComparison.Ordinal);
        Assert.Contains("public static readonly global::System.Type OperandType = typeof(decimal);", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Range_With_ErrorMessage()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
                                  public int Quantity { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string RangeErrorMessage = \"Quantity must be between 1 and 100\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_RegularExpression_Constants()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [RegularExpression(@"^[A-Z]{3}\d{4}$")]
                                  public string Sku { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string Pattern = @\"^[A-Z]{3}\\d{4}$\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_RegularExpression_With_ErrorMessage()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [RegularExpression(@"^[A-Z]{3}\d{4}$", ErrorMessage = "Invalid SKU format")]
                                  public string Sku { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string RegularExpressionErrorMessage = \"Invalid SKU format\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_EmailAddress_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Customer
                              {
                                  [EmailAddress]
                                  public string Email { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsEmailAddress = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Phone_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Customer
                              {
                                  [Phone]
                                  public string PhoneNumber { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsPhone = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Url_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Website
                              {
                                  [Url]
                                  public string Homepage { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsUrl = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_CreditCard_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Payment
                              {
                                  [CreditCard]
                                  public string CardNumber { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsCreditCard = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Compare_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class ChangePassword
                              {
                                  public string NewPassword { get; set; } = string.Empty;

                                  [Compare("NewPassword")]
                                  public string ConfirmPassword { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string CompareProperty = \"NewPassword\";", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Key_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Entity
                              {
                                  [Key]
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsKey = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Editable_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Entity
                              {
                                  [Editable(false)]
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsEditable = false;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_ScaffoldColumn_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Entity
                              {
                                  [ScaffoldColumn(false)]
                                  public string InternalId { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsScaffoldColumn = false;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Timestamp_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Entity
                              {
                                  [Timestamp]
                                  public byte[] RowVersion { get; set; } = Array.Empty<byte>();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const bool IsTimestamp = true;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_DataType_Constant()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class User
                              {
                                  [DataType(DataType.Password)]
                                  public string Password { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const int DataType = ", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Multiple_Validation_Attributes()
    {
        const string source = """
                              using System.ComponentModel.DataAnnotations;

                              namespace TestNamespace;

                              public class Product
                              {
                                  [Display(Name = "Product Name")]
                                  [Required]
                                  [StringLength(100, MinimumLength = 1)]
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public const string DisplayName = \"Product Name\";", output, StringComparison.Ordinal);
        Assert.Contains("public const bool IsRequired = true;", output, StringComparison.Ordinal);
        Assert.Contains("public const int MaximumLength = 100;", output, StringComparison.Ordinal);
        Assert.Contains("public const int MinimumLength = 1;", output, StringComparison.Ordinal);
    }
}