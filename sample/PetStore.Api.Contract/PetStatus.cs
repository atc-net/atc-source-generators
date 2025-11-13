namespace PetStore.Api.Contract;

/// <summary>
/// Represents the status of a pet.
/// </summary>
public enum PetStatus
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