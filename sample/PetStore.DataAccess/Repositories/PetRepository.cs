// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable RedundantArgumentDefaultValue
namespace PetStore.DataAccess.Repositories;

/// <summary>
/// In-memory repository implementation for pet operations.
/// </summary>
[Registration(Lifetime.Singleton)]
public class PetRepository : IPetRepository
{
    private readonly Dictionary<Guid, PetEntity> pets;

    /// <summary>
    /// Initializes a new instance of the <see cref="PetRepository"/> class.
    /// </summary>
    public PetRepository()
    {
        // Initialize with sample data
        var pet1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var pet2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var pet3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var pet4Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        pets = new Dictionary<Guid, PetEntity>
        {
            [pet1Id] = new PetEntity
            {
                Id = pet1Id,
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Age = 3,
                Status = PetStatusEntity.Available,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-30),
            },
            [pet2Id] = new PetEntity
            {
                Id = pet2Id,
                Name = "Whiskers",
                Species = "Cat",
                Breed = "Siamese",
                Age = 2,
                Status = PetStatusEntity.Adopted,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-45),
            },
            [pet3Id] = new PetEntity
            {
                Id = pet3Id,
                Name = "Max",
                Species = "Dog",
                Breed = "German Shepherd",
                Age = 5,
                Status = PetStatusEntity.Pending,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-15),
            },
            [pet4Id] = new PetEntity
            {
                Id = pet4Id,
                Name = "Luna",
                Species = "Cat",
                Breed = "Maine Coon",
                Age = 1,
                Status = PetStatusEntity.Available,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
            },
        };
    }

    /// <summary>
    /// Gets a pet by ID.
    /// </summary>
    /// <param name="id">The pet ID.</param>
    /// <returns>The pet domain model, or null if not found.</returns>
    public PetEntity? GetById(Guid id)
        => pets.GetValueOrDefault(id);

    /// <summary>
    /// Gets all pets.
    /// </summary>
    /// <returns>Collection of pet domain models.</returns>
    public IEnumerable<PetEntity> GetAll()
        => pets.Values;

    /// <summary>
    /// Gets pets by status.
    /// </summary>
    /// <param name="status">The pet status.</param>
    /// <returns>Collection of pets with the specified status.</returns>
    public IEnumerable<PetEntity> GetByStatus(PetStatusEntity status)
        => pets.Values
            .Where(e => e.Status == status);

    /// <summary>
    /// Creates a new pet.
    /// </summary>
    /// <param name="pet">The pet to create.</param>
    /// <returns>The created pet with generated ID.</returns>
    public PetEntity Create(PetEntity pet)
    {
        var entity = new PetEntity
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            Age = pet.Age,
            Status = pet.Status,
            CreatedAt = pet.CreatedAt,
        };

        pets[entity.Id] = entity;

        return entity;
    }
}