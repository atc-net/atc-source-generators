namespace Atc.SourceGenerators.AnnotationConstants.Models;

/// <summary>
/// Example product model demonstrating DataAnnotation attributes.
/// The AnnotationConstantsGenerator will create compile-time accessible constants
/// for all the annotation metadata.
/// </summary>
public class Product
{
    [Display(Name = "Product Name", Description = "The display name of the product")]
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description", Description = "Detailed product description")]
    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Price", Description = "Product price in USD")]
    [Required]
    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    public decimal Price { get; set; }

    [Display(Name = "SKU", ShortName = "SKU")]
    [Required]
    [RegularExpression(@"^[A-Z]{3}-\d{6}$", ErrorMessage = "SKU must be in format XXX-123456")]
    public string Sku { get; set; } = string.Empty;

    [Display(Name = "Stock Quantity", GroupName = "Inventory")]
    [Required]
    [Range(0, 100000)]
    public int StockQuantity { get; set; }

    [Display(Name = "Category", GroupName = "Classification")]
    [Required]
    public string Category { get; set; } = string.Empty;
}