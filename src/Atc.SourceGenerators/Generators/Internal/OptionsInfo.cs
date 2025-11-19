namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record OptionsInfo(
    string ClassName,
    string Namespace,
    string AssemblyName,
    string SectionName,
    bool ValidateOnStart,
    bool ValidateDataAnnotations,
    int Lifetime,
    string? ValidatorType,
    string? Name,
    bool ErrorOnMissingKeys,
    string? OnChange,
    string? PostConfigure,
    string? ConfigureAll);