namespace Atc.SourceGenerators.AnnotationConstants.Models;

/// <summary>
/// Example model demonstrating the Compare attribute.
/// </summary>
public class PasswordReset
{
    [Display(Name = "New Password")]
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Confirm Password")]
    [Required]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}