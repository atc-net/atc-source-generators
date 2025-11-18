namespace PetStore.Domain.Models;

/// <summary>
/// Represents an email notification in the domain layer (demonstrates factory method).
/// </summary>
[MapTo(typeof(Api.Contract.EmailNotificationDto), Factory = nameof(CreateEmailNotificationDto))]
public partial class EmailNotification : Notification
{
    public string To { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Factory method: Creates an EmailNotificationDto with the CreatedAt timestamp set to current UTC time.
    /// This demonstrates using a factory to initialize properties with runtime values.
    /// </summary>
    /// <returns>A new EmailNotificationDto instance.</returns>
    internal static Api.Contract.EmailNotificationDto CreateEmailNotificationDto()
        => new()
        {
            CreatedAt = DateTimeOffset.UtcNow,
        };
}
