// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
// ReSharper disable DuplicatedStatements
namespace Atc.SourceGenerators.Generators;

/// <summary>
/// Source generator for configuration-based object-to-object mapping.
/// Supports mapping third-party types without attributes via [MappingConfiguration] classes
/// and [assembly: MapTypes] attributes.
/// </summary>
[SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK")]
[Generator]
public class MappingConfigurationGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Atc.SourceGenerators.Annotations";
    private const string MappingConfigurationAttributeName = "MappingConfigurationAttribute";
    private const string MapConfigIgnoreAttributeName = "MapConfigIgnoreAttribute";
    private const string MapConfigPropertyAttributeName = "MapConfigPropertyAttribute";
    private const string MapConfigOptionsAttributeName = "MapConfigOptionsAttribute";
    private const string MapTypesAttributeName = "MapTypesAttribute";

    private static readonly DiagnosticDescriptor ConfigClassMustBeStaticDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.ConfigClassMustBeStatic,
        title: "Configuration class must be static",
        messageFormat: "Mapping configuration class '{0}' must be declared as static",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConfigClassMustBePartialDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.ConfigClassMustBePartial,
        title: "Configuration class must be partial",
        messageFormat: "Mapping configuration class '{0}' must be declared as partial",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MethodMustBeExtensionMethodDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MethodMustBeExtensionMethod,
        title: "Method must be extension method",
        messageFormat: "Mapping method '{0}' must be an extension method (first parameter must use 'this')",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor IgnoredPropertyNotFoundDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.IgnoredPropertyNotFound,
        title: "Ignored property not found",
        messageFormat: "Ignored property '{0}' not found on source type '{1}'",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RenamedSourcePropertyNotFoundDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.RenamedSourcePropertyNotFound,
        title: "Renamed source property not found",
        messageFormat: "Source property '{0}' specified in MapConfigProperty not found on type '{1}'",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RenamedTargetPropertyNotFoundDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.RenamedTargetPropertyNotFound,
        title: "Renamed target property not found",
        messageFormat: "Target property '{0}' specified in MapConfigProperty not found on type '{1}'",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor EmptyConfigurationClassDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.EmptyConfigurationClass,
        title: "Empty configuration class",
        messageFormat: "Mapping configuration class '{0}' has no partial extension methods",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MethodReturnTypeInvalidDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MethodReturnTypeInvalid,
        title: "Return type invalid",
        messageFormat: "Mapping method '{0}' return type must be a class, record, or struct",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TypeNotResolvedDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.TypeNotResolved,
        title: "Type not resolved",
        messageFormat: "Type '{0}' could not be resolved in method '{1}'. If this type is produced by another source generator, note that source generators cannot reference types from sibling generators in the same compilation. Consider moving the type to a referenced project or pre-compiling the other generator's output.",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateAttributeAndConfigurationDescriptor = new(
        id: RuleIdentifierConstants.ObjectMappingExtended.DuplicateAttributeAndConfiguration,
        title: "Duplicate attribute and configuration mapping",
        messageFormat: "Type '{0}' has both [MapTo(typeof({1}))] attribute and configuration-based mapping. Attribute takes precedence.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateConfigurationMappingDescriptor = new(
        id: RuleIdentifierConstants.ObjectMappingExtended.DuplicateConfigurationMapping,
        title: "Duplicate configuration mapping",
        messageFormat: "Type pair '{0}' -> '{1}' is configured multiple times. First configuration is used.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConfigTargetTypeMustBeClassOrStructDescriptor = new(
        id: RuleIdentifierConstants.ObjectMappingExtended.ConfigTargetTypeMustBeClassOrStruct,
        title: "Target type must be a class or struct",
        messageFormat: "Configuration target type '{0}' must be a class or struct",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AutoDetectedEnumPartialMatchDescriptor = new(
        id: RuleIdentifierConstants.EnumMapping.AutoDetectedEnumPartialMatch,
        title: "Partial enum match",
        messageFormat: "Auto-detected enum mapping '{0}' -> '{1}': {2} of {3} values unmatched: {4}",
        category: RuleCategoryConstants.EnumMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AutoDetectedEnumNoMatchDescriptor = new(
        id: RuleIdentifierConstants.EnumMapping.AutoDetectedEnumNoMatch,
        title: "No enum match",
        messageFormat: "Enum types '{0}' and '{1}' have no matching values; falling back to cast",
        category: RuleCategoryConstants.EnumMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MapCallRequiresTypeArgumentsDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MapCallRequiresTypeArguments,
        title: "Map() requires type arguments",
        messageFormat: "Map() requires exactly two type arguments (source and target type)",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AddMappingsLambdaRequiredDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.AddMappingsLambdaRequired,
        title: "Lambda required",
        messageFormat: "AddMappings() requires a lambda expression argument",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate fallback attribute definitions
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("MappingConfigurationAttribute.g.cs", SourceText.From(GenerateMappingConfigurationAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapConfigIgnoreAttribute.g.cs", SourceText.From(GenerateMapConfigIgnoreAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapConfigPropertyAttribute.g.cs", SourceText.From(GenerateMapConfigPropertyAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapConfigOptionsAttribute.g.cs", SourceText.From(GenerateMapConfigOptionsAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapTypesAttribute.g.cs", SourceText.From(GenerateMapTypesAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MappingBuilder.g.cs", SourceText.From(GenerateMappingBuilderSource(), Encoding.UTF8));
        });

        // Find classes with [MappingConfiguration] or [MapTypes] attributes
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)
            .Collect();

        // Find AddMappings() / MappingBuilder.Configure() invocations
        var inlineInvocations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsInlineMappingInvocation(s),
                transform: static (ctx, _) => GetInlineMappingInvocation(ctx))
            .Where(static m => m is not null)
            .Collect();

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider
            .Combine(classDeclarations)
            .Combine(inlineInvocations);

        // Generate source
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) =>
            Execute(source.Left.Left, source.Left.Right!, source.Right!, spc));
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

                var attrType = attributeSymbol.ContainingType;
                if (MappingTypeAnalyzer.IsAttributeMatch(attrType, MappingConfigurationAttributeName) ||
                    MappingTypeAnalyzer.IsAttributeMatch(attrType, MapTypesAttributeName))
                {
                    return classDeclaration;
                }
            }
        }

        return null;
    }

    private static bool IsInlineMappingInvocation(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            _ => string.Empty,
        };

        return methodName is "AddMappings" or "Configure";
    }

    private static InvocationExpressionSyntax? GetInlineMappingInvocation(
        GeneratorSyntaxContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType?.ToDisplayString() ?? string.Empty;

            // Match MappingBuilder.Configure() or *.AddMappings()
            if (containingType == $"{AttributeNamespace}.MappingBuilder" && methodSymbol.Name == "Configure")
            {
                return invocation;
            }

            // AddMappings is an extension method on IServiceCollection
            // We match by name and parameter type: Action<MappingBuilder>
            if (methodSymbol.Name == "AddMappings" &&
                methodSymbol.Parameters.Length >= 1 &&
                methodSymbol.Parameters.Last().Type is INamedTypeSymbol paramType &&
                paramType.Name == "Action" &&
                paramType.TypeArguments.Length == 1 &&
                paramType.TypeArguments[0].Name == "MappingBuilder")
            {
                return invocation;
            }
        }

        // Fallback: if the method is not yet resolved (common for generated methods),
        // check syntax pattern
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var name = memberAccess.Name.Identifier.Text;
            if (name is "AddMappings" or "Configure" &&
                invocation.ArgumentList.Arguments.Count >= 1)
            {
                var arg = invocation.ArgumentList.Arguments.Last().Expression;
                if (arg is SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax)
                {
                    return invocation;
                }
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classes,
        ImmutableArray<InvocationExpressionSyntax> inlineInvocations,
        SourceProductionContext context)
    {
        var configuredMappings = new List<ConfiguredMappingInfo>();
        var autoDetectedEnums = new List<AutoDetectedEnumMapping>();

        // Process [MappingConfiguration] classes and class-level [MapTypes] classes
        if (!classes.IsDefaultOrEmpty)
        {
            // Deduplicate by fully qualified symbol name to handle partial classes across multiple files
            var seenClasses = new HashSet<string>(StringComparer.Ordinal);

            foreach (var classDeclaration in classes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is not { } classSymbol)
                {
                    continue;
                }

                if (!seenClasses.Add(classSymbol.ToDisplayString()))
                {
                    continue;
                }

                // Check if this is a [MappingConfiguration] class
                var hasMappingConfigAttr = classSymbol.GetAttributes()
                    .Any(a => MappingTypeAnalyzer.IsAttributeMatch(a.AttributeClass, MappingConfigurationAttributeName));

                if (hasMappingConfigAttr)
                {
                    var mappings = ProcessMappingConfigurationClass(classSymbol, classDeclaration, context, autoDetectedEnums);
                    if (mappings is not null)
                    {
                        configuredMappings.AddRange(mappings);
                    }
                }

                // Check if this is a class with [MapTypes] attributes
                var classMapTypesAttrs = classSymbol.GetAttributes()
                    .Where(a => MappingTypeAnalyzer.IsAttributeMatch(a.AttributeClass, MapTypesAttributeName))
                    .ToList();

                if (classMapTypesAttrs.Count > 0)
                {
                    var mappings = ProcessClassLevelMapTypesAttributes(classMapTypesAttrs, context, autoDetectedEnums);
                    if (mappings is not null)
                    {
                        configuredMappings.AddRange(mappings);
                    }
                }
            }
        }

        // Process [assembly: MapTypes] attributes
        var assemblyMappings = ProcessAssemblyMapTypesAttributes(compilation, context, autoDetectedEnums);
        if (assemblyMappings is not null)
        {
            configuredMappings.AddRange(assemblyMappings);
        }

        // Process AddMappings() / MappingBuilder.Configure() inline invocations
        if (!inlineInvocations.IsDefaultOrEmpty)
        {
            foreach (var invocation in inlineInvocations.Distinct())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var mappings = ProcessInlineMappingInvocation(invocation, compilation, context, autoDetectedEnums);
                if (mappings is not null)
                {
                    configuredMappings.AddRange(mappings);
                }
            }
        }

        // Generate AddMappings extension method if IServiceCollection is available
        GenerateAddMappingsExtension(compilation, context);

        if (configuredMappings.Count == 0)
        {
            return;
        }

        // Check for duplicates with attribute-based mappings
        DetectDuplicatesWithAttributeMappings(compilation, configuredMappings, context);

        // Deduplicate within config-based mappings
        DeduplicateConfigMappings(configuredMappings, context);

        // Generate code for mapper class pattern (partial classes)
        var mapperClassMappings = configuredMappings.Where(m => m.ContainingClass is not null).ToList();
        if (mapperClassMappings.Count > 0)
        {
            GenerateMapperClassCode(mapperClassMappings, autoDetectedEnums, context);
        }

        // Generate code for assembly-level pattern
        var assemblyLevelMappings = configuredMappings.Where(m => m.ContainingClass is null).ToList();
        if (assemblyLevelMappings.Count > 0)
        {
            GenerateAssemblyLevelCode(assemblyLevelMappings, autoDetectedEnums, context);
        }
    }

    private static List<ConfiguredMappingInfo>? ProcessMappingConfigurationClass(
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclaration,
        SourceProductionContext context,
        List<AutoDetectedEnumMapping> autoDetectedEnums)
    {
        // Validate class is static
        if (!classSymbol.IsStatic)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    ConfigClassMustBeStaticDescriptor,
                    classSymbol.Locations.FirstOrDefault() ?? Location.None,
                    classSymbol.Name));
            return null;
        }

        // Validate class is partial
        if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    ConfigClassMustBePartialDescriptor,
                    classSymbol.Locations.FirstOrDefault() ?? Location.None,
                    classSymbol.Name));
            return null;
        }

        // Find partial methods
        var partialMethods = classSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.IsPartialDefinition && !m.IsImplicitlyDeclared)
            .ToList();

        if (partialMethods.Count == 0)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    EmptyConfigurationClassDescriptor,
                    classSymbol.Locations.FirstOrDefault() ?? Location.None,
                    classSymbol.Name));
            return null;
        }

        var mappings = new List<ConfiguredMappingInfo>();

        foreach (var method in partialMethods)
        {
            var mapping = ProcessMappingMethod(method, classSymbol, context, autoDetectedEnums);
            if (mapping is not null)
            {
                mappings.Add(mapping);
            }
        }

        return mappings.Count > 0 ? mappings : null;
    }

    private static ConfiguredMappingInfo? ProcessMappingMethod(
        IMethodSymbol method,
        INamedTypeSymbol containingClass,
        SourceProductionContext context,
        List<AutoDetectedEnumMapping> autoDetectedEnums)
    {
        // Validate extension method
        if (!method.IsExtensionMethod)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodMustBeExtensionMethodDescriptor,
                    method.Locations.FirstOrDefault() ?? Location.None,
                    method.Name));
            return null;
        }

        // Check for unresolved return type (possibly from sibling source generator)
        if (!method.ReturnsVoid && method.ReturnType.TypeKind == TypeKind.Error)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    TypeNotResolvedDescriptor,
                    method.Locations.FirstOrDefault() ?? Location.None,
                    method.ReturnType.Name,
                    method.Name));
            return null;
        }

        // Validate return type
        if (method.ReturnsVoid ||
            method.ReturnType is not INamedTypeSymbol targetType ||
            (targetType.TypeKind != TypeKind.Class && targetType.TypeKind != TypeKind.Struct) ||
            targetType.SpecialType != SpecialType.None)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodReturnTypeInvalidDescriptor,
                    method.Locations.FirstOrDefault() ?? Location.None,
                    method.Name));
            return null;
        }

        // Get source type from first parameter
        var sourceType = method.Parameters[0].Type as INamedTypeSymbol;
        if (sourceType is null)
        {
            return null;
        }

        // Check for unresolved source type (possibly from sibling source generator)
        if (sourceType.TypeKind == TypeKind.Error)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    TypeNotResolvedDescriptor,
                    method.Locations.FirstOrDefault() ?? Location.None,
                    sourceType.Name,
                    method.Name));
            return null;
        }

        // Extract attributes
        var ignoredProperties = new List<string>();
        var propertyRenames = new List<(string Source, string Target)>();
        var bidirectional = false;
        var enableFlattening = false;
        var propertyNameStrategy = PropertyNameStrategy.PascalCase;

        foreach (var attr in method.GetAttributes())
        {
            if (MappingTypeAnalyzer.IsAttributeMatch(attr.AttributeClass, MapConfigIgnoreAttributeName))
            {
                if (attr.ConstructorArguments.Length > 0 &&
                    attr.ConstructorArguments[0].Value is string propName)
                {
                    ignoredProperties.Add(propName);
                }
            }
            else if (MappingTypeAnalyzer.IsAttributeMatch(attr.AttributeClass, MapConfigPropertyAttributeName))
            {
                if (attr.ConstructorArguments.Length >= 2 &&
                    attr.ConstructorArguments[0].Value is string sourcePropName &&
                    attr.ConstructorArguments[1].Value is string targetPropName)
                {
                    propertyRenames.Add((sourcePropName, targetPropName));
                }
            }
            else if (MappingTypeAnalyzer.IsAttributeMatch(attr.AttributeClass, MapConfigOptionsAttributeName))
            {
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "Bidirectional")
                    {
                        bidirectional = namedArg.Value.Value as bool? ?? false;
                    }
                    else if (namedArg.Key == "EnableFlattening")
                    {
                        enableFlattening = namedArg.Value.Value as bool? ?? false;
                    }
                    else if (namedArg.Key == "PropertyNameStrategy" &&
                             namedArg.Value.Value is int strategyValue)
                    {
                        propertyNameStrategy = (PropertyNameStrategy)strategyValue;
                    }
                }
            }
        }

        // Validate ignored properties exist on source type
        var sourceProperties = GetAllProperties(sourceType, includePrivateMembers: false);
        foreach (var ignoredProp in ignoredProperties)
        {
            if (!sourceProperties.Any(p => string.Equals(p.Name, ignoredProp, StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        IgnoredPropertyNotFoundDescriptor,
                        method.Locations.FirstOrDefault() ?? Location.None,
                        ignoredProp,
                        sourceType.Name));
            }
        }

        // Validate renamed properties exist
        var targetProperties = GetAllProperties(targetType, includePrivateMembers: false);
        foreach (var rename in propertyRenames)
        {
            if (!sourceProperties.Any(p => string.Equals(p.Name, rename.Source, StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        RenamedSourcePropertyNotFoundDescriptor,
                        method.Locations.FirstOrDefault() ?? Location.None,
                        rename.Source,
                        sourceType.Name));
            }

            if (!targetProperties.Any(p => string.Equals(p.Name, rename.Target, StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        RenamedTargetPropertyNotFoundDescriptor,
                        method.Locations.FirstOrDefault() ?? Location.None,
                        rename.Target,
                        targetType.Name));
            }
        }

        // Get property mappings with ignores and renames applied
        var propertyMappings = GetPropertyMappingsWithConfig(
            sourceType,
            targetType,
            enableFlattening,
            propertyNameStrategy,
            ignoredProperties,
            propertyRenames,
            autoDetectedEnums,
            context);

        // Find best matching constructor
        var (constructor, constructorParameterNames, constructorDefaultParameters) = FindBestConstructor(sourceType, targetType, propertyRenames);

        return new ConfiguredMappingInfo(
            SourceType: sourceType,
            TargetType: targetType,
            Method: method,
            ContainingClass: containingClass,
            PropertyMappings: propertyMappings,
            Bidirectional: bidirectional,
            EnableFlattening: enableFlattening,
            Constructor: constructor,
            ConstructorParameterNames: constructorParameterNames,
            ConstructorDefaultParameters: constructorDefaultParameters,
            PropertyNameStrategy: propertyNameStrategy,
            IgnoredProperties: ignoredProperties,
            PropertyRenames: propertyRenames);
    }

    private static List<ConfiguredMappingInfo>? ProcessClassLevelMapTypesAttributes(
        List<AttributeData> mapTypesAttrs,
        SourceProductionContext context,
        List<AutoDetectedEnumMapping> autoDetectedEnums)
    {
        var mappings = new List<ConfiguredMappingInfo>();

        foreach (var attr in mapTypesAttrs)
        {
            var mapping = ExtractMapTypesMapping(attr, context, autoDetectedEnums);
            if (mapping is not null)
            {
                mappings.Add(mapping);
            }
        }

        return mappings.Count > 0 ? mappings : null;
    }

    private static List<ConfiguredMappingInfo>? ProcessInlineMappingInvocation(
        InvocationExpressionSyntax invocation,
        Compilation compilation,
        SourceProductionContext context,
        List<AutoDetectedEnumMapping> autoDetectedEnums)
    {
        // Extract the lambda argument
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count == 0)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    AddMappingsLambdaRequiredDescriptor,
                    invocation.GetLocation()));
            return null;
        }

        var lambdaArg = arguments.Last().Expression;
        SyntaxNode? lambdaBody = lambdaArg switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Body,
            ParenthesizedLambdaExpressionSyntax parenLambda => parenLambda.Body,
            _ => null,
        };

        if (lambdaBody is null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    AddMappingsLambdaRequiredDescriptor,
                    invocation.GetLocation()));
            return null;
        }

        var semanticModel = compilation.GetSemanticModel(invocation.SyntaxTree);
        var mappings = new List<ConfiguredMappingInfo>();

        // Find all Map() calls inside the lambda body
        var mapCalls = lambdaBody.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var mapCall in mapCalls)
        {
            var methodName = mapCall.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                _ => null,
            };

            if (methodName is null || methodName.Identifier.Text != "Map")
            {
                continue;
            }

            var mapping = ParseMapCallInvocation(mapCall, methodName, semanticModel, context, autoDetectedEnums);
            if (mapping is not null)
            {
                mappings.Add(mapping);
            }
        }

        return mappings.Count > 0 ? mappings : null;
    }

    private static ConfiguredMappingInfo? ParseMapCallInvocation(
        InvocationExpressionSyntax mapCall,
        SimpleNameSyntax methodName,
        SemanticModel semanticModel,
        SourceProductionContext context,
        List<AutoDetectedEnumMapping> autoDetectedEnums)
    {
        INamedTypeSymbol? sourceType = null;
        INamedTypeSymbol? targetType = null;

        // Check for generic Map<TSource, TTarget>() syntax
        if (methodName is GenericNameSyntax genericName && genericName.TypeArgumentList.Arguments.Count == 2)
        {
            var sourceTypeInfo = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[0]);
            var targetTypeInfo = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[1]);

            sourceType = sourceTypeInfo.Type as INamedTypeSymbol;
            targetType = targetTypeInfo.Type as INamedTypeSymbol;
        }
        else
        {
            // Check for Map(typeof(Source), typeof(Target)) syntax
            var args = mapCall.ArgumentList.Arguments;
            var positionalArgs = args.Where(a => a.NameColon is null).ToList();

            if (positionalArgs.Count >= 2)
            {
                sourceType = ExtractTypeFromTypeOfExpression(positionalArgs[0].Expression, semanticModel);
                targetType = ExtractTypeFromTypeOfExpression(positionalArgs[1].Expression, semanticModel);
            }
        }

        if (sourceType is null || targetType is null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MapCallRequiresTypeArgumentsDescriptor,
                    mapCall.GetLocation()));
            return null;
        }

        // Check for unresolved types (possibly from sibling source generator)
        if (sourceType.TypeKind == TypeKind.Error || targetType.TypeKind == TypeKind.Error)
        {
            var unresolvedType = sourceType.TypeKind == TypeKind.Error ? sourceType : targetType;
            context.ReportDiagnostic(
                Diagnostic.Create(
                    TypeNotResolvedDescriptor,
                    mapCall.GetLocation(),
                    unresolvedType.Name,
                    "Map"));
            return null;
        }

        // Validate target type
        if (targetType.TypeKind != TypeKind.Class && targetType.TypeKind != TypeKind.Struct)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    ConfigTargetTypeMustBeClassOrStructDescriptor,
                    mapCall.GetLocation(),
                    targetType.Name));
            return null;
        }

        // Extract named arguments
        var bidirectional = false;
        var propertyNameStrategy = PropertyNameStrategy.PascalCase;
        var ignoredProperties = new List<string>();
        var propertyRenames = new List<(string Source, string Target)>();

        foreach (var arg in mapCall.ArgumentList.Arguments)
        {
            if (arg.NameColon is null)
            {
                continue;
            }

            var argName = arg.NameColon.Name.Identifier.Text;

            if (argName == "bidirectional" && arg.Expression is LiteralExpressionSyntax bidirectionalLiteral)
            {
                bidirectional = bidirectionalLiteral.IsKind(SyntaxKind.TrueLiteralExpression);
            }
            else if (argName == "propertyMap")
            {
                var values = ExtractStringArrayFromExpression(arg.Expression);
                foreach (var mapping in values)
                {
                    var parts = mapping.Split(':');
                    if (parts.Length == 2)
                    {
                        propertyRenames.Add((parts[0].Trim(), parts[1].Trim()));
                    }
                }
            }
            else if (argName == "ignoreSourceProperties")
            {
                ignoredProperties.AddRange(ExtractStringArrayFromExpression(arg.Expression));
            }
            else if (argName == "propertyNameStrategy" && arg.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var strategyName = memberAccess.Name.Identifier.Text;
                if (Enum.TryParse<PropertyNameStrategy>(strategyName, out var strategy))
                {
                    propertyNameStrategy = strategy;
                }
            }
        }

        // Get property mappings
        var propertyMappings = GetPropertyMappingsWithConfig(
            sourceType,
            targetType,
            enableFlattening: false,
            propertyNameStrategy,
            ignoredProperties,
            propertyRenames,
            autoDetectedEnums,
            context);

        // Find best matching constructor
        var (constructor, constructorParameterNames, constructorDefaultParameters) = FindBestConstructor(sourceType, targetType, propertyRenames);

        return new ConfiguredMappingInfo(
            SourceType: sourceType,
            TargetType: targetType,
            Method: null,
            ContainingClass: null,
            PropertyMappings: propertyMappings,
            Bidirectional: bidirectional,
            EnableFlattening: false,
            Constructor: constructor,
            ConstructorParameterNames: constructorParameterNames,
            ConstructorDefaultParameters: constructorDefaultParameters,
            PropertyNameStrategy: propertyNameStrategy,
            IgnoredProperties: ignoredProperties,
            PropertyRenames: propertyRenames);
    }

    private static INamedTypeSymbol? ExtractTypeFromTypeOfExpression(
        ExpressionSyntax expression,
        SemanticModel semanticModel)
    {
        if (expression is TypeOfExpressionSyntax typeOfExpression)
        {
            var typeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type);
            return typeInfo.Type as INamedTypeSymbol;
        }

        return null;
    }

    private static List<string> ExtractStringArrayFromExpression(
        ExpressionSyntax expression)
    {
        var values = new List<string>();

        // Handle: new[] { "A", "B" } or new string[] { "A", "B" }
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            foreach (var expr in implicitArray.Initializer.Expressions)
            {
                if (expr is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    values.Add(literal.Token.ValueText);
                }
            }
        }
        else if (expression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer is not null)
        {
            foreach (var expr in arrayCreation.Initializer.Expressions)
            {
                if (expr is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    values.Add(literal.Token.ValueText);
                }
            }
        }
        else if (expression is CollectionExpressionSyntax collectionExpression)
        {
            foreach (var element in collectionExpression.Elements)
            {
                if (element is ExpressionElementSyntax exprElement &&
                    exprElement.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    values.Add(literal.Token.ValueText);
                }
            }
        }

        return values;
    }

    private static ConfiguredMappingInfo? ExtractMapTypesMapping(
        AttributeData attr,
        SourceProductionContext context,
        List<AutoDetectedEnumMapping> autoDetectedEnums)
    {
        if (attr.ConstructorArguments.Length < 2)
        {
            return null;
        }

        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol sourceType)
        {
            return null;
        }

        if (attr.ConstructorArguments[1].Value is not INamedTypeSymbol targetType)
        {
            return null;
        }

        // Check for unresolved types (possibly from sibling source generator)
        if (sourceType.TypeKind == TypeKind.Error)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    TypeNotResolvedDescriptor,
                    attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                    sourceType.Name,
                    "MapTypes"));
            return null;
        }

        if (targetType.TypeKind == TypeKind.Error)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    TypeNotResolvedDescriptor,
                    attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                    targetType.Name,
                    "MapTypes"));
            return null;
        }

        // Validate target type
        if (targetType.TypeKind != TypeKind.Class && targetType.TypeKind != TypeKind.Struct)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    ConfigTargetTypeMustBeClassOrStructDescriptor,
                    attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                    targetType.Name));
            return null;
        }

        // Extract named arguments
        var ignoredProperties = new List<string>();
        var propertyRenames = new List<(string Source, string Target)>();
        var bidirectional = false;
        var propertyNameStrategy = PropertyNameStrategy.PascalCase;

        foreach (var namedArg in attr.NamedArguments)
        {
            if (namedArg.Key == "IgnoreSourceProperties" && namedArg.Value.Values.Length > 0)
            {
                foreach (var val in namedArg.Value.Values)
                {
                    if (val.Value is string propName)
                    {
                        ignoredProperties.Add(propName);
                    }
                }
            }
            else if (namedArg.Key == "PropertyMap" && namedArg.Value.Values.Length > 0)
            {
                foreach (var val in namedArg.Value.Values)
                {
                    if (val.Value is string mapping)
                    {
                        var parts = mapping.Split(':');
                        if (parts.Length == 2)
                        {
                            propertyRenames.Add((parts[0].Trim(), parts[1].Trim()));
                        }
                    }
                }
            }
            else if (namedArg.Key == "Bidirectional")
            {
                bidirectional = namedArg.Value.Value as bool? ?? false;
            }
            else if (namedArg.Key == "PropertyNameStrategy" &&
                     namedArg.Value.Value is int strategyValue)
            {
                propertyNameStrategy = (PropertyNameStrategy)strategyValue;
            }
        }

        // Get property mappings
        var propertyMappings = GetPropertyMappingsWithConfig(
            sourceType,
            targetType,
            enableFlattening: false,
            propertyNameStrategy,
            ignoredProperties,
            propertyRenames,
            autoDetectedEnums,
            context);

        // Find best matching constructor
        var (constructor, constructorParameterNames, constructorDefaultParameters) = FindBestConstructor(sourceType, targetType, propertyRenames);

        return new ConfiguredMappingInfo(
            SourceType: sourceType,
            TargetType: targetType,
            Method: null,
            ContainingClass: null,
            PropertyMappings: propertyMappings,
            Bidirectional: bidirectional,
            EnableFlattening: false,
            Constructor: constructor,
            ConstructorParameterNames: constructorParameterNames,
            ConstructorDefaultParameters: constructorDefaultParameters,
            PropertyNameStrategy: propertyNameStrategy,
            IgnoredProperties: ignoredProperties,
            PropertyRenames: propertyRenames);
    }

    private static void GenerateAddMappingsExtension(
        Compilation compilation,
        SourceProductionContext context)
    {
        // Only generate if IServiceCollection is available in the compilation
        var serviceCollectionType = compilation.GetTypeByMetadataName(
            "Microsoft.Extensions.DependencyInjection.IServiceCollection");

        if (serviceCollectionType is null)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLineLf("// <auto-generated/>");
        sb.AppendLineLf("#nullable enable");
        sb.AppendLineLf();
        sb.AppendLineLf("namespace Microsoft.Extensions.DependencyInjection;");
        sb.AppendLineLf();
        sb.AppendLineLf("[global::System.CodeDom.Compiler.GeneratedCode(\"Atc.SourceGenerators.MappingConfiguration\", \"1.0.0\")]");
        sb.AppendLineLf("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLineLf("[global::System.Runtime.CompilerServices.CompilerGenerated]");
        sb.AppendLineLf("[global::System.Diagnostics.DebuggerNonUserCode]");
        sb.AppendLineLf("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        sb.AppendLineLf("public static class AtcMappingServiceCollectionExtensions");
        sb.AppendLineLf("{");
        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf("    /// Registers compile-time mapping configurations. The lambda body is analyzed by the source generator");
        sb.AppendLineLf("    /// to produce mapping extension methods. At runtime, this is a no-op.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    public static IServiceCollection AddMappings(");
        sb.AppendLineLf("        this IServiceCollection services,");
        sb.AppendLineLf("        global::System.Action<global::Atc.SourceGenerators.Annotations.MappingBuilder> configure)");
        sb.AppendLineLf("        => services;");
        sb.AppendLineLf("}");

        context.AddSource("AtcMappingServiceCollectionExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static List<ConfiguredMappingInfo>? ProcessAssemblyMapTypesAttributes(
        Compilation compilation,
        SourceProductionContext context,
        List<AutoDetectedEnumMapping> autoDetectedEnums)
    {
        var mappings = new List<ConfiguredMappingInfo>();

        var assemblyAttributes = compilation.Assembly
            .GetAttributes()
            .Where(a => MappingTypeAnalyzer.IsAttributeMatch(a.AttributeClass, MapTypesAttributeName));

        foreach (var attr in assemblyAttributes)
        {
            var mapping = ExtractMapTypesMapping(attr, context, autoDetectedEnums);
            if (mapping is not null)
            {
                mappings.Add(mapping);
            }
        }

        return mappings.Count > 0 ? mappings : null;
    }

    private static List<PropertyMapping> GetPropertyMappingsWithConfig(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType,
        bool enableFlattening,
        PropertyNameStrategy propertyNameStrategy,
        List<string> ignoredProperties,
        List<(string Source, string Target)> propertyRenames,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        var mappings = new List<PropertyMapping>();

        var sourceProperties = GetAllProperties(sourceType, includePrivateMembers: false);
        var targetProperties = GetAllProperties(targetType, includePrivateMembers: false);

        foreach (var sourceProp in sourceProperties)
        {
            // Check if property is ignored
            if (ignoredProperties.Any(ip =>
                string.Equals(ip, sourceProp.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            // Check if property has custom rename
            var rename = propertyRenames.FirstOrDefault(r =>
                string.Equals(r.Source, sourceProp.Name, StringComparison.OrdinalIgnoreCase));

            var targetPropertyName = rename.Target ??
                PropertyNameUtility.ConvertPropertyName(sourceProp.Name, propertyNameStrategy);

            // Find target property
            var targetProp = targetProperties.FirstOrDefault(t =>
                string.Equals(t.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase) &&
                SymbolEqualityComparer.Default.Equals(t.Type, sourceProp.Type));

            if (targetProp is not null)
            {
                // Direct type match
                mappings.Add(new PropertyMapping(
                    SourceProperty: sourceProp,
                    TargetProperty: targetProp,
                    RequiresConversion: false,
                    IsNested: false,
                    HasEnumMapping: false,
                    IsCollection: false,
                    CollectionElementType: null,
                    SourceCollectionElementType: null,
                    CollectionTargetType: null,
                    IsFlattened: false,
                    FlattenedNestedProperty: null,
                    IsBuiltInTypeConversion: false,
                    SourceRequiresUnsafeAccessor: false,
                    TargetRequiresUnsafeAccessor: false));
            }
            else
            {
                // Check for different but mappable types
                targetProp = targetProperties.FirstOrDefault(t =>
                    string.Equals(t.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase));

                if (targetProp is not null)
                {
                    // Check for collection mapping
                    var isSourceCollection = MappingTypeAnalyzer.IsCollectionType(sourceProp.Type, out var sourceElementType);
                    var isTargetCollection = MappingTypeAnalyzer.IsCollectionType(targetProp.Type, out var targetElementType);

                    if (isSourceCollection && isTargetCollection && sourceElementType is not null && targetElementType is not null)
                    {
                        // Auto-detect enum mapping for collection element types
                        var hasCollectionEnumMapping = false;
                        if (MappingTypeAnalyzer.IsEnumConversion(sourceElementType, targetElementType))
                        {
                            hasCollectionEnumMapping = HasEnumMappingAttribute(sourceElementType, targetElementType);

                            if (!hasCollectionEnumMapping &&
                                sourceElementType is INamedTypeSymbol sourceElementEnumType &&
                                targetElementType is INamedTypeSymbol targetElementEnumType)
                            {
                                hasCollectionEnumMapping = TryAutoDetectEnumMapping(
                                    sourceElementEnumType, targetElementEnumType, autoDetectedEnums, context);
                            }
                        }

                        mappings.Add(new PropertyMapping(
                            SourceProperty: sourceProp,
                            TargetProperty: targetProp,
                            RequiresConversion: false,
                            IsNested: false,
                            HasEnumMapping: hasCollectionEnumMapping,
                            IsCollection: true,
                            CollectionElementType: targetElementType,
                            SourceCollectionElementType: sourceElementType,
                            CollectionTargetType: MappingTypeAnalyzer.GetCollectionTargetType(targetProp.Type),
                            IsFlattened: false,
                            FlattenedNestedProperty: null,
                            IsBuiltInTypeConversion: false,
                            SourceRequiresUnsafeAccessor: false,
                            TargetRequiresUnsafeAccessor: false));
                    }
                    else
                    {
                        var requiresConversion = MappingTypeAnalyzer.IsEnumConversion(sourceProp.Type, targetProp.Type);
                        var isNested = MappingTypeAnalyzer.IsNestedMapping(sourceProp.Type, targetProp.Type);
                        var isBuiltInTypeConversion = MappingTypeAnalyzer.IsBuiltInTypeConversion(sourceProp.Type, targetProp.Type);
                        var isNumericConversion = MappingTypeAnalyzer.IsNumericTypeConversion(sourceProp.Type, targetProp.Type);

                        // Auto-detect enum mapping (unwrap nullable for enum checks)
                        var hasEnumMapping = false;
                        if (requiresConversion)
                        {
                            var unwrappedSourceType = MappingTypeAnalyzer.UnwrapNullable(sourceProp.Type);
                            var unwrappedTargetType = MappingTypeAnalyzer.UnwrapNullable(targetProp.Type);
                            hasEnumMapping = HasEnumMappingAttribute(unwrappedSourceType, unwrappedTargetType);

                            // If no explicit mapping, try auto-detection
                            if (!hasEnumMapping &&
                                unwrappedSourceType is INamedTypeSymbol sourceEnumType &&
                                unwrappedTargetType is INamedTypeSymbol targetEnumType)
                            {
                                var autoMapping = TryAutoDetectEnumMapping(
                                    sourceEnumType, targetEnumType, autoDetectedEnums, context);
                                hasEnumMapping = autoMapping;
                            }
                        }

                        // Built-in type conversions take precedence over nested mapping detection
                        if (isBuiltInTypeConversion)
                        {
                            isNested = false;
                        }

                        if (requiresConversion || isNested || isBuiltInTypeConversion || isNumericConversion)
                        {
                            mappings.Add(new PropertyMapping(
                                SourceProperty: sourceProp,
                                TargetProperty: targetProp,
                                RequiresConversion: requiresConversion || isNumericConversion,
                                IsNested: isNested,
                                HasEnumMapping: hasEnumMapping,
                                IsCollection: false,
                                CollectionElementType: null,
                                SourceCollectionElementType: null,
                                CollectionTargetType: null,
                                IsFlattened: false,
                                FlattenedNestedProperty: null,
                                IsBuiltInTypeConversion: isBuiltInTypeConversion,
                                SourceRequiresUnsafeAccessor: false,
                                TargetRequiresUnsafeAccessor: false));
                        }
                    }
                }
            }
        }

        // Handle flattening if enabled
        if (enableFlattening)
        {
            foreach (var sourceProp in sourceProperties)
            {
                if (ignoredProperties.Any(ip =>
                    string.Equals(ip, sourceProp.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (sourceProp.Type is not INamedTypeSymbol namedType ||
                    (namedType.TypeKind != TypeKind.Class && namedType.TypeKind != TypeKind.Struct))
                {
                    continue;
                }

                var typeStr = namedType.SpecialType.ToString();
                if (typeStr.StartsWith("System", StringComparison.Ordinal))
                {
                    continue;
                }

                var nestedProperties = namedType
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => p.GetMethod is not null)
                    .ToList();

                foreach (var nestedProp in nestedProperties)
                {
                    var flattenedName = $"{sourceProp.Name}{nestedProp.Name}";
                    var targetProp = targetProperties.FirstOrDefault(t =>
                        string.Equals(t.Name, flattenedName, StringComparison.OrdinalIgnoreCase) &&
                        SymbolEqualityComparer.Default.Equals(t.Type, nestedProp.Type));

                    if (targetProp is not null)
                    {
                        mappings.Add(new PropertyMapping(
                            SourceProperty: sourceProp,
                            TargetProperty: targetProp,
                            RequiresConversion: false,
                            IsNested: false,
                            HasEnumMapping: false,
                            IsCollection: false,
                            CollectionElementType: null,
                            SourceCollectionElementType: null,
                            CollectionTargetType: null,
                            IsFlattened: true,
                            FlattenedNestedProperty: nestedProp,
                            IsBuiltInTypeConversion: false,
                            SourceRequiresUnsafeAccessor: false,
                            TargetRequiresUnsafeAccessor: false));
                    }
                }
            }
        }

        return mappings;
    }

    private static bool TryAutoDetectEnumMapping(
        INamedTypeSymbol sourceEnum,
        INamedTypeSymbol targetEnum,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        // Check if already auto-detected
        if (autoDetectedEnums.Any(m =>
            SymbolEqualityComparer.Default.Equals(m.SourceEnum, sourceEnum) &&
            SymbolEqualityComparer.Default.Equals(m.TargetEnum, targetEnum)))
        {
            return true;
        }

        var valueMappings = EnumMappingHelper.GetEnumValueMappings(sourceEnum, targetEnum);
        var mappedCount = valueMappings.Count(m => m.IsMapped);
        var totalCount = valueMappings.Count;

        if (mappedCount == 0)
        {
            // No matches at all - fall back to cast
            context.ReportDiagnostic(
                Diagnostic.Create(
                    AutoDetectedEnumNoMatchDescriptor,
                    sourceEnum.Locations.FirstOrDefault() ?? Location.None,
                    sourceEnum.Name,
                    targetEnum.Name));
            return false;
        }

        if (mappedCount < totalCount)
        {
            // Partial match
            var unmappedValues = valueMappings
                .Where(m => !m.IsMapped)
                .Select(m => m.SourceValue);
            var unmappedCount = totalCount - mappedCount;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    AutoDetectedEnumPartialMatchDescriptor,
                    sourceEnum.Locations.FirstOrDefault() ?? Location.None,
                    sourceEnum.Name,
                    targetEnum.Name,
                    unmappedCount,
                    totalCount,
                    string.Join(", ", unmappedValues)));
        }

        // Add to auto-detected list
        autoDetectedEnums.Add(new AutoDetectedEnumMapping(
            SourceEnum: sourceEnum,
            TargetEnum: targetEnum,
            ValueMappings: valueMappings));

        return true;
    }

    private static List<IPropertySymbol> GetAllProperties(
        INamedTypeSymbol type,
        bool includePrivateMembers,
        bool requireSetter = false)
    {
        var properties = new List<IPropertySymbol>();
        var propertyNames = new HashSet<string>(StringComparer.Ordinal);
        var currentType = type;

        while (currentType is not null &&
               currentType.SpecialType != SpecialType.System_Object &&
               currentType.ToDisplayString() != "object")
        {
            var typeProperties = currentType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p =>
                {
                    if (p.GetMethod is null)
                    {
                        return false;
                    }

                    if (requireSetter && p.SetMethod is null && currentType.TypeKind != TypeKind.Struct)
                    {
                        return false;
                    }

                    if (MappingTypeAnalyzer.HasMapIgnoreAttribute(p))
                    {
                        return false;
                    }

                    if (!includePrivateMembers && p.DeclaredAccessibility != Accessibility.Public)
                    {
                        return false;
                    }

                    return true;
                })
                .ToList();

            foreach (var prop in typeProperties)
            {
                if (!propertyNames.Contains(prop.Name))
                {
                    properties.Add(prop);
                    propertyNames.Add(prop.Name);
                }
            }

            currentType = currentType.BaseType;
        }

        return properties;
    }

    private static bool HasEnumMappingAttribute(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (sourceType is not INamedTypeSymbol sourceEnum || targetType is not INamedTypeSymbol targetEnum)
        {
            return false;
        }

        // Check if source enum has [MapTo(typeof(TargetEnum))]
        foreach (var attr in sourceEnum.GetAttributes())
        {
            if (!MappingTypeAnalyzer.IsAttributeMatch(attr.AttributeClass, "MapToAttribute"))
            {
                continue;
            }

            if (attr.ConstructorArguments.Length == 0)
            {
                continue;
            }

            if (attr.ConstructorArguments[0].Value is INamedTypeSymbol attrTargetType &&
                SymbolEqualityComparer.Default.Equals(attrTargetType, targetEnum))
            {
                return true;
            }
        }

        // Check if target enum has [MapTo(typeof(SourceEnum), Bidirectional = true)]
        foreach (var attr in targetEnum.GetAttributes())
        {
            if (!MappingTypeAnalyzer.IsAttributeMatch(attr.AttributeClass, "MapToAttribute"))
            {
                continue;
            }

            if (attr.ConstructorArguments.Length == 0)
            {
                continue;
            }

            if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol attrSourceType ||
                !SymbolEqualityComparer.Default.Equals(attrSourceType, sourceEnum))
            {
                continue;
            }

            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg is { Key: "Bidirectional", Value.Value: true })
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static (IMethodSymbol? Constructor, List<string> ParameterNames, List<ConstructorDefaultParameter> DefaultParameters) FindBestConstructor(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType,
        List<(string Source, string Target)>? propertyRenames = null)
    {
        var constructors = targetType
            .Constructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic)
            .ToList();

        if (constructors.Count == 0)
        {
            return (null, [], []);
        }

        var sourceProperties = GetAllProperties(sourceType, includePrivateMembers: false);

        // First pass: try exact match (all parameters have matching source properties)
        foreach (var constructor in constructors.OrderByDescending(c => c.Parameters.Length))
        {
            var parameterNames = new List<string>();
            var allParametersMatch = true;

            foreach (var parameter in constructor.Parameters)
            {
                var matchingSourceProperty = FindSourcePropertyForParameter(
                    sourceProperties, parameter.Name, propertyRenames);

                if (matchingSourceProperty is null)
                {
                    allParametersMatch = false;
                    break;
                }

                // Verify type compatibility: reject clear mismatches (e.g., string vs int)
                // Allow custom type mismatches since mapping methods may bridge the gap
                // (e.g., Address → AddressDto, StatusEnum → StatusEnumDto)
                var sourcePropertyType = matchingSourceProperty.Type;
                var parameterType = parameter.Type;
                if (!SymbolEqualityComparer.Default.Equals(sourcePropertyType, parameterType) &&
                    !MappingTypeAnalyzer.IsImplicitlyConvertible(sourcePropertyType, parameterType) &&
                    !AreBothCustomOrEnumTypes(sourcePropertyType, parameterType) &&
                    !MappingTypeAnalyzer.IsNumericTypeConversion(sourcePropertyType, parameterType) &&
                    !MappingTypeAnalyzer.IsBuiltInTypeConversion(sourcePropertyType, parameterType) &&
                    !AreBothCollectionTypesWithCompatibleElements(sourcePropertyType, parameterType))
                {
                    allParametersMatch = false;
                    break;
                }

                parameterNames.Add(parameter.Name);
            }

            if (allParametersMatch && constructor.Parameters.Length > 0)
            {
                return (constructor, parameterNames, []);
            }
        }

        // Second pass: partial match for types without parameterless constructor
        // (e.g., positional records). Fill unmatched parameters with defaults.
        var hasParameterlessConstructor = constructors.Any(c => c.Parameters.Length == 0);
        if (!hasParameterlessConstructor)
        {
            foreach (var constructor in constructors.OrderByDescending(c => c.Parameters.Length))
            {
                var matchedNames = new List<string>();
                var defaultParams = new List<ConstructorDefaultParameter>();
                var hasIncompatibleParam = false;

                foreach (var parameter in constructor.Parameters)
                {
                    var matchingSourceProperty = FindSourcePropertyForParameter(
                        sourceProperties, parameter.Name, propertyRenames);

                    if (matchingSourceProperty is null)
                    {
                        // Unmatched parameter: will use default value
                        defaultParams.Add(new ConstructorDefaultParameter(
                            parameter.Name,
                            parameter.Type,
                            GetDefaultExpression(parameter.Type)));
                        continue;
                    }

                    var sourcePropertyType = matchingSourceProperty.Type;
                    var parameterType = parameter.Type;
                    if (!SymbolEqualityComparer.Default.Equals(sourcePropertyType, parameterType) &&
                        !MappingTypeAnalyzer.IsImplicitlyConvertible(sourcePropertyType, parameterType) &&
                        !AreBothCustomOrEnumTypes(sourcePropertyType, parameterType) &&
                        !MappingTypeAnalyzer.IsNumericTypeConversion(sourcePropertyType, parameterType) &&
                        !AreBothCollectionTypesWithCompatibleElements(sourcePropertyType, parameterType))
                    {
                        hasIncompatibleParam = true;
                        break;
                    }

                    matchedNames.Add(parameter.Name);
                }

                // Accept if at least one parameter matched and no type incompatibilities
                if (!hasIncompatibleParam && matchedNames.Count > 0)
                {
                    return (constructor, matchedNames, defaultParams);
                }
            }
        }

        return (null, [], []);
    }

    private static string GetDefaultExpression(ITypeSymbol type)
    {
        // Nullable reference type (string?, object?, etc.)
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return "null";
        }

        // Nullable value type (int?, DateTime?, etc.)
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T })
        {
            return "null";
        }

        // Value types (int, bool, enums, structs)
        if (type.IsValueType)
        {
            return "default";
        }

        // Non-nullable reference types
        return "default!";
    }

    private static IPropertySymbol? FindSourcePropertyForParameter(
        List<IPropertySymbol> sourceProperties,
        string parameterName,
        List<(string Source, string Target)>? propertyRenames)
    {
        // Direct name match
        var match = sourceProperties.FirstOrDefault(p =>
            string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
        {
            return match;
        }

        // Check property renames: parameterName matches a rename Target → find source by rename Source
        if (propertyRenames is not null)
        {
            var rename = propertyRenames.FirstOrDefault(r =>
                string.Equals(r.Target, parameterName, StringComparison.OrdinalIgnoreCase));

            if (rename != default)
            {
                return sourceProperties.FirstOrDefault(p =>
                    string.Equals(p.Name, rename.Source, StringComparison.OrdinalIgnoreCase));
            }
        }

        return null;
    }

    private static bool AreBothCustomOrEnumTypes(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (sourceType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } ns)
        {
            sourceType = ns.TypeArguments[0];
        }

        if (targetType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nt)
        {
            targetType = nt.TypeArguments[0];
        }

        if (sourceType.TypeKind == TypeKind.Enum && targetType.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        var isSourceCustom = sourceType.TypeKind is TypeKind.Class or TypeKind.Struct &&
                             sourceType.SpecialType == SpecialType.None;
        var isTargetCustom = targetType.TypeKind is TypeKind.Class or TypeKind.Struct &&
                             targetType.SpecialType == SpecialType.None;

        return isSourceCustom && isTargetCustom;
    }

    private static bool AreBothCollectionTypesWithCompatibleElements(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (!MappingTypeAnalyzer.IsCollectionType(sourceType, out var sourceElement) ||
            !MappingTypeAnalyzer.IsCollectionType(targetType, out var targetElement))
        {
            return false;
        }

        if (sourceElement is null || targetElement is null)
        {
            return false;
        }

        // Same element type
        if (SymbolEqualityComparer.Default.Equals(sourceElement, targetElement))
        {
            return true;
        }

        // Compatible element types (custom/enum types that can be mapped)
        return AreBothCustomOrEnumTypes(sourceElement, targetElement);
    }

    private static void DetectDuplicatesWithAttributeMappings(
        Compilation compilation,
        List<ConfiguredMappingInfo> configuredMappings,
        SourceProductionContext context)
    {
        // Scan for all types with [MapTo] attribute in the compilation
        var attributeMappedTypes = new HashSet<(string Source, string Target)>(
            new TypePairComparer());

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol typeSymbol)
                {
                    continue;
                }

                foreach (var attr in typeSymbol.GetAttributes())
                {
                    if (!MappingTypeAnalyzer.IsAttributeMatch(attr.AttributeClass, "MapToAttribute"))
                    {
                        continue;
                    }

                    if (attr.ConstructorArguments.Length > 0 &&
                        attr.ConstructorArguments[0].Value is INamedTypeSymbol targetType)
                    {
                        attributeMappedTypes.Add((
                            typeSymbol.ToDisplayString(),
                            targetType.ToDisplayString()));
                    }
                }
            }
        }

        // Check config mappings against attribute mappings
        foreach (var mapping in configuredMappings)
        {
            var sourceName = mapping.SourceType.ToDisplayString();
            var targetName = mapping.TargetType.ToDisplayString();

            if (attributeMappedTypes.Contains((sourceName, targetName)))
            {
                var location = mapping.Method?.Locations.First() ?? Location.None;
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DuplicateAttributeAndConfigurationDescriptor,
                        location,
                        sourceName,
                        targetName));
            }
        }
    }

    private static void DeduplicateConfigMappings(
        List<ConfiguredMappingInfo> configuredMappings,
        SourceProductionContext context)
    {
        var seen = new HashSet<(string Source, string Target)>(new TypePairComparer());
        var toRemove = new List<ConfiguredMappingInfo>();

        foreach (var mapping in configuredMappings)
        {
            var key = (mapping.SourceType.ToDisplayString(), mapping.TargetType.ToDisplayString());
            if (!seen.Add(key))
            {
                var location = mapping.Method?.Locations.First() ?? Location.None;
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DuplicateConfigurationMappingDescriptor,
                        location,
                        key.Item1,
                        key.Item2));
                toRemove.Add(mapping);
            }
        }

        foreach (var item in toRemove)
        {
            configuredMappings.Remove(item);
        }
    }

    private sealed class TypePairComparer : IEqualityComparer<(string Source, string Target)>
    {
        public bool Equals(
            (string Source, string Target) x,
            (string Source, string Target) y)
            => string.Equals(x.Source, y.Source, StringComparison.Ordinal) &&
               string.Equals(x.Target, y.Target, StringComparison.Ordinal);

        public int GetHashCode((string Source, string Target) obj)
            => StringComparer.Ordinal.GetHashCode(obj.Source) ^
               StringComparer.Ordinal.GetHashCode(obj.Target);
    }

    private static void GenerateMapperClassCode(
        List<ConfiguredMappingInfo> mappings,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        // Group mappings by containing class
        var groupedByClass = mappings
            .GroupBy(m => m.ContainingClass!, SymbolEqualityComparer.Default);

        foreach (var group in groupedByClass)
        {
            var containingClass = (INamedTypeSymbol)group.Key!;
            var classMappings = group.ToList();

            var sb = new StringBuilder();
            var namespaces = new HashSet<string>(StringComparer.Ordinal);

            // Collect namespaces
            foreach (var mapping in classMappings)
            {
                namespaces.Add(mapping.SourceType.ContainingNamespace.ToDisplayString());
                namespaces.Add(mapping.TargetType.ContainingNamespace.ToDisplayString());
            }

            sb.AppendLineLf("// <auto-generated/>");
            sb.AppendLineLf("#nullable enable");
            sb.AppendLineLf();

            foreach (var ns in namespaces.OrderBy(x => x, StringComparer.Ordinal))
            {
                sb.AppendLineLf($"using {ns};");
            }

            sb.AppendLineLf();

            var classNamespace = containingClass.ContainingNamespace.ToDisplayString();
            sb.AppendLineLf($"namespace {classNamespace};");
            sb.AppendLineLf();

            sb.AppendLineLf("[global::System.CodeDom.Compiler.GeneratedCode(\"Atc.SourceGenerators.MappingConfiguration\", \"1.0.0\")]");
            sb.AppendLineLf($"public static partial class {containingClass.Name}");
            sb.AppendLineLf("{");

            foreach (var mapping in classMappings)
            {
                GenerateMappingMethodBody(sb, mapping);

                // Generate reverse mapping if bidirectional
                if (mapping.Bidirectional)
                {
                    GenerateReverseMappingMethod(sb, mapping, autoDetectedEnums, context);
                }
            }

            // Generate auto-detected enum mapping methods
            GenerateAutoDetectedEnumMethods(sb, autoDetectedEnums, classMappings);

            // Generate nested type mapping methods for collection element types
            GenerateCollectionElementTypeMappingMethods(sb, classMappings, autoDetectedEnums, context);

            sb.AppendLineLf("}");

            var fileName = $"{containingClass.Name}.g.cs";
            context.AddSource(fileName, SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private static void GenerateAssemblyLevelCode(
        List<ConfiguredMappingInfo> mappings,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        var sb = new StringBuilder();
        var namespaces = new HashSet<string>(StringComparer.Ordinal);

        foreach (var mapping in mappings)
        {
            namespaces.Add(mapping.SourceType.ContainingNamespace.ToDisplayString());
            namespaces.Add(mapping.TargetType.ContainingNamespace.ToDisplayString());
        }

        sb.AppendLineLf("// <auto-generated/>");
        sb.AppendLineLf("#nullable enable");
        sb.AppendLineLf();

        foreach (var ns in namespaces.OrderBy(x => x, StringComparer.Ordinal))
        {
            sb.AppendLineLf($"using {ns};");
        }

        sb.AppendLineLf();
        sb.AppendLineLf("namespace Atc.Mapping;");
        sb.AppendLineLf();
        sb.AppendLineLf("[global::System.CodeDom.Compiler.GeneratedCode(\"Atc.SourceGenerators.MappingConfiguration\", \"1.0.0\")]");
        sb.AppendLineLf("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLineLf("[global::System.Runtime.CompilerServices.CompilerGenerated]");
        sb.AppendLineLf("[global::System.Diagnostics.DebuggerNonUserCode]");
        sb.AppendLineLf("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        sb.AppendLineLf("public static class ConfiguredMappingExtensions");
        sb.AppendLineLf("{");

        foreach (var mapping in mappings)
        {
            GenerateExtensionMethodBody(sb, mapping);

            if (mapping.Bidirectional)
            {
                GenerateReverseExtensionMethod(sb, mapping, autoDetectedEnums, context);
            }
        }

        // Generate auto-detected enum mapping methods
        GenerateAutoDetectedEnumMethods(sb, autoDetectedEnums, mappings);

        // Generate nested type mapping methods for collection element types
        GenerateCollectionElementTypeMappingMethods(sb, mappings, autoDetectedEnums, context);

        sb.AppendLineLf("}");

        context.AddSource("ConfiguredMappingExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateMappingMethodBody(
        StringBuilder sb,
        ConfiguredMappingInfo mapping)
    {
        var methodName = mapping.Method!.Name;
        var targetTypeName = mapping.TargetType.ToDisplayString();
        var sourceTypeName = mapping.SourceType.ToDisplayString();

        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Maps <see cref=\"{sourceTypeName}\"/> to <see cref=\"{targetTypeName}\"/>.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf($"    public static partial {targetTypeName} {methodName}(");
        sb.AppendLineLf($"        this {sourceTypeName} source)");
        sb.AppendLineLf("    {");

        GenerateMethodContent(sb, mapping);

        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateExtensionMethodBody(
        StringBuilder sb,
        ConfiguredMappingInfo mapping)
    {
        var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(mapping.TargetType.Name)}";
        var targetTypeName = mapping.TargetType.ToDisplayString();
        var sourceTypeName = mapping.SourceType.ToDisplayString();

        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Maps <see cref=\"{sourceTypeName}\"/> to <see cref=\"{targetTypeName}\"/>.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf($"    public static {targetTypeName} {methodName}(");
        sb.AppendLineLf($"        this {sourceTypeName} source)");
        sb.AppendLineLf("    {");

        GenerateMethodContent(sb, mapping);

        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateMethodContent(
        StringBuilder sb,
        ConfiguredMappingInfo mapping)
    {
        var targetTypeName = mapping.TargetType.ToDisplayString();

        if (!mapping.SourceType.IsValueType)
        {
            sb.AppendLineLf("        if (source is null)");
            sb.AppendLineLf("        {");
            sb.AppendLineLf("            return default!;");
            sb.AppendLineLf("        }");
            sb.AppendLineLf();
        }

        var useConstructor = mapping.Constructor is not null && mapping.ConstructorParameterNames.Count > 0;

        if (useConstructor)
        {
            var constructorParamSet = new HashSet<string>(mapping.ConstructorParameterNames, StringComparer.OrdinalIgnoreCase);
            var defaultParamSet = new HashSet<string>(
                mapping.ConstructorDefaultParameters.Select(d => d.Name), StringComparer.OrdinalIgnoreCase);
            var constructorProps = new List<PropertyMapping>();
            var initializerProps = new List<PropertyMapping>();

            foreach (var prop in mapping.PropertyMappings)
            {
                if (constructorParamSet.Contains(prop.TargetProperty.Name))
                {
                    constructorProps.Add(prop);
                }
                else if (!defaultParamSet.Contains(prop.TargetProperty.Name))
                {
                    initializerProps.Add(prop);
                }
            }

            // Build ordered list of all constructor arguments (matched + defaults)
            // by iterating the actual constructor parameters in order
            var constructorArguments = new List<(string Value, bool IsDefault)>();
            foreach (var param in mapping.Constructor!.Parameters)
            {
                var matchedProp = constructorProps.FirstOrDefault(p =>
                    string.Equals(p.TargetProperty.Name, param.Name, StringComparison.OrdinalIgnoreCase));

                if (matchedProp is not null)
                {
                    constructorArguments.Add((GeneratePropertyMappingValue(matchedProp, "source"), false));
                }
                else
                {
                    var defaultParam = mapping.ConstructorDefaultParameters.FirstOrDefault(d =>
                        string.Equals(d.Name, param.Name, StringComparison.OrdinalIgnoreCase));

                    if (defaultParam is not null)
                    {
                        constructorArguments.Add((defaultParam.DefaultExpression, true));
                    }
                }
            }

            sb.AppendLineLf($"        return new {targetTypeName}(");

            for (var i = 0; i < constructorArguments.Count; i++)
            {
                var (value, _) = constructorArguments[i];
                var isLast = i == constructorArguments.Count - 1;
                var comma = isLast && initializerProps.Count == 0 ? string.Empty : ",";

                sb.AppendLineLf($"            {value}{comma}");
            }

            if (initializerProps.Count > 0)
            {
                sb.AppendLineLf("        )");
                sb.AppendLineLf("        {");
                GeneratePropertyInitializers(sb, initializerProps);
                sb.AppendLineLf("        };");
            }
            else
            {
                sb.AppendLineLf("        );");
            }
        }
        else
        {
            sb.AppendLineLf($"        return new {targetTypeName}");
            sb.AppendLineLf("        {");
            GeneratePropertyInitializers(sb, mapping.PropertyMappings);
            sb.AppendLineLf("        };");
        }
    }

    private static void GenerateReverseMappingMethod(
        StringBuilder sb,
        ConfiguredMappingInfo mapping,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        var reverseMethodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(mapping.SourceType.Name)}";
        var sourceTypeName = mapping.TargetType.ToDisplayString();
        var targetTypeName = mapping.SourceType.ToDisplayString();

        var reverseMappings = GetPropertyMappingsWithConfig(
            mapping.TargetType,
            mapping.SourceType,
            mapping.EnableFlattening,
            mapping.PropertyNameStrategy,
            [],
            [],
            autoDetectedEnums,
            context);

        var (reverseConstructor, reverseConstructorParams, reverseConstructorDefaults) = FindBestConstructor(mapping.TargetType, mapping.SourceType);

        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Maps <see cref=\"{sourceTypeName}\"/> to <see cref=\"{targetTypeName}\"/>.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf($"    public static {targetTypeName} {reverseMethodName}(");
        sb.AppendLineLf($"        this {sourceTypeName} source)");
        sb.AppendLineLf("    {");

        var reverseMapping = new ConfiguredMappingInfo(
            SourceType: mapping.TargetType,
            TargetType: mapping.SourceType,
            Method: null,
            ContainingClass: null,
            PropertyMappings: reverseMappings,
            Bidirectional: false,
            EnableFlattening: mapping.EnableFlattening,
            Constructor: reverseConstructor,
            ConstructorParameterNames: reverseConstructorParams,
            ConstructorDefaultParameters: reverseConstructorDefaults,
            PropertyNameStrategy: mapping.PropertyNameStrategy,
            IgnoredProperties: [],
            PropertyRenames: []);

        GenerateMethodContent(sb, reverseMapping);

        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateReverseExtensionMethod(
        StringBuilder sb,
        ConfiguredMappingInfo mapping,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        GenerateReverseMappingMethod(sb, mapping, autoDetectedEnums, context);
    }

    private static void GenerateAutoDetectedEnumMethods(
        StringBuilder sb,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        List<ConfiguredMappingInfo> mappings)
    {
        // Generate enum mapping methods that are referenced by the mappings
        var generated = new HashSet<string>(StringComparer.Ordinal);

        foreach (var enumMapping in autoDetectedEnums)
        {
            var key = $"{enumMapping.SourceEnum.ToDisplayString()}->{enumMapping.TargetEnum.ToDisplayString()}";
            if (!generated.Add(key))
            {
                continue;
            }

            // Check if any mapping in this scope references this enum pair
            // (either as a direct property conversion or as a collection element type)
            // Unwrap nullable types when comparing to handle Nullable<EnumType> properties
            var isReferenced = mappings.Any(m =>
                m.PropertyMappings.Any(p =>
                    (p.RequiresConversion &&
                     p.HasEnumMapping &&
                     SymbolEqualityComparer.Default.Equals(MappingTypeAnalyzer.UnwrapNullable(p.SourceProperty.Type), enumMapping.SourceEnum) &&
                     SymbolEqualityComparer.Default.Equals(MappingTypeAnalyzer.UnwrapNullable(p.TargetProperty.Type), enumMapping.TargetEnum)) ||
                    (p.IsCollection &&
                     p.HasEnumMapping &&
                     MappingTypeAnalyzer.IsCollectionType(p.SourceProperty.Type, out var srcElem) &&
                     srcElem is not null &&
                     SymbolEqualityComparer.Default.Equals(srcElem, enumMapping.SourceEnum) &&
                     SymbolEqualityComparer.Default.Equals(p.CollectionElementType, enumMapping.TargetEnum))));

            if (!isReferenced)
            {
                continue;
            }

            var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(enumMapping.TargetEnum.Name)}";

            sb.AppendLineLf("    /// <summary>");
            sb.AppendLineLf($"    /// Maps <see cref=\"{enumMapping.SourceEnum.ToDisplayString()}\"/> to <see cref=\"{enumMapping.TargetEnum.ToDisplayString()}\"/>.");
            sb.AppendLineLf("    /// </summary>");
            sb.AppendLineLf($"    private static {enumMapping.TargetEnum.ToDisplayString()} {methodName}(");
            sb.AppendLineLf($"        this {enumMapping.SourceEnum.ToDisplayString()} source)");
            sb.AppendLineLf("        => source switch");
            sb.AppendLineLf("        {");

            foreach (var valueMapping in enumMapping.ValueMappings)
            {
                if (valueMapping.IsMapped && valueMapping.TargetValue is not null)
                {
                    sb.AppendLineLf($"            {enumMapping.SourceEnum.ToDisplayString()}.{valueMapping.SourceValue} => {enumMapping.TargetEnum.ToDisplayString()}.{valueMapping.TargetValue},");
                }
            }

            sb.AppendLineLf("            _ => throw new global::System.ArgumentOutOfRangeException(nameof(source), source, \"Unmapped enum value\"),");
            sb.AppendLineLf("        };");
            sb.AppendLineLf();
        }
    }

    private static void GenerateCollectionElementTypeMappingMethods(
        StringBuilder sb,
        List<ConfiguredMappingInfo> mappings,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        var generated = new HashSet<string>(StringComparer.Ordinal);

        // Collect all top-level mapping pairs (source -> target) so we don't duplicate them
        var topLevelMappingPairs = new HashSet<string>(StringComparer.Ordinal);
        foreach (var mapping in mappings)
        {
            topLevelMappingPairs.Add($"{mapping.SourceType.ToDisplayString()}->{mapping.TargetType.ToDisplayString()}");
        }

        // Use a queue for recursive processing of nested type pairs
        var queue = new Queue<(INamedTypeSymbol Source, INamedTypeSymbol Target)>();
        CollectNestedTypePairs(mappings, queue);

        // Track sub-mappings for enum method generation
        var subMappings = new List<ConfiguredMappingInfo>();

        while (queue.Count > 0)
        {
            var (sourceElementType, targetElementType) = queue.Dequeue();

            // Strip nullable annotation from display strings to match top-level pairs correctly
            var sourceDisplay = sourceElementType.ToDisplayString().TrimEnd('?');
            var targetDisplay = targetElementType.ToDisplayString().TrimEnd('?');
            var key = $"{sourceDisplay}->{targetDisplay}";

            // Skip if already generated or if there's already a top-level mapping for this pair
            if (!generated.Add(key) || topLevelMappingPairs.Contains(key))
            {
                continue;
            }

            // Generate a nested mapping method for this type pair
            var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(targetElementType.Name)}";
            var elementPropertyMappings = GetPropertyMappingsWithConfig(
                sourceElementType,
                targetElementType,
                enableFlattening: false,
                PropertyNameStrategy.PascalCase,
                [],
                [],
                autoDetectedEnums,
                context);

            var (constructor, constructorParameterNames, constructorDefaultParameters) = FindBestConstructor(sourceElementType, targetElementType);

            var elementMapping = new ConfiguredMappingInfo(
                SourceType: sourceElementType,
                TargetType: targetElementType,
                Method: null,
                ContainingClass: null,
                PropertyMappings: elementPropertyMappings,
                Bidirectional: false,
                EnableFlattening: false,
                Constructor: constructor,
                ConstructorParameterNames: constructorParameterNames,
                ConstructorDefaultParameters: constructorDefaultParameters,
                PropertyNameStrategy: PropertyNameStrategy.PascalCase,
                IgnoredProperties: [],
                PropertyRenames: []);

            sb.AppendLineLf("    /// <summary>");
            sb.AppendLineLf($"    /// Maps <see cref=\"{sourceElementType.ToDisplayString()}\"/> to <see cref=\"{targetElementType.ToDisplayString()}\"/>.");
            sb.AppendLineLf("    /// </summary>");
            sb.AppendLineLf($"    private static {targetElementType.ToDisplayString()} {methodName}(");
            sb.AppendLineLf($"        this {sourceElementType.ToDisplayString()} source)");
            sb.AppendLineLf("    {");

            GenerateMethodContent(sb, elementMapping);

            sb.AppendLineLf("    }");
            sb.AppendLineLf();

            subMappings.Add(elementMapping);

            // Recursively discover nested types within this sub-method's properties
            CollectNestedTypePairs([elementMapping], queue);
        }

        // Generate enum mapping methods discovered during sub-method generation
        if (subMappings.Count > 0)
        {
            GenerateAutoDetectedEnumMethods(sb, autoDetectedEnums, subMappings);
        }
    }

    private static void CollectNestedTypePairs(
        List<ConfiguredMappingInfo> mappings,
        Queue<(INamedTypeSymbol Source, INamedTypeSymbol Target)> queue)
    {
        foreach (var mapping in mappings)
        {
            foreach (var prop in mapping.PropertyMappings)
            {
                // Collection element type pairs
                if (prop.IsCollection &&
                    prop.SourceCollectionElementType is not null &&
                    prop.CollectionElementType is not null &&
                    !SymbolEqualityComparer.Default.Equals(prop.SourceCollectionElementType, prop.CollectionElementType) &&
                    !prop.HasEnumMapping &&
                    prop.SourceCollectionElementType is INamedTypeSymbol sourceElem &&
                    prop.CollectionElementType is INamedTypeSymbol targetElem &&
                    sourceElem.SpecialType == SpecialType.None &&
                    targetElem.SpecialType == SpecialType.None)
                {
                    queue.Enqueue((sourceElem, targetElem));
                }

                // Non-collection nested property type pairs
                if (prop.IsNested)
                {
                    var unwrappedSource = MappingTypeAnalyzer.UnwrapNullable(prop.SourceProperty.Type) as INamedTypeSymbol;
                    var unwrappedTarget = MappingTypeAnalyzer.UnwrapNullable(prop.TargetProperty.Type) as INamedTypeSymbol;

                    if (unwrappedSource is not null &&
                        unwrappedTarget is not null &&
                        unwrappedSource.SpecialType == SpecialType.None &&
                        unwrappedTarget.SpecialType == SpecialType.None)
                    {
                        queue.Enqueue((unwrappedSource, unwrappedTarget));
                    }
                }
            }
        }
    }

    private static void GeneratePropertyInitializers(
        StringBuilder sb,
        List<PropertyMapping> properties)
    {
        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var isLast = i == properties.Count - 1;
            var comma = isLast ? string.Empty : ",";

            var value = GeneratePropertyMappingValue(prop, "source");
            sb.AppendLineLf($"            {prop.TargetProperty.Name} = {value}{comma}");
        }
    }

    private static string GeneratePropertyMappingValue(
        PropertyMapping prop,
        string sourceVariable)
    {
        if (prop.IsFlattened && prop.FlattenedNestedProperty is not null)
        {
            var isNullable = prop.SourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                             !prop.SourceProperty.Type.IsValueType;

            if (isNullable)
            {
                return $"{sourceVariable}.{prop.SourceProperty.Name}?.{prop.FlattenedNestedProperty.Name}!";
            }

            return $"{sourceVariable}.{prop.SourceProperty.Name}.{prop.FlattenedNestedProperty.Name}";
        }

        if (prop.IsBuiltInTypeConversion)
        {
            return GenerateBuiltInTypeConversion(prop, sourceVariable);
        }

        if (prop.IsCollection)
        {
            var collectionType = prop.CollectionTargetType!;
            var sourceAccess = $"{sourceVariable}.{prop.SourceProperty.Name}";

            // Check if source and target element types are the same - skip Select if so
            var sameElementType = prop.SourceCollectionElementType is not null &&
                                  prop.CollectionElementType is not null &&
                                  SymbolEqualityComparer.Default.Equals(prop.SourceCollectionElementType, prop.CollectionElementType);

            if (sameElementType)
            {
                // Same element type - just convert the collection type
                if (collectionType == "Array")
                {
                    return $"{sourceAccess}?.ToArray()!";
                }

                if (collectionType == "Collection")
                {
                    return $"new global::System.Collections.ObjectModel.Collection<{prop.CollectionElementType!.ToDisplayString()}>({sourceAccess}?.ToList()!)";
                }

                if (collectionType == "ReadOnlyCollection")
                {
                    return $"new global::System.Collections.ObjectModel.ReadOnlyCollection<{prop.CollectionElementType!.ToDisplayString()}>({sourceAccess}?.ToList()!)";
                }

                return $"{sourceAccess}?.ToList()!";
            }

            var elementTypeName = prop.CollectionElementType!.Name;
            var mappingMethodName = $"MapTo{elementTypeName}";

            if (collectionType == "Array")
            {
                return $"{sourceAccess}?.Select(x => x.{mappingMethodName}()).ToArray()!";
            }

            if (collectionType == "Collection")
            {
                return $"new global::System.Collections.ObjectModel.Collection<{prop.CollectionElementType.ToDisplayString()}>({sourceAccess}?.Select(x => x.{mappingMethodName}()).ToList()!)";
            }

            if (collectionType == "ReadOnlyCollection")
            {
                return $"new global::System.Collections.ObjectModel.ReadOnlyCollection<{prop.CollectionElementType.ToDisplayString()}>({sourceAccess}?.Select(x => x.{mappingMethodName}()).ToList()!)";
            }

            return $"{sourceAccess}?.Select(x => x.{mappingMethodName}()).ToList()!";
        }

        if (prop.RequiresConversion)
        {
            if (prop.HasEnumMapping)
            {
                var sourceIsNullable = MappingTypeAnalyzer.IsNullableValueType(prop.SourceProperty.Type);
                var targetIsNullable = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type);
                var unwrappedTarget = MappingTypeAnalyzer.UnwrapNullable(prop.TargetProperty.Type);
                var enumMappingMethodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(unwrappedTarget.Name)}";

                if (sourceIsNullable && targetIsNullable)
                {
                    var varName = $"__{char.ToLowerInvariant(prop.SourceProperty.Name[0])}{prop.SourceProperty.Name.Substring(1)}";
                    return $"{sourceVariable}.{prop.SourceProperty.Name} is {{ }} {varName} ? {varName}.{enumMappingMethodName}() : null";
                }

                if (sourceIsNullable)
                {
                    return $"{sourceVariable}.{prop.SourceProperty.Name}!.Value.{enumMappingMethodName}()";
                }

                return $"{sourceVariable}.{prop.SourceProperty.Name}.{enumMappingMethodName}()";
            }

            // Numeric conversion - handle nullable wrapping
            return $"({prop.TargetProperty.Type.ToDisplayString()}){sourceVariable}.{prop.SourceProperty.Name}";
        }

        if (prop.IsNested)
        {
            var unwrappedSourceType = MappingTypeAnalyzer.UnwrapNullable(prop.SourceProperty.Type);
            var unwrappedTargetType = MappingTypeAnalyzer.UnwrapNullable(prop.TargetProperty.Type);
            var nestedMethodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(unwrappedTargetType.Name)}";
            var sourceIsNullable = MappingTypeAnalyzer.IsNullableValueType(prop.SourceProperty.Type);
            var targetIsNullable = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type);

            if (sourceIsNullable && targetIsNullable)
            {
                var varName = $"__{char.ToLowerInvariant(prop.SourceProperty.Name[0])}{prop.SourceProperty.Name.Substring(1)}";
                return $"{sourceVariable}.{prop.SourceProperty.Name} is {{ }} {varName} ? {varName}.{nestedMethodName}() : null";
            }

            if (prop.SourceProperty.Type.IsValueType && !sourceIsNullable)
            {
                return $"{sourceVariable}.{prop.SourceProperty.Name}.{nestedMethodName}()";
            }

            if (prop.TargetProperty.Type.IsValueType && !targetIsNullable)
            {
                return $"{sourceVariable}.{prop.SourceProperty.Name} is not null ? {sourceVariable}.{prop.SourceProperty.Name}.{nestedMethodName}() : default";
            }

            return $"{sourceVariable}.{prop.SourceProperty.Name}?.{nestedMethodName}()!";
        }

        return $"{sourceVariable}.{prop.SourceProperty.Name}";
    }

    private static string GenerateBuiltInTypeConversion(
        PropertyMapping prop,
        string sourceVariable)
    {
        var sourceTypeName = prop.SourceProperty.Type.ToDisplayString();
        var targetTypeName = prop.TargetProperty.Type.ToDisplayString();
        var sourcePropertyAccess = $"{sourceVariable}.{prop.SourceProperty.Name}";

        // Normalize type names by stripping nullable annotation for matching
        var normalizedSource = sourceTypeName.TrimEnd('?');
        var normalizedTarget = targetTypeName.TrimEnd('?');
        var sourceIsNullableRef = sourceTypeName.EndsWith("?", StringComparison.Ordinal) &&
                                  !prop.SourceProperty.Type.IsValueType;
        var targetIsNullableRef = targetTypeName.EndsWith("?", StringComparison.Ordinal) &&
                                  !prop.TargetProperty.Type.IsValueType;

        if (sourceTypeName is "System.DateTime" or "System.DateTimeOffset" && targetTypeName == "string")
        {
            return $"{sourcePropertyAccess}.ToString(\"O\", global::System.Globalization.CultureInfo.InvariantCulture)";
        }

        if (sourceTypeName == "string" && targetTypeName == "System.DateTime")
        {
            return $"global::System.DateTime.Parse({sourcePropertyAccess}, global::System.Globalization.CultureInfo.InvariantCulture)";
        }

        if (sourceTypeName == "string" && targetTypeName == "System.DateTimeOffset")
        {
            return $"global::System.DateTimeOffset.Parse({sourcePropertyAccess}, global::System.Globalization.CultureInfo.InvariantCulture)";
        }

        if (sourceTypeName == "System.Guid" && targetTypeName == "string")
        {
            return $"{sourcePropertyAccess}.ToString()";
        }

        if (sourceTypeName == "string" && targetTypeName == "System.Guid")
        {
            return $"global::System.Guid.Parse({sourcePropertyAccess})";
        }

        if (MappingTypeAnalyzer.IsNumericType(sourceTypeName) && targetTypeName == "string")
        {
            return $"{sourcePropertyAccess}.ToString(global::System.Globalization.CultureInfo.InvariantCulture)";
        }

        if (sourceTypeName == "string" && MappingTypeAnalyzer.IsNumericType(targetTypeName))
        {
            var parts = targetTypeName.Split('.');
            var simpleTypeName = parts[parts.Length - 1];
            return $"{simpleTypeName}.Parse({sourcePropertyAccess}, global::System.Globalization.CultureInfo.InvariantCulture)";
        }

        if (sourceTypeName == "bool" && targetTypeName == "string")
        {
            return $"{sourcePropertyAccess}.ToString()";
        }

        if (sourceTypeName == "string" && targetTypeName == "bool")
        {
            return $"bool.Parse({sourcePropertyAccess})";
        }

        // string <-> Uri conversions
        if (normalizedSource == "string" && normalizedTarget == "System.Uri")
        {
            if (sourceIsNullableRef || targetIsNullableRef)
            {
                return $"{sourcePropertyAccess} is not null ? new global::System.Uri({sourcePropertyAccess}) : null";
            }

            return $"new global::System.Uri({sourcePropertyAccess})";
        }

        if (normalizedSource == "System.Uri" && normalizedTarget == "string")
        {
            if (sourceIsNullableRef)
            {
                return $"{sourcePropertyAccess}?.ToString()";
            }

            return $"{sourcePropertyAccess}.ToString()";
        }

        // string <-> enum conversions
        var unwrappedSourceType = MappingTypeAnalyzer.UnwrapNullable(prop.SourceProperty.Type);
        var unwrappedTargetType = MappingTypeAnalyzer.UnwrapNullable(prop.TargetProperty.Type);

        if (normalizedSource == "string" && unwrappedTargetType.TypeKind == TypeKind.Enum)
        {
            var enumType = unwrappedTargetType.ToDisplayString();
            var sourceIsNullable = sourceIsNullableRef ||
                                   prop.SourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated;
            var targetIsNullableValue = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type);

            if (sourceIsNullable && targetIsNullableValue)
            {
                var varName = $"__{char.ToLowerInvariant(prop.SourceProperty.Name[0])}{prop.SourceProperty.Name.Substring(1)}";
                return $"{sourcePropertyAccess} is {{ }} {varName} ? (global::System.Enum.TryParse<global::{enumType}>({varName}, out var __parsed) ? __parsed : default({enumType}?)) : null";
            }

            return $"global::System.Enum.TryParse<global::{enumType}>({sourcePropertyAccess}, out var __parsed) ? __parsed : default(global::{enumType})";
        }

        if (unwrappedSourceType.TypeKind == TypeKind.Enum && normalizedTarget == "string")
        {
            var sourceIsNullableValue = MappingTypeAnalyzer.IsNullableValueType(prop.SourceProperty.Type);
            if (sourceIsNullableValue)
            {
                return $"{sourcePropertyAccess}?.ToString()";
            }

            return $"{sourcePropertyAccess}.ToString()";
        }

        return sourcePropertyAccess;
    }

    private static string GenerateMappingConfigurationAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Marks a static partial class as a mapping configuration container.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.MappingConfiguration", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
               public sealed class MappingConfigurationAttribute : global::System.Attribute
               {
               }
           }
           """;

    private static string GenerateMapConfigIgnoreAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Specifies a property to exclude from configuration-based mapping.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.MappingConfiguration", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
               public sealed class MapConfigIgnoreAttribute : global::System.Attribute
               {
                   /// <summary>
                   /// Initializes a new instance of the <see cref="MapConfigIgnoreAttribute"/> class.
                   /// </summary>
                   /// <param name="propertyName">The name of the source property to exclude from mapping.</param>
                   public MapConfigIgnoreAttribute(string propertyName)
                   {
                       PropertyName = propertyName;
                   }

                   /// <summary>
                   /// Gets the name of the source property to exclude from mapping.
                   /// </summary>
                   public string PropertyName { get; }
               }
           }
           """;

    private static string GenerateMapConfigPropertyAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Specifies a property name mapping between source and target types.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.MappingConfiguration", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
               public sealed class MapConfigPropertyAttribute : global::System.Attribute
               {
                   /// <summary>
                   /// Initializes a new instance of the <see cref="MapConfigPropertyAttribute"/> class.
                   /// </summary>
                   /// <param name="sourcePropertyName">The name of the property on the source type.</param>
                   /// <param name="targetPropertyName">The name of the property on the target type.</param>
                   public MapConfigPropertyAttribute(string sourcePropertyName, string targetPropertyName)
                   {
                       SourcePropertyName = sourcePropertyName;
                       TargetPropertyName = targetPropertyName;
                   }

                   /// <summary>
                   /// Gets the name of the property on the source type.
                   /// </summary>
                   public string SourcePropertyName { get; }

                   /// <summary>
                   /// Gets the name of the property on the target type.
                   /// </summary>
                   public string TargetPropertyName { get; }
               }
           }
           """;

    private static string GenerateMapConfigOptionsAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Configures advanced mapping options for a configuration-based mapping method.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.MappingConfiguration", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
               public sealed class MapConfigOptionsAttribute : global::System.Attribute
               {
                   /// <summary>
                   /// Gets or sets a value indicating whether to generate bidirectional mappings.
                   /// </summary>
                   public bool Bidirectional { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to enable property flattening.
                   /// </summary>
                   public bool EnableFlattening { get; set; }

                   /// <summary>
                   /// Gets or sets the naming strategy for property name conversion.
                   /// </summary>
                   public PropertyNameStrategy PropertyNameStrategy { get; set; } = PropertyNameStrategy.PascalCase;
               }
           }
           """;

    private static string GenerateMapTypesAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Defines a mapping between two types at the assembly level.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.MappingConfiguration", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Assembly | global::System.AttributeTargets.Class, AllowMultiple = true)]
               public sealed class MapTypesAttribute : global::System.Attribute
               {
                   /// <summary>
                   /// Initializes a new instance of the <see cref="MapTypesAttribute"/> class.
                   /// </summary>
                   public MapTypesAttribute(global::System.Type sourceType, global::System.Type targetType)
                   {
                       SourceType = sourceType;
                       TargetType = targetType;
                   }

                   /// <summary>
                   /// Gets the source type to map from.
                   /// </summary>
                   public global::System.Type SourceType { get; }

                   /// <summary>
                   /// Gets the target type to map to.
                   /// </summary>
                   public global::System.Type TargetType { get; }

                   /// <summary>
                   /// Gets or sets the property name mappings in "SourceProperty:TargetProperty" format.
                   /// </summary>
                   public string[]? PropertyMap { get; set; }

                   /// <summary>
                   /// Gets or sets the source property names to exclude from mapping.
                   /// </summary>
                   public string[]? IgnoreSourceProperties { get; set; }

                   /// <summary>
                   /// Gets or sets the target property names to exclude from mapping.
                   /// </summary>
                   public string[]? IgnoreTargetProperties { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to generate bidirectional mappings.
                   /// </summary>
                   public bool Bidirectional { get; set; }

                   /// <summary>
                   /// Gets or sets the naming strategy for property name conversion.
                   /// </summary>
                   public PropertyNameStrategy PropertyNameStrategy { get; set; } = PropertyNameStrategy.PascalCase;
               }
           }
           """;

    private static string GenerateMappingBuilderSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Provides a fluent API for configuring type mappings inline.
               /// The Map() calls are analyzed at compile time by the source generator to produce mapping extension methods.
               /// At runtime, this class is a no-op.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.MappingConfiguration", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               public sealed class MappingBuilder
               {
                   internal MappingBuilder() { }

                   /// <summary>
                   /// Registers a mapping between two types using typeof() expressions.
                   /// </summary>
                   public MappingBuilder Map(
                       global::System.Type sourceType,
                       global::System.Type targetType,
                       bool bidirectional = false,
                       string[]? propertyMap = null,
                       string[]? ignoreSourceProperties = null,
                       string[]? ignoreTargetProperties = null,
                       PropertyNameStrategy propertyNameStrategy = PropertyNameStrategy.PascalCase)
                       => this;

                   /// <summary>
                   /// Registers a mapping between two types using generic type parameters.
                   /// </summary>
                   public MappingBuilder Map<TSource, TTarget>(
                       bool bidirectional = false,
                       string[]? propertyMap = null,
                       string[]? ignoreSourceProperties = null,
                       string[]? ignoreTargetProperties = null,
                       PropertyNameStrategy propertyNameStrategy = PropertyNameStrategy.PascalCase)
                       => this;

                   /// <summary>
                   /// Configures mapping registrations without requiring dependency injection.
                   /// The lambda body is analyzed at compile time by the source generator.
                   /// </summary>
                   public static void Configure(global::System.Action<MappingBuilder> configure)
                   {
                       // No-op — mappings are generated at compile time
                   }
               }
           }
           """;
}