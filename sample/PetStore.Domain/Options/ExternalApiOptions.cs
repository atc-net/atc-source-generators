namespace PetStore.Domain.Options;

/// <summary>
/// External API endpoint configuration options.
/// Demonstrates:
/// - PostConfigure feature for normalizing API URLs after binding
/// - AlsoRegisterDirectType feature for third-party library compatibility
/// Ensures all URLs are lowercase and properly formatted for consistent API communication.
/// </summary>
/// <remarks>
/// AlsoRegisterDirectType = true allows this class to be injected both as IOptions&lt;ExternalApiOptions&gt;
/// and as ExternalApiOptions directly. This is useful when integrating with third-party API client
/// libraries that expect configuration objects directly in their constructors.
/// </remarks>
[OptionsBinding(
    "ExternalApis",
    ValidateDataAnnotations = true,
    ValidateOnStart = true,
    PostConfigure = nameof(NormalizeUrls),
    AlsoRegisterDirectType = true)]
public partial class ExternalApiOptions
{
    /// <summary>
    /// Gets or sets the base URL for the payment processing API.
    /// </summary>
    [Required]
    [Url]
    public string PaymentApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the inventory management API.
    /// </summary>
    [Required]
    [Url]
    public string InventoryApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the shipping provider API.
    /// </summary>
    [Required]
    [Url]
    public string ShippingApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the analytics endpoint URL.
    /// </summary>
    public string AnalyticsUrl { get; set; } = string.Empty;

    /// <summary>
    /// Post-configuration method to normalize all API URLs.
    /// Converts URLs to lowercase and ensures they don't end with trailing slashes
    /// for consistent API endpoint construction.
    /// Signature: static void MethodName(TOptions options)
    /// </summary>
    internal static void NormalizeUrls(ExternalApiOptions options)
    {
        // Normalize all URLs to lowercase and remove trailing slashes
        options.PaymentApiUrl = NormalizeUrl(options.PaymentApiUrl);
        options.InventoryApiUrl = NormalizeUrl(options.InventoryApiUrl);
        options.ShippingApiUrl = NormalizeUrl(options.ShippingApiUrl);
        options.AnalyticsUrl = NormalizeUrl(options.AnalyticsUrl);
    }

    /// <summary>
    /// Normalizes a URL by converting to lowercase and removing trailing slashes.
    /// This ensures consistent URL formatting for API endpoint construction.
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        // Convert to lowercase for case-insensitive URL matching
        var normalized = url.ToLowerInvariant();

        // Remove trailing slash for consistent endpoint path construction
        // e.g., baseUrl + "/endpoint" works consistently whether baseUrl has trailing slash or not
        normalized = normalized.TrimEnd('/');

        return normalized;
    }
}
