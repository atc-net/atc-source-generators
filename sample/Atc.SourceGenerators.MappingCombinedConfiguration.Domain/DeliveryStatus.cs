namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

public class DeliveryStatus
{
    public int ReportId { get; set; }

    public int NotificationId { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public string? FailureReason { get; set; }
}