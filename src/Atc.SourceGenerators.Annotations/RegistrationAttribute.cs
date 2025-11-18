// ReSharper disable RedundantAttributeUsageProperty
// ReSharper disable CheckNamespace
#pragma warning disable IDE0130
namespace Atc.DependencyInjection;

/// <summary>
/// Marks a class for automatic registration in the dependency injection container.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RegistrationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationAttribute"/> class.
    /// </summary>
    /// <param name="lifetime">The service lifetime. Default is <see cref="Lifetime.Singleton"/>.</param>
    public RegistrationAttribute(Lifetime lifetime = Lifetime.Singleton)
        => Lifetime = lifetime;

    /// <summary>
    /// Gets the service lifetime.
    /// </summary>
    public Lifetime Lifetime { get; }

    /// <summary>
    /// Gets or sets the service type to register against (typically an interface).
    /// If not specified, the service will be registered as its concrete type.
    /// </summary>
    public global::System.Type? As { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to also register the concrete type
    /// when <see cref="As"/> is specified. Default is false.
    /// </summary>
    /// <remarks>
    /// When true, the service will be registered both as the interface (specified in <see cref="As"/>)
    /// and as its concrete type, allowing resolution of both.
    /// </remarks>
    public bool AsSelf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this service is a decorator.
    /// When true, this service wraps the previous registration of the same interface.
    /// </summary>
    /// <remarks>
    /// Decorators are useful for implementing cross-cutting concerns like logging, caching,
    /// validation, or retry logic without modifying the original service implementation.
    /// The decorator's constructor must accept the interface it decorates as the first parameter.
    /// </remarks>
    public bool Decorator { get; set; }

    /// <summary>
    /// Gets or sets the name of a static field, property, or parameterless method that provides a pre-created instance.
    /// When specified, the instance will be registered as a singleton.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instance registration is useful when you have a pre-configured singleton instance that should be shared across the application.
    /// The referenced member must be static and return a compatible type (the class itself or the registered interface).
    /// </para>
    /// <para>
    /// Note: Instance registration only supports Singleton lifetime. Using Instance with Scoped or Transient lifetime will result in a compile error.
    /// Instance and Factory parameters are mutually exclusive - you cannot use both on the same service.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Registration(As = typeof(IConfiguration), Instance = nameof(DefaultInstance))]
    /// public class AppConfiguration : IConfiguration
    /// {
    ///     public static readonly AppConfiguration DefaultInstance = new AppConfiguration
    ///     {
    ///         Setting1 = "default",
    ///         Setting2 = 42
    ///     };
    /// }
    /// </code>
    /// </example>
    public string? Instance { get; set; }

    /// <summary>
    /// Gets or sets the configuration key path that determines whether this service should be registered.
    /// The service will only be registered if the configuration value at this path evaluates to true.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Conditional registration allows services to be registered based on runtime configuration values,
    /// such as feature flags or environment-specific settings. The condition string should be a valid
    /// configuration key path (e.g., "Features:UseRedisCache").
    /// </para>
    /// <para>
    /// Prefix the condition with "!" to negate it. For example, "!Features:UseRedisCache" will register
    /// the service only when the configuration value is false.
    /// </para>
    /// <para>
    /// When conditional registration is used, an IConfiguration parameter will be added to the registration
    /// method signature, and the configuration value will be checked at runtime before registering the service.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register RedisCache only when Features:UseRedisCache is true
    /// [Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
    /// public class RedisCache : ICache { }
    ///
    /// // Register MemoryCache only when Features:UseRedisCache is false
    /// [Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
    /// public class MemoryCache : ICache { }
    /// </code>
    /// </example>
    public string? Condition { get; set; }
}