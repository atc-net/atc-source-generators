namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a dog in the domain layer.
/// </summary>
[MapTo(typeof(Contract.DogDto))]
public partial class Dog : Animal
{
    public string Breed { get; set; } = string.Empty;
}