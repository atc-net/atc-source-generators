namespace PetStore.Api.Contract;

/// <summary>
/// Detailed response model for a pet with string-based types (demonstrates type conversion).
/// </summary>
public class PetDetailsDto
{
    /// <summary>
    /// Gets or sets the pet's unique identifier as string (converted from Guid).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's display name (friendly name for UI).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's species.
    /// </summary>
    public string Species { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's age as string (converted from int).
    /// </summary>
    public string Age { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the pet was added as string (converted from DateTimeOffset using ISO 8601 format).
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;
}