namespace PetStore.Domain.Models;

/// <summary>
/// Domain model for a pet.
/// </summary>
[MapTo(typeof(PetResponse))]
[MapTo(typeof(PetEntity), Bidirectional = true)]
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
}