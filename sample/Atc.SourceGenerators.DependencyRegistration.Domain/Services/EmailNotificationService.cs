namespace Atc.SourceGenerators.DependencyRegistration.Domain.Services;

/// <summary>
/// Email notification service auto-registered against INotificationService and also as itself.
/// </summary>
[Registration(AsSelf = true)]
public class EmailNotificationService : INotificationService
{
    public void Send(string message)
    {
        Console.WriteLine($"Sending email: {message}");
    }

    public void SendBulk(IEnumerable<string> messages)
    {
        if (messages == null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        foreach (var message in messages)
        {
            Send(message);
        }
    }
}