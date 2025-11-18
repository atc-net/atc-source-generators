namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Specifies a derived type mapping for polymorphic object mapping.
/// Used in conjunction with <see cref="MapToAttribute"/> to define how derived types should be mapped.
/// </summary>
/// <remarks>
/// <para>
/// When mapping abstract base classes or interfaces, use <see cref="MapDerivedTypeAttribute"/> to specify
/// how each derived type should be mapped. The generator creates a switch expression that performs
/// type pattern matching at runtime and delegates to the appropriate mapping method for each derived type.
/// </para>
/// <para>
/// Each derived type must have its own <see cref="MapToAttribute"/> that creates a mapping to the corresponding target type.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define abstract base classes
/// public abstract class AnimalEntity { }
/// public class DogEntity : AnimalEntity { public string Breed { get; set; } = ""; }
/// public class CatEntity : AnimalEntity { public int Lives { get; set; } }
///
/// public abstract class Animal { }
/// public class Dog : Animal { public string Breed { get; set; } = ""; }
/// public class Cat : Animal { public int Lives { get; set; } }
///
/// // Configure polymorphic mapping on the base class
/// [MapTo(typeof(Animal))]
/// [MapDerivedType(typeof(DogEntity), typeof(Dog))]
/// [MapDerivedType(typeof(CatEntity), typeof(Cat))]
/// public abstract partial class AnimalEntity { }
///
/// // Define mappings for each derived type
/// [MapTo(typeof(Dog))]
/// public partial class DogEntity : AnimalEntity { }
///
/// [MapTo(typeof(Cat))]
/// public partial class CatEntity : AnimalEntity { }
///
/// // Generated polymorphic mapping method:
/// public static Animal MapToAnimal(this AnimalEntity source)
/// {
///     return source switch
///     {
///         DogEntity dog => dog.MapToDog(),
///         CatEntity cat => cat.MapToCat(),
///         _ => throw new ArgumentException($"Unknown derived type: {source.GetType().Name}")
///     };
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class MapDerivedTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapDerivedTypeAttribute"/> class.
    /// </summary>
    /// <param name="sourceType">The source derived type to match.</param>
    /// <param name="targetType">The target derived type to map to.</param>
    public MapDerivedTypeAttribute(
        Type sourceType,
        Type targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    /// <summary>
    /// Gets the source derived type to match during polymorphic mapping.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the target derived type to map to when the source type matches.
    /// </summary>
    public Type TargetType { get; }
}