namespace PetStore.Domain.Models;

/// <summary>
/// Represents the status of a pet in the domain.
/// </summary>
[MapTo(typeof(Api.Contract.PetStatus))]
[MapTo(typeof(PetStatusEntity), Bidirectional = true)]
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