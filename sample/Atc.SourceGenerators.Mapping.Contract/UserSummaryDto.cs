namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Summary DTO for user data optimized for IQueryable projections.
/// Contains only simple properties (no nested objects) to support EF Core server-side projection.
/// </summary>
public class UserSummaryDto
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
    /// Gets or sets the user's display name (mapped from PreferredName).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's status.
    /// </summary>
    public UserStatusDto Status { get; set; }

    /// <summary>
    /// Gets or sets when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}