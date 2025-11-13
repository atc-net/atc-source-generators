namespace Atc.SourceGenerators.Mapping.Domain.ApiContracts;

/// <summary>
/// Data transfer object for UserStatus (used for API responses).
/// </summary>
public enum UserStatusDto
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