namespace Atc.SourceGenerators.MappingOnlyConfiguration.ExternalCrm;

/// <summary>
/// Simulates a third-party CRM SDK type that cannot be decorated with mapping attributes.
/// </summary>
public class Contact
{
    public int ContactId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public ContactAddress? PrimaryAddress { get; set; }

    public ContactType Type { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string InternalTrackingCode { get; set; } = string.Empty;
}