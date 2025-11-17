namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Email sender service interface.
/// </summary>
public interface IEmailSender
{
    Task SendEmailAsync(
        string recipient,
        string subject,
        string body);
}