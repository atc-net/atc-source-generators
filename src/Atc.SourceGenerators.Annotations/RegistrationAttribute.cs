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
}