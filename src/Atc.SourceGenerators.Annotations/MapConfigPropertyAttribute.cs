namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Specifies a property name mapping between source and target types in configuration-based mapping.
/// Applied to partial methods in a <see cref="MappingConfigurationAttribute"/> class.
/// </summary>
/// <remarks>
/// <para>
/// Multiple <see cref="MapConfigPropertyAttribute"/> instances can be applied to a single method
/// to rename multiple properties during mapping.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [MappingConfiguration]
/// public static partial class ExternalMappings
/// {
///     [MapConfigProperty("FullName", "DisplayName")]
///     [MapConfigProperty("EmailAddress", "Email")]
///     public static partial CustomerDto MapToCustomerDto(this ThirdParty.Contact source);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapConfigPropertyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapConfigPropertyAttribute"/> class.
    /// </summary>
    /// <param name="sourcePropertyName">The name of the property on the source type.</param>
    /// <param name="targetPropertyName">The name of the property on the target type.</param>
    public MapConfigPropertyAttribute(
        string sourcePropertyName,
        string targetPropertyName)
    {
        SourcePropertyName = sourcePropertyName;
        TargetPropertyName = targetPropertyName;
    }

    /// <summary>
    /// Gets the name of the property on the source type.
    /// </summary>
    public string SourcePropertyName { get; }

    /// <summary>
    /// Gets the name of the property on the target type.
    /// </summary>
    public string TargetPropertyName { get; }
}