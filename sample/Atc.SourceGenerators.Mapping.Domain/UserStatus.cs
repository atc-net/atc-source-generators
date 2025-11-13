namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents the status of a user account.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User account is active.
    /// </summary>
    Active = 0,

    /// <summary>
    /// User account is inactive.
    /// </summary>
    Inactive = 1,

    /// <summary>
    /// User account is suspended.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// User account is deleted.
    /// </summary>
    Deleted = 3,
}