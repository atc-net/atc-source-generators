namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// An enum-to-enum mapping as equatable data (no symbols), so the incremental pipeline can cache it.
/// </summary>
/// <param name="SourceEnumName">The fully qualified (global::) source enum name.</param>
/// <param name="SourceEnumSimpleName">The simple source enum name (used for the reverse method name).</param>
/// <param name="SourceNamespace">The source enum namespace, or <c>null</c> for the global namespace.</param>
/// <param name="TargetEnumName">The fully qualified (global::) target enum name.</param>
/// <param name="TargetEnumSimpleName">The simple target enum name (used for the method name).</param>
/// <param name="TargetNamespace">The target enum namespace, or <c>null</c> for the global namespace.</param>
/// <param name="ValueMappings">The source-to-target value mappings.</param>
/// <param name="ReverseValueMappings">The target-to-source value mappings (empty when not bidirectional).</param>
/// <param name="Bidirectional">Whether the reverse mapping is generated too.</param>
internal sealed record EnumMappingInfo(
    string SourceEnumName,
    string SourceEnumSimpleName,
    string? SourceNamespace,
    string TargetEnumName,
    string TargetEnumSimpleName,
    string? TargetNamespace,
    EquatableArray<EnumMappingHelper.EnumValueMapping> ValueMappings,
    EquatableArray<EnumMappingHelper.EnumValueMapping> ReverseValueMappings,
    bool Bidirectional);