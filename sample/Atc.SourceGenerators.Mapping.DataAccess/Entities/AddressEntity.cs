namespace Atc.SourceGenerators.Mapping.DataAccess.Entities;

/// <summary>
/// Database entity for address (maps to Domain.Address).
/// </summary>
[MapTo(typeof(Address))]
public partial class AddressEntity
{
    /// <summary>
    /// Gets or sets the database ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state or province.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}