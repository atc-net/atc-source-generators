namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

[MappingConfiguration]
public static partial class ExternalMappings
{
    [MapConfigProperty("NotificationId", "Id")]
    [MapConfigProperty("RecipientToken", "Recipient")]
    [MapConfigProperty("Body", "Message")]
    [MapConfigProperty("IsDelivered", "Delivered")]
    [MapConfigProperty("Priority", "Urgency")]
    public static partial Notification MapToNotification(
        this PushNotification source);

    [MapConfigProperty("EventId", "Id")]
    [MapConfigProperty("EventName", "Name")]
    public static partial ActivityEvent MapToActivityEvent(
        this AnalyticsEvent source);

    [MapConfigProperty("SessionId", "Id")]
    public static partial BrowsingSession MapToBrowsingSession(
        this UserSession source);
}