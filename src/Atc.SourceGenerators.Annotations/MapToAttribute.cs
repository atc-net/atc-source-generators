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

    /// <summary>
    /// Gets or sets the name of a static method to call before performing the mapping.
    /// The method must have the signature: static void MethodName(SourceType source).
    /// Use this for custom validation or preprocessing logic before mapping.
    /// </summary>
    /// <example>
    /// <code>
    /// [MapTo(typeof(UserDto), BeforeMap = nameof(ValidateUser))]
    /// public partial class User
    /// {
    ///     public Guid Id { get; set; }
    ///     public string Name { get; set; } = string.Empty;
    ///
    ///     private static void ValidateUser(User source)
    ///     {
    ///         if (string.IsNullOrWhiteSpace(source.Name))
    ///             throw new ArgumentException("Name cannot be empty");
    ///     }
    /// }
    /// </code>
    /// </example>
    public string? BeforeMap { get; set; }

    /// <summary>
    /// Gets or sets the name of a static method to call after performing the mapping.
    /// The method must have the signature: static void MethodName(SourceType source, TargetType target).
    /// Use this for custom post-processing logic after mapping.
    /// </summary>
    /// <example>
    /// <code>
    /// [MapTo(typeof(UserDto), AfterMap = nameof(EnrichUserDto))]
    /// public partial class User
    /// {
    ///     public Guid Id { get; set; }
    ///     public string Name { get; set; } = string.Empty;
    ///
    ///     private static void EnrichUserDto(User source, UserDto target)
    ///     {
    ///         target.DisplayName = $"{source.Name} (ID: {source.Id})";
    ///     }
    /// }
    /// </code>
    /// </example>
    public string? AfterMap { get; set; }

    /// <summary>
    /// Gets or sets the name of a static factory method to use for creating the target instance.
    /// The method must have the signature: static TargetType MethodName().
    /// Use this to customize object creation (e.g., setting default values, using object pooling, etc.).
    /// </summary>
    /// <example>
    /// <code>
    /// [MapTo(typeof(UserDto), Factory = nameof(CreateUserDto))]
    /// public partial class User
    /// {
    ///     public Guid Id { get; set; }
    ///     public string Name { get; set; } = string.Empty;
    ///
    ///     private static UserDto CreateUserDto()
    ///     {
    ///         return new UserDto { CreatedAt = DateTime.UtcNow };
    ///     }
    /// }
    /// </code>
    /// </example>
    public string? Factory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate an additional method overload
    /// that updates an existing target instance instead of creating a new one.
    /// When enabled, generates both MapToX() and MapToX(target) methods.
    /// Useful for updating EF Core tracked entities.
    /// Default is false.
    /// </summary>
    /// <example>
    /// <code>
    /// [MapTo(typeof(UserDto), UpdateTarget = true)]
    /// public partial class User
    /// {
    ///     public Guid Id { get; set; }
    ///     public string Name { get; set; } = string.Empty;
    /// }
    ///
    /// // Generated methods:
    /// // 1. public static UserDto MapToUserDto(this User source) { ... }
    /// // 2. public static void MapToUserDto(this User source, UserDto target) { ... }
    ///
    /// // Usage for updating existing instance:
    /// var existingDto = repository.GetDto(id);
    /// user.MapToUserDto(existingDto);  // Updates existing instance
    /// </code>
    /// </example>
    public bool UpdateTarget { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate an Expression projection method
    /// for use with IQueryable (EF Core server-side projection).
    /// When enabled, generates a ProjectToX() method that returns Expression&lt;Func&lt;TSource, TTarget&gt;&gt;.
    /// This enables efficient database queries with only required columns selected.
    /// Default is false.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Projection expressions have limitations:
    /// - Cannot use BeforeMap/AfterMap hooks (expressions can't call methods)
    /// - Cannot use Factory methods (expressions must use object initializers)
    /// - Cannot map nested objects (no chained mapping calls in expressions)
    /// - Only simple property-to-property mappings are supported
    /// </para>
    /// <para>
    /// Use projections for read-only scenarios where you need efficient database queries.
    /// Use standard mapping methods for complex mappings with hooks and nested objects.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [MapTo(typeof(UserDto), GenerateProjection = true)]
    /// public partial class User
    /// {
    ///     public Guid Id { get; set; }
    ///     public string Name { get; set; } = string.Empty;
    ///     public string Email { get; set; } = string.Empty;
    /// }
    ///
    /// // Generated method:
    /// // public static Expression&lt;Func&lt;User, UserDto&gt;&gt; ProjectToUserDto()
    /// // {
    /// //     return source =&gt; new UserDto
    /// //     {
    /// //         Id = source.Id,
    /// //         Name = source.Name,
    /// //         Email = source.Email
    /// //     };
    /// // }
    ///
    /// // Usage with EF Core:
    /// var users = await dbContext.Users
    ///     .Where(u =&gt; u.IsActive)
    ///     .Select(User.ProjectToUserDto())
    ///     .ToListAsync();
    /// // SQL: SELECT Id, Name, Email FROM Users WHERE IsActive = 1
    /// </code>
    /// </example>
    public bool GenerateProjection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include private and internal members in the mapping.
    /// When enabled, uses UnsafeAccessor (NET 8+) for AOT-safe, zero-overhead access to private members.
    /// Default is false (only public members).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires .NET 8.0 or later for UnsafeAccessor support.
    /// Generates external accessor methods for each private/internal member.
    /// Fully AOT compatible with zero runtime reflection.
    /// </para>
    /// <para>
    /// This feature allows mapping to/from:
    /// - Private properties
    /// - Internal properties
    /// - Protected properties (when source generator has access)
    /// - Private fields
    /// - Internal fields
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [MapTo(typeof(UserDto), IncludePrivateMembers = true)]
    /// public partial class User
    /// {
    ///     public Guid Id { get; set; }
    ///     private string _internalCode { get; set; } = string.Empty;  // Will be mapped
    ///     internal int Version { get; set; }  // Will be mapped
    /// }
    /// </code>
    /// </example>
    public bool IncludePrivateMembers { get; set; }
}