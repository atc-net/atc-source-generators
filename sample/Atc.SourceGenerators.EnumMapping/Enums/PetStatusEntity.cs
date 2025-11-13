namespace Atc.SourceGenerators.EnumMapping.Enums;

/// <summary>
/// Pet status in the database layer.
/// Demonstrates special case mapping (None → Unknown).
/// </summary>
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatusEntity
{
    None,
    Pending,
    Available,
    Adopted,
}