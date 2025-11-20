namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Legacy integration configuration options.
/// Demonstrates Feature #13: Direct type registration (AlsoRegisterDirectType).
/// This feature allows registering the options class both as IOptions&lt;T&gt; AND as the direct type T.
/// Useful for migration scenarios or third-party library compatibility.
/// </summary>
/// <remarks>
/// <para>
/// <b>Use case:</b> You're integrating with a legacy library that expects the options class directly
/// in its constructor, not wrapped in IOptions&lt;T&gt;. Instead of creating a wrapper service,
/// you can use AlsoRegisterDirectType = true to register both patterns.
/// </para>
/// <para>
/// <b>Trade-offs:</b>
/// - Direct injection gets a snapshot at resolution time
/// - No change detection - configuration updates won't be reflected
/// - Should be used sparingly for migration/compatibility only
/// </para>
/// </remarks>
[OptionsBinding("LegacyIntegration", ValidateDataAnnotations = true, AlsoRegisterDirectType = true)]
public partial class LegacyIntegrationOptions
{
    /// <summary>
    /// Gets or sets the API endpoint for the legacy system.
    /// </summary>
    [Required]
    [Url]
    public string ApiEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    [Required]
    [MinLength(32)]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds for API calls.
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL/TLS for connections.
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;
}