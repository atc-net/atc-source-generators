namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

[MapTo(typeof(UrgencyLevel))]
public enum Urgency
{
    Unknown,
    Low,
    Medium,
    High,
    Urgent,
}