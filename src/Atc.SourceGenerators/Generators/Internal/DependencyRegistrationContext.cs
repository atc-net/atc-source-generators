namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// The compilation data the registration code needs, as equatable data.
/// </summary>
/// <param name="AssemblyName">The assembly name of the compilation.</param>
/// <param name="FilterRules">The [RegistrationFilter] rules of the assembly.</param>
internal sealed record DependencyRegistrationContext(
    string AssemblyName,
    FilterRulesInfo FilterRules);