namespace Atc.SourceGenerators.DependencyRegistration.Services.Internal;

/// <summary>
/// Internal utility service that should be excluded from registration via RegistrationFilter.
/// </summary>
[Registration]
public class InternalUtility : IInternalUtility
{
    public void DoInternalWork()
    {
        Console.WriteLine("[InternalUtility] This service should be excluded from registration!");
    }
}