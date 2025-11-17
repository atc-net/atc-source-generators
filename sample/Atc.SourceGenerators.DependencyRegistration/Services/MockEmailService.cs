namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Mock email service that should be excluded from registration via pattern filter (*Mock*).
/// </summary>
[Registration]
public class MockEmailService : IMockEmailService
{
    public void SendMockEmail(
        string recipient,
        string message)
    {
        Console.WriteLine($"[MockEmailService] Sending mock email to {recipient}: {message}");
    }
}