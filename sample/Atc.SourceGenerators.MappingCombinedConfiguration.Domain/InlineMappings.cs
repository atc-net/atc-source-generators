namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

// Approach 4: Inline registration using MappingBuilder.Configure()
// The lambda body is analyzed at compile time to generate mapping extension methods.
// At runtime, MappingBuilder.Configure() is a no-op.
internal static class InlineMappings
{
    internal static void Register()
    {
        MappingBuilder.Configure(map =>
        {
            // Map domain ActivityEvent -> contract AnalyticsEventDto (record with constructor)
            map.Map<ActivityEvent, Contract.AnalyticsEventDto>();

            // Map domain BrowsingSession -> contract SessionDto (record with constructor)
            map.Map<BrowsingSession, Contract.SessionDto>();
        });
    }
}