namespace PetStore.Domain.Services;

/// <summary>
/// Notification service implementation using factory method for initialization.
/// Demonstrates factory method registration in a real-world scenario where
/// the service needs custom initialization with configuration values.
/// </summary>
[Registration(Lifetime.Scoped, As = typeof(INotificationService), Factory = nameof(CreateNotificationService))]
public class NotificationService : INotificationService
{
    private readonly string notificationEndpoint;
    private readonly bool enableNotifications;

    private NotificationService(
        string notificationEndpoint,
        bool enableNotifications)
    {
        this.notificationEndpoint = notificationEndpoint;
        this.enableNotifications = enableNotifications;
    }

    /// <inheritdoc/>
    public Task SendNotificationAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!enableNotifications)
        {
            Console.WriteLine("NotificationService: Notifications are disabled");
            return Task.CompletedTask;
        }

        Console.WriteLine($"NotificationService: Sending notification to {notificationEndpoint}");
        Console.WriteLine($"  Message: {message}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Factory method for creating NotificationService instances.
    /// This allows for custom initialization logic and dependency resolution.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>A configured INotificationService instance.</returns>
    public static INotificationService CreateNotificationService(
        IServiceProvider serviceProvider)
    {
        const string notificationEndpoint = "https://notifications.example.com/api";
        const bool enableNotifications = true;

        Console.WriteLine($"NotificationService: Factory creating instance with endpoint={notificationEndpoint}, enabled={enableNotifications}");

        return new NotificationService(notificationEndpoint, enableNotifications);
    }
}