namespace Atc.SourceGenerators.EnumMapping.Enums;

/// <summary>
/// Feature state in the domain layer.
/// Demonstrates exact name matching.
/// </summary>
[MapTo(typeof(FeatureFlag))]
public enum FeatureState
{
    Active,
    Inactive,
    Testing,
}