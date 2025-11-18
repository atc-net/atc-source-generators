namespace PetStore.Domain.Models;

/// <summary>
/// Represents an email notification in the domain layer.
/// </summary>
[MapTo(typeof(Api.Contract.EmailNotificationDto))]
public partial class EmailNotification : Notification
{
    public string To { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
}
