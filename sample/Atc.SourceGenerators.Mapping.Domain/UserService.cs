namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Domain service for user operations.
/// </summary>
public class UserService
{
    private readonly IUserRepository repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="repository">The user repository.</param>
    public UserService(IUserRepository repository)
    {
        this.repository = repository;
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user domain model, or null if not found.</returns>
    public User? GetById(Guid id)
        => repository.GetById(id);

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>Collection of user domain models.</returns>
    public IEnumerable<User> GetAll()
        => repository.GetAll();
}