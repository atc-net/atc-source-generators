namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The options and diagnostics extracted from one [OptionsBinding] class, as equatable data for incremental caching.
/// </summary>
/// <param name="ClassKey">The fully qualified class name (a partial class can be found more than once).</param>
/// <param name="Options">The options registrations of the class.</param>
/// <param name="Diagnostics">The diagnostics for the class.</param>
internal sealed record OptionsClassResult(
    string ClassKey,
    EquatableArray<OptionsInfo> Options,
    EquatableArray<DiagnosticInfo> Diagnostics);