namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// API configuration options with explicit section name.
/// </summary>
[OptionsBinding("App:Api")]
public partial class ApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}