namespace PetStore.Domain.Models;

/// <summary>
/// Analytics data for a pet (demonstrates PropertyNameStrategy with camelCase for JS dashboards).
/// </summary>
[MapTo(typeof(PetAnalyticsDto), PropertyNameStrategy = PropertyNameStrategy.CamelCase)]
public partial class PetAnalytics
{
    /// <summary>
    /// Gets or sets the pet's unique identifier.
    /// </summary>
    public Guid PetId { get; set; }

    /// <summary>
    /// Gets or sets the total number of visits.
    /// </summary>
    public int TotalVisits { get; set; }

    /// <summary>
    /// Gets or sets the total number of adoptions.
    /// </summary>
    public int TotalAdoptions { get; set; }

    /// <summary>
    /// Gets or sets the average visit duration in minutes.
    /// </summary>
    public double AverageVisitDuration { get; set; }

    /// <summary>
    /// Gets or sets the most popular viewing time of day.
    /// </summary>
    public string MostPopularTimeSlot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last analytics update timestamp.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }
}

