namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Default logger implementation using TryAdd registration.
/// Demonstrates TryAdd pattern - provides a default implementation that can be overridden.
/// </summary>
/// <remarks>
/// TryAdd registration means this logger will only be registered if no other ILogger is registered first.
/// This is useful for library authors who want to provide default implementations that application code can override.
/// </remarks>
[Registration(As = typeof(ILogger), TryAdd = true)]
public class DefaultLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[DefaultLogger] {message}");
    }
}