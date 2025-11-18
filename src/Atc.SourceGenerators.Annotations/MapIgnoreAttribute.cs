namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Marks a property to be excluded from automatic mapping code generation.
/// Properties decorated with this attribute will be skipped when generating mapping methods.
/// </summary>
/// <remarks>
/// <para>
/// Use this attribute to exclude sensitive data, internal state, or audit fields
/// that should not be mapped to target types (e.g., DTOs, API responses).
/// </para>
/// <para>
/// The attribute can be applied to properties on either the source or target type.
/// When applied to the source type, the property will not be read during mapping.
/// When applied to the target type, the property will not be set during mapping.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [MapTo(typeof(UserDto))]
/// public partial class User
/// {
///     public Guid Id { get; set; }
///     public string Name { get; set; } = string.Empty;
///
///     [MapIgnore]
///     public byte[] PasswordHash { get; set; } = Array.Empty&lt;byte&gt;();  // Excluded from mapping
///
///     [MapIgnore]
///     public DateTime CreatedAt { get; set; }  // Internal audit field
/// }
///
/// public class UserDto
/// {
///     public Guid Id { get; set; }
///     public string Name { get; set; } = string.Empty;
///     // PasswordHash and CreatedAt are NOT mapped
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class MapIgnoreAttribute : Attribute
{
}