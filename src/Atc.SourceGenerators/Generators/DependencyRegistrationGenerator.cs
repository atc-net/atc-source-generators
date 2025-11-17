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

            // Constructor argument (lifetime)
            if (attributeData.ConstructorArguments.Length > 0)
            {
                var lifetimeValue = attributeData.ConstructorArguments[0].Value;
                if (lifetimeValue is int intValue)
                {
                    lifetime = (ServiceLifetime)intValue;
                }
            }

            // Named arguments (As, AsSelf)
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

            // Check if this assembly contains any types with [Registration] attribute
            // Traverse all types in the assembly's global namespace
            if (!HasRegistrationAttributeInNamespace(assemblySymbol.GlobalNamespace))
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

    private static bool HasRegistrationAttributeInNamespace(
        INamespaceSymbol namespaceSymbol)
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
                        if (attribute.AttributeClass?.ToDisplayString() == AttributeFullName)
                        {
                            return true;
                        }
                    }

                    break;
                }

                case INamespaceSymbol childNamespace when
                    HasRegistrationAttributeInNamespace(childNamespace):
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

        var validServices = new List<ServiceRegistrationInfo>();

        // Validate and emit diagnostics
        foreach (var service in services)
        {
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
            var implementsInterface = service.ClassSymbol.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i, asType));

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
            var implementationType = service.ClassSymbol.ToDisplayString();

            // Hosted services use AddHostedService instead of regular lifetime methods
            if (service.IsHostedService)
            {
                sb.AppendLineLf($"        services.AddHostedService<{implementationType}>();");
            }
            else
            {
                var lifetimeMethod = service.Lifetime switch
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
                        sb.AppendLineLf($"        services.{lifetimeMethod}<{serviceType}, {implementationType}>();");
                    }

                    // Also register as self if requested
                    if (service.AsSelf)
                    {
                        sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>();");
                    }
                }
                else
                {
                    // No interfaces - register as concrete type
                    sb.AppendLineLf($"        services.{lifetimeMethod}<{implementationType}>();");
                }
            }
        }
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
             }
             """;
}