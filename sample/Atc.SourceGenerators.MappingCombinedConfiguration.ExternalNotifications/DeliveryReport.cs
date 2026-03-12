namespace Atc.SourceGenerators.MappingCombinedConfiguration.ExternalNotifications;

public record DeliveryReport(
    int ReportId,
    int NotificationId,
    DateTime? DeliveredAt,
    string? FailureReason);