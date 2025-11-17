namespace Atc.SourceGenerators.DependencyRegistration.Services.Internal;

/// <summary>
/// Internal utility interface that should be excluded from registration.
/// </summary>
public interface IInternalUtility
{
    void DoInternalWork();
}