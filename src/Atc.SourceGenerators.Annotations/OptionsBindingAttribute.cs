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

    /// <summary>
    /// Gets or sets a value indicating whether to throw an exception if the configuration section is missing or empty.
    /// When true, generates validation that ensures the configuration section exists and contains data.
    /// Recommended to combine with <c>ValidateOnStart = true</c> to detect missing configuration at application startup.
    /// Default is false.
    /// </summary>
    public bool ErrorOnMissingKeys { get; set; }

    /// <summary>
    /// Gets or sets the name of a static method to call when configuration changes are detected.
    /// Only applicable when <c>Lifetime = OptionsLifetime.Monitor</c>.
    /// The method must have the signature: <c>static void MethodName(TOptions options, string? name)</c>
    /// where TOptions is the options class type.
    /// The callback will be automatically registered via an IHostedService when the application starts.
    /// Default is null (no change callback).
    /// </summary>
    /// <remarks>
    /// Configuration change detection only works with file-based configuration providers (e.g., appsettings.json with reloadOnChange: true).
    /// The callback is invoked whenever the configuration file changes and is reloaded.
    /// </remarks>
    public string? OnChange { get; set; }

    /// <summary>
    /// Gets or sets the name of a static method to call after configuration binding and validation.
    /// The method must have the signature: <c>static void MethodName(TOptions options)</c>
    /// where TOptions is the options class type.
    /// This is useful for applying defaults, normalizing values, or computing derived properties.
    /// The post-configuration action runs after binding and validation, using the <c>.PostConfigure()</c> pattern.
    /// Default is null (no post-configuration).
    /// </summary>
    /// <remarks>
    /// Post-configuration is executed after the options are bound from configuration and after validation.
    /// This allows for final transformations like ensuring paths end with separators, normalizing URLs, or setting computed properties.
    /// Cannot be used with named options.
    /// </remarks>
    public string? PostConfigure { get; set; }

    /// <summary>
    /// Gets or sets the name of a static method to configure ALL named instances with default values.
    /// The method must have the signature: <c>static void MethodName(TOptions options)</c>
    /// where TOptions is the options class type.
    /// This is useful for setting default values that apply to all named instances before individual configurations override them.
    /// The configuration action runs using the <c>.ConfigureAll()</c> pattern before individual <c>Configure()</c> calls.
    /// Default is null (no configure-all).
    /// Only applicable when the class has multiple named instances (Name property specified on multiple attributes).
    /// </summary>
    /// <remarks>
    /// ConfigureAll is executed BEFORE individual named instance configurations, allowing you to set defaults.
    /// For example, set MaxRetries=3 for all database connections, then override for specific instances.
    /// Specify ConfigureAll on any one of the [OptionsBinding] attributes when using named options.
    /// Cannot be used with single unnamed instances (use PostConfigure instead).
    /// </remarks>
    public string? ConfigureAll { get; set; }
}