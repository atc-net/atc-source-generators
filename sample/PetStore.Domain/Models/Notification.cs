namespace PetStore.Domain.Models;

/// <summary>
/// Abstract base class representing a notification in the domain layer.
/// </summary>
[MapTo(typeof(Api.Contract.NotificationDto))]
[MapDerivedType(typeof(EmailNotification), typeof(Api.Contract.EmailNotificationDto))]
[MapDerivedType(typeof(SmsNotification), typeof(Api.Contract.SmsNotificationDto))]
public abstract partial class Notification
{
    public Guid Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
