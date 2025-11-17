namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Product entity for demonstrating generic repository pattern.
/// </summary>
public class Product : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
