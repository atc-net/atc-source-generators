namespace Atc.SourceGenerators.Mapping.Domain;

/// <summary>
/// Represents a cat in the domain layer.
/// </summary>
[MapTo(typeof(Contract.CatDto))]
public partial class Cat : Animal
{
    public int Lives { get; set; }
}