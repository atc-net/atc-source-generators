namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a user event with strongly-typed properties.
/// Demonstrates automatic type conversion to string-based DTOs.
/// </summary>
[MapTo(typeof(UserEventDto))]
public partial class UserEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds.
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets whether the event was successful.
    /// </summary>
    public bool Success { get; set; }
}