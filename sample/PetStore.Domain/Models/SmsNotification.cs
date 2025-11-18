namespace PetStore.Domain.Models;

/// <summary>
/// Represents an SMS notification in the domain layer.
/// </summary>
[MapTo(typeof(Api.Contract.SmsNotificationDto))]
public partial class SmsNotification : Notification
{
    public string PhoneNumber { get; set; } = string.Empty;
}
