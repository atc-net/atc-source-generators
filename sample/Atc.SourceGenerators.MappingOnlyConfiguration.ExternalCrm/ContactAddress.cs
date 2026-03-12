namespace Atc.SourceGenerators.MappingOnlyConfiguration.ExternalCrm;

/// <summary>
/// Address type from the CRM SDK.
/// </summary>
public class ContactAddress
{
    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}