namespace Atc.SourceGenerators.MappingCombinedConfiguration.Contract;

public class NotificationDto
{
    public int Id { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public UrgencyLevel Urgency { get; set; }

    public DateTime SentAt { get; set; }

    public bool Delivered { get; set; }
}