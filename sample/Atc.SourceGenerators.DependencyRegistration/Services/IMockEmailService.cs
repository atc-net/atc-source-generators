namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Mock email service interface for testing.
/// </summary>
public interface IMockEmailService
{
    void SendMockEmail(
        string recipient,
        string message);
}