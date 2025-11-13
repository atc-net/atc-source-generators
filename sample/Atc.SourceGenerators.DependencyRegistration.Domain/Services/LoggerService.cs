namespace Atc.SourceGenerators.DependencyRegistration.Domain.Services;

/// <summary>
/// Transient logger service - new instance created each time it's requested.
/// </summary>
[Registration(Lifetime.Transient)]
public class LoggerService
{
    private readonly Guid instanceId = Guid.NewGuid();

    public void Log(string message)
    {
        Console.WriteLine($"[{instanceId:N}] {message}");
    }
}