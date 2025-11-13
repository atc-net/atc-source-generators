namespace Atc.SourceGenerators.OptionsBinding.Domain.Options;

/// <summary>
/// Feature flags configuration.
/// Demonstrates Monitor lifetime for hot-reload scenarios.
/// </summary>
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor)]
public partial class FeatureOptions
{
    public bool EnableNewDashboard { get; set; }

    public bool EnableBetaFeatures { get; set; }

    public bool EnableAdvancedSearch { get; set; }

    public int MaxSearchResults { get; set; } = 100;
}