namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Display attribute information extracted from DisplayAttribute.
/// </summary>
internal sealed record DisplayAnnotationInfo(
    string? Name,
    string? Description,
    string? ShortName,
    string? GroupName,
    string? Prompt,
    int? Order);