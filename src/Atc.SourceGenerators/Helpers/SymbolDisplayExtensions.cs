namespace Atc.SourceGenerators.Helpers;

/// <summary>
/// Provides display strings for symbols that are emitted into generated code.
/// </summary>
internal static class SymbolDisplayExtensions
{
    /// <summary>
    /// Fully qualified with <c>global::</c> (and keeping nullable reference annotations), so a type name in
    /// generated code can't be captured by the namespace the generated code is declared in
    /// (e.g. <c>Mapping.Models.X</c> inside <c>namespace Atc.Mapping;</c> would bind to <c>Atc.Mapping.Models</c>).
    /// </summary>
    private static readonly SymbolDisplayFormat GlobalFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>
    /// Gets the <c>global::</c>-qualified display string of a symbol, for use in generated code.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>The fully qualified display string, e.g. <c>global::My.Namespace.MyType</c>.</returns>
    public static string ToGlobalDisplayString(this ISymbol symbol)
        => symbol.ToDisplayString(GlobalFormat);
}