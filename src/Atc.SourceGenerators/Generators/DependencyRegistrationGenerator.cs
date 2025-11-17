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
        title: "Service 'As' type must be an interface",
        messageFormat: "The type '{0}' specified in As parameter must be an interface, but is a {1}",
        category: RuleCategoryConstants.DependencyInjection,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ClassDoesNotImplementInterfaceDescriptor = new(
        id: RuleIdentifierConstants.DependencyInjection.ClassDoesNotImplementInterface,
        title: "Class does not implement specified interface",
        messageFormat: "Class '{0}' does not implement interface '{1}' specified in As parameter",
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

            // Constructor argument (lifetime)
            if (attributeData.ConstructorArguments.Length > 0)
            {
                var lifetimeValue = attributeData.ConstructorArguments[0].Value;
                if (lifetimeValue is int intValue)
                {
                    lifetime = (ServiceLifetime)intValue;
                }
            }

            // Named arguments (As, AsSelf, Key, Factory, TryAdd)
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
                }
            }

            // Determine which types to register against
            ImmutableArray<ITypeSymbol> asTypes;
            if (explicitAsType is not null)
            {
                // Explicit As parameter takes precedence
                asTypes = ImmutableArray.Create(explicitAsType);
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

        return result.ToImmutableArray();
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
            excludedNamespaces.ToImmutableArray(),
            excludedPatterns.ToImmutableArray(),
            excludedInterfaces.ToImmutableArray());
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

        // Validate each interface type
        foreach (var asType in service.AsTypes)
        {
            // Check if As is an interface
            if (asType.TypeKind != TypeKind.Interface)
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

            // Check if the class implements the interface
            var implementsInterface = false;

            // For generic types, we need to compare the original definitions
            if (asType is INamedTypeSymbol asNamedType && asNamedType.IsGenericType)
            {
                var asTypeOriginal = asNamedType.OriginalDefinition;
                implementsInterface = service.ClassSymbol.AllInterfaces.Any(i =>
                {
                    if (i is INamedTypeSymbol iNamedType && iNamedType.IsGenericType)
                    {
                        return SymbolEqualityComparer.Default.Equals(iNamedType.OriginalDefinition, asTypeOriginal);
                    }

                    return SymbolEqualityComparer.Default.Equals(i, asType);
                });
            }
            else
            {
                implementsInterface = service.ClassSymbol.AllInterfaces.Any(i =>
                    SymbolEqualityComparer.Default.Equals(i, asType));
            }

            if (!implementsInterface)
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
                : (ITypeSymbol)service.ClassSymbol;

            // Validate factory method signature
            var hasValidSignature =
                factoryMethod.IsStatic &&
                factoryMethod.Parameters.Length == 1 &&
                factoryMethod.Parameters[0].Type.ToDisplayString() == "System.IServiceProvider" &&
                SymbolEqualityComparer.Default.Equals(factoryMethod.ReturnType, expectedReturnType);

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

        sb.AppendLineLf("// <auto-generated />");
        sb.AppendLineLf("#nullable enable");
        sb.AppendLineLf();
        sb.AppendLineLf("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLineLf("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLineLf();
        sb.AppendLineLf("namespace Atc.DependencyInjection;");
        sb.AppendLineLf();
        sb.AppendLineLf("/// <summary>");
        sb.AppendLineLf("/// Extension methods for registering services decorated with [Registration] attribute.");
        sb.AppendLineLf("/// </summary>");
        sb.AppendLineLf("public static class ServiceCollectionExtensions");
        sb.AppendLineLf("{");

        // Overload 1: Default (existing behavior, no transitive calls)
        GenerateDefaultOverload(sb, methodName, assemblyName, services);

        // Always generate all overloads for consistency (even if no referenced assemblies)
        // Overload 2: Auto-detect all referenced assemblies
        GenerateAutoDetectOverload(sb, methodName, assemblyName, services, referencedAssemblies);

        // Overload 3: Specific assembly by name
        GenerateSpecificAssemblyOverload(sb, methodName, assemblyName, services, referencedAssemblies, assemblyPrefix);

        // Overload 4: Multiple assemblies by name
        GenerateMultipleAssembliesOverload(sb, methodName, assemblyName, services, referencedAssemblies, assemblyPrefix);

        sb.AppendLineLf("}");

        return sb.ToString();
    }

    private static void GenerateDefaultOverload(
        StringBuilder sb,
        string methodName,
        string assemblyName,
        List<ServiceRegistrationInfo> services)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(this IServiceCollection services)");
        sb.AppendLineLf("    {");

        GenerateServiceRegistrationCalls(sb, services);

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
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute,");
        sb.AppendLineLf("    /// optionally including services from referenced assemblies.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLineLf("    /// <param name=\"includeReferencedAssemblies\">If true, also registers services from all referenced assemblies with [Registration] attributes.</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(");
        sb.AppendLineLf("        this IServiceCollection services,");
        sb.AppendLineLf("        bool includeReferencedAssemblies)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        if (includeReferencedAssemblies)");
        sb.AppendLineLf("        {");

        // Build context for smart suffix calculation
        var allAssemblies = new List<string> { assemblyName };
        allAssemblies.AddRange(referencedAssemblies.Select(r => r.AssemblyName));

        // Generate calls to all referenced assemblies (recursive)
        foreach (var refAssembly in referencedAssemblies)
        {
            var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
            var refMethodName = $"AddDependencyRegistrationsFrom{refSmartSuffix}";
            sb.AppendLineLf($"            services.{refMethodName}(includeReferencedAssemblies: true);");
        }

        sb.AppendLineLf("        }");
        sb.AppendLineLf();

        GenerateServiceRegistrationCalls(sb, services);

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
        string assemblyPrefix)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute,");
        sb.AppendLineLf("    /// optionally including a specific referenced assembly.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLineLf("    /// <param name=\"referencedAssemblyName\">The name of the referenced assembly to include (full name or short name).</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(");
        sb.AppendLineLf("        this IServiceCollection services,");
        sb.AppendLineLf("        string referencedAssemblyName)");
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
            sb.AppendLineLf($"            services.{refMethodName}(referencedAssemblyName);");
            sb.AppendLineLf("        }");
            sb.AppendLineLf();
        }

        GenerateServiceRegistrationCalls(sb, services);

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
        string assemblyPrefix)
    {
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Registers all services from {assemblyName} that are decorated with [Registration] attribute,");
        sb.AppendLineLf("    /// optionally including specific referenced assemblies.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLineLf("    /// <param name=\"referencedAssemblyNames\">The names of the referenced assemblies to include (full names or short names).</param>");
        sb.AppendLineLf("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLineLf($"    public static IServiceCollection {methodName}(");
        sb.AppendLineLf("        this IServiceCollection services,");
        sb.AppendLineLf("        params string[] referencedAssemblyNames)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        foreach (var name in referencedAssemblyNames)");
        sb.AppendLineLf("        {");

        // Build context for smart suffix calculation
        var allAssemblies = new List<string> { assemblyName };
        allAssemblies.AddRange(referencedAssemblies.Select(r => r.AssemblyName));

        // Generate if-blocks for each referenced assembly (with prefix filtering)
        var filteredAssemblies = referencedAssemblies
            .Where(a => a.AssemblyName.StartsWith(assemblyPrefix, StringComparison.Ordinal))
            .ToList();

        for (var i = 0; i < filteredAssemblies.Count; i++)
        {
            var refAssembly = filteredAssemblies[i];
            var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
            var refMethodName = $"AddDependencyRegistrationsFrom{refSmartSuffix}";
            var ifKeyword = i == 0 ? "if" : "else if";

            sb.AppendLineLf($"            {ifKeyword} (string.Equals(name, \"{refAssembly.AssemblyName}\", global::System.StringComparison.OrdinalIgnoreCase) ||");
            sb.AppendLineLf($"                string.Equals(name, \"{refAssembly.ShortName}\", global::System.StringComparison.OrdinalIgnoreCase))");
            sb.AppendLineLf("            {");
            sb.AppendLineLf($"                services.{refMethodName}(referencedAssemblyNames);");
            sb.AppendLineLf("            }");
        }

        sb.AppendLineLf("        }");
        sb.AppendLineLf();

        GenerateServiceRegistrationCalls(sb, services);

        sb.AppendLineLf();
        sb.AppendLineLf("        return services;");
        sb.AppendLineLf("    }");
    }

    private static void GenerateServiceRegistrationCalls(
        StringBuilder sb,
        List<ServiceRegistrationInfo> services)
    {
        foreach (var service in services)
        {
            var isGeneric = service.ClassSymbol.IsGenericType;
            var implementationType = service.ClassSymbol.ToDisplayString();
            var hasKey = service.Key is not null;
            var keyString = FormatKeyValue(service.Key);

            var hasFactory = !string.IsNullOrEmpty(service.FactoryMethodName);

            // Hosted services use AddHostedService instead of regular lifetime methods
            if (service.IsHostedService)
            {
                if (isGeneric)
                {
                    var openGenericImplementationType = GetOpenGenericTypeName(service.ClassSymbol);
                    sb.AppendLineLf($"        services.AddHostedService(typeof({openGenericImplementationType}));");
                }
                else
                {
                    sb.AppendLineLf($"        services.AddHostedService<{implementationType}>();");
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
                        sb.AppendLineLf($"        services.{lifetimeMethod}<{serviceType}>(sp => {implementationType}.{service.FactoryMethodName}(sp));");
                    }

                    // Also register as self if requested
                    if (service.AsSelf)
                    {
                        sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>(sp => {implementationType}.{service.FactoryMethodName}(sp));");
                    }
                }
                else
                {
                    // No interfaces - register as concrete type with factory
                    sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>(sp => {implementationType}.{service.FactoryMethodName}(sp));");
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

                            if (hasKey)
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}(typeof({openGenericServiceType}), {keyString}, typeof({openGenericImplementationType}));");
                            }
                            else
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}(typeof({openGenericServiceType}), typeof({openGenericImplementationType}));");
                            }
                        }
                        else
                        {
                            // Regular non-generic registration
                            if (hasKey)
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}<{serviceType}, {implementationType}>({keyString});");
                            }
                            else
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}<{serviceType}, {implementationType}>();");
                            }
                        }
                    }

                    // Also register as self if requested
                    if (service.AsSelf)
                    {
                        if (isGeneric)
                        {
                            var openGenericImplementationType = GetOpenGenericTypeName(service.ClassSymbol);

                            if (hasKey)
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}(typeof({openGenericImplementationType}), {keyString});");
                            }
                            else
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}(typeof({openGenericImplementationType}));");
                            }
                        }
                        else
                        {
                            if (hasKey)
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>({keyString});");
                            }
                            else
                            {
                                sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>();");
                            }
                        }
                    }
                }
                else
                {
                    // No interfaces - register as concrete type
                    if (isGeneric)
                    {
                        var openGenericImplementationType = GetOpenGenericTypeName(service.ClassSymbol);

                        if (hasKey)
                        {
                            sb.AppendLineLf($"        services.{lifetimeMethod}(typeof({openGenericImplementationType}), {keyString});");
                        }
                        else
                        {
                            sb.AppendLineLf($"        services.{lifetimeMethod}(typeof({openGenericImplementationType}));");
                        }
                    }
                    else
                    {
                        if (hasKey)
                        {
                            sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>({keyString});");
                        }
                        else
                        {
                            sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>();");
                        }
                    }
                }
            }
        }
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