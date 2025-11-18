namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Abstract base class representing an animal in the domain layer.
/// </summary>
[MapTo(typeof(Contract.AnimalDto))]
[MapDerivedType(typeof(Dog), typeof(Contract.DogDto))]
[MapDerivedType(typeof(Cat), typeof(Contract.CatDto))]
public abstract partial class Animal
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}