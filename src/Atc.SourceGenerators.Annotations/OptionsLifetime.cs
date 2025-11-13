namespace Atc.SourceGenerators.Annotations;

/// <summary>
/// Options lifetime enum for configuration options binding.
/// </summary>
public enum OptionsLifetime
{
    /// <summary>
    /// Specifies that options will be resolved using IOptions&lt;T&gt; (singleton).
    /// Options are computed once and cached for the lifetime of the application.
    /// </summary>
    Singleton = 0,

    /// <summary>
    /// Specifies that options will be resolved using IOptionsSnapshot&lt;T&gt; (scoped).
    /// Options are computed once per request/scope and support reloadable configuration.
    /// </summary>
    Scoped = 1,

    /// <summary>
    /// Specifies that options will be resolved using IOptionsMonitor&lt;T&gt;.
    /// Options support change notifications and are recomputed when configuration changes.
    /// </summary>
    Monitor = 2,
}