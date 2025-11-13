namespace Atc.SourceGenerators.DependencyRegistration.Domain.Abstractions;

/// <summary>
/// Service for managing users.
/// </summary>
public interface IUserService
{
    void CreateUser(string name);

    void DeleteUser(string name);
}