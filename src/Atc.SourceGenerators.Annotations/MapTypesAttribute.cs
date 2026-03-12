namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Defines a mapping between two types at the assembly level.
/// This is a shorthand alternative to <see cref="MappingConfigurationAttribute"/> for simple mapping scenarios.
/// </summary>
/// <remarks>
/// <para>
/// Use this attribute when you need to map types from external assemblies without creating
/// a full mapping configuration class. For more advanced scenarios (property ignoring,
/// renaming, etc.), use <see cref="MappingConfigurationAttribute"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [assembly: MapTypes(typeof(ExternalLib.Contact), typeof(MyApp.Customer),
///     PropertyMap = new[] { "EmailAddress:Email", "PhoneNumber:Phone" },
///     IgnoreSourceProperties = new[] { "InternalId" },
///     Bidirectional = true)]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
public sealed class MapTypesAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapTypesAttribute"/> class.
    /// </summary>
    /// <param name="sourceType">The source type to map from.</param>
    /// <param name="targetType">The target type to map to.</param>
    public MapTypesAttribute(
        Type sourceType,
        Type targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    /// <summary>
    /// Gets the source type to map from.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the target type to map to.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Gets or sets the property name mappings in "SourceProperty:TargetProperty" format.
    /// </summary>
    /// <example>
    /// <code>
    /// PropertyMap = new[] { "EmailAddress:Email", "PhoneNumber:Phone" }
    /// </code>
    /// </example>
    public string[]? PropertyMap { get; set; }

    /// <summary>
    /// Gets or sets the source property names to exclude from mapping.
    /// </summary>
    public string[]? IgnoreSourceProperties { get; set; }

    /// <summary>
    /// Gets or sets the target property names to exclude from mapping.
    /// </summary>
    public string[]? IgnoreTargetProperties { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate bidirectional mappings.
    /// Default is false.
    /// </summary>
    public bool Bidirectional { get; set; }

    /// <summary>
    /// Gets or sets the naming strategy for property name conversion.
    /// Default is PascalCase (no transformation).
    /// </summary>
    public PropertyNameStrategy PropertyNameStrategy { get; set; } = PropertyNameStrategy.PascalCase;
}