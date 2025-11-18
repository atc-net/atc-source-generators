namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Defines strategies for converting property names during mapping.
/// Used to map between different naming conventions (PascalCase, camelCase, snake_case, kebab-case).
/// </summary>
public enum PropertyNameStrategy
{
    /// <summary>
    /// No transformation. Property names must match exactly (case-insensitive comparison).
    /// This is the default behavior.
    /// Example: FirstName → FirstName
    /// </summary>
    PascalCase = 0,

    /// <summary>
    /// Convert PascalCase source properties to camelCase for matching.
    /// Example: FirstName → firstName
    /// </summary>
    CamelCase = 1,

    /// <summary>
    /// Convert PascalCase source properties to snake_case for matching.
    /// Example: FirstName → first_name
    /// </summary>
    SnakeCase = 2,

    /// <summary>
    /// Convert PascalCase source properties to kebab-case for matching.
    /// Example: FirstName → first-name
    /// </summary>
    KebabCase = 3,
}