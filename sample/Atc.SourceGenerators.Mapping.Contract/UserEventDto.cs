namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Data transfer object for UserEvent (demonstrates type conversion).
/// Uses string representations for DateTime, Guid, and numeric types.
/// </summary>
public class UserEventDto
{
    /// <summary>
    /// Gets or sets the event ID as string (converted from Guid).
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID as string (converted from Guid).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp as string (converted from DateTimeOffset using ISO 8601 format).
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration in seconds as string (converted from int).
    /// </summary>
    public string DurationSeconds { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the event was successful as string (converted from bool).
    /// </summary>
    public string Success { get; set; } = string.Empty;
}