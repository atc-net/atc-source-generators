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

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate the attribute definition as fallback
        // If Atc.SourceGenerators.Annotations is referenced, CS0436 warning will be suppressed via project settings
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

            var optionsInfo = ExtractOptionsInfo(classSymbol, context);
            if (optionsInfo is not null)
            {
                optionsToGenerate.Add(optionsInfo);
            }
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

    private static OptionsInfo? ExtractOptionsInfo(
        INamedTypeSymbol classSymbol,
        SourceProductionContext context)
    {
        // Check if class is partial
        if (!classSymbol.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is ClassDeclarationSyntax c &&
                                                            c.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    OptionsClassMustBePartialDescriptor,
                    classSymbol.Locations.First(),
                    classSymbol.Name));
            return null;
        }

        // Get the attribute
        var attribute = classSymbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == FullAttributeName);

        if (attribute is null)
        {
            return null;
        }

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
            }
        }

        return new OptionsInfo(
            classSymbol.Name,
            classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.ContainingAssembly.Name,
            sectionName!, // Guaranteed non-null after validation above
            validateOnStart,
            validateDataAnnotations,
            lifetime);
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

                if (matchingReference is not null)
                {
                    var referencedSymbol = compilation.GetAssemblyOrModuleSymbol(matchingReference) as IAssemblySymbol;
                    if (referencedSymbol is not null)
                    {
                        queue.Enqueue(referencedSymbol);
                    }
                }
            }
        }

        return result.ToImmutableArray();
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

        if (includeReferencedAssemblies)
        {
""");

        foreach (var refAssembly in referencedAssemblies)
        {
            var refSmartSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);
            sb.AppendLine($"            AddOptionsFrom{refSmartSuffix}(services, configuration, includeReferencedAssemblies: true);");
        }

        sb.AppendLine("""
        }

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
}
""");

        return sb.ToString();
    }

    private static void GenerateOptionsRegistration(
        StringBuilder sb,
        OptionsInfo option)
    {
        var optionsType = $"global::{option.Namespace}.{option.ClassName}";
        var sectionName = option.SectionName;

        // Add comment indicating which interface to inject based on lifetime
        var lifetimeComment = option.Lifetime switch
        {
            0 => "IOptions<T>",         // Singleton
            1 => "IOptionsSnapshot<T>", // Scoped
            2 => "IOptionsMonitor<T>",  // Monitor
            _ => "IOptions<T>",         // Default
        };

        sb.AppendLineLf($"        // Configure {option.ClassName} - Inject using {lifetimeComment}");
        sb.Append("        services.AddOptions<");
        sb.Append(optionsType);
        sb.AppendLineLf(">()");
        sb.Append("            .Bind(configuration.GetSection(\"");
        sb.Append(sectionName);
        sb.AppendLineLf("\"))");

        if (option.ValidateDataAnnotations)
        {
            sb.AppendLineLf("            .ValidateDataAnnotations()");
        }

        if (option.ValidateOnStart)
        {
            sb.AppendLineLf("            .ValidateOnStart()");
        }

        sb.AppendLineLf("            ;");
        sb.AppendLineLf();
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
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.OptionsBinding", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
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
               }
           }
           """;

    private sealed record OptionsInfo(
        string ClassName,
        string Namespace,
        string AssemblyName,
        string SectionName,
        bool ValidateOnStart,
        bool ValidateDataAnnotations,
        int Lifetime);

    private sealed record ReferencedAssemblyInfo(
        string AssemblyName,
        string SanitizedName,
        string ShortName);
}