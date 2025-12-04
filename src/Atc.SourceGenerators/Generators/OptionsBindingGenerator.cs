// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable StringLiteralTypo
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

    private static readonly DiagnosticDescriptor ConfigureAllRequiresMultipleNamedOptionsDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.ConfigureAllRequiresMultipleNamedOptions,
        "ConfigureAll requires multiple named options",
        "ConfigureAll callback '{0}' can only be used when the class has multiple named instances (Name property specified)",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConfigureAllCallbackNotFoundDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.ConfigureAllCallbackNotFound,
        "ConfigureAll callback method not found",
        "ConfigureAll callback method '{0}' not found in class '{1}'",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConfigureAllCallbackInvalidSignatureDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.ConfigureAllCallbackInvalidSignature,
        "ConfigureAll callback method has invalid signature",
        "ConfigureAll callback method '{0}' must have signature: static void {0}({1} options)",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ChildSectionsCannotBeUsedWithNameDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.ChildSectionsCannotBeUsedWithName,
        "ChildSections cannot be used with Name property",
        "ChildSections cannot be used with Name property. Use either ChildSections or Name, not both.",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ChildSectionsRequiresAtLeastTwoItemsDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.ChildSectionsRequiresAtLeastTwoItems,
        "ChildSections requires at least 2 items",
        "ChildSections requires at least 2 items. Found {0} item(s).",
        RuleCategoryConstants.OptionsBinding,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ChildSectionsItemsCannotBeNullOrEmptyDescriptor = new(
        RuleIdentifierConstants.OptionsBinding.ChildSectionsItemsCannotBeNullOrEmpty,
        "ChildSections items cannot be null or empty",
        "ChildSections array contains null or empty value at index {0}",
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

        // Generate OptionsInstanceCache only when there are options classes
        context.AddSource("OptionsInstanceCache.g.cs", SourceText.From(GenerateOptionsInstanceCacheSource(), Encoding.UTF8));

        // Detect referenced assemblies with [OptionsBinding] attributes and collect their options
        var referencedAssemblies = GetReferencedAssembliesWithOptionsBinding(compilation, out var referencedAssemblyOptions);

        // Group by assembly
        var groupedByAssembly = optionsToGenerate
            .GroupBy(x => x.AssemblyName, StringComparer.Ordinal)
            .ToList();

        foreach (var assemblyGroup in groupedByAssembly)
        {
            var source = GenerateExtensionMethod(assemblyGroup.Key, assemblyGroup.ToList(), referencedAssemblies, referencedAssemblyOptions);
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
            var optionsInfoList = ExtractOptionsInfoFromAttribute(classSymbol, attribute, context);
            if (optionsInfoList is not null)
            {
                result.AddRange(optionsInfoList);
            }
        }

        // Validate ConfigureAll if specified
        var configureAllOption = result.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.ConfigureAll));
        if (configureAllOption is not null)
        {
            // ConfigureAll requires multiple named instances
            var namedInstancesCount = result.Count(o => !string.IsNullOrWhiteSpace(o.Name));
            if (namedInstancesCount < 2)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        ConfigureAllRequiresMultipleNamedOptionsDescriptor,
                        classSymbol.Locations.First(),
                        configureAllOption.ConfigureAll));

                // Remove ConfigureAll from all instances to prevent code generation issues
                result = result
                    .Select(o => o with { ConfigureAll = null })
                    .ToList();
            }
            else
            {
                // Validate callback method exists and has correct signature
                var callbackMethod = classSymbol
                    .GetMembers(configureAllOption.ConfigureAll!)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault();

                if (callbackMethod is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            ConfigureAllCallbackNotFoundDescriptor,
                            classSymbol.Locations.First(),
                            configureAllOption.ConfigureAll,
                            classSymbol.Name));

                    result = result
                        .Select(o => o with { ConfigureAll = null })
                        .ToList();
                }
                else if (!callbackMethod.IsStatic ||
                         !callbackMethod.ReturnsVoid ||
                         callbackMethod.Parameters.Length != 1 ||
                         !SymbolEqualityComparer.Default.Equals(callbackMethod.Parameters[0].Type, classSymbol))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            ConfigureAllCallbackInvalidSignatureDescriptor,
                            classSymbol.Locations.First(),
                            configureAllOption.ConfigureAll,
                            classSymbol.Name));

                    result = result
                        .Select(o => o with { ConfigureAll = null })
                        .ToList();
                }
            }
        }

        return result;
    }

    private static List<OptionsInfo>? ExtractOptionsInfoFromAttribute(
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
        string? configureAll = null;
        string?[]? childSections = null;
        var alsoRegisterDirectType = false;

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
                case "ConfigureAll":
                    configureAll = namedArg.Value.Value as string;
                    break;
                case "ChildSections":
                    if (namedArg.Value.Kind == TypedConstantKind.Array)
                    {
                        var values = namedArg.Value.Values;

                        // Always set childSections, even if empty, so validation can detect and report errors
                        childSections = values.IsDefaultOrEmpty
                            ? Array.Empty<string?>()
                            : values
                                .Select(v => v.Value as string)
                                .ToArray();
                    }

                    break;
                case "AlsoRegisterDirectType":
                    alsoRegisterDirectType = namedArg.Value.Value as bool? ?? false;
                    break;
            }
        }

        // Validate ChildSections requirements
        if (childSections is not null)
        {
            // ChildSections cannot be used with Name
            if (!string.IsNullOrWhiteSpace(name))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        ChildSectionsCannotBeUsedWithNameDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None));

                return null;
            }

            // ChildSections requires at least 2 items
            if (childSections.Length < 2)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        ChildSectionsRequiresAtLeastTwoItemsDescriptor,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                        childSections.Length));

                return null;
            }

            // ChildSections items cannot be null or empty
            for (int i = 0; i < childSections.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(childSections[i]))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            ChildSectionsItemsCannotBeNullOrEmptyDescriptor,
                            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                            i));

                    return null;
                }
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

        // If ChildSections is specified, expand into multiple OptionsInfo instances
        if (childSections is not null)
        {
            var result = new List<OptionsInfo>();
            foreach (var childSection in childSections)
            {
                // Each child section becomes a named instance
                // Name = childSection, SectionName = "{ParentSection}:{ChildSection}"
                // Note: childSection is guaranteed non-null by validation above
                var childSectionName = string.IsNullOrWhiteSpace(sectionName)
                    ? childSection!
                    : $"{sectionName}:{childSection!}";

                result.Add(new OptionsInfo(
                    classSymbol.Name,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    classSymbol.ContainingAssembly.Name,
                    childSectionName,
                    validateOnStart,
                    validateDataAnnotations,
                    lifetime,
                    validatorTypeName,
                    childSection,  // Name is set to the child section name
                    errorOnMissingKeys,
                    onChange,
                    postConfigure,
                    configureAll,
                    childSections,  // Store ChildSections to indicate this is part of a child sections group
                    alsoRegisterDirectType));
            }

            return result;
        }

        // Single OptionsInfo instance (no ChildSections)
        return
        [
            new OptionsInfo(
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
                postConfigure,
                configureAll,
                null, // No ChildSections
                alsoRegisterDirectType)
        ];
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
        Compilation compilation,
        out Dictionary<string, List<OptionsInfo>> referencedAssemblyOptions)
    {
        var result = new List<ReferencedAssemblyInfo>();
        referencedAssemblyOptions = new Dictionary<string, List<OptionsInfo>>(StringComparer.Ordinal);
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

                // Collect options from this referenced assembly
                var options = CollectOptionsFromAssembly(assemblySymbol);
                if (options.Count > 0)
                {
                    referencedAssemblyOptions[assemblyName] = options;
                }
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

        return [.. result];
    }

    private static List<OptionsInfo> CollectOptionsFromAssembly(
        IAssemblySymbol assemblySymbol)
    {
        var result = new List<OptionsInfo>();
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(assemblySymbol.GlobalNamespace);

        while (stack.Count > 0)
        {
            var currentNamespace = stack.Pop();

            // Process types in this namespace
            foreach (var typeMember in currentNamespace.GetTypeMembers())
            {
                if (typeMember is not INamedTypeSymbol { TypeKind: TypeKind.Class } namedType)
                {
                    continue;
                }

                // Check if type has OptionsBinding attribute
                var optionsAttributes = namedType
                    .GetAttributes()
                    .Where(a => a.AttributeClass?.ToDisplayString() == FullAttributeName)
                    .ToList();

                if (optionsAttributes.Count <= 0)
                {
                    continue;
                }

                // Extract basic info for dispatcher (no validation needed)
                foreach (var attribute in optionsAttributes)
                {
                    var optionsInfo = ExtractBasicOptionsInfo(namedType, attribute);
                    if (optionsInfo is not null)
                    {
                        result.Add(optionsInfo);
                    }
                }
            }

            // Process nested namespaces
            foreach (var nestedNamespace in currentNamespace.GetNamespaceMembers())
            {
                stack.Push(nestedNamespace);
            }
        }

        return result;
    }

    private static OptionsInfo? ExtractBasicOptionsInfo(
        INamedTypeSymbol namedType,
        AttributeData attribute)
    {
        // Extract only the essential information needed for dispatcher
        var className = namedType.Name;
        var namespaceName = namedType.ContainingNamespace.ToDisplayString();
        var assemblyName = namedType.ContainingAssembly.Name;

        // Check if this is a named option (has Name property set)
        string? nameValue = null;
        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key == "Name")
            {
                nameValue = namedArg.Value.Value as string;
                break;
            }
        }

        if (!string.IsNullOrWhiteSpace(nameValue))
        {
            // Skip named options - dispatcher only works with unnamed options
            return null;
        }

        // Create minimal OptionsInfo for dispatcher
        return new OptionsInfo(
            className,
            namespaceName,
            assemblyName,
            SectionName: string.Empty,  // Not needed for dispatcher
            ValidateDataAnnotations: false,
            ValidateOnStart: false,
            ErrorOnMissingKeys: false,
            ValidatorType: null,
            Lifetime: 0,
            OnChange: null,
            PostConfigure: null,
            ConfigureAll: null,
            Name: null,
            ChildSections: null,
            AlsoRegisterDirectType: false);
    }

    private static bool HasOptionsBindingAttributeInNamespace(
        IAssemblySymbol assemblySymbol)
    {
        // Check if any types in the assembly have the [OptionsBinding] attribute
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(assemblySymbol.GlobalNamespace);

        while (stack.Count > 0)
        {
            var currentNamespace = stack.Pop();

            // Check types in this namespace
            foreach (var typeMember in currentNamespace.GetTypeMembers())
            {
                if (typeMember is not INamedTypeSymbol { TypeKind: TypeKind.Class } namedType)
                {
                    continue;
                }

                // Check if type has OptionsBinding attribute
                if (namedType.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == FullAttributeName))
                {
                    return true;
                }
            }

            // Check nested namespaces
            foreach (var nestedNamespace in currentNamespace.GetNamespaceMembers())
            {
                stack.Push(nestedNamespace);
            }
        }

        return false;
    }

    private static string GenerateExtensionMethod(
        string assemblyName,
        List<OptionsInfo> options,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies,
        Dictionary<string, List<OptionsInfo>> referencedAssemblyOptions)
    {
        var sb = new StringBuilder();
        var methodSuffix = GetSmartMethodSuffix(assemblyName, referencedAssemblies);

        sb.AppendLine($$"""
// <auto-generated/>
#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring options from the {{assemblyName}} assembly.
/// </summary>
public static class OptionsBindingExtensions{{methodSuffix}}
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

        // Generate ConfigureAll if any option has it (for named instances)
        var namedOptions = options
            .Where(o => !string.IsNullOrWhiteSpace(o.Name))
            .ToList();
        if (namedOptions.Count > 0)
        {
            var configureAllOption = namedOptions.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.ConfigureAll));
            if (configureAllOption is not null)
            {
                var optionsType = $"global::{configureAllOption.Namespace}.{configureAllOption.ClassName}";

                sb.AppendLineLf($"        // Configure defaults for ALL named instances of {configureAllOption.ClassName}");
                sb.Append("        services.ConfigureAll<");
                sb.Append(optionsType);
                sb.Append(">(options => ");
                sb.Append(optionsType);
                sb.Append('.');
                sb.Append(configureAllOption.ConfigureAll);
                sb.AppendLineLf("(options));");
                sb.AppendLineLf();
            }
        }

        foreach (var option in options)
        {
            GenerateOptionsRegistration(sb, option, methodSuffix);
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
                sb.AppendLine($"            services.AddOptionsFrom{refSmartSuffix}(configuration, includeReferencedAssemblies: true);");
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
                sb.AppendLine($"                services.AddOptionsFrom{refSmartSuffix}(configuration, includeReferencedAssemblies: true);");
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

        // Generate early access GetOrAdd methods for unnamed options only
        var unnamedOptions = options
            .Where(o => string.IsNullOrWhiteSpace(o.Name))
            .ToList();
        if (unnamedOptions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLineLf("    // ========== Early Access Methods (Avoid BuildServiceProvider Anti-Pattern) ==========");
            sb.AppendLine();

            foreach (var option in unnamedOptions)
            {
                GenerateGetMethod(sb, option, methodSuffix);
                GenerateGetOrAddMethod(sb, option, methodSuffix);
            }

            // Generate smart dispatcher generic GetOptions<T>() method
            // ONLY in assemblies that have referenced assemblies with OptionsBinding
            // This prevents CS0121 ambiguity by ensuring only the consuming assembly has the dispatcher
            if (referencedAssemblies.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLineLf("    // ========== Generic Convenience Method (Smart Dispatcher) ==========");
                sb.AppendLineLf("    // This method intelligently dispatches to the correct assembly-specific method");
                sb.AppendLineLf("    // based on the type parameter, avoiding CS0121 ambiguity errors.");
                sb.AppendLineLf("    // Includes options from current and referenced assemblies.");
                sb.AppendLine();

                GenerateGenericGetOptionsMethod(sb, unnamedOptions, methodSuffix, referencedAssemblies, referencedAssemblyOptions);
            }
            else
            {
                sb.AppendLine();
                sb.AppendLineLf("    // ========== Generic Method Not Generated ==========");
                sb.AppendLineLf("    // GetOptions<T>() is not generated in library assemblies without references.");
                sb.AppendLineLf("    // It will be generated in consuming assemblies that reference this library.");
                sb.AppendLineLf("    // Use assembly-specific methods:");
                foreach (var option in unnamedOptions)
                {
                    sb.Append("    //   - GetOrAdd");
                    sb.Append(option.ClassName);
                    sb.Append("From");
                    sb.Append(methodSuffix);
                    sb.AppendLineLf("()");
                }

                sb.AppendLine();
            }
        }

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
        OptionsInfo option,
        string methodSuffix)
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

        // Check if this option needs the fluent API (validation, error checking, etc.)
        var needsFluentApi = option.ValidateDataAnnotations ||
                            option.ErrorOnMissingKeys ||
                            option.ValidateOnStart ||
                            !string.IsNullOrWhiteSpace(option.PostConfigure);

        if (isNamed && !needsFluentApi)
        {
            // Named options without validation - use simple Configure<T>(name, ...) pattern
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
            // Use fluent API pattern (supports both named and unnamed options)
            sb.AppendLineLf(isNamed
                ? $"        // Configure {option.ClassName} (Named: \"{option.Name}\") - Inject using IOptionsSnapshot<T>.Get(\"{option.Name}\")"
                : $"        // Configure {option.ClassName} - Inject using {lifetimeComment}");

            sb.Append("        services.AddOptions<");
            sb.Append(optionsType);
            sb.Append(">(");
            if (isNamed)
            {
                sb.Append('"');
                sb.Append(option.Name);
                sb.Append('"');
            }

            sb.AppendLineLf(")");
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
                sb.AppendLineLf("                    throw new global::System.InvalidOperationException(");
                sb.AppendLineLf($"                        \"Configuration section '{sectionName}' is missing. \" +");
                sb.AppendLineLf("                        \"Ensure the section exists in your appsettings.json or other configuration sources.\");");
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

            // For unnamed options, add to shared cache for early access via GetOptionInstanceOf<T>()
            if (!isNamed)
            {
                sb.AppendLineLf();
                sb.Append("            .PostConfigure(options =>");
                sb.AppendLineLf();
                sb.AppendLineLf("            {");
                sb.AppendLineLf("                // Add to shared cache for early access");
                sb.Append("                global::Atc.OptionsBinding.OptionsInstanceCache.Add(options, \"");
                sb.Append(methodSuffix);
                sb.AppendLineLf("\");");
                sb.Append("            })");
            }

            if (option.ValidateOnStart)
            {
                sb.AppendLineLf();
                sb.Append("            .ValidateOnStart()");
            }

            // Semicolon on same line as last method call
            sb.AppendLineLf(";");

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

        // Register custom validator if specified (works for both named and unnamed options)
        if (!string.IsNullOrWhiteSpace(option.ValidatorType))
        {
            sb.AppendLineLf();
            sb.Append("        services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<");
            sb.Append(optionsType);
            sb.Append(">, ");
            sb.Append(option.ValidatorType);
            sb.AppendLineLf(">();");
        }

        // Register direct type if AlsoRegisterDirectType is true
        if (option.AlsoRegisterDirectType && !isNamed)
        {
            sb.AppendLineLf();
            sb.AppendLineLf($"        // Also register {option.ClassName} as direct type (for legacy code or third-party library compatibility)");

            // Choose service lifetime and options interface based on OptionsLifetime
            var (serviceMethod, optionsInterface, valueAccess) = option.Lifetime switch
            {
                0 => ("AddSingleton", "IOptions", "Value"),                          // Singleton
                1 => ("AddScoped", "IOptionsSnapshot", "Value"),                     // Scoped
                2 => ("AddSingleton", "IOptionsMonitor", "CurrentValue"),            // Monitor
                _ => ("AddSingleton", "IOptions", "Value"),                          // Default
            };

            sb.Append("        services.");
            sb.Append(serviceMethod);
            sb.Append("(sp => sp.GetRequiredService<global::Microsoft.Extensions.Options.");
            sb.Append(optionsInterface);
            sb.Append('<');
            sb.Append(optionsType);
            sb.Append(">>().");
            sb.Append(valueAccess);
            sb.AppendLineLf(");");
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

    private static void GenerateGetMethod(
        StringBuilder sb,
        OptionsInfo option,
        string methodSuffix)
    {
        var optionsType = $"global::{option.Namespace}.{option.ClassName}";
        var sectionName = option.SectionName;

        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.Append("    /// Gets an instance of ");
        sb.Append(option.ClassName);
        sb.AppendLineLf(" with configuration binding.");
        sb.AppendLineLf("    /// If an instance was previously cached by GetOrAdd, returns that instance.");
        sb.AppendLineLf("    /// Otherwise, creates a new instance without adding it to the cache.");
        sb.AppendLineLf("    /// This method does not populate the cache (no side effects), but benefits from existing cached values.");
        sb.AppendLineLf("    /// For caching behavior, use GetOrAdd instead.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection (used for extension method chaining).</param>");
        sb.AppendLineLf("    /// <param name=\"configuration\">The configuration instance.</param>");
        sb.Append("    /// <returns>A bound and validated ");
        sb.Append(option.ClassName);
        sb.AppendLineLf(" instance.</returns>");
        sb.Append("    public static ");
        sb.Append(optionsType);
        sb.Append(" Get");
        sb.Append(option.ClassName);
        sb.Append("From");
        sb.AppendLineLf(methodSuffix);
        sb.AppendLineLf("        (this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,");
        sb.AppendLineLf("         global::Microsoft.Extensions.Configuration.IConfiguration configuration)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        // Check cache first (read-only, no side effects)");
        sb.Append("        var cached = global::Atc.OptionsBinding.OptionsInstanceCache.TryGet<");
        sb.Append(optionsType);
        sb.AppendLineLf(">();");
        sb.AppendLineLf("        if (cached is not null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            return cached;");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Create and bind instance (not cached)");
        sb.Append("        var options = new ");
        sb.Append(optionsType);
        sb.AppendLineLf("();");
        sb.Append("        var section = configuration.GetSection(\"");
        sb.Append(sectionName);
        sb.AppendLineLf("\");");
        sb.AppendLineLf("        section.Bind(options);");
        sb.AppendLineLf();

        // Add ErrorOnMissingKeys validation if specified
        if (option.ErrorOnMissingKeys)
        {
            sb.AppendLineLf("        // Validate section exists (ErrorOnMissingKeys)");
            sb.AppendLineLf("        if (!section.Exists())");
            sb.AppendLineLf("        {");
            sb.AppendLineLf("            throw new global::System.InvalidOperationException(");
            sb.Append("                \"Configuration section '");
            sb.Append(sectionName);
            sb.AppendLineLf("' is missing. \" +");
            sb.AppendLineLf("                \"Ensure the section exists in your appsettings.json or other configuration sources.\");");
            sb.AppendLineLf("        }");
            sb.AppendLineLf();
        }

        // Add DataAnnotations validation if specified
        if (option.ValidateDataAnnotations)
        {
            sb.AppendLineLf("        // Validate immediately (DataAnnotations)");
            sb.AppendLineLf("        var validationContext = new global::System.ComponentModel.DataAnnotations.ValidationContext(options);");
            sb.AppendLineLf("        global::System.ComponentModel.DataAnnotations.Validator.ValidateObject(options, validationContext, validateAllProperties: true);");
            sb.AppendLineLf();
        }

        // Apply PostConfigure if specified
        if (!string.IsNullOrWhiteSpace(option.PostConfigure))
        {
            sb.AppendLineLf("        // Apply post-configuration");
            sb.Append("        ");
            sb.Append(optionsType);
            sb.Append('.');
            sb.Append(option.PostConfigure);
            sb.AppendLineLf("(options);");
            sb.AppendLineLf();
        }

        // Register custom validator if specified (even for non-cached Get)
        if (!string.IsNullOrWhiteSpace(option.ValidatorType))
        {
            sb.AppendLineLf("        // Register custom validator");
            sb.Append("        services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<");
            sb.Append(optionsType);
            sb.Append(">, ");
            sb.Append(option.ValidatorType);
            sb.AppendLineLf(">();");
            sb.AppendLineLf();
        }

        sb.AppendLineLf("        return options;");
        sb.AppendLineLf("    }");
    }

    private static void GenerateGetOrAddMethod(
        StringBuilder sb,
        OptionsInfo option,
        string methodSuffix)
    {
        var optionsType = $"global::{option.Namespace}.{option.ClassName}";
        var sectionName = option.SectionName;

        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.Append("    /// Gets or creates a cached instance of ");
        sb.Append(option.ClassName);
        sb.AppendLineLf(" with configuration binding for early access.");
        sb.AppendLineLf("    /// If already cached, returns the existing instance. Otherwise, creates, binds, validates, and caches the instance.");
        sb.AppendLineLf("    /// This method is idempotent and safe to call multiple times.");
        sb.AppendLineLf("    /// This method enables early access to options during service registration without calling BuildServiceProvider().");
        sb.AppendLineLf("    /// Note: This method is called on-demand when you need early access, not automatically by AddOptionsFrom.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection (used for extension method chaining).</param>");
        sb.AppendLineLf("    /// <param name=\"configuration\">The configuration instance.</param>");
        sb.Append("    /// <returns>The bound and validated ");
        sb.Append(option.ClassName);
        sb.AppendLineLf(" instance for immediate use during service registration.</returns>");
        sb.Append("    public static ");
        sb.Append(optionsType);
        sb.Append(" GetOrAdd");
        sb.Append(option.ClassName);
        sb.Append("From");
        sb.AppendLineLf(methodSuffix);
        sb.AppendLineLf("        (this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,");
        sb.AppendLineLf("         global::Microsoft.Extensions.Configuration.IConfiguration configuration)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        // Check if already registered (idempotent)");
        sb.Append("        var existing = global::Atc.OptionsBinding.OptionsInstanceCache.TryGet<");
        sb.Append(optionsType);
        sb.AppendLineLf(">();");
        sb.AppendLineLf("        if (existing is not null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            return existing;");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        // Create and bind instance");
        sb.Append("        var options = new ");
        sb.Append(optionsType);
        sb.AppendLineLf("();");
        sb.Append("        var section = configuration.GetSection(\"");
        sb.Append(sectionName);
        sb.AppendLineLf("\");");
        sb.AppendLineLf("        section.Bind(options);");
        sb.AppendLineLf();

        // Add ErrorOnMissingKeys validation if specified
        if (option.ErrorOnMissingKeys)
        {
            sb.AppendLineLf("        // Validate section exists (ErrorOnMissingKeys)");
            sb.AppendLineLf("        if (!section.Exists())");
            sb.AppendLineLf("        {");
            sb.AppendLineLf("            throw new global::System.InvalidOperationException(");
            sb.Append("                \"Configuration section '");
            sb.Append(sectionName);
            sb.AppendLineLf("' is missing. \" +");
            sb.AppendLineLf("                \"Ensure the section exists in your appsettings.json or other configuration sources.\");");
            sb.AppendLineLf("        }");
            sb.AppendLineLf();
        }

        // Add DataAnnotations validation if specified
        if (option.ValidateDataAnnotations)
        {
            sb.AppendLineLf("        // Validate immediately (DataAnnotations)");
            sb.AppendLineLf("        var validationContext = new global::System.ComponentModel.DataAnnotations.ValidationContext(options);");
            sb.AppendLineLf("        global::System.ComponentModel.DataAnnotations.Validator.ValidateObject(options, validationContext, validateAllProperties: true);");
            sb.AppendLineLf();
        }

        // Apply PostConfigure if specified
        if (!string.IsNullOrWhiteSpace(option.PostConfigure))
        {
            sb.AppendLineLf("        // Apply post-configuration");
            sb.Append("        ");
            sb.Append(optionsType);
            sb.Append('.');
            sb.Append(option.PostConfigure);
            sb.AppendLineLf("(options);");
            sb.AppendLineLf();
        }

        sb.AppendLineLf("        // Add to shared cache for early access and smart dispatcher");
        sb.Append("        global::Atc.OptionsBinding.OptionsInstanceCache.Add(options, \"");
        sb.Append(methodSuffix);
        sb.AppendLineLf("\");");
        sb.AppendLineLf();

        // Register custom validator if specified
        if (!string.IsNullOrWhiteSpace(option.ValidatorType))
        {
            sb.AppendLineLf("        // Register custom validator");
            sb.Append("        services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<");
            sb.Append(optionsType);
            sb.Append(">, ");
            sb.Append(option.ValidatorType);
            sb.AppendLineLf(">();");
            sb.AppendLineLf();
        }

        sb.AppendLineLf("        return options;");
        sb.AppendLineLf("    }");
    }

    private static void GenerateGenericGetOptionsMethod(
        StringBuilder sb,
        List<OptionsInfo> currentAssemblyOptions,
        string currentAssemblySuffix,
        ImmutableArray<ReferencedAssemblyInfo> referencedAssemblies,
        Dictionary<string, List<OptionsInfo>> referencedAssemblyOptions)
    {
        // Build context for smart suffix calculation
        var allAssemblies = new List<string>();
        foreach (var refAsm in referencedAssemblies)
        {
            allAssemblies.Add(refAsm.AssemblyName);
        }

        // Collect all available options for error message
        var allOptionsTypes = new List<string>();
        foreach (var option in currentAssemblyOptions)
        {
            allOptionsTypes.Add($"global::{option.Namespace}.{option.ClassName}");
        }

        foreach (var refAssembly in referencedAssemblies)
        {
            if (referencedAssemblyOptions.TryGetValue(refAssembly.AssemblyName, out var refOptions))
            {
                foreach (var option in refOptions)
                {
                    allOptionsTypes.Add($"global::{option.Namespace}.{option.ClassName}");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf("    /// Gets options of the specified type, intelligently dispatching to the correct");
        sb.AppendLineLf("    /// assembly-specific method based on the type parameter.");
        sb.AppendLineLf("    /// <para>This smart dispatcher eliminates CS0121 ambiguity errors in multi-assembly scenarios</para>");
        sb.AppendLineLf("    /// <para>by routing to the appropriate Get{OptionsName}From{Assembly}() method.</para>");
        sb.AppendLineLf("    /// <para>Note: This method calls the pure Get methods (no caching side effects).</para>");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <typeparam name=\"T\">The options type.</typeparam>");
        sb.AppendLineLf("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLineLf("    /// <param name=\"configuration\">The configuration instance.</param>");
        sb.AppendLineLf("    /// <returns>The bound and validated options instance.</returns>");
        sb.AppendLineLf("    /// <exception cref=\"global::System.InvalidOperationException\">Thrown when type T is not a registered options type.</exception>");
        sb.AppendLineLf("    public static T GetOptions<T>(");
        sb.AppendLineLf("        this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,");
        sb.AppendLineLf("        global::Microsoft.Extensions.Configuration.IConfiguration configuration)");
        sb.AppendLineLf("        where T : class");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        var type = typeof(T);");
        sb.AppendLineLf();

        // Generate type dispatch for current assembly options
        if (currentAssemblyOptions.Count > 0)
        {
            sb.AppendLineLf("        // Current assembly options");
            foreach (var option in currentAssemblyOptions)
            {
                var optionsType = $"global::{option.Namespace}.{option.ClassName}";
                sb.Append("        if (type == typeof(");
                sb.Append(optionsType);
                sb.AppendLineLf("))");
                sb.Append("            return (T)(object)services.Get");
                sb.Append(option.ClassName);
                sb.Append("From");
                sb.Append(currentAssemblySuffix);
                sb.AppendLineLf("(configuration);");
                sb.AppendLineLf();
            }
        }

        // Generate type dispatch for referenced assembly options
        if (referencedAssemblies.Length > 0)
        {
            sb.AppendLineLf("        // Referenced assembly options");
            foreach (var refAssembly in referencedAssemblies)
            {
                if (referencedAssemblyOptions.TryGetValue(refAssembly.AssemblyName, out var refOptions))
                {
                    // Get smart suffix for this referenced assembly
                    var refMethodSuffix = GetSmartMethodSuffixFromContext(refAssembly.AssemblyName, allAssemblies);

                    foreach (var option in refOptions)
                    {
                        var optionsType = $"global::{option.Namespace}.{option.ClassName}";
                        sb.Append("        if (type == typeof(");
                        sb.Append(optionsType);
                        sb.AppendLineLf("))");
                        sb.Append("            return (T)(object)services.Get");
                        sb.Append(option.ClassName);
                        sb.Append("From");
                        sb.Append(refMethodSuffix);
                        sb.AppendLineLf("(configuration);");
                        sb.AppendLineLf();
                    }
                }
            }
        }

        // Generate error for unrecognized types
        sb.AppendLineLf("        // Type not recognized - generate helpful error message");
        sb.AppendLineLf("        var availableTypes = new[]");
        sb.AppendLineLf("        {");
        for (int i = 0; i < allOptionsTypes.Count; i++)
        {
            sb.Append("            \"");
            sb.Append(allOptionsTypes[i]);
            sb.Append('"');
            if (i < allOptionsTypes.Count - 1)
            {
                sb.Append(',');
            }

            sb.AppendLineLf();
        }

        sb.AppendLineLf("        };");
        sb.AppendLineLf();
        sb.AppendLineLf("        throw new global::System.InvalidOperationException(");
        sb.AppendLineLf("            $\"Type '{type.FullName}' is not a registered options type. \" +");
        sb.AppendLineLf("            $\"Available types: {string.Join(\", \", availableTypes.Select(t => t.Split('.').Last()))}\");");
        sb.AppendLineLf("    }");
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

        // Multiple assemblies have this suffix, useful sanitized name to avoid conflicts
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

                   /// <summary>
                   /// Gets or sets the name of a static method to configure ALL named instances with default values.
                   /// The method must have the signature: <c>static void MethodName(TOptions options)</c>
                   /// where TOptions is the options class type.
                   /// This is useful for setting default values that apply to all named instances before individual configurations override them.
                   /// The configuration action runs using the <c>.ConfigureAll()</c> pattern before individual <c>Configure()</c> calls.
                   /// Default is null (no configure-all).
                   /// Only applicable when the class has multiple named instances (Name property specified on multiple attributes).
                   /// </summary>
                   /// <remarks>
                   /// ConfigureAll is executed BEFORE individual named instance configurations, allowing you to set defaults.
                   /// For example, set MaxRetries=3 for all database connections, then override for specific instances.
                   /// Specify ConfigureAll on any one of the [OptionsBinding] attributes when using named options.
                   /// Cannot be used with single unnamed instances (use PostConfigure instead).
                   /// </remarks>
                   public string? ConfigureAll { get; set; }

                   /// <summary>
                   /// Gets or sets an array of child section names to bind under the parent section.
                   /// This provides a concise way to create multiple named options instances from child sections.
                   /// Each child section name becomes both the instance name and the section path suffix.
                   /// For example, <c>ChildSections = new[] { "Primary", "Secondary" }</c> with <c>SectionName = "Database"</c>
                   /// creates named instances accessible via <c>IOptionsSnapshot&lt;T&gt;.Get("Primary")</c>
                   /// bound to sections "Database:Primary" and "Database:Secondary".
                   /// Default is null (no child sections).
                   /// </summary>
                   /// <remarks>
                   /// Cannot be used with the Name property - they are mutually exclusive.
                   /// Requires at least 2 child sections.
                   /// Useful for multi-tenant scenarios, regional configurations, or environment-specific settings.
                   /// </remarks>
                   public string[]? ChildSections { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to also register the options type
                   /// as a direct service (not wrapped in IOptions&lt;T&gt;).
                   /// Default is false.
                   /// </summary>
                   public bool AlsoRegisterDirectType { get; set; }
               }
           }
           """;

    private static string GenerateOptionsInstanceCacheSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.OptionsBinding
           {
               using System.Collections.Concurrent;
               using System.Collections.Generic;
               using System.Linq;

               /// <summary>
               /// Internal cache for storing option instances for early access during service registration.
               /// This allows retrieving bound and validated options instances without calling BuildServiceProvider().
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.OptionsBinding", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               internal static class OptionsInstanceCache
               {
                   private static readonly ConcurrentDictionary<global::System.Type, global::System.Collections.Generic.List<(object Instance, string AssemblyName)>> instances = new();
                   private static readonly object lockObject = new();

                   /// <summary>
                   /// Adds an options instance to the cache with assembly metadata.
                   /// </summary>
                   internal static void Add<T>(T instance, string assemblyName) where T : class
                   {
                       lock (lockObject)
                       {
                           var type = typeof(T);
                           if (!instances.TryGetValue(type, out var list))
                           {
                               list = new global::System.Collections.Generic.List<(object, string)>();
                               instances[type] = list;
                           }

                           // Check if already registered from this assembly (idempotency)
                           var existing = list.FirstOrDefault(x => x.AssemblyName == assemblyName);
                           if (existing.Instance != null)
                           {
                               // Already registered from this assembly - skip
                               return;
                           }

                           list.Add((instance, assemblyName));
                       }
                   }

                   /// <summary>
                   /// Tries to get an options instance from the cache (returns first match if multiple).
                   /// </summary>
                   internal static T? TryGet<T>() where T : class
                   {
                       if (instances.TryGetValue(typeof(T), out var list) && list.Count > 0)
                       {
                           return (T)list[0].Instance;
                       }

                       return null;
                   }

                   /// <summary>
                   /// Finds all registrations for a given type across all assemblies.
                   /// </summary>
                   internal static global::System.Collections.Generic.List<(object Instance, string AssemblyName)> FindAll<T>() where T : class
                   {
                       if (instances.TryGetValue(typeof(T), out var list))
                       {
                           return new global::System.Collections.Generic.List<(object Instance, string AssemblyName)>(list);
                       }

                       return new global::System.Collections.Generic.List<(object Instance, string AssemblyName)>();
                   }
               }
           }

           """;
}