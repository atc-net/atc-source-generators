// =============================================================================
// Section 1: Attribute-Based Mapping (Notification -> NotificationDto)
// =============================================================================

Console.WriteLine("=== Section 1: Attribute-Based Mapping ===");
Console.WriteLine();

var notification = new Notification
{
    Id = 1,
    Recipient = "user@example.com",
    Title = "Welcome",
    Message = "Welcome to our platform!",
    Urgency = Urgency.High,
    SentAt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc),
    Delivered = true,
};

Console.WriteLine("--- Source: Domain Notification ---");
Console.WriteLine($"  Id: {notification.Id}");
Console.WriteLine($"  Recipient: {notification.Recipient}");
Console.WriteLine($"  Title: {notification.Title}");
Console.WriteLine($"  Message: {notification.Message}");
Console.WriteLine($"  Urgency: {notification.Urgency}");
Console.WriteLine($"  SentAt: {notification.SentAt}");
Console.WriteLine($"  Delivered: {notification.Delivered}");
Console.WriteLine();

var notificationDto = notification.MapToNotificationDto();

Console.WriteLine("--- Target: NotificationDto ---");
Console.WriteLine($"  Id: {notificationDto.Id}");
Console.WriteLine($"  Recipient: {notificationDto.Recipient}");
Console.WriteLine($"  Title: {notificationDto.Title}");
Console.WriteLine($"  Message: {notificationDto.Message}");
Console.WriteLine($"  Urgency: {notificationDto.Urgency}");
Console.WriteLine($"  SentAt: {notificationDto.SentAt}");
Console.WriteLine($"  Delivered: {notificationDto.Delivered}");
Console.WriteLine();

// =============================================================================
// Section 2: Configuration-Based Mapping (External -> Domain)
// =============================================================================

Console.WriteLine("=== Section 2: Configuration-Based Mapping ===");
Console.WriteLine();

// PushNotification -> Notification
var pushNotification = new PushNotification
{
    NotificationId = 42,
    RecipientToken = "device-token-abc123",
    Title = "New Message",
    Body = "You have a new message from support.",
    Priority = NotificationPriority.High,
    SentAt = new DateTime(2025, 7, 1, 14, 0, 0, DateTimeKind.Utc),
    IsDelivered = false,
};

Console.WriteLine("--- Source: PushNotification (External SDK) ---");
Console.WriteLine($"  NotificationId: {pushNotification.NotificationId}");
Console.WriteLine($"  RecipientToken: {pushNotification.RecipientToken}");
Console.WriteLine($"  Title: {pushNotification.Title}");
Console.WriteLine($"  Body: {pushNotification.Body}");
Console.WriteLine($"  Priority: {pushNotification.Priority}");
Console.WriteLine($"  SentAt: {pushNotification.SentAt}");
Console.WriteLine($"  IsDelivered: {pushNotification.IsDelivered}");
Console.WriteLine();

var mappedNotification = pushNotification.MapToNotification();

Console.WriteLine("--- Target: Domain Notification (via config mapping) ---");
Console.WriteLine($"  Id: {mappedNotification.Id}");
Console.WriteLine($"  Recipient: {mappedNotification.Recipient}");
Console.WriteLine($"  Title: {mappedNotification.Title}");
Console.WriteLine($"  Message: {mappedNotification.Message}");
Console.WriteLine($"  Urgency: {mappedNotification.Urgency}");
Console.WriteLine($"  SentAt: {mappedNotification.SentAt}");
Console.WriteLine($"  Delivered: {mappedNotification.Delivered}");
Console.WriteLine();

// AnalyticsEvent -> ActivityEvent
var analyticsEvent = new AnalyticsEvent
{
    EventId = Guid.NewGuid(),
    EventName = "UserLogin",
    Severity = EventSeverity.Information,
    Timestamp = DateTimeOffset.UtcNow,
    UserId = "user-456",
    Metadata = "{\"ip\":\"192.168.1.1\"}",
};

Console.WriteLine("--- Source: AnalyticsEvent (External SDK) ---");
Console.WriteLine($"  EventId: {analyticsEvent.EventId}");
Console.WriteLine($"  EventName: {analyticsEvent.EventName}");
Console.WriteLine($"  Severity: {analyticsEvent.Severity}");
Console.WriteLine($"  Timestamp: {analyticsEvent.Timestamp}");
Console.WriteLine($"  UserId: {analyticsEvent.UserId}");
Console.WriteLine($"  Metadata: {analyticsEvent.Metadata}");
Console.WriteLine();

var activityEvent = analyticsEvent.MapToActivityEvent();

Console.WriteLine("--- Target: Domain ActivityEvent (via config mapping) ---");
Console.WriteLine($"  Id: {activityEvent.Id}");
Console.WriteLine($"  Name: {activityEvent.Name}");
Console.WriteLine($"  Severity: {activityEvent.Severity}");
Console.WriteLine($"  Timestamp: {activityEvent.Timestamp}");
Console.WriteLine($"  UserId: {activityEvent.UserId}");
Console.WriteLine($"  Metadata: {activityEvent.Metadata}");
Console.WriteLine();

// UserSession -> BrowsingSession
var userSession = new UserSession(
    SessionId: Guid.NewGuid(),
    UserId: "user-789",
    StartTime: DateTimeOffset.UtcNow.AddHours(-2),
    EndTime: DateTimeOffset.UtcNow,
    PageViews: 15);

Console.WriteLine("--- Source: UserSession (External SDK) ---");
Console.WriteLine($"  SessionId: {userSession.SessionId}");
Console.WriteLine($"  UserId: {userSession.UserId}");
Console.WriteLine($"  StartTime: {userSession.StartTime}");
Console.WriteLine($"  EndTime: {userSession.EndTime}");
Console.WriteLine($"  PageViews: {userSession.PageViews}");
Console.WriteLine();

var browsingSession = userSession.MapToBrowsingSession();

Console.WriteLine("--- Target: Domain BrowsingSession (via config mapping) ---");
Console.WriteLine($"  Id: {browsingSession.Id}");
Console.WriteLine($"  UserId: {browsingSession.UserId}");
Console.WriteLine($"  StartTime: {browsingSession.StartTime}");
Console.WriteLine($"  EndTime: {browsingSession.EndTime}");
Console.WriteLine($"  PageViews: {browsingSession.PageViews}");
Console.WriteLine();

// =============================================================================
// Section 3: Multi-Hop Mapping Chain (PushNotification -> Notification -> NotificationDto)
// =============================================================================

Console.WriteLine("=== Section 3: Multi-Hop Mapping Chain ===");
Console.WriteLine();

var externalPush = new PushNotification
{
    NotificationId = 99,
    RecipientToken = "device-token-xyz789",
    Title = "Order Shipped",
    Body = "Your order #12345 has been shipped!",
    Priority = NotificationPriority.Medium,
    SentAt = new DateTime(2025, 8, 10, 9, 15, 0, DateTimeKind.Utc),
    IsDelivered = true,
};

Console.WriteLine("--- Source: PushNotification (External SDK) ---");
Console.WriteLine($"  NotificationId: {externalPush.NotificationId}");
Console.WriteLine($"  Body: {externalPush.Body}");
Console.WriteLine($"  Priority: {externalPush.Priority}");
Console.WriteLine();

// Chain: PushNotification -> Notification -> NotificationDto
var domainNotification = externalPush.MapToNotification();
var dto = domainNotification.MapToNotificationDto();

Console.WriteLine("--- Intermediate: Domain Notification ---");
Console.WriteLine($"  Id: {domainNotification.Id}");
Console.WriteLine($"  Message: {domainNotification.Message}");
Console.WriteLine($"  Urgency: {domainNotification.Urgency}");
Console.WriteLine();

Console.WriteLine("--- Final: NotificationDto ---");
Console.WriteLine($"  Id: {dto.Id}");
Console.WriteLine($"  Message: {dto.Message}");
Console.WriteLine($"  Urgency: {dto.Urgency}");
Console.WriteLine();

// =============================================================================
// Section 4: Enum Auto-Detection Demonstrations
// =============================================================================

Console.WriteLine("=== Section 4: Enum Auto-Detection ===");
Console.WriteLine();

Console.WriteLine("--- Attribute-Based Enum Mapping (Urgency -> UrgencyLevel) ---");
Console.WriteLine($"  Urgency.Unknown -> UrgencyLevel.{Urgency.Unknown.MapToUrgencyLevel()}");
Console.WriteLine($"  Urgency.Low     -> UrgencyLevel.{Urgency.Low.MapToUrgencyLevel()}");
Console.WriteLine($"  Urgency.Medium  -> UrgencyLevel.{Urgency.Medium.MapToUrgencyLevel()}");
Console.WriteLine($"  Urgency.High    -> UrgencyLevel.{Urgency.High.MapToUrgencyLevel()}");
Console.WriteLine($"  Urgency.Urgent  -> UrgencyLevel.{Urgency.Urgent.MapToUrgencyLevel()}");
Console.WriteLine();

Console.WriteLine("--- Config-Based Enum Auto-Detection (NotificationPriority -> Urgency) ---");
Console.WriteLine("  (Enum values are auto-detected when mapping PushNotification -> Notification)");
Console.WriteLine($"  PushNotification with Priority=Low    -> Urgency.{new PushNotification { Priority = NotificationPriority.Low }.MapToNotification().Urgency}");
Console.WriteLine($"  PushNotification with Priority=High   -> Urgency.{new PushNotification { Priority = NotificationPriority.High }.MapToNotification().Urgency}");
Console.WriteLine();

Console.WriteLine("--- Config-Based Enum Auto-Detection (EventSeverity -> ActivitySeverity) ---");
Console.WriteLine("  (Enum values are auto-detected when mapping AnalyticsEvent -> ActivityEvent)");
Console.WriteLine($"  AnalyticsEvent with Severity=Warning -> ActivitySeverity.{new AnalyticsEvent { Severity = EventSeverity.Warning }.MapToActivityEvent().Severity}");
Console.WriteLine($"  AnalyticsEvent with Severity=Error   -> ActivitySeverity.{new AnalyticsEvent { Severity = EventSeverity.Error }.MapToActivityEvent().Severity}");
Console.WriteLine();

// =============================================================================
// Section 5: Class-Level [MapTypes] Mapping (DeliveryReport -> DeliveryStatus)
// =============================================================================

Console.WriteLine("=== Section 5: Class-Level [MapTypes] Mapping ===");
Console.WriteLine();

var deliveryReport = new DeliveryReport(
    ReportId: 101,
    NotificationId: 42,
    DeliveredAt: new DateTime(2025, 7, 1, 14, 30, 0, DateTimeKind.Utc),
    FailureReason: null);

Console.WriteLine("--- Source: DeliveryReport (External SDK) ---");
Console.WriteLine($"  ReportId: {deliveryReport.ReportId}");
Console.WriteLine($"  NotificationId: {deliveryReport.NotificationId}");
Console.WriteLine($"  DeliveredAt: {deliveryReport.DeliveredAt}");
Console.WriteLine($"  FailureReason: {deliveryReport.FailureReason ?? "(null)"}");
Console.WriteLine();

var deliveryStatus = deliveryReport.MapToDeliveryStatus();

Console.WriteLine("--- Target: DeliveryStatus (via class-level [MapTypes]) ---");
Console.WriteLine($"  ReportId: {deliveryStatus.ReportId}");
Console.WriteLine($"  NotificationId: {deliveryStatus.NotificationId}");
Console.WriteLine($"  DeliveredAt: {deliveryStatus.DeliveredAt}");
Console.WriteLine($"  FailureReason: {deliveryStatus.FailureReason ?? "(null)"}");
Console.WriteLine();

// =============================================================================
// Section 6: Inline MappingBuilder.Configure() Registration
// =============================================================================

Console.WriteLine("=== Section 6: Inline MappingBuilder.Configure() Registration ===");
Console.WriteLine();

// ActivityEvent -> AnalyticsEventDto (configured via MappingBuilder.Configure in InlineMappings.cs)
var activityEventForDto = new ActivityEvent
{
    Id = Guid.NewGuid(),
    Name = "PageView",
    Severity = Atc.SourceGenerators.MappingCombinedConfiguration.Domain.ActivitySeverity.Information,
    Timestamp = DateTimeOffset.UtcNow,
    UserId = "user-123",
    Metadata = "{\"page\":\"/home\"}",
};

Console.WriteLine("--- Source: Domain ActivityEvent ---");
Console.WriteLine($"  Id: {activityEventForDto.Id}");
Console.WriteLine($"  Name: {activityEventForDto.Name}");
Console.WriteLine($"  Severity: {activityEventForDto.Severity}");
Console.WriteLine();

var analyticsEventDto = activityEventForDto.MapToAnalyticsEventDto();

Console.WriteLine("--- Target: Contract AnalyticsEventDto (via MappingBuilder.Configure) ---");
Console.WriteLine($"  Id: {analyticsEventDto.Id}");
Console.WriteLine($"  Name: {analyticsEventDto.Name}");
Console.WriteLine($"  Severity: {analyticsEventDto.Severity}");
Console.WriteLine();

// BrowsingSession -> SessionDto (configured via MappingBuilder.Configure in InlineMappings.cs)
var browsingSessionForDto = new BrowsingSession
{
    Id = Guid.NewGuid(),
    UserId = "user-456",
    StartTime = DateTimeOffset.UtcNow.AddHours(-1),
    EndTime = DateTimeOffset.UtcNow,
    PageViews = 42,
};

Console.WriteLine("--- Source: Domain BrowsingSession ---");
Console.WriteLine($"  Id: {browsingSessionForDto.Id}");
Console.WriteLine($"  UserId: {browsingSessionForDto.UserId}");
Console.WriteLine($"  PageViews: {browsingSessionForDto.PageViews}");
Console.WriteLine();

var sessionDto = browsingSessionForDto.MapToSessionDto();

Console.WriteLine("--- Target: Contract SessionDto (via MappingBuilder.Configure) ---");
Console.WriteLine($"  Id: {sessionDto.Id}");
Console.WriteLine($"  UserId: {sessionDto.UserId}");
Console.WriteLine($"  PageViews: {sessionDto.PageViews}");
Console.WriteLine();

Console.WriteLine("=== Demo Complete ===");