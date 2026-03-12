namespace Atc.SourceGenerators.MappingCombinedConfiguration.Contract;

public record AnalyticsEventDto(
    Guid Id,
    string Name,
    ActivitySeverity Severity,
    DateTimeOffset Timestamp,
    string? UserId,
    string? Metadata);