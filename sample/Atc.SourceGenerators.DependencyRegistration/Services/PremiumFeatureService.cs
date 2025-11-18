namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Premium feature service - registered only when Features:UsePremiumFeatures is true.
/// </summary>
[Registration(Lifetime.Scoped, As = typeof(IPremiumFeatureService), Condition = "Features:UsePremiumFeatures")]
public class PremiumFeatureService : IPremiumFeatureService
{
    public void ExecutePremiumFeature(string featureName)
    {
        Console.WriteLine($"  ✨ Executing premium feature: {featureName}");
        Console.WriteLine($"  This service is only registered when Features:UsePremiumFeatures is true in configuration.");
    }
}
