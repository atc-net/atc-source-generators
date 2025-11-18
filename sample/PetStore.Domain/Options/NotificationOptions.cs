namespace PetStore.Domain.Options;

/// <summary>
/// Notification channel options with support for multiple named configurations.
/// Demonstrates Named Options feature for configuring multiple notification channels
/// (Email, SMS, Push) with different settings.
/// </summary>
[OptionsBinding("Notifications:Email", Name = "Email")]
[OptionsBinding("Notifications:SMS", Name = "SMS")]
[OptionsBinding("Notifications:Push", Name = "Push")]
public partial class NotificationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether this notification channel is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the provider name for this notification channel.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key or credential for the provider.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender identifier (email address, phone number, or app ID).
    /// </summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds for notification delivery.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
