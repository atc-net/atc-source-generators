namespace PetStore.Domain.Services;

/// <summary>
/// Default health check implementation using TryAdd registration.
/// Provides a basic health check that always returns healthy.
/// This can be overridden by application code by registering a custom implementation before the library registration.
/// </summary>
[Registration(Lifetime.Singleton, As = typeof(IHealthCheck), TryAdd = true)]
public class DefaultHealthCheck : IHealthCheck
{
    /// <inheritdoc/>
    public Task<bool> CheckHealthAsync()
    {
        Console.WriteLine("DefaultHealthCheck: Performing basic health check (always healthy)");
        return Task.FromResult(true);
    }
}