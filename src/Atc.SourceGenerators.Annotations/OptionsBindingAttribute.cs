// ReSharper disable RedundantAttributeUsageProperty
namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Marks a class for automatic options binding from configuration.
/// <para>Section name resolution priority:</para>
/// <list type="number">
/// <item><description>Explicit sectionName parameter</description></item>
/// <item><description>Public const string SectionName in the class</description></item>
/// <item><description>Public const string NameTitle in the class</description></item>
/// <item><description>Public const string Name in the class</description></item>
/// <item><description>Auto-inferred from class name (uses full class name)</description></item>
/// </list>
/// <para>Supports multiple named instances by applying the attribute multiple times with different Name values.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class OptionsBindingAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsBindingAttribute"/> class.
    /// </summary>
    /// <param name="sectionName">
    /// The configuration section name. If null, the section name is resolved in this order:
    /// <list type="number">
    /// <item><description>From public const string SectionName (highest priority)</description></item>
    /// <item><description>From public const string NameTitle</description></item>
    /// <item><description>From public const string Name</description></item>
    /// <item><description>Auto-inferred from class name (uses full class name as-is)</description></item>
    /// </list>
    /// </param>
    public OptionsBindingAttribute(string? sectionName = null)
        => SectionName = sectionName;

    /// <summary>
    /// Gets the configuration section name.
    /// If null, the section name will be resolved from const SectionName/NameTitle/Name or auto-inferred from the class name.
    /// </summary>
    public string? SectionName { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate the options on application start.
    /// Default is false.
    /// </summary>
    public bool ValidateOnStart { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate using data annotations.
    /// Default is false.
    /// </summary>
    public bool ValidateDataAnnotations { get; set; }

    /// <summary>
    /// Gets or sets the options lifetime.
    /// Default is <see cref="OptionsLifetime.Singleton"/>.
    /// </summary>
    public OptionsLifetime Lifetime { get; set; } = OptionsLifetime.Singleton;

    /// <summary>
    /// Gets or sets the validator type for custom validation logic.
    /// The type must implement <c>IValidateOptions&lt;T&gt;</c> where T is the options class.
    /// The validator will be registered as a singleton and executed during options validation.
    /// Default is null (no custom validator).
    /// </summary>
    public Type? Validator { get; set; }

    /// <summary>
    /// Gets or sets the name for named options instances.
    /// When specified, enables multiple configurations of the same options type with different names.
    /// Use <c>IOptionsSnapshot&lt;T&gt;.Get(name)</c> to retrieve specific named instances.
    /// Default is null (unnamed options).
    /// </summary>
    public string? Name { get; set; }
}
