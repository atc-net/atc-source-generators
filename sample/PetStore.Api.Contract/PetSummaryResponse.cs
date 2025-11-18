namespace PetStore.Api.Contract;

/// <summary>
/// Summary response model for a pet with flattened owner properties.
/// Demonstrates property flattening feature.
/// </summary>
public class PetSummaryResponse
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
    /// Gets or sets the pet's display name (friendly name for UI).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's species (e.g., Dog, Cat, Bird).
    /// </summary>
    public string Species { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's age in years.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Gets or sets the pet's status.
    /// </summary>
    public PetStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the owner's name (flattened from Owner.Name).
    /// </summary>
    public string? OwnerName { get; set; }

    /// <summary>
    /// Gets or sets the owner's email (flattened from Owner.Email).
    /// </summary>
    public string? OwnerEmail { get; set; }

    /// <summary>
    /// Gets or sets the owner's phone (flattened from Owner.Phone).
    /// </summary>
    public string? OwnerPhone { get; set; }

    /// <summary>
    /// Gets or sets when the pet was added to the system.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}