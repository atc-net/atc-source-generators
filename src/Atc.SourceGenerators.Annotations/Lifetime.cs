#pragma warning disable IDE0130
namespace Atc.DependencyInjection;

/// <summary>
/// Service lifetime enum matching Microsoft.Extensions.DependencyInjection.ServiceLifetime.
/// </summary>
public enum Lifetime
{
    /// <summary>
    /// Specifies that a single instance of the service will be created.
    /// </summary>
    Singleton = 0,

    /// <summary>
    /// Specifies that a new instance of the service will be created for each scope.
    /// </summary>
    Scoped = 1,

    /// <summary>
    /// Specifies that a new instance of the service will be created every time it is requested.
    /// </summary>
    Transient = 2,
}