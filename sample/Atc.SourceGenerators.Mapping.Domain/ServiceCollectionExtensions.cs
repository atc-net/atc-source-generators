namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Extension methods for service collection registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds user services to the service collection.
    /// Note: You must register an IUserRepository implementation separately.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserServices(
        this IServiceCollection services)
    {
        services.AddSingleton<UserService>();
        return services;
    }
}