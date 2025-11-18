namespace Atc.SourceGenerators.Mapping.Contract;

/// <summary>
/// Represents a dog DTO.
/// </summary>
public class DogDto : AnimalDto
{
    public string Breed { get; set; } = string.Empty;
}