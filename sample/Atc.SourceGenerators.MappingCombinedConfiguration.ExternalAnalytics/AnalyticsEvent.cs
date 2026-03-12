namespace Atc.SourceGenerators.MappingCombinedConfiguration.ExternalAnalytics;

public class AnalyticsEvent
{
    public Guid EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public EventSeverity Severity { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public string? UserId { get; set; }

    public string? Metadata { get; set; }
}