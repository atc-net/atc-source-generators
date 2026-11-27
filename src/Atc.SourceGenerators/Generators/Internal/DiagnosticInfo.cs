namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// A diagnostic as equatable data, so it can travel through an incremental pipeline model.
/// <see cref="Diagnostic"/> and <see cref="Location"/> hold syntax trees and don't compare by value, which
/// would keep the compilation alive and break caching.
/// </summary>
/// <param name="Descriptor">The diagnostic descriptor.</param>
/// <param name="FilePath">The file path of the location, or <c>null</c> for no location.</param>
/// <param name="Span">The text span of the location.</param>
/// <param name="LineSpan">The line span of the location.</param>
/// <param name="Arguments">The message format arguments.</param>
internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    string? FilePath,
    TextSpan Span,
    LinePositionSpan LineSpan,
    EquatableArray<string> Arguments)
{
    /// <summary>
    /// Creates a diagnostic info at the first location of a symbol.
    /// </summary>
    public static DiagnosticInfo Create(
        DiagnosticDescriptor descriptor,
        ISymbol symbol,
        params string[] arguments)
    {
        var location = symbol.Locations.FirstOrDefault(l => l.IsInSource);
        return location is null
            ? new DiagnosticInfo(descriptor, null, default, default, new EquatableArray<string>(arguments))
            : new DiagnosticInfo(
                descriptor,
                location.SourceTree?.FilePath ?? string.Empty,
                location.SourceSpan,
                location.GetLineSpan().Span,
                new EquatableArray<string>(arguments));
    }

    /// <summary>
    /// Creates the <see cref="Diagnostic"/> to report.
    /// </summary>
    public Diagnostic ToDiagnostic()
        => Diagnostic.Create(
            Descriptor,
            FilePath is null ? Location.None : Location.Create(FilePath, Span, LineSpan),
            Arguments.Cast<object>().ToArray());
}