namespace Atc.SourceGenerators.Mapping.DataAccess;

/// <summary>
/// In-memory user repository for demo purposes.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Dictionary<Guid, UserEntity> users = new()
    {
        {
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            new UserEntity
            {
                DatabaseId = 1,
                Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = UserStatusEntity.Active,
                Address = new AddressEntity
                {
                    Id = 1,
                    Street = "123 Main St",
                    City = "Springfield",
                    State = "IL",
                    PostalCode = "62701",
                    Country = "USA",
                    CreatedAt = DateTime.Parse("2024-01-15T10:00:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind),
                    UpdatedAt = DateTime.Parse("2025-01-13T15:30:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind),
                },
                CreatedAt = DateTimeOffset.Parse("2024-01-15T10:00:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind),
                UpdatedAt = DateTimeOffset.Parse("2025-01-13T15:30:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind),
                IsDeleted = false,
                RowVersion = [],
            }
        },
        {
            Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
            new UserEntity
            {
                DatabaseId = 2,
                Id = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = UserStatusEntity.Active,
                Address = new AddressEntity
                {
                    Id = 2,
                    Street = "456 Oak Avenue",
                    City = "Chicago",
                    State = "IL",
                    PostalCode = "60601",
                    Country = "USA",
                    CreatedAt = DateTime.Parse("2024-03-20T14:22:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind),
                },
                CreatedAt = DateTimeOffset.Parse("2024-03-20T14:22:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind),
                IsDeleted = false,
                RowVersion = [],
            }
        },
    };

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user domain model, or null if not found.</returns>
    public User? GetById(Guid id)
        => !users.TryGetValue(id, out var entity)
            ? null
            : entity.MapToUser();

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>Collection of user domain models.</returns>
    public IEnumerable<User> GetAll()
        => users.Values.Select(e => e.MapToUser());
}