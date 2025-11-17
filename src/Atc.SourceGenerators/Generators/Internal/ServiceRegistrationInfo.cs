namespace Atc.SourceGenerators.Generators.Internal;

internal sealed record ServiceRegistrationInfo(
    INamedTypeSymbol ClassSymbol,
    ServiceLifetime Lifetime,
    ImmutableArray<ITypeSymbol> AsTypes,
    bool AsSelf,
    bool IsHostedService,
    object? Key,
    string? FactoryMethodName,
    bool TryAdd,
    Location Location);