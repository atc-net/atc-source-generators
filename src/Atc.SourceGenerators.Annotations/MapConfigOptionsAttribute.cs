namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Configures advanced mapping options for a configuration-based mapping method.
/// Applied to partial methods in a <see cref="MappingConfigurationAttribute"/> class.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is optional. When not specified, default mapping behavior is used.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [MappingConfiguration]
/// public static partial class ExternalMappings
/// {
///     [MapConfigOptions(Bidirectional = true)]
///     public static partial OrderDto MapToOrderDto(this ExternalLib.Order source);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class MapConfigOptionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether to generate bidirectional mappings
    /// (both Source -> Target and Target -> Source).
    /// Default is false.
    /// </summary>
    public bool Bidirectional { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable property flattening.
    /// When enabled, nested object properties are flattened using naming convention {PropertyName}{NestedPropertyName}.
    /// Default is false.
    /// </summary>
    public bool EnableFlattening { get; set; }

    /// <summary>
    /// Gets or sets the naming strategy for property name conversion during mapping.
    /// Allows automatic mapping between different naming conventions.
    /// Default is PascalCase (no transformation).
    /// </summary>
    public PropertyNameStrategy PropertyNameStrategy { get; set; } = PropertyNameStrategy.PascalCase;
}