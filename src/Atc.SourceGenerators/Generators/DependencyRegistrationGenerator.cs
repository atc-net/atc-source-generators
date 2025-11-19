// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
// ReSharper disable NotAccessedPositionalProperty.Local
// ReSharper disable MemberHidesStaticFromOuterClass
namespace Atc.SourceGenerators.Generators;

/// <summary>
/// Source generator that automatically registers services decorated with [Registration] attribute
/// into the dependency injection container.
/// </summary>
[SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK")]
[Generator]
public class DependencyRegistrationGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Atc.DependencyInjection";
    private const string AttributeName = "RegistrationAttribute";
    private const string AttributeFullName = $"{AttributeNamespace}.{AttributeName}";
    private const string FilterAttributeName = "RegistrationFilterAttribute";
    private const string FilterAttributeFullName = $"{AttributeNamespace}.{FilterAttributeName}";

    private static readonly DiagnosticDescriptor AsTypeMustBeInterfaceDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.AsTypeMustBeInterface,
        title: "Service 'As' type must be an interface or abstract class",
        messageFormat: "The type '{0}' specified in As parameter must be an interface or abstract class, but is a {1}",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ClassDoesNotImplementInterfaceDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.ClassDoesNotImplementInterface,
        title: "Class does not implement specified interface or inherit from abstract class",
        messageFormat: "Class '{0}' does not implement interface or inherit from abstract class '{1}' specified in As parameter",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateRegistrationDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.DuplicateRegistration,
        title: "Duplicate service registration with different lifetime",
        messageFormat: "Service '{0}' is registered multiple times with different lifetimes ({1} and {2})",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor HostedServiceMustBeSingletonDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.HostedServiceMustBeSingleton,
        title: "Hosted services must use Singleton lifetime",
        messageFormat: "Hosted service '{0}' must use Singleton lifetime (or default [Registration]), but is registered with {1} lifetime",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor FactoryMethodNotFoundDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.FactoryMethodNotFound,
        title: "Factory method not found",
        messageFormat: "Factory method '{0}' not found in class '{1}'. Factory method must be static and return the service type.",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor FactoryMethodInvalidSignatureDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.FactoryMethodInvalidSignature,
        title: "Factory method has invalid signature",
        messageFormat: "Factory method '{0}' must be static, accept IServiceProvider as parameter, and return '{1}'",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InstanceMemberNotFoundDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.InstanceMemberNotFound,
        title: "Instance member not found",
        messageFormat: "Instance member '{0}' not found in class '{1}'. The member must be a static field, property, or parameterless method.",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InstanceMemberMustBeStaticDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.InstanceMemberMustBeStatic,
        title: "Instance member must be static",
        messageFormat: "Instance member '{0}' must be static",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InstanceAndFactoryMutuallyExclusiveDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.InstanceAndFactoryMutuallyExclusive,
        title: "Instance and Factory are mutually exclusive",
        messageFormat: "Cannot use both Instance and Factory parameters on the same service. Use either Instance for pre-created instances or Factory for custom creation logic.",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InstanceRequiresSingletonLifetimeDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.InstanceRequiresSingletonLifetime,
        title: "Instance registration requires Singleton lifetime",
        messageFormat: "Instance registration can only be used with Singleton lifetime. Current lifetime is '{0}'.",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate attribute fallback for projects that don't reference Atc.SourceGenerators.Annotations
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("RegistrationAttribute.g.cs", SourceText.From(GenerateAttributeCode(), Encoding.UTF8));
        });

        // Find all classes with the [Registration] attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateClass(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)
            .Collect();

        // Combine with compilation to generate the registration code
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations);

        context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
        {
            GenerateServiceRegistrations(source.Left, source.Right!, spc);
        });
    }

    private static bool IsCandidateClass(SyntaxNode node)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static bool IsSystemInterface(ITypeSymbol interfaceSymbol)
    {
        var namespaceName = interfaceSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Filter out System.* and Microsoft.* namespaces
        if (namespaceName.StartsWith("System", StringComparison.Ordinal) ||
            namespaceName.StartsWith("Microsoft", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static bool IsHostedService(INamedTypeSymbol classSymbol)
    {
        // Check if the class implements IHostedService or inherits from BackgroundService
        const string iHostedServiceFullName = "Microsoft.Extensions.Hosting.IHostedService";
        const string backgroundServiceFullName = "Microsoft.Extensions.Hosting.BackgroundService";

        // Check for IHostedService interface
        foreach (var iface in classSymbol.AllInterfaces)
        {
            if (iface.ToDisplayString() == iHostedServiceFullName)
            {
                return true;
            }
        }

        // Check for BackgroundService base class
        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            if (baseType.ToDisplayString() == backgroundServiceFullName)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static ServiceRegistrationInfo? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol is null)
        {
            return null;
        }

        // Look for the [Registration] attribute
        foreach (var attributeData in classSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass?.ToDisplayString() != AttributeFullName)
            {
                continue;
            }

            // Parse attribute arguments
            var lifetime = ServiceLifetime.Singleton; // default
            ITypeSymbol? explicitAsType = null;
            var asSelf = false;
            object? key = null;
            string? factoryMethodName = null;
            var tryAdd = false;
            var decorator = false;
            string? instanceMemberName = null;
            string? condition = null;

            // Constructor argument (lifetime)
            if (attributeData.ConstructorArguments.Length > 0)
            {
                var lifetimeValue = attributeData.ConstructorArguments[0].Value;
                if (lifetimeValue is int intValue)
                {
                    lifetime = (ServiceLifetime)intValue;
                }
            }

            // Named arguments (As, AsSelf, Key, Factory, TryAdd, Decorator, Instance, Condition)
            foreach (var namedArg in attributeData.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "As":
                        explicitAsType = namedArg.Value.Value as ITypeSymbol;
                        break;
                    case "AsSelf":
                        if (namedArg.Value.Value is bool selfValue)
                        {
                            asSelf = selfValue;
                        }

                        break;
                    case "Key":
                        key = namedArg.Value.Value;
                        break;
                    case "Factory":
                        factoryMethodName = namedArg.Value.Value as string;
                        break;
                    case "TryAdd":
                        if (namedArg.Value.Value is bool tryAddValue)
                        {
                            tryAdd = tryAddValue;
                        }

                        break;
                    case "Decorator":
                        if (namedArg.Value.Value is bool decoratorValue)
                        {
                            decorator = decoratorValue;
                        }

                        break;
                    case "Instance":
                        instanceMemberName = namedArg.Value.Value as string;
                        break;
                    case "Condition":
                        condition = namedArg.Value.Value as string;
                        break;
                }
            }

            // Determine which types to register against
            ImmutableArray<ITypeSymbol> asTypes;
            if (explicitAsType is not null)
            {
                // Explicit As parameter takes precedence
                asTypes = [explicitAsType];
            }
            else
            {
                // Auto-detect interfaces - get all non-system interfaces
                var interfaces = classSymbol.AllInterfaces
                    .Where(i => !IsSystemInterface(i))
                    .Cast<ITypeSymbol>()
                    .ToImmutableArray();

                asTypes = interfaces;
            }

            // Check if this is a hosted service
            var isHostedService = IsHostedService(classSymbol);

            return new ServiceRegistrationInfo(
                classSymbol,
                lifetime,
                asTypes,
                asSelf,
                isHostedService,
                key,
                factoryMethodName,
                tryAdd,
                decorator,
                instanceMemberName,
                condition,
                classDeclaration.GetLocation());
        }

        return null;
    }

    private static string GetAssemblyPrefix(string assemblyName)
    {
        // Extract prefix before first dot (e.g., "PetStore" from "PetStore.Domain")
        var dotIndex = assemblyName.IndexOf('.');
        return dotIndex > 0 ? assemblyName.Substring(0, dotIndex) : assemblyName;
    }

    private static ImmutableArray<ReferencedAssemblyInfo> GetReferencedAssembliesWithRegistrations(
        Compilation compilation)
    {
        var result = new List<ReferencedAssemblyInfo>();

        // Scan all referenced assemblies
        foreach (var reference in compilation.References)
        {
            // Get the assembly symbol for this reference
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
            {
                continue;
            }

            // Parse filter rules from the assembly
            var filterRules = ParseFilterRules(assemblySymbol);

            // Check if this assembly contains any types with [Registration] attribute
            // Traverse all types in the assembly's global namespace
            if (!HasRegistrationAttributeInNamespace(assemblySymbol.GlobalNamespace, filterRules))
            {
                continue;
            }

            // Extract assembly information
            var assemblyName = assemblySymbol.Name;
            var sanitizedName = SanitizeAssemblyName(assemblyName);
            var parts = assemblyName.Split('.');
            var shortName = parts[parts.Length - 1];

            result.Add(new ReferencedAssemblyInfo(assemblyName, sanitizedName, shortName));
        }

        return [.. result];
    }

    private static FilterRules ParseFilterRules(IAssemblySymbol assemblySymbol)
    {
        var excludedNamespaces = new List<string>();
        var excludedPatterns = new List<string>();
        var excludedInterfaces = new List<ITypeSymbol>();

        // Get all RegistrationFilter attributes on the assembly
        foreach (var attribute in assemblySymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != FilterAttributeFullName)
            {
                continue;
            }

            // Parse named arguments
            foreach (var namedArg in attribute.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "ExcludeNamespaces":
                        if (namedArg.Value.Kind == TypedConstantKind.Array &&
                            !namedArg.Value.IsNull)
                        {
                            foreach (var value in namedArg.Value.Values)
                            {
                                if (value.Value is string ns)
                                {
                                    excludedNamespaces.Add(ns);
                                }
                            }
                        }

                        break;

                    case "ExcludePatterns":
                        if (namedArg.Value.Kind == TypedConstantKind.Array &&
                            !namedArg.Value.IsNull)
                        {
                            foreach (var value in namedArg.Value.Values)
                            {
                                if (value.Value is string pattern)
                                {
                                    excludedPatterns.Add(pattern);
                                }
                            }
                        }

                        break;

                    case "ExcludeImplementing":
                        if (namedArg.Value.Kind == TypedConstantKind.Array &&
                            !namedArg.Value.IsNull)
                        {
                            foreach (var value in namedArg.Value.Values)
                            {
                                if (value.Value is ITypeSymbol typeSymbol)
                                {
                                    excludedInterfaces.Add(typeSymbol);
                                }
                            }
                        }

                        break;
                }
            }
        }

        return new FilterRules(
            [.. excludedNamespaces],
            [.. excludedPatterns],
            [.. excludedInterfaces]);
    }

    private static bool HasRegistrationAttributeInNamespace(
        INamespaceSymbol namespaceSymbol,
        FilterRules filterRules)
    {
        // Check all types in this namespace
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case INamedTypeSymbol typeSymbol:
                {
                    // Check if this type has the [Registration] attribute
                    foreach (var attribute in typeSymbol.GetAttributes())
                    {
                        if (attribute.AttributeClass?.ToDisplayString() == AttributeFullName &&
                            !filterRules.ShouldExclude(typeSymbol))
                        {
                            // Check if this type should be excluded by filter rules
                            return true;
                        }
                    }

                    break;
                }

                case INamespaceSymbol childNamespace when
                    HasRegistrationAttributeInNamespace(childNamespace, filterRules):
                    return true;
            }
        }

        return false;
    }

    private static void GenerateServiceRegistrations(
        Compilation compilation,
        ImmutableArray<ServiceRegistrationInfo> services,
        SourceProductionContext context)
    {
        if (services.IsEmpty)
        {
            return;
        }

        // Parse filter rules from the current assembly
        var filterRules = ParseFilterRules(compilation.Assembly);

        var validServices = new List<ServiceRegistrationInfo>();

        // Validate and emit diagnostics, and apply filters
        foreach (var service in services)
        {
            // Check if service should be excluded by filter rules
            if (filterRules.ShouldExclude(service.ClassSymbol))
            {
                continue;
            }

            var isValid = ValidateService(service, context);
            if (isValid)
            {
                validServices.Add(service);
            }
        }

        if (validServices.Count == 0)
        {
            return;
        }

        // Check for duplicate registrations
        CheckForDuplicateRegistrations(validServices, context);

        // Detect referenced assemblies with [Registration] attributes
        var referencedAssemblies = GetReferencedAssembliesWithRegistrations(compilation);

        // Generate the extension method
        var source = GenerateExtensionMethod(
            validServices,
            compilation.AssemblyName ?? "GeneratedAssembly",
            referencedAssemblies);
        context.AddSource("ServiceCollectionExtensions.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static bool ValidateService(
        ServiceRegistrationInfo service,
        SourceProductionContext context)
    {
        // Check if hosted service has non-Singleton lifetime
        if (service.IsHostedService &&
            service.Lifetime != ServiceLifetime.Singleton)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    HostedServiceMustBeSingletonDescriptor,
                    service.Location,
                    service.ClassSymbol.Name,
                    service.Lifetime));

            return false;
        }

        // Validate each interface or abstract class type
        foreach (var asType in service.AsTypes)
        {
            // Check if As is an interface or abstract class
            var isInterface = asType.TypeKind == TypeKind.Interface;
            var isAbstractClass = asType.TypeKind == TypeKind.Class && asType.IsAbstract;

            if (!isInterface && !isAbstractClass)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        AsTypeMustBeInterfaceDescriptor,
                        service.Location,
                        asType.Name,
                        asType
                            .TypeKind
                            .ToString()
                            .ToLowerInvariant()));

                return false;
            }

            // Check if the class implements the interface or inherits from abstract class
            bool implementsInterfaceOrInheritsAbstractClass;

            if (isInterface)
            {
                // Original interface check logic
                // For generic types, we need to compare the original definitions
                if (asType is INamedTypeSymbol { IsGenericType: true } asNamedType)
                {
                    var asTypeOriginal = asNamedType.OriginalDefinition;
                    implementsInterfaceOrInheritsAbstractClass = service.ClassSymbol.AllInterfaces.Any(i =>
                    {
                        if (i is INamedTypeSymbol { IsGenericType: true } iNamedType)
                        {
                            return SymbolEqualityComparer.Default.Equals(iNamedType.OriginalDefinition, asTypeOriginal);
                        }

                        return SymbolEqualityComparer.Default.Equals(i, asType);
                    });
                }
                else
                {
                    implementsInterfaceOrInheritsAbstractClass = service.ClassSymbol.AllInterfaces.Any(i =>
                        SymbolEqualityComparer.Default.Equals(i, asType));
                }
            }
            else
            {
                // Abstract class check
                // Walk up the inheritance hierarchy to check if the class inherits from the abstract class
                var baseType = service.ClassSymbol.BaseType;
                implementsInterfaceOrInheritsAbstractClass = false;

                while (baseType is not null)
                {
                    if (asType is INamedTypeSymbol { IsGenericType: true } asNamedType)
                    {
                        // Handle generic abstract classes
                        var asTypeOriginal = asNamedType.OriginalDefinition;
                        if (baseType is INamedTypeSymbol { IsGenericType: true } baseNamedType &&
                            SymbolEqualityComparer.Default.Equals(baseNamedType.OriginalDefinition, asTypeOriginal))
                        {
                            implementsInterfaceOrInheritsAbstractClass = true;
                            break;
                        }
                    }
                    else if (SymbolEqualityComparer.Default.Equals(baseType, asType))
                    {
                        implementsInterfaceOrInheritsAbstractClass = true;
                        break;
                    }

                    baseType = baseType.BaseType;
                }
            }

            if (!implementsInterfaceOrInheritsAbstractClass)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        ClassDoesNotImplementInterfaceDescriptor,
                        service.Location,
                        service.ClassSymbol.Name,
                        asType.Name));

                return false;
            }
        }

        // Validate factory method if specified
        if (!string.IsNullOrEmpty(service.FactoryMethodName))
        {
            var factoryMethod = service.ClassSymbol
                .GetMembers(service.FactoryMethodName!)
                .OfType<IMethodSymbol>()
                .FirstOrDefault();

            if (factoryMethod is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        FactoryMethodNotFoundDescriptor,
                        service.Location,
                        service.FactoryMethodName,
                        service.ClassSymbol.Name));

                return false;
            }

            // Determine the expected return type (first AsType if specified, otherwise the class itself)
            var expectedReturnType = service.AsTypes.Length > 0
                ? service.AsTypes[0]
                : service.ClassSymbol;

            // Validate factory method signature
            // Factory method must:
            // - Be static
            // - Accept IServiceProvider as single parameter
            // - Return the expected type or a type assignable to it (for abstract classes)
            var hasValidSignature =
                factoryMethod is { IsStatic: true, Parameters.Length: 1 } &&
                factoryMethod.Parameters[0].Type.ToDisplayString() == "System.IServiceProvider" &&
                IsReturnTypeValid(factoryMethod.ReturnType, expectedReturnType);

            if (!hasValidSignature)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        FactoryMethodInvalidSignatureDescriptor,
                        service.Location,
                        service.FactoryMethodName,
                        expectedReturnType.ToDisplayString()));

                return false;
            }
        }

        // Validate instance registration if specified
        if (!string.IsNullOrEmpty(service.InstanceMemberName))
        {
            // Check mutually exclusive with Factory
            if (!string.IsNullOrEmpty(service.FactoryMethodName))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InstanceAndFactoryMutuallyExclusiveDescriptor,
                        service.Location));

                return false;
            }

            // Check lifetime is Singleton
            if (service.Lifetime != ServiceLifetime.Singleton)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InstanceRequiresSingletonLifetimeDescriptor,
                        service.Location,
                        service.Lifetime.ToString()));

                return false;
            }

            // Find the instance member (field, property, or method)
            var members = service.ClassSymbol.GetMembers(service.InstanceMemberName!);

            // Try to find as field or property first
            var fieldSymbols = members.OfType<IFieldSymbol>();
            ISymbol? fieldMember = fieldSymbols.FirstOrDefault();
            var propertySymbols = members.OfType<IPropertySymbol>();
            var propertyMember = propertySymbols.FirstOrDefault();
            var instanceMember = fieldMember ?? propertyMember;

            // If not found, try as parameterless method
            if (instanceMember is null)
            {
                var methodSymbols = members.OfType<IMethodSymbol>();
                instanceMember = methodSymbols.FirstOrDefault(m => m.Parameters.Length == 0);
            }

            if (instanceMember is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InstanceMemberNotFoundDescriptor,
                        service.Location,
                        service.InstanceMemberName,
                        service.ClassSymbol.Name));

                return false;
            }

            // Validate member is static
            var isStatic = instanceMember switch
            {
                IFieldSymbol field => field.IsStatic,
                IPropertySymbol property => property.IsStatic,
                IMethodSymbol method => method.IsStatic,
                _ => false,
            };

            if (!isStatic)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InstanceMemberMustBeStaticDescriptor,
                        service.Location,
                        service.InstanceMemberName));

                return false;
            }
        }

        return true;
    }

    private static void CheckForDuplicateRegistrations(
        List<ServiceRegistrationInfo> services,
        SourceProductionContext context)
    {
        var registrations = new Dictionary<string, (ServiceLifetime Lifetime, Location Location)>(StringComparer.Ordinal);

        foreach (var service in services)
        {
            // Check each registered interface type
            if (service.AsTypes.Length > 0)
            {
                foreach (var asType in service.AsTypes)
                {
                    var key = asType.ToDisplayString();

                    if (registrations.TryGetValue(key, out var existing))
                    {
                        if (existing.Lifetime != service.Lifetime)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    DuplicateRegistrationDescriptor,
                                    service.Location,
                                    key,
                                    existing.Lifetime,
                                    service.Lifetime));
                        }
                    }
                    else
                    {
                        registrations[key] = (service.Lifetime, service.Location);
                    }
                }
            }
            else
            {
                // No interfaces - register as concrete type
                var key = service.ClassSymbol.ToDisplayString();
                if (registrations.TryGetValue(key, out var existing))
                {
                    if (existing.Lifetime != service.Lifetime)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DuplicateRegistrationDescriptor,
                                service.Location,
                                key,
                                existing.Lifetime,
                                service.Lifetime));
                    }
                }
                else
                {
                    registrations[key] = (service.Lifetime, service.Location);
                }
            }
        }
    }

    private static string SanitizeAssemblyName(string assemblyName)
    {
        // Remove common prefixes and sanitize to create a valid method name suffix
        var sanitized = assemblyName
            .Replace(".", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);

        return sanitized;
    }

    /// <summary>
    /// Determines the smart method suffix based on assembly suffix uniqueness.
    /// If the assembly suffix (last segment) is unique among all assemblies, use just the suffix.
    /// Otherwise, use the full sanitized assembly name to avoid conflicts.
    /// </summary>
    private static string GetSmartMethodSuffix(
        string currentAssemblyName,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies)
    {
        // Build list of all assemblies in this context
        var allAssemblies = new List<string> { currentAssemblyName };
        allAssemblies.AddRange(referencedAssemblies.Select(r => r.AssemblyName));

        return GetSmartMethodSuffixFromContext(currentAssemblyName, allAssemblies);
    }

    /// <summary>
    /// Calculates smart method suffix for a given assembly name within a context of all assemblies.
    /// </summary>
    private static string GetSmartMethodSuffixFromContext(
        string assemblyName,
        List<string> allAssembliesInContext)
    {
        // Get the suffix (last segment after final dot) of the target assembly
        var parts = assemblyName.Split('.');
        var suffix = parts[parts.Length - 1];

        // Check how many assemblies in the context have this same suffix
        int count = 0;

        foreach (var asmName in allAssembliesInContext)
        {
            var asmParts = asmName.Split('.');
            var asmSuffix = asmParts[asmParts.Length - 1];

            if (asmSuffix.Equals(suffix, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        if (count == 1)
        {
            // Only one assembly in context has this suffix, use just the suffix
            return SanitizeAssemblyName(suffix);
        }

        // Multiple assemblies have this suffix, use full sanitized name to avoid conflicts
        return SanitizeAssemblyName(assemblyName);
    }

    private static string GenerateExtensionMethod(
        List<ServiceRegistrationInfo> services,
        string assemblyName,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies)
    {
        var sb = new StringBuilder();
        var smartSuffix = GetSmartMethodSuffix(assemblyName, referencedAssemblies);
        var methodName = $"AddDependencyRegistrationsFrom{smartSuffix}";
        var assemblyPrefix = GetAssemblyPrefix(assemblyName);

        // Check if any services have conditions
        var hasConditionalServices = services.Any(s => !string.IsNullOrEmpty(s.Condition));

        sb.AppendLineLf("// <auto-generated />");
        sb.AppendLineLf("#nullable enable");
        sb.AppendLineLf();
        sb.AppendLineLf("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLineLf("using Microsoft.Extensions.DependencyInjection.Extensions;");

        // Add configuration namespace if conditional services exist
        if (hasConditionalServices)
        {
            sb.AppendLineLf("using Microsoft.Extensions.Configuration;");
        }

        sb.AppendLineLf();
        sb.AppendLineLf("namespace Atc.DependencyInjection;");
        sb.AppendLineLf();
        sb.AppendLineLf("/// <summary>");
        sb.AppendLineLf("/// Extension methods for registering services decorated with [Registration] attribute.");
        sb.AppendLineLf("/// </summary>");
        sb.AppendLineLf("public static class ServiceCollectionExtensions");
        sb.AppendLineLf("{");

        // Generate runtime filtering helper method
        GenerateRuntimeFilteringHelper(sb);

        // Overload 1: Default (existing behavior, no transitive calls)
        GenerateDefaultOverload(sb, methodName, assemblyName, services, hasConditionalServices);

        // Always generate all overloads for consistency (even if no referenced assemblies)
        // Overload 2: Auto-detect all referenced assemblies
        GenerateAutoDetectOverload(sb, methodName, assemblyName, services, referencedAssemblies, hasConditionalServices);

        // Overload 3: Specific assembly by name
        GenerateSpecificAssemblyOverload(sb, methodName, assemblyName, services, referencedAssemblies, assemblyPrefix, hasConditionalServices);

        // Overload 4: Multiple assemblies by name
        GenerateMultipleAssembliesOverload(sb, methodName, assemblyName, services, referencedAssemblies, assemblyPrefix, hasConditionalServices);

        sb.AppendLineLf("}");

        return sb.ToString();
    }

    private static void GenerateRuntimeFilteringHelper(StringBuilder sb)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf("    /// Determines if a service type should be excluded from registration based on runtime filters.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    private static bool ShouldExcludeService(");
        sb.AppendLineLf("        global::System.Type serviceType,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedNamespaces,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedPatterns,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<global::System.Type>? excludedTypes)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        // Check if explicitly excluded by type");
        sb.AppendLineLf("        if (excludedTypes != null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            foreach (var excludedType in excludedTypes)");
        sb.AppendLineLf("            {");
        sb.AppendLineLf("                if (serviceType == excludedType || serviceType.IsAssignableFrom(excludedType))");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    return true;");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("            }");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Check namespace exclusion");
        sb.AppendLineLf("        if (excludedNamespaces != null && serviceType.Namespace != null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            foreach (var excludedNs in excludedNamespaces)");
        sb.AppendLineLf("            {");
        sb.AppendLineLf("                // Exact match or sub-namespace match");
        sb.AppendLineLf("                if (serviceType.Namespace.Equals(excludedNs, global::System.StringComparison.Ordinal) ||");
        sb.AppendLineLf("                    serviceType.Namespace.StartsWith($\"{excludedNs}.\", global::System.StringComparison.Ordinal))");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    return true;");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("            }");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Check pattern exclusion (wildcard matching)");
        sb.AppendLineLf("        if (excludedPatterns != null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            var typeName = serviceType.Name;");
        sb.AppendLineLf("            var fullTypeName = serviceType.FullName ?? serviceType.Name;");
        sb.AppendLineLf();
        sb.AppendLineLf("            foreach (var pattern in excludedPatterns)");
        sb.AppendLineLf("            {");
        sb.AppendLineLf("                if (MatchesPattern(typeName, pattern) || MatchesPattern(fullTypeName, pattern))");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    return true;");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("            }");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        return false;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf("    /// Matches a string against a wildcard pattern.");
        sb.AppendLineLf("    /// Supports * (any characters) and ? (single character).");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    private static bool MatchesPattern(");
        sb.AppendLineLf("        string value,");
        sb.AppendLineLf("        string pattern)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        // Convert wildcard pattern to regex");
        sb.AppendLineLf("        var escapedPattern = global::System.Text.RegularExpressions.Regex.Escape(pattern);");
        sb.AppendLineLf("        var replacedStars = escapedPattern.Replace(\"\\\\*\", \".*\");");
        sb.AppendLineLf("        var replacedQuestions = replacedStars.Replace(\"\\\\?\", \".\");");
        sb.AppendLineLf("        var regexPattern = $\"^{replacedQuestions}$\";");
        sb.AppendLineLf();
        sb.AppendLineLf("        return global::System.Text.RegularExpressions.Regex.IsMatch(");
        sb.AppendLineLf("            value,");
        sb.AppendLineLf("            regexPattern,");
        sb.AppendLineLf("            global::System.Text.RegularExpressions.RegexOptions.IgnoreCase,");
        sb.AppendLineLf("            global::System.TimeSpan.FromSeconds(1));");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf("    /// Decorates a registered service with a decorator implementation.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    private static IServiceCollection Decorate<TService>(");
        sb.AppendLineLf("        this IServiceCollection services,");
        sb.AppendLineLf("        global::System.Func<global::System.IServiceProvider, TService, TService> decorator)");
        sb.AppendLineLf("        where TService : class");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        // Find existing service descriptor");
        sb.AppendLineLf("        var descriptor = services.LastOrDefault(d => d.ServiceType == typeof(TService));");
        sb.AppendLineLf("        if (descriptor == null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            throw new global::System.InvalidOperationException(");
        sb.AppendLineLf("                $\"No service of type {typeof(TService).Name} is registered. Decorators must be registered after the base service.\");");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Remove existing descriptor");
        sb.AppendLineLf("        services.Remove(descriptor);");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Create new descriptor that wraps the original");
        sb.AppendLineLf("        var lifetime = descriptor.Lifetime;");
        sb.AppendLineLf("        services.Add(new ServiceDescriptor(");
        sb.AppendLineLf("            typeof(TService),");
        sb.AppendLineLf("            provider =>");
        sb.AppendLineLf("            {");
        sb.AppendLineLf("                // Resolve the inner service");
        sb.AppendLineLf("                TService inner;");
        sb.AppendLineLf("                if (descriptor.ImplementationInstance != null)");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    inner = (TService)descriptor.ImplementationInstance;");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("                else if (descriptor.ImplementationFactory != null)");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    inner = (TService)descriptor.ImplementationFactory(provider);");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("                else if (descriptor.ImplementationType != null)");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    inner = (TService)ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("                else");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    throw new global::System.InvalidOperationException(\"Invalid service descriptor\");");
        sb.AppendLineLf("                }");
        sb.AppendLineLf();
        sb.AppendLineLf("                // Apply decorator");
        sb.AppendLineLf("                return decorator(provider, inner);");
        sb.AppendLineLf("            },");
        sb.AppendLineLf("            lifetime));");
        sb.AppendLineLf();
        sb.AppendLineLf("        return services;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf("    /// Decorates a registered open generic service with a decorator implementation.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    private static IServiceCollection Decorate(");
        sb.AppendLineLf("        this IServiceCollection services,");
        sb.AppendLineLf("        global::System.Type serviceType,");
        sb.AppendLineLf("        global::System.Func<global::System.IServiceProvider, object, object> decorator)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        // Find existing service descriptor");
        sb.AppendLineLf("        var descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);");
        sb.AppendLineLf("        if (descriptor == null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            throw new global::System.InvalidOperationException(");
        sb.AppendLineLf("                $\"No service of type {serviceType.Name} is registered. Decorators must be registered after the base service.\");");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Remove existing descriptor");
        sb.AppendLineLf("        services.Remove(descriptor);");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Create new descriptor that wraps the original");
        sb.AppendLineLf("        var lifetime = descriptor.Lifetime;");
        sb.AppendLineLf("        services.Add(new ServiceDescriptor(");
        sb.AppendLineLf("            serviceType,");
        sb.AppendLineLf("            provider =>");
        sb.AppendLineLf("            {");
        sb.AppendLineLf("                // Resolve the inner service");
        sb.AppendLineLf("                object inner;");
        sb.AppendLineLf("                if (descriptor.ImplementationInstance != null)");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    inner = descriptor.ImplementationInstance;");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("                else if (descriptor.ImplementationFactory != null)");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    inner = descriptor.ImplementationFactory(provider);");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("                else if (descriptor.ImplementationType != null)");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    inner = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);");
        sb.AppendLineLf("                }");
        sb.AppendLineLf("                else");
        sb.AppendLineLf("                {");
        sb.AppendLineLf("                    throw new global::System.InvalidOperationException(\"Invalid service descriptor\");");
        sb.AppendLineLf("                }");
        sb.AppendLineLf();
        sb.AppendLineLf("                // Apply decorator");
        sb.AppendLineLf("                return decorator(provider, inner);");
        sb.AppendLineLf("            },");
        sb.AppendLineLf("            lifetime));");
        sb.AppendLineLf();
        sb.AppendLineLf("        return services;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateDefaultOverload(
        StringBuilder sb,
        string methodName,
        string assemblyName,
        List<ServiceRegistrationInfo> services,
        bool hasConditionalServices)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("    /// <param name=\"configuration\">The configuration used for conditional service registration.</param>");
        }

        sb.AppendLineLf("    /// <param name=\"excludedNamespaces\">Optional. Namespaces to exclude from registration.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedPatterns\">Optional. Wildcard patterns (* and ?) to exclude types by name.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedTypes\">Optional. Specific types to exclude from registration.</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(");
        sb.AppendLineLf("        this IServiceCollection services,");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("        IConfiguration configuration,");
        }

        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedNamespaces = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedPatterns = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<global::System.Type>? excludedTypes = null)");
        sb.AppendLineLf("    {");

        GenerateServiceRegistrationCalls(sb, services, includeRuntimeFiltering: true);

        sb.AppendLineLf();
        sb.AppendLineLf("        return services;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateAutoDetectOverload(
        StringBuilder sb,
        string methodName,
        string assemblyName,
        List<ServiceRegistrationInfo> services,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies,
        bool hasConditionalServices)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute,");
        sb.AppendLineLf("    /// optionally including services from referenced assemblies.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("    /// <param name=\"configuration\">The configuration used for conditional service registration.</param>");
        }

        sb.AppendLineLf("    /// <param name=\"includeReferencedAssemblies\">If true, also registers services from all referenced assemblies with [Registration] attributes.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedNamespaces\">Optional. Namespaces to exclude from registration.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedPatterns\">Optional. Wildcard patterns (* and ?) to exclude types by name.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedTypes\">Optional. Specific types to exclude from registration.</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(");
        sb.AppendLineLf("        this IServiceCollection services,");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("        IConfiguration configuration,");
        }

        sb.AppendLineLf("        bool includeReferencedAssemblies,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedNamespaces = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedPatterns = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<global::System.Type>? excludedTypes = null)");
        sb.AppendLineLf("    {");

        // Build context for smart suffix calculation
        var allAssemblies = new List<string> { assemblyName };
        allAssemblies.AddRange(referencedAssemblies.Select(r => r.AssemblyName));

        // Only generate the if block if there are referenced assemblies to call
        if (referencedAssemblies.Length > 0)
        {
            sb.AppendLineLf("        if (includeReferencedAssemblies)");
            sb.AppendLineLf("        {");

            // Generate calls to all referenced assemblies (recursive)
            // Note: We don't pass configuration to referenced assemblies as we don't know if they need it
            // Each assembly manages its own conditional services and should be called directly with configuration if needed
            foreach (var refAssembly in referencedAssemblies)
            {
                var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
                var refMethodName = $"AddDependencyRegistrationsFrom{refSmartSuffix}";
                sb.AppendLineLf($"            services.{refMethodName}(includeReferencedAssemblies: true, excludedNamespaces: excludedNamespaces, excludedPatterns: excludedPatterns, excludedTypes: excludedTypes);");
            }

            sb.AppendLineLf("        }");
            sb.AppendLineLf();
        }

        GenerateServiceRegistrationCalls(sb, services, includeRuntimeFiltering: true);

        sb.AppendLineLf();
        sb.AppendLineLf("        return services;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateSpecificAssemblyOverload(
        StringBuilder sb,
        string methodName,
        string assemblyName,
        List<ServiceRegistrationInfo> services,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies,
        string assemblyPrefix,
        bool hasConditionalServices)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute,");
        sb.AppendLineLf("    /// optionally including a specific referenced assembly.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("    /// <param name=\"configuration\">The configuration used for conditional service registration.</param>");
        }

        sb.AppendLineLf("    /// <param name=\"referencedAssemblyName\">The name of the referenced assembly to include (full name or short name).</param>");
        sb.AppendLineLf("    /// <param name=\"excludedNamespaces\">Optional. Namespaces to exclude from registration.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedPatterns\">Optional. Wildcard patterns (* and ?) to exclude types by name.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedTypes\">Optional. Specific types to exclude from registration.</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(");
        sb.AppendLineLf("        this IServiceCollection services,");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("        IConfiguration configuration,");
        }

        sb.AppendLineLf("        string referencedAssemblyName,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedNamespaces = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedPatterns = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<global::System.Type>? excludedTypes = null)");
        sb.AppendLineLf("    {");

        // Build context for smart suffix calculation
        var allAssemblies = new List<string> { assemblyName };
        allAssemblies.AddRange(referencedAssemblies.Select(r => r.AssemblyName));

        // Generate if-blocks for each referenced assembly (with prefix filtering)
        var filteredAssemblies = referencedAssemblies
            .Where(a => a.AssemblyName.StartsWith(assemblyPrefix, StringComparison.Ordinal))
            .ToList();

        foreach (var refAssembly in filteredAssemblies)
        {
            var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
            var refMethodName = $"AddDependencyRegistrationsFrom{refSmartSuffix}";
            sb.AppendLineLf($"        if (string.Equals(referencedAssemblyName, \"{refAssembly.AssemblyName}\", global::System.StringComparison.OrdinalIgnoreCase) ||");
            sb.AppendLineLf($"            string.Equals(referencedAssemblyName, \"{refAssembly.ShortName}\", global::System.StringComparison.OrdinalIgnoreCase))");
            sb.AppendLineLf("        {");

            sb.AppendLineLf($"            services.{refMethodName}(referencedAssemblyName: referencedAssemblyName, excludedNamespaces: excludedNamespaces, excludedPatterns: excludedPatterns, excludedTypes: excludedTypes);");

            sb.AppendLineLf("        }");
            sb.AppendLineLf();
        }

        GenerateServiceRegistrationCalls(sb, services, includeRuntimeFiltering: true);

        sb.AppendLineLf();
        sb.AppendLineLf("        return services;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateMultipleAssembliesOverload(
        StringBuilder sb,
        string methodName,
        string assemblyName,
        List<ServiceRegistrationInfo> services,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies,
        string assemblyPrefix,
        bool hasConditionalServices)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute,");
        sb.AppendLineLf("    /// optionally including specific referenced assemblies.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("    /// <param name=\"configuration\">The configuration used for conditional service registration.</param>");
        }

        sb.AppendLineLf("    /// <param name=\"excludedNamespaces\">Optional. Namespaces to exclude from registration.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedPatterns\">Optional. Wildcard patterns (* and ?) to exclude types by name.</param>");
        sb.AppendLineLf("    /// <param name=\"excludedTypes\">Optional. Specific types to exclude from registration.</param>");
        sb.AppendLineLf("    /// <param name=\"referencedAssemblyNames\">The names of the referenced assemblies to include (full names or short names).</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(");
        sb.AppendLineLf("        this IServiceCollection services,");

        if (hasConditionalServices)
        {
            sb.AppendLineLf("        IConfiguration configuration,");
        }

        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedNamespaces = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<string>? excludedPatterns = null,");
        sb.AppendLineLf("        global::System.Collections.Generic.IEnumerable<global::System.Type>? excludedTypes = null,");
        sb.AppendLineLf("        params string[] referencedAssemblyNames)");
        sb.AppendLineLf("    {");

        // Build context for smart suffix calculation
        var allAssemblies = new List<string> { assemblyName };
        allAssemblies.AddRange(referencedAssemblies.Select(r => r.AssemblyName));

        // Generate if-blocks for each referenced assembly (with prefix filtering)
        var filteredAssemblies = referencedAssemblies
            .Where(a => a.AssemblyName.StartsWith(assemblyPrefix, StringComparison.Ordinal))
            .ToList();

        // Only generate the foreach block if there are filtered assemblies to process
        if (filteredAssemblies.Count > 0)
        {
            sb.AppendLineLf("        foreach (var name in referencedAssemblyNames)");
            sb.AppendLineLf("        {");

            for (var i = 0; i < filteredAssemblies.Count; i++)
            {
                var refAssembly = filteredAssemblies[i];
                var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
                var refMethodName = $"AddDependencyRegistrationsFrom{refSmartSuffix}";
                var ifKeyword = i == 0 ? "if" : "else if";

                sb.AppendLineLf($"            {ifKeyword} (string.Equals(name, \"{refAssembly.AssemblyName}\", global::System.StringComparison.OrdinalIgnoreCase) ||");
                sb.AppendLineLf($"                string.Equals(name, \"{refAssembly.ShortName}\", global::System.StringComparison.OrdinalIgnoreCase))");
                sb.AppendLineLf("            {");

                sb.AppendLineLf($"                services.{refMethodName}(excludedNamespaces: excludedNamespaces, excludedPatterns: excludedPatterns, excludedTypes: excludedTypes, referencedAssemblyNames: referencedAssemblyNames);");

                sb.AppendLineLf("            }");
            }

            sb.AppendLineLf("        }");
            sb.AppendLineLf();
        }

        GenerateServiceRegistrationCalls(sb, services, includeRuntimeFiltering: true);

        sb.AppendLineLf();
        sb.AppendLineLf("        return services;");
        sb.AppendLineLf("    }");
    }

    private static void GenerateServiceRegistrationCalls(
        StringBuilder sb,
        List<ServiceRegistrationInfo> services,
        bool includeRuntimeFiltering = false)
    {
        // Separate decorators from base services
        var nonDecoratorServices = services.Where(s => !s.Decorator);
        var baseServices = nonDecoratorServices.ToList();
        var decoratorServices = services.Where(s => s.Decorator);
        var decorators = decoratorServices.ToList();

        // Determine base indentation level based on runtime filtering
        var baseIndent = includeRuntimeFiltering ? "            " : "        ";

        // Register base services first
        foreach (var service in baseServices)
        {
            var isGeneric = service.ClassSymbol.IsGenericType;
            var implementationType = service.ClassSymbol.ToDisplayString();
            var hasKey = service.Key is not null;
            var keyString = FormatKeyValue(service.Key);

            var hasFactory = !string.IsNullOrEmpty(service.FactoryMethodName);
            var hasInstance = !string.IsNullOrEmpty(service.InstanceMemberName);

            // Generate runtime filtering check if enabled
            if (includeRuntimeFiltering)
            {
                var typeForExclusion = isGeneric
                    ? $"typeof({GetOpenGenericTypeName(service.ClassSymbol)})"
                    : $"typeof({implementationType})";

                sb.AppendLineLf();
                sb.AppendLineLf($"        // Check runtime exclusions for {service.ClassSymbol.Name}");
                sb.AppendLineLf($"        if (!ShouldExcludeService({typeForExclusion}, excludedNamespaces, excludedPatterns, excludedTypes))");
                sb.AppendLineLf("        {");
            }

            // Generate conditional registration check if needed
            var hasCondition = !string.IsNullOrEmpty(service.Condition);
            if (hasCondition)
            {
                var condition = service.Condition!;
                var isNegated = condition.StartsWith("!", StringComparison.Ordinal);
                var configKey = isNegated ? condition.Substring(1) : condition;
                var conditionCheck = isNegated
                    ? $"!configuration.GetValue<bool>(\"{configKey}\")"
                    : $"configuration.GetValue<bool>(\"{configKey}\")";

                sb.AppendLineLf();
                sb.AppendLineLf($"{baseIndent}// Conditional registration for {service.ClassSymbol.Name}");
                sb.AppendLineLf($"{baseIndent}if ({conditionCheck})");
                sb.AppendLineLf($"{baseIndent}{{");
            }

            // Determine indentation for registration calls (may be nested in conditional)
            var registrationIndent = hasCondition ? baseIndent + "    " : baseIndent;

            // Hosted services use AddHostedService instead of regular lifetime methods
            if (service.IsHostedService)
            {
                if (isGeneric)
                {
                    var openGenericImplementationType = GetOpenGenericTypeName(service.ClassSymbol);
                    sb.AppendLineLf($"{registrationIndent}services.AddHostedService(typeof({openGenericImplementationType}));");
                }
                else
                {
                    sb.AppendLineLf($"{registrationIndent}services.AddHostedService<{implementationType}>();");
                }
            }
            else if (hasInstance)
            {
                // Instance registration - pre-created singleton instances
                // Instance registration always uses AddSingleton (validated earlier)
                var lifetimeMethod = service.TryAdd ? "TryAddSingleton" : "AddSingleton";

                // Determine how to access the instance (field, property, or method call)
                var instanceAccess = service.InstanceMemberName!;

                // Check if it's a method (simple heuristic - if we stored more info we could be certain)
                // For now, we'll check if the member is a method when generating
                var members = service.ClassSymbol.GetMembers(service.InstanceMemberName!);
                var methodSymbols = members.OfType<IMethodSymbol>();
                var isMethod = methodSymbols.Any(m => m.Parameters.Length == 0);

                var instanceExpression = isMethod
                    ? $"{implementationType}.{instanceAccess}()"
                    : $"{implementationType}.{instanceAccess}";

                // Register against each interface using the instance
                if (service.AsTypes.Length > 0)
                {
                    foreach (var asType in service.AsTypes)
                    {
                        var serviceType = asType.ToDisplayString();
                        sb.AppendLineLf($"{registrationIndent}services.{lifetimeMethod}<{serviceType}>({instanceExpression});");
                    }

                    // Also register as self if requested
                    if (service.AsSelf)
                    {
                        sb.AppendLineLf($"{registrationIndent}services.{lifetimeMethod}<{implementationType}>({instanceExpression});");
                    }
                }
                else
                {
                    // No interfaces - register as concrete type with instance
                    sb.AppendLineLf($"{registrationIndent}services.{lifetimeMethod}<{implementationType}>({instanceExpression});");
                }
            }
            else if (hasFactory)
            {
                // Factory method registration
                var lifetimeMethod = service.TryAdd
                    ? service.Lifetime switch
                    {
                        ServiceLifetime.Singleton => "TryAddSingleton",
                        ServiceLifetime.Scoped => "TryAddScoped",
                        ServiceLifetime.Transient => "TryAddTransient",
                        _ => "TryAddSingleton",
                    }
                    : service.Lifetime switch
                    {
                        ServiceLifetime.Singleton => "AddSingleton",
                        ServiceLifetime.Scoped => "AddScoped",
                        ServiceLifetime.Transient => "AddTransient",
                        _ => "AddSingleton",
                    };

                // Register against each interface using factory
                if (service.AsTypes.Length > 0)
                {
                    foreach (var asType in service.AsTypes)
                    {
                        var serviceType = asType.ToDisplayString();
                        sb.AppendLineLf($"{registrationIndent}services.{lifetimeMethod}<{serviceType}>(sp => {implementationType}.{service.FactoryMethodName}(sp));");
                    }

                    // Also register as self if requested
                    if (service.AsSelf)
                    {
                        sb.AppendLineLf($"{registrationIndent}services.{lifetimeMethod}<{implementationType}>(sp => {implementationType}.{service.FactoryMethodName}(sp));");
                    }
                }
                else
                {
                    // No interfaces - register as concrete type with factory
                    sb.AppendLineLf($"{registrationIndent}services.{lifetimeMethod}<{implementationType}>(sp => {implementationType}.{service.FactoryMethodName}(sp));");
                }
            }
            else
            {
                var lifetimeMethod = hasKey
                    ? service.Lifetime switch
                    {
                        ServiceLifetime.Singleton => "AddKeyedSingleton",
                        ServiceLifetime.Scoped => "AddKeyedScoped",
                        ServiceLifetime.Transient => "AddKeyedTransient",
                        _ => "AddKeyedSingleton",
                    }
                    : service.TryAdd
                        ? service.Lifetime switch
                        {
                            ServiceLifetime.Singleton => "TryAddSingleton",
                            ServiceLifetime.Scoped => "TryAddScoped",
                            ServiceLifetime.Transient => "TryAddTransient",
                            _ => "TryAddSingleton",
                        }
                        : service.Lifetime switch
                        {
                            ServiceLifetime.Singleton => "AddSingleton",
                            ServiceLifetime.Scoped => "AddScoped",
                            ServiceLifetime.Transient => "AddTransient",
                            _ => "AddSingleton",
                        };

                // Register against each interface
                if (service.AsTypes.Length > 0)
                {
                    foreach (var asType in service.AsTypes)
                    {
                        var serviceType = asType.ToDisplayString();

                        // Check if the interface is generic
                        var isInterfaceGeneric = asType is INamedTypeSymbol namedType && namedType.IsGenericType;

                        if (isGeneric && isInterfaceGeneric)
                        {
                            // Both service and interface are generic - use typeof() syntax
                            var openGenericServiceType = GetOpenGenericTypeName(asType);
                            var openGenericImplementationType = GetOpenGenericTypeName(service.ClassSymbol);

                            sb.AppendLineLf(hasKey
                                ? $"{registrationIndent}services.{lifetimeMethod}(typeof({openGenericServiceType}), {keyString}, typeof({openGenericImplementationType}));"
                                : $"{registrationIndent}services.{lifetimeMethod}(typeof({openGenericServiceType}), typeof({openGenericImplementationType}));");
                        }
                        else
                        {
                            // Regular non-generic registration
                            sb.AppendLineLf(hasKey
                                ? $"{registrationIndent}services.{lifetimeMethod}<{serviceType}, {implementationType}>({keyString});"
                                : $"{registrationIndent}services.{lifetimeMethod}<{serviceType}, {implementationType}>();");
                        }
                    }

                    // Also register as self if requested
                    if (service.AsSelf)
                    {
                        if (isGeneric)
                        {
                            var openGenericImplementationType = GetOpenGenericTypeName(service.ClassSymbol);

                            sb.AppendLineLf(hasKey
                                ? $"{registrationIndent}services.{lifetimeMethod}(typeof({openGenericImplementationType}), {keyString});"
                                : $"{registrationIndent}services.{lifetimeMethod}(typeof({openGenericImplementationType}));");
                        }
                        else
                        {
                            sb.AppendLineLf(hasKey
                                ? $"{registrationIndent}services.{lifetimeMethod}<{implementationType}>({keyString});"
                                : $"{registrationIndent}services.{lifetimeMethod}<{implementationType}>();");
                        }
                    }
                }
                else
                {
                    // No interfaces - register as concrete type
                    if (isGeneric)
                    {
                        var openGenericImplementationType = GetOpenGenericTypeName(service.ClassSymbol);

                        sb.AppendLineLf(hasKey
                            ? $"{registrationIndent}services.{lifetimeMethod}(typeof({openGenericImplementationType}), {keyString});"
                            : $"{registrationIndent}services.{lifetimeMethod}(typeof({openGenericImplementationType}));");
                    }
                    else
                    {
                        sb.AppendLineLf(hasKey
                            ? $"{registrationIndent}services.{lifetimeMethod}<{implementationType}>({keyString});"
                            : $"{registrationIndent}services.{lifetimeMethod}<{implementationType}>();");
                    }
                }
            }

            // Close conditional registration check if needed
            if (hasCondition)
            {
                sb.AppendLineLf($"{baseIndent}}}");
            }

            // Close runtime filtering check if enabled
            if (includeRuntimeFiltering)
            {
                sb.AppendLineLf("        }");
            }
        }

        // Register decorators after base services
        foreach (var decorator in decorators)
        {
            var isGeneric = decorator.ClassSymbol.IsGenericType;
            var decoratorType = decorator.ClassSymbol.ToDisplayString();

            // Decorators require an explicit As type
            if (decorator.AsTypes.Length == 0)
            {
                continue; // Skip decorators without explicit interface
            }

            // Generate runtime filtering check if enabled
            if (includeRuntimeFiltering)
            {
                var typeForExclusion = isGeneric
                    ? $"typeof({GetOpenGenericTypeName(decorator.ClassSymbol)})"
                    : $"typeof({decoratorType})";

                sb.AppendLineLf();
                sb.AppendLineLf($"        // Check runtime exclusions for decorator {decorator.ClassSymbol.Name}");
                sb.AppendLineLf($"        if (!ShouldExcludeService({typeForExclusion}, excludedNamespaces, excludedPatterns, excludedTypes))");
                sb.AppendLineLf("        {");
            }

            // Generate conditional registration check if needed
            var hasDecoratorCondition = !string.IsNullOrEmpty(decorator.Condition);
            if (hasDecoratorCondition)
            {
                var condition = decorator.Condition!;
                var isNegated = condition.StartsWith("!", StringComparison.Ordinal);
                var configKey = isNegated ? condition.Substring(1) : condition;
                var conditionCheck = isNegated
                    ? $"!configuration.GetValue<bool>(\"{configKey}\")"
                    : $"configuration.GetValue<bool>(\"{configKey}\")";

                sb.AppendLineLf();
                sb.AppendLineLf($"{baseIndent}// Conditional registration for decorator {decorator.ClassSymbol.Name}");
                sb.AppendLineLf($"{baseIndent}if ({conditionCheck})");
                sb.AppendLineLf($"{baseIndent}{{");
            }

            // Determine indentation for decorator registration calls (may be nested in conditional)
            var decoratorRegistrationIndent = hasDecoratorCondition ? baseIndent + "    " : baseIndent;

            // Generate decorator registration for each interface
            foreach (var asType in decorator.AsTypes)
            {
                var serviceType = asType.ToDisplayString();
                var isInterfaceGeneric = asType is INamedTypeSymbol namedType && namedType.IsGenericType;

#pragma warning disable S3923 // All conditionals intentionally return same value - reserved for future lifetime-specific behavior
                var lifetimeMethod = decorator.Lifetime switch
                {
                    ServiceLifetime.Singleton => "Decorate",
                    ServiceLifetime.Scoped => "Decorate",
                    ServiceLifetime.Transient => "Decorate",
                    _ => "Decorate",
                };
#pragma warning restore S3923

                sb.AppendLineLf();
                sb.AppendLineLf($"{decoratorRegistrationIndent}// Decorator: {decorator.ClassSymbol.Name}");

                if (isGeneric && isInterfaceGeneric)
                {
                    // Open generic decorator
                    var openGenericServiceType = GetOpenGenericTypeName(asType);
                    var openGenericDecoratorType = GetOpenGenericTypeName(decorator.ClassSymbol);

                    sb.AppendLineLf($"{decoratorRegistrationIndent}services.{lifetimeMethod}(typeof({openGenericServiceType}), (provider, inner) =>");
                    sb.AppendLineLf($"{decoratorRegistrationIndent}{{");
                    sb.AppendLineLf($"{decoratorRegistrationIndent}    var decoratorInstance = ActivatorUtilities.CreateInstance(provider, typeof({openGenericDecoratorType}), inner);");
                    sb.AppendLineLf($"{decoratorRegistrationIndent}    return decoratorInstance;");
                    sb.AppendLineLf($"{decoratorRegistrationIndent}}});");
                }
                else
                {
                    // Regular decorator
                    sb.AppendLineLf($"{decoratorRegistrationIndent}services.{lifetimeMethod}<{serviceType}>((provider, inner) =>");
                    sb.AppendLineLf($"{decoratorRegistrationIndent}{{");
                    sb.AppendLineLf($"{decoratorRegistrationIndent}    return ActivatorUtilities.CreateInstance<{decoratorType}>(provider, inner);");
                    sb.AppendLineLf($"{decoratorRegistrationIndent}}});");
                }
            }

            // Close conditional registration check if needed
            if (hasDecoratorCondition)
            {
                sb.AppendLineLf($"{baseIndent}}}");
            }

            // Close runtime filtering check if enabled
            if (includeRuntimeFiltering)
            {
                sb.AppendLineLf("        }");
            }
        }
    }

    /// <summary>
    /// Validates if a factory method's return type is compatible with the expected type.
    /// Supports exact match, interface implementation, and abstract class inheritance.
    /// </summary>
    private static bool IsReturnTypeValid(
        ITypeSymbol returnType,
        ITypeSymbol expectedType)
    {
        // Exact match
        if (SymbolEqualityComparer.Default.Equals(returnType, expectedType))
        {
            return true;
        }

        // Also check by display string as a fallback for test harness compatibility
        if (returnType.ToDisplayString() == expectedType.ToDisplayString())
        {
            return true;
        }

        // Check if expectedType is an interface and returnType implements it
        if (expectedType.TypeKind == TypeKind.Interface)
        {
            if (expectedType is INamedTypeSymbol { IsGenericType: true } expectedNamedType)
            {
                var expectedTypeOriginal = expectedNamedType.OriginalDefinition;
                return returnType is INamedTypeSymbol returnNamedType &&
                       returnNamedType.AllInterfaces.Any(i =>
                       {
                           if (i is INamedTypeSymbol { IsGenericType: true } iNamedType)
                           {
                               return SymbolEqualityComparer.Default.Equals(iNamedType.OriginalDefinition, expectedTypeOriginal);
                           }

                           return SymbolEqualityComparer.Default.Equals(i, expectedType);
                       });
            }

            return returnType is INamedTypeSymbol returnTypeNamed &&
                   returnTypeNamed.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, expectedType));
        }

        // Check if expectedType is an abstract class and returnType inherits from it (or is the same abstract class)
        if (expectedType.TypeKind == TypeKind.Class && expectedType.IsAbstract)
        {
            // returnType is also the same abstract class (covered by exact match above, but doublecheck with string comparison)
            if (returnType.TypeKind == TypeKind.Class && returnType.IsAbstract &&
                returnType.ToDisplayString() == expectedType.ToDisplayString())
            {
                return true;
            }

            // Check if returnType inherits from the abstract class
            var baseType = returnType.BaseType;
            while (baseType is not null)
            {
                if (expectedType is INamedTypeSymbol { IsGenericType: true } expectedNamedType)
                {
                    var expectedTypeOriginal = expectedNamedType.OriginalDefinition;
                    if (baseType is INamedTypeSymbol { IsGenericType: true } baseNamedType &&
                        SymbolEqualityComparer.Default.Equals(baseNamedType.OriginalDefinition, expectedTypeOriginal))
                    {
                        return true;
                    }
                }
                else if (SymbolEqualityComparer.Default.Equals(baseType, expectedType))
                {
                    return true;
                }
                else if (baseType.ToDisplayString() == expectedType.ToDisplayString())
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }
        }

        return false;
    }

    /// <summary>
    /// Formats a key value for code generation.
    /// String keys are wrapped in quotes, type keys use typeof() syntax.
    /// </summary>
    private static string FormatKeyValue(object? key)
        => key switch
        {
            null => "null",
            string stringKey => $"\"{stringKey}\"",
            ITypeSymbol typeKey => $"typeof({typeKey.ToDisplayString()})",
            _ => key.ToString() ?? "null",
        };

    /// <summary>
    /// Gets the open generic type name for a generic type symbol.
    /// Examples: "IRepository&lt;&gt;" for one parameter, "IHandler&lt;,&gt;" for two parameters.
    /// </summary>
    private static string GetOpenGenericTypeName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return typeSymbol.ToDisplayString();
        }

        // Get the full namespace and type name
        var namespaceName = namedTypeSymbol.ContainingNamespace?.ToDisplayString();
        var typeName = namedTypeSymbol.Name;

        // Build the open generic type name (e.g., "IRepository<>" or "IHandler<,>")
        var typeParameterCount = namedTypeSymbol.TypeParameters.Length;
        var openGenericMarkers = typeParameterCount switch
        {
            0 => string.Empty,
            1 => "<>",
            2 => "<,>",
            3 => "<,,>",
            _ => "<" + new string(',', typeParameterCount - 1) + ">",
        };

        if (string.IsNullOrEmpty(namespaceName))
        {
            return $"{typeName}{openGenericMarkers}";
        }

        return $"{namespaceName}.{typeName}{openGenericMarkers}";
    }

    private static string GenerateAttributeCode()
        => $$"""
             // <auto-generated />
             #nullable enable
             #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

             namespace {{AttributeNamespace}};

             /// <summary>
             /// Service lifetime enum matching Microsoft.Extensions.DependencyInjection.ServiceLifetime.
             /// </summary>
             [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.DependencyRegistration", "1.0.0")]
             [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
             [global::System.Runtime.CompilerServices.CompilerGenerated]
             public enum Lifetime
             {
                 /// <summary>
                 /// Specifies that a single instance of the service will be created.
                 /// </summary>
                 Singleton = 0,

                 /// <summary>
                 /// Specifies that a new instance of the service will be created for each scope.
                 /// </summary>
                 Scoped = 1,

                 /// <summary>
                 /// Specifies that a new instance of the service will be created every time it is requested.
                 /// </summary>
                 Transient = 2
             }

             /// <summary>
             /// Marks a class for automatic registration in the dependency injection container.
             /// </summary>
             /// <remarks>
             /// Use this attribute to automatically register services without manually adding them
             /// to the service collection. The generator will create an extension method
             /// that registers all decorated services.
             /// </remarks>
             /// <example>
             /// <code>
             /// // Register as concrete type with scoped lifetime (default)
             /// [Registration]
             /// public class MyService { }
             ///
             /// // Register as interface with singleton lifetime
             /// [Registration(Lifetime.Singleton, As = typeof(IMyService))]
             /// public class MyService : IMyService { }
             ///
             /// // Register as both interface and concrete type
             /// [Registration(As = typeof(IMyService), AsSelf = true)]
             /// public class MyService : IMyService { }
             /// </code>
             /// </example>
             [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.DependencyRegistration", "1.0.0")]
             [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
             [global::System.Runtime.CompilerServices.CompilerGenerated]
             [global::System.Diagnostics.DebuggerNonUserCode]
             [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
             [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
             public sealed class RegistrationAttribute : global::System.Attribute
             {
                 /// <summary>
                 /// Initializes a new instance of the <see cref="RegistrationAttribute"/> class.
                 /// </summary>
                 /// <param name="lifetime">The service lifetime. Default is <see cref="Lifetime.Singleton"/>.</param>
                 public RegistrationAttribute(Lifetime lifetime = Lifetime.Singleton)
                 {
                     Lifetime = lifetime;
                 }

                 /// <summary>
                 /// Gets the service lifetime.
                 /// </summary>
                 public Lifetime Lifetime { get; }

                 /// <summary>
                 /// Gets or sets the service type to register against (typically an interface or abstract class).
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
                 /// Gets or sets the service key for keyed service registration.
                 /// Enables multiple implementations of the same interface to be registered and resolved by key.
                 /// </summary>
                 /// <remarks>
                 /// When specified, uses AddKeyed{Lifetime}() methods for registration (.NET 8+).
                 /// Can be a string or type used to distinguish between multiple implementations.
                 /// </remarks>
                 /// <example>
                 /// <code>
                 /// [Registration(As = typeof(IPaymentProcessor), Key = "Stripe")]
                 /// public class StripePaymentProcessor : IPaymentProcessor { }
                 ///
                 /// [Registration(As = typeof(IPaymentProcessor), Key = "PayPal")]
                 /// public class PayPalPaymentProcessor : IPaymentProcessor { }
                 /// </code>
                 /// </example>
                 public object? Key { get; set; }

                 /// <summary>
                 /// Gets or sets the factory method name for custom service instantiation.
                 /// The factory method must be static and accept IServiceProvider as a parameter.
                 /// </summary>
                 /// <remarks>
                 /// When specified, the service will be registered using a factory delegate instead of direct instantiation.
                 /// Useful for services requiring complex initialization logic or configuration-based setup.
                 /// </remarks>
                 /// <example>
                 /// <code>
                 /// [Registration(As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
                 /// public class EmailSender : IEmailSender
                 /// {
                 ///     private static EmailSender CreateEmailSender(IServiceProvider provider)
                 ///     {
                 ///         var config = provider.GetRequiredService&lt;IConfiguration&gt;();
                 ///         return new EmailSender(config["ApiKey"]);
                 ///     }
                 /// }
                 /// </code>
                 /// </example>
                 public string? Factory { get; set; }

                 /// <summary>
                 /// Gets or sets a value indicating whether to use TryAdd registration.
                 /// When true, the service is only registered if not already registered.
                 /// </summary>
                 /// <remarks>
                 /// TryAdd registration is useful for library authors who want to provide default implementations
                 /// that can be overridden by application code. If a service is already registered, TryAdd will
                 /// skip the registration, allowing user code to take precedence.
                 /// </remarks>
                 /// <example>
                 /// <code>
                 /// // Library code - provides default implementation
                 /// [Registration(As = typeof(ILogger), TryAdd = true)]
                 /// public class DefaultLogger : ILogger { }
                 ///
                 /// // User code can override by registering before library
                 /// services.AddScoped&lt;ILogger, CustomLogger&gt;();  // This takes precedence
                 /// services.AddDependencyRegistrationsFromLibrary();    // TryAdd skips DefaultLogger
                 /// </code>
                 /// </example>
                 public bool TryAdd { get; set; }

                 /// <summary>
                 /// Gets or sets a value indicating whether this service is a decorator.
                 /// When true, this service wraps the previous registration of the same interface.
                 /// </summary>
                 /// <remarks>
                 /// Decorators are useful for implementing cross-cutting concerns like logging, caching,
                 /// validation, or retry logic without modifying the original service implementation.
                 /// The decorator's constructor must accept the interface it decorates as the first parameter.
                 /// </remarks>
                 /// <example>
                 /// <code>
                 /// // Base service
                 /// [Registration(As = typeof(IOrderService))]
                 /// public class OrderService : IOrderService { }
                 ///
                 /// // Decorator that adds logging
                 /// [Registration(As = typeof(IOrderService), Decorator = true)]
                 /// public class LoggingOrderServiceDecorator : IOrderService
                 /// {
                 ///     private readonly IOrderService _inner;
                 ///     private readonly ILogger _logger;
                 ///
                 ///     public LoggingOrderServiceDecorator(IOrderService inner, ILogger logger)
                 ///     {
                 ///         _inner = inner;
                 ///         _logger = logger;
                 ///     }
                 /// }
                 /// </code>
                 /// </example>
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

             /// <summary>
             /// Filters types from automatic registration during transitive assembly scanning.
             /// Apply at assembly level to exclude specific namespaces, naming patterns, or interface implementations.
             /// </summary>
             /// <remarks>
             /// This attribute is used to exclude certain types from automatic registration when using
             /// includeReferencedAssemblies option. Multiple filters can be specified by using the attribute
             /// multiple times or by passing arrays to the properties.
             /// </remarks>
             /// <example>
             /// <code>
             /// // Exclude specific namespace
             /// [assembly: RegistrationFilter(ExcludeNamespaces = new[] { "MyApp.Internal" })]
             ///
             /// // Exclude by naming pattern
             /// [assembly: RegistrationFilter(ExcludePatterns = new[] { "*Test*", "*Mock*" })]
             ///
             /// // Exclude types implementing specific interface
             /// [assembly: RegistrationFilter(ExcludeImplementing = new[] { typeof(ITestUtility) })]
             ///
             /// // Multiple filters in one attribute
             /// [assembly: RegistrationFilter(
             ///     ExcludeNamespaces = new[] { "MyApp.Internal", "MyApp.Tests" },
             ///     ExcludePatterns = new[] { "*Test*" })]
             /// </code>
             /// </example>
             [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.DependencyRegistration", "1.0.0")]
             [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
             [global::System.Runtime.CompilerServices.CompilerGenerated]
             [global::System.Diagnostics.DebuggerNonUserCode]
             [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
             [global::System.AttributeUsage(global::System.AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
             public sealed class RegistrationFilterAttribute : global::System.Attribute
             {
                 /// <summary>
                 /// Gets or sets the namespaces to exclude from registration.
                 /// Types in these namespaces (or sub-namespaces) will not be registered.
                 /// </summary>
                 /// <example>
                 /// <code>
                 /// [assembly: RegistrationFilter(ExcludeNamespaces = new[] { "MyApp.Internal", "MyApp.Tests" })]
                 /// </code>
                 /// </example>
                 public string[]? ExcludeNamespaces { get; set; }

                 /// <summary>
                 /// Gets or sets the naming patterns to exclude from registration.
                 /// Supports wildcards: * matches any characters, ? matches single character.
                 /// </summary>
                 /// <example>
                 /// <code>
                 /// [assembly: RegistrationFilter(ExcludePatterns = new[] { "*Test*", "*Mock*", "Temp*" })]
                 /// </code>
                 /// </example>
                 public string[]? ExcludePatterns { get; set; }

                 /// <summary>
                 /// Gets or sets the interface types to exclude from registration.
                 /// Types implementing any of these interfaces will not be registered.
                 /// </summary>
                 /// <example>
                 /// <code>
                 /// [assembly: RegistrationFilter(ExcludeImplementing = new[] { typeof(ITestUtility), typeof(IInternal) })]
                 /// </code>
                 /// </example>
                 public global::System.Type[]? ExcludeImplementing { get; set; }
             }
             """;
}