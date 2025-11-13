namespace PetStore.Domain.Services;

/// <summary>
/// Domain service for pet operations.
/// </summary>
[Registration]
public class PetService : IPetService
{
    private readonly IPetRepository repository;
    private readonly PetStoreOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PetService"/> class.
    /// </summary>
    /// <param name="repository">The pet repository.</param>
    /// <param name="options">The pet store options.</param>
    public PetService(
        IPetRepository repository,
        IOptions<PetStoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.repository = repository;
        this.options = options.Value;
    }

    /// <summary>
    /// Gets a pet by ID.
    /// </summary>
    /// <param name="id">The pet ID.</param>
    /// <returns>The pet domain model, or null if not found.</returns>
    public Pet? GetById(Guid id)
    {
        var entity = repository.GetById(id);
        return entity?.MapToPet();
    }

    /// <summary>
    /// Gets all pets with pagination.
    /// </summary>
    /// <returns>Collection of pet domain models.</returns>
    public IEnumerable<Pet> GetAll() =>
        repository
            .GetAll()
            .Select(e => e.MapToPet())
            .Take(options.MaxPetsPerPage);

    /// <summary>
    /// Gets pets by status.
    /// </summary>
    /// <param name="status">The pet status.</param>
    /// <returns>Collection of pets with the specified status.</returns>
    public IEnumerable<Pet> GetByStatus(Models.PetStatus status)
        => repository
            .GetByStatus((PetStatusEntity)status)
            .Select(e => e.MapToPet());

    /// <summary>
    /// Creates a new pet.
    /// </summary>
    /// <param name="request">The create pet request.</param>
    /// <returns>The created pet.</returns>
    public Pet CreatePet(CreatePetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            Age = request.Age,
            Status = Models.PetStatus.Available,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var entity = pet.MapToPetEntity();
        var createdEntity = repository.Create(entity);
        return createdEntity.MapToPet();
    }
}