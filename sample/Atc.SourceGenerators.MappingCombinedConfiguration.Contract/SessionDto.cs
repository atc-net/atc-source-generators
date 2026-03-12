namespace Atc.SourceGenerators.MappingCombinedConfiguration.Contract;

public record SessionDto(
    Guid Id,
    string UserId,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    int PageViews);