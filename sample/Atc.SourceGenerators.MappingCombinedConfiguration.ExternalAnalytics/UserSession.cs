namespace Atc.SourceGenerators.MappingCombinedConfiguration.ExternalAnalytics;

public record UserSession(
    Guid SessionId,
    string UserId,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    int PageViews);