namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a physical address.
/// </summary>
[MapTo(typeof(AddressDto))]
[MapTo(typeof(AddressEntity), Bidirectional = true)]
public partial class Address
{
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
}