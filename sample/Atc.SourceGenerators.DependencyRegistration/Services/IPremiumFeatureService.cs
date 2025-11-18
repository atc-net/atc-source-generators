namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Premium feature service interface.
/// </summary>
public interface IPremiumFeatureService
{
    /// <summary>
    /// Executes a premium feature.
    /// </summary>
    void ExecutePremiumFeature(string featureName);
}
