namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// A source location as equatable data (a <see cref="Location"/> holds its syntax tree).
/// </summary>
internal sealed record LocationInfo(
    string? FilePath,
    TextSpan Span,
    LinePositionSpan LineSpan)
{
    public static LocationInfo From(Location location)
        => location.IsInSource
            ? new LocationInfo(location.SourceTree?.FilePath ?? string.Empty, location.SourceSpan, location.GetLineSpan().Span)
            : new LocationInfo(null, default, default);

    public Location ToLocation()
        => FilePath is null ? Location.None : Location.Create(FilePath, Span, LineSpan);
}