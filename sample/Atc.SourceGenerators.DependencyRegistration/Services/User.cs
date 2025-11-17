namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// User entity for demonstrating generic repository pattern.
/// </summary>
public class User : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}