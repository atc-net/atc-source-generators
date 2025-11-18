namespace PetStore.Api.Contract;

/// <summary>
/// Lightweight DTO for pet list items optimized for IQueryable projections.
/// Contains only simple properties (no nested objects) to support EF Core server-side projection.
/// Perfect for list/grid views where you need to fetch minimal data efficiently.
/// </summary>
public class PetListItemDto
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
    /// Gets or sets the pet's display name (mapped from NickName).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

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
}