namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Local copy of PropertyNameStrategy enum for use within source generator.
/// Must match the public enum in Atc.SourceGenerators.Annotations.
/// </summary>
internal enum PropertyNameStrategy
{
    PascalCase = 0,
    CamelCase = 1,
    SnakeCase = 2,
    KebabCase = 3,
}

