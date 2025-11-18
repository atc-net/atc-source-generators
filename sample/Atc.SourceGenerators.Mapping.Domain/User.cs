namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a user in the system.
/// </summary>
[MapTo(typeof(UserDto))]
[MapTo(typeof(UserFlatDto), EnableFlattening = true)]
[MapTo(typeof(UserEntity), Bidirectional = true)]
public partial class User
{
    /// <summary>
    /// Gets or sets the user's unique identifier.
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
    /// Gets or sets the user's preferred name (nickname or chosen name).
    /// Maps to DisplayName in UserDto for API responses.
    /// </summary>
    [MapProperty("DisplayName")]
    public string PreferredName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's status.
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the user's physical address.
    /// </summary>
    public Address? Address { get; set; }

    /// <summary>
    /// Gets or sets when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the user was last updated.
    /// Internal audit field - excluded from DTO mapping.
    /// </summary>
    [MapIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user's password hash.
    /// Sensitive data - never map to DTOs.
    /// </summary>
    [MapIgnore]
    public byte[] PasswordHash { get; set; } = [];

    /// <summary>
    /// Gets or sets internal notes for administrative purposes.
    /// Internal field - excluded from all mappings.
    /// </summary>
    [MapIgnore]
    public string InternalNotes { get; set; } = string.Empty;
}