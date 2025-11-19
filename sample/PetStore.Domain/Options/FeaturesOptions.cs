namespace PetStore.Domain.Options;

/// <summary>
/// Feature toggle configuration options.
/// Demonstrates configuration change callbacks with Monitor lifetime.
/// Changes to feature flags in appsettings.json are detected automatically.
/// </summary>
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
public partial class FeaturesOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the new UI is enabled.
    /// </summary>
    public bool EnableNewUI { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether advanced search is enabled.
    /// </summary>
    public bool EnableAdvancedSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether pet recommendations are enabled.
    /// </summary>
    public bool EnableRecommendations { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether beta features are enabled.
    /// </summary>
    public bool EnableBetaFeatures { get; set; }

    /// <summary>
    /// Called automatically when the Features configuration section changes.
    /// Requires appsettings.json to have reloadOnChange: true.
    /// </summary>
    internal static void OnFeaturesChanged(
        FeaturesOptions options,
        string? name)
    {
        Console.WriteLine("[OnChange Callback] Feature flags changed:");
        Console.WriteLine($"  EnableNewUI: {options.EnableNewUI}");
        Console.WriteLine($"  EnableAdvancedSearch: {options.EnableAdvancedSearch}");
        Console.WriteLine($"  EnableRecommendations: {options.EnableRecommendations}");
        Console.WriteLine($"  EnableBetaFeatures: {options.EnableBetaFeatures}");
        Console.WriteLine();
    }
}
