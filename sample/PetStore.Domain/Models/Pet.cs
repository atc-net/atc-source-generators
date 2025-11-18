namespace PetStore.Domain.Models;

/// <summary>
/// Domain model for a pet.
/// </summary>
[MapTo(typeof(PetResponse), BeforeMap = nameof(ValidatePet), AfterMap = nameof(EnrichPetResponse))]
[MapTo(typeof(PetSummaryResponse), EnableFlattening = true)]
[MapTo(typeof(PetDetailsDto))]
[MapTo(typeof(UpdatePetRequest))]
[MapTo(typeof(PetEntity), Bidirectional = true, UpdateTarget = true)]
[MapTo(typeof(PetListItemDto), GenerateProjection = true)]
public partial class Pet
{
    /// <summary>
    /// Gets or sets the pet's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the pet's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's nickname (friendly name).
    /// Maps to DisplayName in PetResponse for API responses.
    /// </summary>
    [MapProperty("DisplayName")]
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's species (e.g., Dog, Cat, Bird).
    /// </summary>
    public string Species { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's breed.
    /// </summary>
    public string Breed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's age in years.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Gets or sets the pet's status.
    /// </summary>
    public PetStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the pet's owner information.
    /// </summary>
    public Owner? Owner { get; set; }

    /// <summary>
    /// Gets or sets when the pet was added to the system.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the pet was last modified.
    /// Internal audit field - excluded from API response.
    /// </summary>
    [MapIgnore]
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets who last modified the pet.
    /// Internal audit field - excluded from API response.
    /// </summary>
    [MapIgnore]
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the pet's offspring/children.
    /// </summary>
    public IList<Pet> Children { get; set; } = new List<Pet>();

    /// <summary>
    /// BeforeMap hook: Validates the pet data before mapping to PetResponse.
    /// This demonstrates validation that ensures data integrity before API responses.
    /// </summary>
    /// <param name="source">The source Pet object to validate.</param>
    /// <exception cref="ArgumentException">Thrown when pet data is invalid.</exception>
    internal static void ValidatePet(Pet source)
    {
        if (string.IsNullOrWhiteSpace(source.Name))
        {
            throw new ArgumentException("Pet name cannot be empty", nameof(source));
        }

        if (string.IsNullOrWhiteSpace(source.Species))
        {
            throw new ArgumentException($"Pet '{source.Name}' must have a species specified", nameof(source));
        }

        if (source.Age < 0)
        {
            throw new ArgumentException($"Pet '{source.Name}' cannot have a negative age", nameof(source));
        }
    }

    /// <summary>
    /// AfterMap hook: Enriches the PetResponse with computed properties after mapping.
    /// This demonstrates post-processing to add API-specific data.
    /// </summary>
    /// <param name="source">The source Pet object.</param>
    /// <param name="target">The target PetResponse object to enrich.</param>
    internal static void EnrichPetResponse(
        Pet source,
        PetResponse target)
    {
        // Create a formatted description for the pet
        target.Description = !string.IsNullOrWhiteSpace(source.Breed)
            ? $"{source.Name} is a {source.Age}-year-old {source.Breed} {source.Species}"
            : $"{source.Name} is a {source.Age}-year-old {source.Species}";

        // Add age category classification
        target.AgeCategory = source.Age switch
        {
            < 2 => "Young",
            < 7 => "Adult",
            < 12 => "Mature",
            _ => "Senior",
        };
    }
}