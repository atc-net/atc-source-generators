namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

public class ActivityEvent
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ActivitySeverity Severity { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public string? UserId { get; set; }

    public string? Metadata { get; set; }
}