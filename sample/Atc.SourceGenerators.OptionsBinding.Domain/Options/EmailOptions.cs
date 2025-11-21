namespace Atc.SourceGenerators.OptionsBinding.Domain.Options;

/// <summary>
/// Email service configuration options.
/// Demonstrates const SectionName usage (2nd priority).
/// </summary>
[OptionsBinding(ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    [EmailAddress]
    public string FromAddress { get; set; } = string.Empty;

    [Required]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string? Username { get; set; }

    public string? Password { get; set; }
}