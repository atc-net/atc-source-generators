namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// User registration domain model (demonstrates required property validation).
/// </summary>
/// <remarks>
/// This model maps to UserRegistrationDto which has required properties (Email, FullName).
/// The mapping generator validates at compile-time that all required properties are mapped.
/// If this model was missing Email or FullName, you would get diagnostic ATCMAP004 at build time.
/// </remarks>
[MapTo(typeof(UserRegistrationDto))]
public partial class UserRegistration
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    /// <remarks>
    /// This property is REQUIRED in the target DTO, so it must exist here to avoid ATCMAP004 warning.
    /// </remarks>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    /// <remarks>
    /// This property is REQUIRED in the target DTO, so it must exist here to avoid ATCMAP004 warning.
    /// </remarks>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's phone number (optional).
    /// </summary>
    /// <remarks>
    /// This property is OPTIONAL in the target DTO, so omitting it would not generate a warning.
    /// </remarks>
    public string? PhoneNumber { get; set; }
}