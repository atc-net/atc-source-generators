namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

[MapTo(typeof(NotificationDto))]
public partial class Notification
{
    public int Id { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Urgency Urgency { get; set; }

    public DateTime SentAt { get; set; }

    public bool Delivered { get; set; }
}