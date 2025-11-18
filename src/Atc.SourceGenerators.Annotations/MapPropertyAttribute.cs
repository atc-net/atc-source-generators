namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Specifies a custom target property name for mapping when property names differ between source and target types.
/// </summary>
/// <remarks>
/// <para>
/// Use this attribute to map properties with different names without having to rename your domain models.
/// This is useful when integrating with external APIs, legacy systems, or when following different naming conventions.
/// </para>
/// <para>
/// The attribute accepts the target property name as a string parameter. You can use either string literals
/// or the <c>nameof()</c> operator for type-safe property names.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [MapTo(typeof(UserDto))]
/// public partial class User
/// {
///     public Guid Id { get; set; }
///
///     [MapProperty(nameof(UserDto.FullName))]
///     public string Name { get; set; } = string.Empty;  // Maps to UserDto.FullName
///
///     [MapProperty("Age")]
///     public int YearsOld { get; set; }  // Maps to UserDto.Age
/// }
///
/// public class UserDto
/// {
///     public Guid Id { get; set; }
///     public string FullName { get; set; } = string.Empty;
///     public int Age { get; set; }
/// }
///
/// // Generated mapping code:
/// var dto = user.MapToUserDto();  // user.Name → dto.FullName, user.YearsOld → dto.Age
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class MapPropertyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapPropertyAttribute"/> class.
    /// </summary>
    /// <param name="targetPropertyName">
    /// The name of the target property to map to. Can be a string literal or use <c>nameof()</c>.
    /// </param>
    public MapPropertyAttribute(string targetPropertyName)
    {
        TargetPropertyName = targetPropertyName;
    }

    /// <summary>
    /// Gets the name of the target property to map to.
    /// </summary>
    public string TargetPropertyName { get; }
}