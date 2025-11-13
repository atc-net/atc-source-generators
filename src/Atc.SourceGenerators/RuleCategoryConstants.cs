// ReSharper disable CommentTypo
namespace Atc.SourceGenerators;

/// <summary>
/// Defines the categories for source generator diagnostics.
/// </summary>
internal static class RuleCategoryConstants
{
    /// <summary>
    /// Dependency Injection category - diagnostic IDs from ATCSG001 to ATCSG099.
    /// </summary>
    public const string DependencyInjection = nameof(DependencyInjection);

    /// <summary>
    /// Options Binding category - diagnostic IDs from ATCOPT001 to ATCOPT099.
    /// </summary>
    public const string OptionsBinding = nameof(OptionsBinding);

    /// <summary>
    /// Object Mapping category - diagnostic IDs from ATCMAP001 to ATCMAP099.
    /// </summary>
    public const string ObjectMapping = nameof(ObjectMapping);

    /// <summary>
    /// Enum Mapping category - diagnostic IDs from ATCENUM001 to ATCENUM099.
    /// </summary>
    public const string EnumMapping = nameof(EnumMapping);
}