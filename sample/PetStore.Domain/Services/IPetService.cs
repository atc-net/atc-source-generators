namespace PetStore.Domain.Services;

/// <summary>
/// Service interface for pet operations.
/// </summary>
public interface IPetService
{
    /// <summary>
    /// Gets a pet by ID.
    /// </summary>
    /// <param name="id">The pet ID.</param>
    /// <returns>The pet domain model, or null if not found.</returns>
    Pet? GetById(Guid id);

    /// <summary>
    /// Gets all pets with pagination.
    /// </summary>
    /// <returns>Collection of pet domain models.</returns>
    IEnumerable<Pet> GetAll();

    /// <summary>
    /// Gets pets by status.
    /// </summary>
    /// <param name="status">The pet status.</param>
    /// <returns>Collection of pets with the specified status.</returns>
    IEnumerable<Pet> GetByStatus(Models.PetStatus status);

    /// <summary>
    /// Creates a new pet.
    /// </summary>
    /// <param name="request">The create pet request.</param>
    /// <returns>The created pet.</returns>
    Pet CreatePet(CreatePetRequest request);
}