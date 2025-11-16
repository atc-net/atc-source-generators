namespace Atc.SourceGenerators.Mapping.DataAccess.Entities;

/// <summary>
/// Database entity for user.
/// </summary>
public class UserEntity
{
    /// <summary>
    /// Gets or sets the database ID (auto-increment).
    /// </summary>
    public int DatabaseId { get; set; }

    /// <summary>
    /// Gets or sets the user's public unique identifier (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's status (stored as int in DB).
    /// </summary>
    public UserStatusEntity Status { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to address.
    /// </summary>
    public int? AddressId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to address.
    /// </summary>
    public AddressEntity? Address { get; set; }

    /// <summary>
    /// Gets or sets when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the user was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets if the record is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}