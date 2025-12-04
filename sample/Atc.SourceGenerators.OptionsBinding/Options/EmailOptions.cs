namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Email server options with support for multiple named configurations.
/// This class demonstrates the ChildSections feature which provides a concise way to create
/// multiple named instances from child configuration sections.
/// It also demonstrates the ConfigureAll feature which sets default values for ALL named instances.
/// </summary>
/// <remarks>
/// Using ChildSections = new[] { "Primary", "Secondary", "Fallback" } is equivalent to:
/// [OptionsBinding("Email:Primary", Name = "Primary")]
/// [OptionsBinding("Email:Secondary", Name = "Secondary")]
/// [OptionsBinding("Email:Fallback", Name = "Fallback")]
/// </remarks>
[OptionsBinding("Email", ChildSections = ["Primary", "Secondary", "Fallback"], ConfigureAll = nameof(SetDefaults))]
public partial class EmailOptions
{
    /// <summary>
    /// Gets or sets the SMTP server address.
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL/TLS.
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// Configures default values for ALL email instances.
    /// This method runs BEFORE individual configurations, allowing defaults to be set
    /// that can be overridden by specific configuration sections.
    /// </summary>
    internal static void SetDefaults(EmailOptions options)
    {
        // Set defaults for all email configurations
        options.UseSsl = true;
        options.TimeoutSeconds = 30;
        options.MaxRetries = 3;
        options.Port = 587;
    }
}