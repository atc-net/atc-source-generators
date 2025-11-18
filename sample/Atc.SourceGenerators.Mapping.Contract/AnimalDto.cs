namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Abstract base class representing an animal DTO.
/// </summary>
public abstract class AnimalDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}