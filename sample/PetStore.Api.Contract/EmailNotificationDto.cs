namespace PetStore.Api.Contract;

/// <summary>
/// Represents an email notification DTO.
/// </summary>
public class EmailNotificationDto : NotificationDto
{
    public string To { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
}