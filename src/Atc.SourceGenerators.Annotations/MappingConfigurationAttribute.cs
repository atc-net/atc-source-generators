namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Marks a static partial class as a mapping configuration container.
/// Methods in this class define mapping operations for types that cannot be decorated with [MapTo] attributes,
/// such as third-party types from external assemblies.
/// </summary>
/// <remarks>
/// <para>
/// The decorated class must be declared as both <c>static</c> and <c>partial</c>.
/// Each partial method in the class defines a mapping from the parameter type (source) to the return type (target).
/// Methods must be extension methods (first parameter uses <c>this</c> keyword).
/// </para>
/// <para>
/// Use <see cref="MapConfigIgnoreAttribute"/> to exclude specific properties from mapping.
/// Use <see cref="MapConfigPropertyAttribute"/> to rename properties during mapping.
/// Use <see cref="MapConfigOptionsAttribute"/> to configure advanced mapping options.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [MappingConfiguration]
/// public static partial class ExternalMappings
/// {
///     public static partial CustomerDto MapToCustomerDto(this ExternalLib.Contact source);
///
///     [MapConfigIgnore("InternalId")]
///     [MapConfigProperty("FullName", "DisplayName")]
///     public static partial UserDto MapToUserDto(this ThirdParty.User source);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MappingConfigurationAttribute : Attribute
{
}