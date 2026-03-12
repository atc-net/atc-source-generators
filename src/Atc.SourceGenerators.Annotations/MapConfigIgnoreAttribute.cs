namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Specifies a property to exclude from configuration-based mapping.
/// Applied to partial methods in a <see cref="MappingConfigurationAttribute"/> class.
/// </summary>
/// <remarks>
/// <para>
/// Multiple <see cref="MapConfigIgnoreAttribute"/> instances can be applied to a single method
/// to exclude multiple properties from the mapping.
/// </para>
/// <para>
/// The property name refers to a property on the source type (the method parameter type).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [MappingConfiguration]
/// public static partial class ExternalMappings
/// {
///     [MapConfigIgnore("InternalId")]
///     [MapConfigIgnore("PasswordHash")]
///     public static partial UserDto MapToUserDto(this ThirdParty.User source);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapConfigIgnoreAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapConfigIgnoreAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the source property to exclude from mapping.</param>
    public MapConfigIgnoreAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the name of the source property to exclude from mapping.
    /// </summary>
    public string PropertyName { get; }
}