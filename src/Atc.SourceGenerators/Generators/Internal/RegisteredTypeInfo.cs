namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// A service type a class is registered as (an interface or abstract class).
/// </summary>
/// <param name="DisplayName">The type name for generated code.</param>
/// <param name="Name">The simple type name.</param>
/// <param name="IsGeneric">Whether the type is generic.</param>
/// <param name="OpenGenericName">The open generic type name, e.g. <c>IRepository&lt;&gt;</c>.</param>
/// <param name="FactoryNeedsCast">Whether the factory's return type differs from this type, so the call needs a cast.</param>
internal sealed record RegisteredTypeInfo(
    string DisplayName,
    string Name,
    bool IsGeneric,
    string OpenGenericName,
    bool FactoryNeedsCast);