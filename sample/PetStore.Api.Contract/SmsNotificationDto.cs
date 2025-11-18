namespace PetStore.Api.Contract;

/// <summary>
/// Represents an SMS notification DTO.
/// </summary>
public class SmsNotificationDto : NotificationDto
{
    public string PhoneNumber { get; set; } = string.Empty;
}