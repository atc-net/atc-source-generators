namespace Atc.SourceGenerators.Mapping.Api.Contracts;

/// <summary>
/// Represents the status of a user in API responses.
/// </summary>
public enum UserStatusResponse
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