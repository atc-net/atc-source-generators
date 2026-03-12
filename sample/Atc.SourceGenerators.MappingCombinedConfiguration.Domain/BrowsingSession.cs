namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

public class BrowsingSession
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public int PageViews { get; set; }
}