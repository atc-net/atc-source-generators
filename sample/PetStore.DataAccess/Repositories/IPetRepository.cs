namespace PetStore.DataAccess.Repositories;

/// <summary>
/// Repository interface for pet operations.
/// </summary>
public interface IPetRepository
{
    /// <summary>
    /// Gets a pet by ID.
    /// </summary>
    /// <param name="id">The pet ID.</param>
    /// <returns>The pet domain model, or null if not found.</returns>
    PetEntity? GetById(Guid id);

    /// <summary>
    /// Gets all pets.
    /// </summary>
    /// <returns>Collection of pet domain models.</returns>
    IEnumerable<PetEntity> GetAll();

    /// <summary>
    /// Gets pets by status.
    /// </summary>
    /// <param name="status">The pet status.</param>
    /// <returns>Collection of pets with the specified status.</returns>
    IEnumerable<PetEntity> GetByStatus(Entities.PetStatusEntity status);

    /// <summary>
    /// Creates a new pet.
    /// </summary>
    /// <param name="pet">The pet to create.</param>
    /// <returns>The created pet with generated ID.</returns>
    PetEntity Create(PetEntity pet);
}