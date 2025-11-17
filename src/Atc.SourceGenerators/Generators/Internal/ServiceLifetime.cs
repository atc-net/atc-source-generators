namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Internal enum matching Microsoft.Extensions.DependencyInjection.ServiceLifetime.
/// Used by the generator to avoid taking a dependency on Microsoft.Extensions.DependencyInjection.
/// </summary>
internal enum ServiceLifetime
{
    Singleton = 0,
    Scoped = 1,
    Transient = 2,
}