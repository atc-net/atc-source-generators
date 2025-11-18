namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Flattened data transfer object for User (demonstrates property flattening).
/// Uses flattened address properties instead of nested Address object.
/// </summary>
public class UserFlatDto
{
    /// <summary>
    /// Gets or sets the user's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display name (preferred name for UI).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's status.
    /// </summary>
    public UserStatusDto Status { get; set; }

    /// <summary>
    /// Gets or sets the street address (flattened from Address.Street).
    /// </summary>
    public string? AddressStreet { get; set; }

    /// <summary>
    /// Gets or sets the city (flattened from Address.City).
    /// </summary>
    public string? AddressCity { get; set; }

    /// <summary>
    /// Gets or sets the state or province (flattened from Address.State).
    /// </summary>
    public string? AddressState { get; set; }

    /// <summary>
    /// Gets or sets the postal code (flattened from Address.PostalCode).
    /// </summary>
    public string? AddressPostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country (flattened from Address.Country).
    /// </summary>
    public string? AddressCountry { get; set; }

    /// <summary>
    /// Gets or sets when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}