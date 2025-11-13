namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Repository interface for user operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user domain model, or null if not found.</returns>
    User? GetById(Guid id);

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>Collection of user domain models.</returns>
    IEnumerable<User> GetAll();
}