namespace PetStore.Api.Contract;

/// <summary>
/// Abstract base class representing a notification DTO.
/// </summary>
public abstract class NotificationDto
{
    public Guid Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}