namespace Atc.SourceGenerators.MappingOnlyConfiguration.Domain;

/// <summary>
/// Domain customer category - note different values from CRM SDK's ContactType.
/// </summary>
public enum CustomerCategory
{
    Unknown,
    Individual,
    Company,
    Government,
    NonProfit,
}