namespace Atc.SourceGenerators.Helpers;

/// <summary>
/// Builds the help link of a diagnostic: its section in the Diagnostics Reference wiki page,
/// which the IDE opens from the diagnostic ID.
/// </summary>
[SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "OK - the documentation location is fixed.")]
internal static class DiagnosticHelpLink
{
    private const string DiagnosticsReferenceUrl = "https://github.com/atc-net/atc-source-generators/wiki/Diagnostics-Reference";

    /// <summary>
    /// Gets the help link for a diagnostic ID.
    /// </summary>
    /// <param name="id">The diagnostic ID, for example <c>ATCMAP014</c>.</param>
    /// <returns>The URL of the diagnostic's section, for example <c>...Diagnostics-Reference#atcmap014</c>.</returns>
    public static string For(string id)
        => $"{DiagnosticsReferenceUrl}#{id.ToLowerInvariant()}";
}