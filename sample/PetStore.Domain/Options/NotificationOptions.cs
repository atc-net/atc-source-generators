namespace PetStore.Domain.Options;

/// <summary>
/// Notification channel options with support for multiple named configurations.
/// This class demonstrates the ChildSections feature which provides a concise way to create
/// multiple named instances from child configuration sections.
/// It also demonstrates the ConfigureAll feature which sets default values for ALL named instances.
/// </summary>
/// <remarks>
/// Using ChildSections = new[] { "Email", "SMS", "Push" } is equivalent to:
/// [OptionsBinding("Notifications:Email", Name = "Email")]
/// [OptionsBinding("Notifications:SMS", Name = "SMS")]
/// [OptionsBinding("Notifications:Push", Name = "Push")]
/// </remarks>
[OptionsBinding("Notifications", ChildSections = new[] { "Email", "SMS", "Push" }, ConfigureAll = nameof(SetCommonDefaults))]
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

    /// <summary>
    /// Gets or sets the rate limit (max notifications per minute).
    /// </summary>
    public int RateLimitPerMinute { get; set; }

    /// <summary>
    /// Sets common default values for ALL notification channels.
    /// This method runs BEFORE individual configuration binding, ensuring all channels
    /// have consistent baseline settings that can be overridden per channel.
    /// </summary>
    internal static void SetCommonDefaults(NotificationOptions options)
    {
        // Set common defaults for all notification channels
        options.TimeoutSeconds = 30;
        options.MaxRetries = 3;
        options.RateLimitPerMinute = 60;
        options.Enabled = true;
    }
}
