namespace PetStore.Domain.Models;

/// <summary>
/// Represents a pet owner.
/// </summary>
public partial class Owner
{
    /// <summary>
    /// Gets or sets the owner's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner's phone number.
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}
