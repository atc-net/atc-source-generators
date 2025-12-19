namespace Atc.SourceGenerators.AnnotationConstants.Models;

/// <summary>
/// Example customer model demonstrating validation and data type annotations.
/// </summary>
public class Customer
{
    [Key]
    [Editable(false)]
    public Guid Id { get; set; }

    [Display(Name = "Full Name", Description = "Customer's full name", Order = 1)]
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email Address", Order = 2)]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number", Order = 3)]
    [Phone]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Website", Order = 4)]
    [Url]
    public string? Website { get; set; }

    [Display(Name = "Credit Card", Order = 5)]
    [CreditCard]
    public string? CreditCardNumber { get; set; }

    [Display(Name = "Notes", Order = 6, Prompt = "Enter additional notes")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [Timestamp]
    [ScaffoldColumn(false)]
    public byte[] RowVersion { get; set; } = [];
}