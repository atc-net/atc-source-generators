namespace Atc.SourceGenerators.MappingOnlyConfiguration.Domain;

/// <summary>
/// Domain address model.
/// </summary>
public class Address
{
    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}