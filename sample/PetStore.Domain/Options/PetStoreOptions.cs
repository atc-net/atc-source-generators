namespace PetStore.Domain.Options;

/// <summary>
/// Configuration options for the pet store.
/// Demonstrates ErrorOnMissingKeys for critical configuration that must be present.
/// </summary>
[OptionsBinding("PetStore", ValidateDataAnnotations = true, ValidateOnStart = true, ErrorOnMissingKeys = true, Validator = typeof(Validators.PetStoreOptionsValidator))]
public partial class PetStoreOptions
{
    /// <summary>
    /// Gets or sets the maximum number of pets to display per page.
    /// </summary>
    [Range(1, 100)]
    public int MaxPetsPerPage { get; set; } = 20;

    /// <summary>
    /// Gets or sets the store name.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string StoreName { get; set; } = "Furry Friends Pet Store";

    /// <summary>
    /// Gets or sets whether to enable automatic status updates.
    /// </summary>
    public bool EnableAutoStatusUpdates { get; set; } = true;
}