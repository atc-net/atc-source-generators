namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Marks a class or enum for automatic mapping code generation.
/// Generates extension methods to map from the decorated type to the specified target type.
/// </summary>
/// <remarks>
/// <para>
/// For classes, the generator creates mapping methods that:
/// - Map matching properties by name and type
/// - Handle nested object mappings recursively
/// - Support enum conversions
/// - Provide compile-time safety
/// </para>
/// <para>
/// For enums, the generator creates switch expression mappings that:
/// - Map enum values by name (case-insensitive)
/// - Handle special cases (None → Unknown, Active → Enabled, etc.)
/// - Provide runtime safety with ArgumentOutOfRangeException
/// </para>
/// <para>
/// Classes must be declared as partial. Enums do not need to be partial.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Class mapping
/// [MapTo(typeof(Api.Contracts.UserResponse))]
/// public partial class User
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public Address Address { get; set; }
/// }
///
/// // Enum mapping
/// [MapTo(typeof(StatusDto))]
/// public enum StatusEntity
/// {
///     None,      // Maps to StatusDto.Unknown (special case)
///     Active,
///     Inactive,
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
public sealed class MapToAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapToAttribute"/> class.
    /// </summary>
    /// <param name="targetType">The target type to map to.</param>
    public MapToAttribute(Type targetType)
    {
        TargetType = targetType;
    }

    /// <summary>
    /// Gets the target type to map to.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate bidirectional mappings
    /// (both Source → Target and Target → Source).
    /// Default is false.
    /// </summary>
    public bool Bidirectional { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable property flattening.
    /// When enabled, nested object properties are flattened using naming convention {PropertyName}{NestedPropertyName}.
    /// For example, source.Address.City maps to target.AddressCity.
    /// Default is false.
    /// </summary>
    public bool EnableFlattening { get; set; }
}