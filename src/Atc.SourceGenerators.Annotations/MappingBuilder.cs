namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Provides a fluent API for configuring type mappings inline.
/// The Map() calls are analyzed at compile time by the source generator to produce mapping extension methods.
/// At runtime, this class is a no-op.
/// </summary>
public sealed class MappingBuilder
{
    internal MappingBuilder()
    {
    }

    /// <summary>
    /// Registers a mapping between two types using typeof() expressions.
    /// </summary>
    /// <param name="sourceType">The source type to map from.</param>
    /// <param name="targetType">The target type to map to.</param>
    /// <param name="bidirectional">Whether to generate bidirectional mappings.</param>
    /// <param name="propertyMap">Property name mappings in "SourceProperty:TargetProperty" format.</param>
    /// <param name="ignoreSourceProperties">Source property names to exclude from mapping.</param>
    /// <param name="ignoreTargetProperties">Target property names to exclude from mapping.</param>
    /// <param name="propertyNameStrategy">The naming strategy for property name conversion.</param>
    /// <returns>This builder for fluent chaining.</returns>
    public MappingBuilder Map(
        Type sourceType,
        Type targetType,
        bool bidirectional = false,
        string[]? propertyMap = null,
        string[]? ignoreSourceProperties = null,
        string[]? ignoreTargetProperties = null,
        PropertyNameStrategy propertyNameStrategy = PropertyNameStrategy.PascalCase)
        => this;

    /// <summary>
    /// Registers a mapping between two types using generic type parameters.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TTarget">The target type to map to.</typeparam>
    /// <param name="bidirectional">Whether to generate bidirectional mappings.</param>
    /// <param name="propertyMap">Property name mappings in "SourceProperty:TargetProperty" format.</param>
    /// <param name="ignoreSourceProperties">Source property names to exclude from mapping.</param>
    /// <param name="ignoreTargetProperties">Target property names to exclude from mapping.</param>
    /// <param name="propertyNameStrategy">The naming strategy for property name conversion.</param>
    /// <returns>This builder for fluent chaining.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "S2326:Unused type parameters should be removed", Justification = "Type parameters are analyzed at compile time by the source generator")]
    public MappingBuilder Map<TSource, TTarget>(
        bool bidirectional = false,
        string[]? propertyMap = null,
        string[]? ignoreSourceProperties = null,
        string[]? ignoreTargetProperties = null,
        PropertyNameStrategy propertyNameStrategy = PropertyNameStrategy.PascalCase)
        => this;

    /// <summary>
    /// Configures mapping registrations without requiring dependency injection.
    /// The lambda body is analyzed at compile time by the source generator.
    /// At runtime, this is a no-op.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    public static void Configure(Action<MappingBuilder> configure)
    {
        // No-op — mappings are generated at compile time
    }
}