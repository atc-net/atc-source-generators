namespace Atc.SourceGenerators.DependencyRegistration.Domain.Abstractions;

/// <summary>
/// Service for sending notifications.
/// </summary>
public interface INotificationService
{
    void Send(string message);
}