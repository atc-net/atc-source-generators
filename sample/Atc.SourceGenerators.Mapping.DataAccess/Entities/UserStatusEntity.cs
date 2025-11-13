namespace Atc.SourceGenerators.Mapping.DataAccess.Entities;

/// <summary>
/// Database representation of user status.
/// </summary>
public enum UserStatusEntity
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