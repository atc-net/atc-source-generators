namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// A [Registration] class as equatable data for incremental caching: everything the registration code needs,
/// precomputed from the symbols in the pipeline transform.
/// </summary>
internal sealed record ServiceRegistrationInfo(
    string ClassName,
    string ClassFullName,
    string ClassNamespace,
    bool IsGeneric,
    string OpenGenericClassName,
    EquatableArray<string> InterfaceDefinitionNames,
    ServiceLifetime Lifetime,
    EquatableArray<RegisteredTypeInfo> AsTypes,
    bool AsSelf,
    bool IsHostedService,
    string? Key,
    string? FactoryMethodName,
    bool FactoryNeedsCastForSelf,
    bool TryAdd,
    bool Decorator,
    string? InstanceMemberName,
    bool IsInstanceMethod,
    string? Condition,
    LocationInfo Location,
    DiagnosticInfo? ValidationError);