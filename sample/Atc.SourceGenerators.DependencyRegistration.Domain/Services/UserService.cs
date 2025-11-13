namespace Atc.SourceGenerators.DependencyRegistration.Domain.Services;

/// <summary>
/// Implementation of IUserService registered with scoped lifetime.
/// Uses auto-detection to register against IUserService interface.
/// </summary>
[Registration(Lifetime.Scoped)]
public class UserService : IUserService
{
    public void CreateUser(string name)
    {
        Console.WriteLine($"Creating user: {name}");
    }

    public void DeleteUser(string name)
    {
        Console.WriteLine($"Deleting user: {name}");
    }
}