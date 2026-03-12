namespace Atc.SourceGenerators.MappingCombinedConfiguration.ExternalNotifications;

public class PushNotification
{
    public int NotificationId { get; set; }

    public string RecipientToken { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public NotificationPriority Priority { get; set; }

    public DateTime SentAt { get; set; }

    public bool IsDelivered { get; set; }
}