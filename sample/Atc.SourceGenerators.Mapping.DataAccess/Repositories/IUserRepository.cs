namespace Atc.SourceGenerators.Mapping.DataAccess.Repositories;

/// <summary>
/// Repository interface for user operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user entity by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user entity, or null if not found.</returns>
    UserEntity? GetById(Guid id);

    /// <summary>
    /// Gets all user entities.
    /// </summary>
    /// <returns>Collection of user entities.</returns>
    IEnumerable<UserEntity> GetAll();
}