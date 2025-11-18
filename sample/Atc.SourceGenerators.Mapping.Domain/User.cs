namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a user in the system.
/// </summary>
[MapTo(typeof(UserDto), BeforeMap = nameof(ValidateUser), AfterMap = nameof(EnrichUserDto))]
[MapTo(typeof(UserFlatDto), EnableFlattening = true)]
[MapTo(typeof(UserEntity), Bidirectional = true)]
[MapTo(typeof(UserSummaryDto), GenerateProjection = true)]
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

    /// <summary>
    /// BeforeMap hook: Validates the user data before mapping to UserDto.
    /// This demonstrates validation that throws exceptions for invalid data.
    /// </summary>
    /// <param name="source">The source User object to validate.</param>
    /// <exception cref="ArgumentException">Thrown when user data is invalid.</exception>
    internal static void ValidateUser(User source)
    {
        if (string.IsNullOrWhiteSpace(source.Email))
        {
            throw new ArgumentException("User email cannot be empty", nameof(source));
        }

        if (!source.Email.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException($"Invalid email format: {source.Email}", nameof(source));
        }

        if (string.IsNullOrWhiteSpace(source.FirstName) || string.IsNullOrWhiteSpace(source.LastName))
        {
            throw new ArgumentException("User must have both first name and last name", nameof(source));
        }
    }

    /// <summary>
    /// AfterMap hook: Enriches the UserDto with computed properties after mapping.
    /// This demonstrates post-processing to add data that doesn't exist in the source.
    /// </summary>
    /// <param name="source">The source User object.</param>
    /// <param name="target">The target UserDto object to enrich.</param>
    internal static void EnrichUserDto(
        User source,
        UserDto target)
    {
        // Compute full name from first and last name
        target.FullName = $"{source.FirstName} {source.LastName}";

        // Add user info label combining name and ID
        target.UserInfo = $"{target.FullName} ({source.Id:N})";
    }
}