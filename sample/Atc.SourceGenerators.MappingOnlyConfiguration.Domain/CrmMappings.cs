namespace Atc.SourceGenerators.MappingOnlyConfiguration.Domain;

/// <summary>
/// Configuration-based mapping for CRM SDK types.
/// These are third-party types that cannot be decorated with [MapTo] attributes.
/// </summary>
[MappingConfiguration]
public static partial class CrmMappings
{
    /// <summary>
    /// Maps CRM Contact to domain Customer with property renaming.
    /// </summary>
    [MapConfigProperty("FullName", "Name")]
    [MapConfigProperty("EmailAddress", "Email")]
    [MapConfigProperty("PhoneNumber", "Phone")]
    [MapConfigProperty("PrimaryAddress", "Address")]
    [MapConfigProperty("CreatedDate", "CreatedAt")]
    [MapConfigProperty("ModifiedDate", "UpdatedAt")]
    [MapConfigProperty("Type", "Category")]
    [MapConfigIgnore("ContactId")]
    [MapConfigIgnore("InternalTrackingCode")]
    public static partial Customer MapToCustomer(this Contact source);

    /// <summary>
    /// Maps CRM ContactAddress to domain Address with ZipCode -> PostalCode rename.
    /// </summary>
    [MapConfigProperty("ZipCode", "PostalCode")]
    public static partial Address MapToAddress(this ContactAddress source);
}