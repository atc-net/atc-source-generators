namespace PetStore.Api.Contract;

/// <summary>
/// Request model for updating a pet with required properties (demonstrates required property validation).
/// </summary>
/// <remarks>
/// This request uses the 'required' keyword (C# 11+) to mark properties that must be set.
/// The mapping generator validates at compile-time that all required properties have mappings from the source type.
/// If the source type is missing a property that maps to a required target property, you'll get diagnostic ATCMAP004:
/// "Required property '{PropertyName}' on target type '{TargetType}' has no mapping from source type '{SourceType}'"
/// </remarks>
public class UpdatePetRequest
{
    /// <summary>
    /// Gets or sets the pet's name (required).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the pet's display name (nickname).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pet's species (required).
    /// </summary>
    public required string Species { get; set; }

    /// <summary>
    /// Gets or sets the pet's age (optional).
    /// </summary>
    public int? Age { get; set; }
}