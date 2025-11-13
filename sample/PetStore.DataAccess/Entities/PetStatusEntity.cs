namespace PetStore.DataAccess.Entities;

/// <summary>
/// Represents the status of a pet in the database.
/// </summary>
public enum PetStatusEntity
{
    /// <summary>
    /// Pet is available for adoption.
    /// </summary>
    Available = 0,

    /// <summary>
    /// Pet is pending adoption.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Pet has been adopted.
    /// </summary>
    Adopted = 2,
}