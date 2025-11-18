namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// User registration DTO with required properties (demonstrates required property validation).
/// </summary>
/// <remarks>
/// This DTO uses the 'required' keyword (C# 11+) to mark properties that must be set during initialization.
/// The mapping generator validates at compile-time that all required properties have mappings from the source type.
/// If the source type is missing a property that maps to a required target property, you'll get diagnostic ATCMAP004:
/// "Required property '{PropertyName}' on target type '{TargetType}' has no mapping from source type '{SourceType}'"
/// </remarks>
public class UserRegistrationDto
{
    /// <summary>
    /// Gets or sets the user's email address (required).
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's full name (required).
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Gets or sets the user's phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; set; }
}