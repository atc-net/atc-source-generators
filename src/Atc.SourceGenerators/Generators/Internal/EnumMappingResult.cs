namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The enum mappings and diagnostics extracted from one source enum, as equatable data.
/// </summary>
/// <param name="Key">The source enum's display name, used to de-duplicate partial or repeated results.</param>
/// <param name="Mappings">The mappings declared on the enum.</param>
/// <param name="Diagnostics">The diagnostics to report for the enum.</param>
internal sealed record EnumMappingResult(
    string Key,
    EquatableArray<EnumMappingInfo> Mappings,
    EquatableArray<DiagnosticInfo> Diagnostics);