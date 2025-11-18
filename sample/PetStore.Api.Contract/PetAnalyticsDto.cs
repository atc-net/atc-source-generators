namespace PetStore.Api.Contract;

/// <summary>
/// Pet analytics data for JavaScript dashboards (camelCase naming).
/// </summary>
public class PetAnalyticsDto
{
    /// <summary>
    /// Gets or sets the pet's unique identifier.
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element should begin with an uppercase letter
    public Guid petId { get; set; }

    /// <summary>
    /// Gets or sets the total number of visits.
    /// </summary>
    public int totalVisits { get; set; }

    /// <summary>
    /// Gets or sets the total number of adoptions.
    /// </summary>
    public int totalAdoptions { get; set; }

    /// <summary>
    /// Gets or sets the average visit duration in minutes.
    /// </summary>
    public double averageVisitDuration { get; set; }

    /// <summary>
    /// Gets or sets the most popular viewing time of day.
    /// </summary>
    public string mostPopularTimeSlot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last analytics update timestamp.
    /// </summary>
    public DateTimeOffset lastUpdated { get; set; }
#pragma warning restore SA1300 // Element should begin with an uppercase letter
#pragma warning restore IDE1006 // Naming Styles
}