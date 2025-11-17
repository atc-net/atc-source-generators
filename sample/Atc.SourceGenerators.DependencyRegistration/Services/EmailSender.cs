namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Email sender implementation using factory method registration.
/// Demonstrates how to use custom initialization logic via factory methods.
/// The factory method can resolve dependencies from IServiceProvider and perform custom setup.
/// </summary>
[Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
public class EmailSender : IEmailSender
{
    private readonly string smtpHost;
    private readonly int smtpPort;

    private EmailSender(
        string smtpHost,
        int smtpPort)
    {
        this.smtpHost = smtpHost;
        this.smtpPort = smtpPort;
    }

    public Task SendEmailAsync(
        string recipient,
        string subject,
        string body)
    {
        Console.WriteLine($"EmailSender: Sending email to {recipient} via {smtpHost}:{smtpPort}");
        Console.WriteLine($"  Subject: {subject}");
        Console.WriteLine($"  Body: {body}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Factory method for creating EmailSender instances.
    /// This method is called by the DI container to create instances.
    /// It can resolve dependencies from the service provider and perform custom initialization.
    /// </summary>
    public static IEmailSender CreateEmailSender(
        IServiceProvider serviceProvider)
    {
        const string smtpHost = "smtp.example.com";
        const int smtpPort = 587;

        Console.WriteLine($"EmailSender: Factory creating instance with SMTP {smtpHost}:{smtpPort}");

        return new EmailSender(smtpHost, smtpPort);
    }
}