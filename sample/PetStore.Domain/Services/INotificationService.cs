namespace PetStore.Domain.Services;

/// <summary>
/// Notification service interface for sending notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendNotificationAsync(
        string message,
        CancellationToken cancellationToken = default);
}