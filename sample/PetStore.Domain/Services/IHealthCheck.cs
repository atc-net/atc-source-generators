namespace PetStore.Domain.Services;

/// <summary>
/// Health check service interface.
/// </summary>
public interface IHealthCheck
{
    /// <summary>
    /// Performs a health check.
    /// </summary>
    /// <returns>True if healthy, otherwise false.</returns>
    Task<bool> CheckHealthAsync();
}