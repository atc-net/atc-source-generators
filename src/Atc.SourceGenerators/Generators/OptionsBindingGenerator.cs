// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.SourceGenerators.Generators;

/// <summary>
/// Source generator for automatic options binding from configuration.
/// </summary>
[SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK")]
[Generator]
public class OptionsBindingGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Atc.SourceGenerators.Annotations";
    private const string AttributeName = "OptionsBindingAttribute";
    private const string FullAttributeName = $"{AttributeNamespace}.{AttributeName}";

    // Diagnostic descriptors
    private static readonly DiagnosticDescriptor OptionsClassMustBePartialDescriptor = new(
        id: RuleIdentifierConstants.OptionsBinding.OptionsClassMustBePartial,
        title: "Options class must be partial",
        messageFormat: "Options class '{0}' must be declared as partial to enable source generation",
        category: RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SectionNameCannotBeEmptyDescriptor = new(
        id: RuleIdentifierConstants.OptionsBinding.SectionNameCannotBeEmpty,
        title: "Section name cannot be null or empty",
        messageFormat: "Section name for options class '{0}' cannot be null or empty",
        category: RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConstSectionNameCannotBeEmptyDescriptor = new(
        id: RuleIdentifierConstants.OptionsBinding.ConstSectionNameCannotBeEmpty,
        title: "Const section name cannot be null or empty",
        messageFormat: "Options class '{0}' has const field '{1}' but its value is null or empty",
        category: RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OnChangeRequiresMonitorLifetimeDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.OnChangeRequiresMonitorLifetime,
        "OnChange callback requires Monitor lifetime",
        "OnChange callback '{0}' can only be used when Lifetime = OptionsLifetime.Monitor",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OnChangeNotSupportedWithNamedOptionsDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.OnChangeNotSupportedWithNamedOptions,
        "OnChange callback not supported with named options",
        "OnChange callback '{0}' cannot be used with named options (Name = '{1}')",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OnChangeCallbackNotFoundDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.OnChangeCallbackNotFound,
        "OnChange callback method not found",
        "OnChange callback method '{0}' not found in class '{1}'",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OnChangeCallbackInvalidSignatureDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.OnChangeCallbackInvalidSignature,
        "OnChange callback method has invalid signature",
        "OnChange callback method '{0}' must have signature: static void {0}({1} options, string? name)",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PostConfigureNotSupportedWithNamedOptionsDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.PostConfigureNotSupportedWithNamedOptions,
        "PostConfigure callback not supported with named options",
        "PostConfigure callback '{0}' cannot be used with named options (Name = '{1}')",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PostConfigureCallbackNotFoundDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.PostConfigureCallbackNotFound,
        "PostConfigure callback method not found",
        "PostConfigure callback method '{0}' not found in class '{1}'",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PostConfigureCallbackInvalidSignatureDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.PostConfigureCallbackInvalidSignature,
        "PostConfigure callback method has invalid signature",
        "PostConfigure callback method '{0}' must have signature: static void {0}({1} options)",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate the attribute definition as fallback
        // If Atc.SourceGenerators.Annotations are referenced, CS0436 warning will be suppressed via project settings
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("OptionsBindingAttribute.g.cs", SourceText.From(GenerateAttributeSource(), Encoding.UTF8));
        });

        // Find classes with OptionsBinding attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)
            .Collect();

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations);

        // Generate source
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in classDeclaration.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == FullAttributeName)
                {
                    return classDeclaration;
                }
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var optionsToGenerate = new List<OptionsInfo>();

        foreach (var classDeclaration in classes.Distinct())
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not { } classSymbol)
            {
                continue;
            }

            var optionsInfoList = ExtractAllOptionsInfo(classSymbol, context);
            optionsToGenerate.AddRange(optionsInfoList);
        }

        if (optionsToGenerate.Count == 0)
        {
            return;
        }

        // Detect referenced assemblies with [OptionsBinding] attributes
        var referencedAssemblies = GetReferencedAssembliesWithOptionsBinding(compilation);

        // Group by assembly
        var groupedByAssembly = optionsToGenerate
            .GroupBy(x => x.AssemblyName, StringComparer.Ordinal)
            .ToList();

        foreach (var assemblyGroup in groupedByAssembly)
        {
            var source = GenerateExtensionMethod(assemblyGroup.Key, assemblyGroup.ToList(), referencedAssemblies);
            context.AddSource($"OptionsBindingExtensions.{SanitizeForMethodName(assemblyGroup.Key)}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static List<OptionsInfo> ExtractAllOptionsInfo(
        INamedTypeSymbol classSymbol,
        SourceProductionContext context)
    {
        var result = new List<OptionsInfo>();

        // Check if class is partial
        if (!classSymbol.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is ClassDeclarationSyntax c &&
                                                            c.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    OptionsClassMustBePartialDescriptor,
                    classSymbol.Locations.First(),
                    classSymbol.Name));
            return result;
        }

        // Get ALL attributes (support for AllowMultiple = true)
        var attributes = classSymbol
            .GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == FullAttributeName)
            .ToList();

        if (attributes.Count == 0)
        {
            return result;
        }

        // Process each attribute separately
        foreach (var attribute in attributes)
        {
            var optionsInfo = ExtractOptionsInfoFromAttribute(classSymbol, attribute, context);
            if (optionsInfo is not null)
            {
                result.Add(optionsInfo);
            }
        }

        return result;
    }

    private static OptionsInfo? ExtractOptionsInfoFromAttribute(
        INamedTypeSymbol classSymbol,
        AttributeData attribute,
        SourceProductionContext context)
    {
        // Extract section name with priority:
        // 1. Explicit constructor argument
        // 2. const string NameTitle or Name
        // 3. Auto-inferred from class name
        string? sectionName = null;
        if (attribute.ConstructorArguments.Length > 0)
        {
            sectionName = attribute.ConstructorArguments[0].Value as string;
        }

        // If section name is null, check for const Name or NameTitle
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            var (constName, constFieldName) = FindConstSectionName(classSymbol);
            if (constFieldName is not null)
            {
                if (string.IsNullOrWhiteSpace(constName))
                {
                    // Const field exists but has empty value - report error
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            ConstSectionNameCannotBeEmptyDescriptor,
                            classSymbol.Locations.First(),
                            classSymbol.Name,
                            constFieldName));
                    return null;
                }

                sectionName = constName;
            }
        }

        // If still null, infer from class name
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = InferSectionNameFromClassName(classSymbol.Name);
        }

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SectionNameCannotBeEmptyDescriptor,
                    classSymbol.Locations.First(),
                    classSymbol.Name));
            return null;
        }

        // Extract named properties
        var validateOnStart = false;
        var validateDataAnnotations = false;
        var lifetime = 0; // Singleton
        INamedTypeSymbol? validatorType = null;
        string? name = null;
        var errorOnMissingKeys = false;
        string? onChange = null;
        string? postConfigure = null;

        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "ValidateOnStart":
                    validateOnStart = namedArg.Value.Value as bool? ?? false;
                    break;
                case "ValidateDataAnnotations":
                    validateDataAnnotations = namedArg.Value.Value as bool? ?? false;
                    break;
                case "Lifetime":
                    lifetime = namedArg.Value.Value as int? ?? 0;
                    break;
                case "Validator":
                    validatorType = namedArg.Value.Value as INamedTypeSymbol;
                    break;
                case "Name":
                    name = namedArg.Value.Value as string;
                    break;
                case "ErrorOnMissingKeys":
                    errorOnMissingKeys = namedArg.Value.Value as bool? ?? false;
                    break;
                case "OnChange":
                    onChange = namedArg.Value.Value as string;
                    break;
                case "PostConfigure":
                    postConfigure = namedArg.Value.Value as string;
                    break;
            }
        }

        // Validate OnChange callback requirements
        if (!string.IsNullOrWhiteSpace(onChange))
        {
            // OnChange only allowed with Monitor lifetime (2 = Monitor)
            if (lifetime != 2)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        OnChangeRequiresMonitorLifetimeDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        onChange));

                return null;
            }

            // OnChange not allowed with named options
            if (!string.IsNullOrWhiteSpace(name))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        OnChangeNotSupportedWithNamedOptionsDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        onChange,
                        name));

                return null;
            }

            // Validate callback method exists and has correct signature
            var callbackMethod = classSymbol
                .GetMembers(onChange!)
                .OfType<IMethodSymbol>()
                .FirstOrDefault();

            if (callbackMethod is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        OnChangeCallbackNotFoundDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        onChange,
                        classSymbol.Name));

                return null;
            }

            // Validate method signature: static void MethodName(TOptions options, string? name)
            if (!callbackMethod.IsStatic ||
                !callbackMethod.ReturnsVoid ||
                callbackMethod.Parameters.Length != 2)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        OnChangeCallbackInvalidSignatureDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        onChange,
                        classSymbol.Name));

                return null;
            }

            // Validate parameter types
            var firstParam = callbackMethod.Parameters[0];
            var secondParam = callbackMethod.Parameters[1];

            if (!SymbolEqualityComparer.Default.Equals(firstParam.Type, classSymbol) ||
                secondParam.Type.ToDisplayString() != "string?")
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        OnChangeCallbackInvalidSignatureDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        onChange,
                        classSymbol.Name));

                return null;
            }
        }

        // Validate PostConfigure callback requirements
        if (!string.IsNullOrWhiteSpace(postConfigure))
        {
            // PostConfigure not allowed with named options
            if (!string.IsNullOrWhiteSpace(name))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        PostConfigureNotSupportedWithNamedOptionsDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        postConfigure,
                        name));

                return null;
            }

            // Validate callback method exists and has correct signature
            var callbackMethod = classSymbol
                .GetMembers(postConfigure!)
                .OfType<IMethodSymbol>()
                .FirstOrDefault();

            if (callbackMethod is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        PostConfigureCallbackNotFoundDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        postConfigure,
                        classSymbol.Name));

                return null;
            }

            // Validate method signature: static void MethodName(TOptions options)
            if (!callbackMethod.IsStatic ||
                !callbackMethod.ReturnsVoid ||
                callbackMethod.Parameters.Length != 1)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        PostConfigureCallbackInvalidSignatureDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        postConfigure,
                        classSymbol.Name));

                return null;
            }

            // Validate parameter type
            var firstParam = callbackMethod.Parameters[0];

            if (!SymbolEqualityComparer.Default.Equals(firstParam.Type, classSymbol))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        PostConfigureCallbackInvalidSignatureDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        postConfigure,
                        classSymbol.Name));

                return null;
            }
        }

        // Convert validator type to full name if present
        var validatorTypeName = validatorType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new OptionsInfo(
            classSymbol.Name,
            classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.ContainingAssembly.Name,
            sectionName!, // Guaranteed non-null after validation above
            validateOnStart,
            validateDataAnnotations,
            lifetime,
            validatorTypeName,
            name,
            errorOnMissingKeys,
            onChange,
            postConfigure);
    }

    private static string InferSectionNameFromClassName(string className)
        => className;

    private static (string? SectionName, string? ConstFieldName) FindConstSectionName(
        INamedTypeSymbol classSymbol)
    {
        // Look for public const string SectionName, NameTitle, or Name
        // Priority: SectionName first, then NameTitle, then Name
        var members = classSymbol.GetMembers();

        var field = FindPublicConstStringField(members, "SectionName");
        if (field is not null)
        {
            return (field.ConstantValue as string, "SectionName");
        }

        field = FindPublicConstStringField(members, "NameTitle");
        if (field is not null)
        {
            return (field.ConstantValue as string, "NameTitle");
        }

        field = FindPublicConstStringField(members, "Name");
        if (field is not null)
        {
            return (field.ConstantValue as string, "Name");
        }

        return (null, null);
    }

    private static IFieldSymbol? FindPublicConstStringField(
        ImmutableArray<ISymbol> members,
        string fieldName)
    {
        foreach (var member in members)
        {
            if (member is IFieldSymbol
                {
                    IsConst: true,
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public,
                    Type.SpecialType: SpecialType.System_String,
                }

                field && field.Name == fieldName)
            {
                return field;
            }
        }

        return null;
    }

    private static string GetAssemblyPrefix(string assemblyName)
    {
        var dotIndex = assemblyName.IndexOf('.');
        return dotIndex > 0 ? assemblyName.Substring(0, dotIndex) : assemblyName;
    }

    private static ImmutableArray<ReferencedAssemblyInfo> GetReferencedAssembliesWithOptionsBinding(
        Compilation compilation)
    {
        var result = new List<ReferencedAssemblyInfo>();
        var prefix = GetAssemblyPrefix(compilation.AssemblyName!);
        var visited = new HashSet<string>(StringComparer.Ordinal) { compilation.AssemblyName! };
        var queue = new Queue<IAssemblySymbol>();

        // Start with direct references
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
            {
                queue.Enqueue(assemblySymbol);
            }
        }

        // Process queue recursively
        while (queue.Count > 0)
        {
            var assemblySymbol = queue.Dequeue();
            var assemblyName = assemblySymbol.Name;

            if (!visited.Add(assemblyName))
            {
                continue;
            }

            // Check if assembly has OptionsBinding attributes and matches prefix
            if (assemblyName.StartsWith(prefix, StringComparison.Ordinal) &&
                HasOptionsBindingAttributeInNamespace(assemblySymbol))
            {
                result.Add(new ReferencedAssemblyInfo(
                    assemblyName,
                    SanitizeForMethodName(assemblyName),
                    assemblyName.Substring(assemblyName.LastIndexOf('.') + 1)));
            }

            // Enqueue referenced assemblies for recursive traversal
            foreach (var referencedAssembly in assemblySymbol.Modules.SelectMany(m => m.ReferencedAssemblies))
            {
                var matchingReference = compilation.References.FirstOrDefault(r =>
                    compilation.GetAssemblyOrModuleSymbol(r) is IAssemblySymbol asm &&
                    asm.Name == referencedAssembly.Name);

                if (matchingReference is not null &&
                    compilation.GetAssemblyOrModuleSymbol(matchingReference) is IAssemblySymbol referencedSymbol)
                {
                    queue.Enqueue(referencedSymbol);
                }
            }
        }

        return [..result];
    }

    private static bool HasOptionsBindingAttributeInNamespace(
        IAssemblySymbol assemblySymbol)
        => assemblySymbol
            .GlobalNamespace
            .GetNamespaceMembers()
            .Any(ns => ns.ToDisplayString() == AttributeNamespace);

    private static string GenerateExtensionMethod(
        string assemblyName,
        List<OptionsInfo> options,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies)
    {
        var sb = new StringBuilder();
        var methodSuffix = GetSmartMethodSuffix(assemblyName, referencedAssemblies);

        sb.AppendLine($$"""
// <auto-generated/>
#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.DependencyInjection;

/// <summary>
/// Extension methods for configuring options from the {{assemblyName}} assembly.
/// </summary>
public static class OptionsBindingExtensions
{
    /// <summary>
    /// Adds and configures options from the {{assemblyName}} assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptionsFrom{{methodSuffix}}(
        this IServiceCollection services,
        IConfiguration configuration)
    {
""");

        foreach (var option in options)
        {
            GenerateOptionsRegistration(sb, option);
        }

        sb.AppendLine("""
        return services;
    }
""");

        // Build context for smart suffix calculation
        var allAssemblies = new List<string> { assemblyName };
        allAssemblies.AddRange(referencedAssemblies.Select(r => r.AssemblyName));

        // Overload 2: Auto-detect all referenced assemblies recursively
        sb.AppendLine();
        sb.AppendLine($$"""
    /// <summary>
    /// Adds and configures options from the {{assemblyName}} assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="includeReferencedAssemblies">If true, automatically registers options from all referenced assemblies with [OptionsBinding] attributes.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptionsFrom{{methodSuffix}}(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeReferencedAssemblies)
    {
        services.AddOptionsFrom{{methodSuffix}}(configuration);
""");

        // Only generate the if-statement if there are referenced assemblies
        if (referencedAssemblies.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("        if (includeReferencedAssemblies)");
            sb.AppendLine("        {");

            foreach (var refAssembly in referencedAssemblies)
            {
                var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
                sb.AppendLine($"            AddOptionsFrom{refSmartSuffix}(services, configuration, includeReferencedAssemblies: true);");
            }

            sb.AppendLine("        }");
        }

        sb.AppendLine("""

        return services;
    }
""");

        // Overload 3: Register specific referenced assembly by name
        sb.AppendLine();
        sb.AppendLine($$"""
    /// <summary>
    /// Adds and configures options from the {{assemblyName}} assembly and a specific referenced assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="referencedAssemblyName">The name of the referenced assembly to include (short name or full name).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptionsFrom{{methodSuffix}}(
        this IServiceCollection services,
        IConfiguration configuration,
        string referencedAssemblyName)
    {
        services.AddOptionsFrom{{methodSuffix}}(configuration);

""");

        if (referencedAssemblies.Length > 0)
        {
            sb.AppendLine("        switch (referencedAssemblyName)");
            sb.AppendLine("        {");

            foreach (var refAssembly in referencedAssemblies)
            {
                var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
                sb.AppendLine($"            case \"{refAssembly.AssemblyName}\":");
                sb.AppendLine($"            case \"{refAssembly.ShortName}\":");
                sb.AppendLine($"                AddOptionsFrom{refSmartSuffix}(services, configuration, includeReferencedAssemblies: true);");
                sb.AppendLine("                break;");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("""
        return services;
    }
""");

        // Overload 4: Register multiple referenced assemblies
        sb.AppendLine();
        sb.AppendLine($$"""
    /// <summary>
    /// Adds and configures options from the {{assemblyName}} assembly and specific referenced assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="referencedAssemblyNames">The names of referenced assemblies to include (short names or full names).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptionsFrom{{methodSuffix}}(
        this IServiceCollection services,
        IConfiguration configuration,
        params string[] referencedAssemblyNames)
    {
        services.AddOptionsFrom{{methodSuffix}}(configuration);

        foreach (var assemblyName in referencedAssemblyNames)
        {
            services.AddOptionsFrom{{methodSuffix}}(configuration, assemblyName);
        }

        return services;
    }
""");

        // Generate hosted service classes for OnChange callbacks
        foreach (var option in options.Where(o => !string.IsNullOrWhiteSpace(o.OnChange)))
        {
            GenerateOnChangeHostedService(sb, option);
        }

        sb.AppendLine("}");
        sb.AppendLine();

        return sb.ToString();
    }

    private static void GenerateOptionsRegistration(
        StringBuilder sb,
        OptionsInfo option)
    {
        var optionsType = $"global::{option.Namespace}.{option.ClassName}";
        var sectionName = option.SectionName;
        var isNamed = !string.IsNullOrWhiteSpace(option.Name);

        // Add comment indicating which interface to inject based on lifetime
        var lifetimeComment = option.Lifetime switch
        {
            0 => "IOptions<T>",         // Singleton
            1 => "IOptionsSnapshot<T>", // Scoped
            2 => "IOptionsMonitor<T>",  // Monitor
            _ => "IOptions<T>",         // Default
        };

        if (isNamed)
        {
            // Named options - use Configure<T>(name, ...) pattern
            sb.AppendLineLf($"        // Configure {option.ClassName} (Named: \"{option.Name}\") - Inject using IOptionsSnapshot<T>.Get(\"{option.Name}\")");
            sb.Append("        services.Configure<");
            sb.Append(optionsType);
            sb.Append(">(\"");
            sb.Append(option.Name);
            sb.Append("\", configuration.GetSection(\"");
            sb.Append(sectionName);
            sb.AppendLineLf("\"));");
        }
        else
        {
            // Unnamed options - use AddOptions<T>() pattern
            sb.AppendLineLf($"        // Configure {option.ClassName} - Inject using {lifetimeComment}");
            sb.Append("        services.AddOptions<");
            sb.Append(optionsType);
            sb.AppendLineLf(">()");
            sb.Append("            .Bind(configuration.GetSection(\"");
            sb.Append(sectionName);
            sb.Append("\"))");

            if (option.ValidateDataAnnotations)
            {
                sb.AppendLineLf();
                sb.Append("            .ValidateDataAnnotations()");
            }

            if (option.ErrorOnMissingKeys)
            {
                sb.AppendLineLf();
                sb.AppendLineLf("            .Validate(options =>");
                sb.AppendLineLf("            {");
                sb.AppendLineLf($"                var section = configuration.GetSection(\"{sectionName}\");");
                sb.AppendLineLf("                if (!section.Exists())");
                sb.AppendLineLf("                {");
                sb.AppendLineLf($"                    throw new global::System.InvalidOperationException(");
                sb.AppendLineLf($"                        \"Configuration section '{sectionName}' is missing. \" +");
                sb.AppendLineLf($"                        \"Ensure the section exists in your appsettings.json or other configuration sources.\");");
                sb.AppendLineLf("                }");
                sb.AppendLineLf();
                sb.AppendLineLf("                return true;");
                sb.AppendLineLf("            })");
            }

            if (!string.IsNullOrWhiteSpace(option.PostConfigure))
            {
                sb.AppendLineLf();
                sb.Append("            .PostConfigure(options => ");
                sb.Append(optionsType);
                sb.Append('.');
                sb.Append(option.PostConfigure);
                sb.Append("(options))");
            }

            if (option.ValidateOnStart)
            {
                sb.AppendLineLf();
                sb.Append("            .ValidateOnStart()");
            }

            sb.AppendLineLf(";");

            // Register custom validator if specified (only for unnamed options)
            if (!string.IsNullOrWhiteSpace(option.ValidatorType))
            {
                sb.AppendLineLf();
                sb.Append("        services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<");
                sb.Append(optionsType);
                sb.Append(">, ");
                sb.Append(option.ValidatorType);
                sb.AppendLineLf(">();");
            }

            // Register OnChange callback listener if specified (only for unnamed options with Monitor lifetime)
            if (!string.IsNullOrWhiteSpace(option.OnChange))
            {
                var listenerClassName = $"{option.ClassName}ChangeListener";
                sb.AppendLineLf();
                sb.Append("        services.AddHostedService<");
                sb.Append(listenerClassName);
                sb.AppendLineLf(">();");
            }
        }

        sb.AppendLineLf();
    }

    private static void GenerateOnChangeHostedService(
        StringBuilder sb,
        OptionsInfo option)
    {
        var optionsType = $"global::{option.Namespace}.{option.ClassName}";
        var listenerClassName = $"{option.ClassName}ChangeListener";

        sb.AppendLineLf();
        sb.AppendLineLf("/// <summary>");
        sb.Append("/// Hosted service that registers configuration change callbacks for ");
        sb.Append(option.ClassName);
        sb.AppendLineLf(".");
        sb.AppendLineLf("/// This service is automatically generated and registered when OnChange callback is specified.");
        sb.AppendLineLf("/// </summary>");
        sb.AppendLineLf("[global::System.CodeDom.Compiler.GeneratedCode(\"Atc.SourceGenerators.OptionsBinding\", \"1.0.0\")]");
        sb.AppendLineLf("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLineLf("[global::System.Diagnostics.DebuggerNonUserCode]");
        sb.AppendLineLf("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        sb.Append("internal sealed class ");
        sb.Append(listenerClassName);
        sb.AppendLineLf(" : global::Microsoft.Extensions.Hosting.IHostedService");
        sb.AppendLineLf("{");
        sb.Append("    private readonly global::Microsoft.Extensions.Options.IOptionsMonitor<");
        sb.Append(optionsType);
        sb.AppendLineLf("> _monitor;");
        sb.AppendLineLf("    private global::System.IDisposable? _changeToken;");
        sb.AppendLineLf();
        sb.Append("    public ");
        sb.Append(listenerClassName);
        sb.Append("(global::Microsoft.Extensions.Options.IOptionsMonitor<");
        sb.Append(optionsType);
        sb.AppendLineLf("> monitor)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        _monitor = monitor ?? throw new global::System.ArgumentNullException(nameof(monitor));");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
        sb.AppendLineLf("    public global::System.Threading.Tasks.Task StartAsync(global::System.Threading.CancellationToken cancellationToken)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        _changeToken = _monitor.OnChange((options, name) =>");
        sb.Append("            ");
        sb.Append(optionsType);
        sb.Append('.');
        sb.Append(option.OnChange);
        sb.AppendLineLf("(options, name));");
        sb.AppendLineLf();
        sb.AppendLineLf("        return global::System.Threading.Tasks.Task.CompletedTask;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
        sb.AppendLineLf("    public global::System.Threading.Tasks.Task StopAsync(global::System.Threading.CancellationToken cancellationToken)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        _changeToken?.Dispose();");
        sb.AppendLineLf("        return global::System.Threading.Tasks.Task.CompletedTask;");
        sb.AppendLineLf("    }");
        sb.AppendLineLf("}");
    }

    private static string SanitizeForMethodName(string assemblyName)
    {
        var sb = new StringBuilder();
        var capitalizeNext = true;

        foreach (var c in assemblyName)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(capitalizeNext ? char.ToUpperInvariant(c) : c);
                capitalizeNext = false;
            }
            else
            {
                capitalizeNext = true;
            }
        }

        return sb.ToString();
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
            return SanitizeForMethodName(suffix);
        }

        // Multiple assemblies have this suffix, use full sanitized name to avoid conflicts
        return SanitizeForMethodName(assemblyName);
    }

    private static string GenerateAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Options lifetime enum for configuration options binding.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.OptionsBinding", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               public enum OptionsLifetime
               {
                   /// <summary>
                   /// Specifies that options will be resolved using IOptions&lt;T&gt; (singleton).
                   /// </summary>
                   Singleton = 0,

                   /// <summary>
                   /// Specifies that options will be resolved using IOptionsSnapshot&lt;T&gt; (scoped).
                   /// </summary>
                   Scoped = 1,

                   /// <summary>
                   /// Specifies that options will be resolved using IOptionsMonitor&lt;T&gt;.
                   /// </summary>
                   Monitor = 2,
               }

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
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.OptionsBinding", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
               public sealed class OptionsBindingAttribute : global::System.Attribute
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
                   public global::System.Type? Validator { get; set; }

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
               }
           }
           """;
}