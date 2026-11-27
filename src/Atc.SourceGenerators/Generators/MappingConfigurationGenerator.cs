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
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.ConfigClassMustBeStatic));

    private static readonly DiagnosticDescriptor ConfigClassMustBePartialDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.ConfigClassMustBePartial,
        title: "Configuration class must be partial",
        messageFormat: "Mapping configuration class '{0}' must be declared as partial",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.ConfigClassMustBePartial));

    private static readonly DiagnosticDescriptor MethodMustBeExtensionMethodDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MethodMustBeExtensionMethod,
        title: "Method must be extension method",
        messageFormat: "Mapping method '{0}' must be an extension method (first parameter must use 'this')",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.MethodMustBeExtensionMethod));

    private static readonly DiagnosticDescriptor IgnoredPropertyNotFoundDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.IgnoredPropertyNotFound,
        title: "Ignored property not found",
        messageFormat: "Ignored property '{0}' not found on source type '{1}'",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.IgnoredPropertyNotFound));

    private static readonly DiagnosticDescriptor RenamedSourcePropertyNotFoundDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.RenamedSourcePropertyNotFound,
        title: "Renamed source property not found",
        messageFormat: "Source property '{0}' specified in MapConfigProperty not found on type '{1}'",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.RenamedSourcePropertyNotFound));

    private static readonly DiagnosticDescriptor RenamedTargetPropertyNotFoundDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.RenamedTargetPropertyNotFound,
        title: "Renamed target property not found",
        messageFormat: "Target property '{0}' specified in MapConfigProperty not found on type '{1}'",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.RenamedTargetPropertyNotFound));

    private static readonly DiagnosticDescriptor EmptyConfigurationClassDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.EmptyConfigurationClass,
        title: "Empty configuration class",
        messageFormat: "Mapping configuration class '{0}' has no partial extension methods",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.EmptyConfigurationClass));

    private static readonly DiagnosticDescriptor MethodReturnTypeInvalidDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MethodReturnTypeInvalid,
        title: "Return type invalid",
        messageFormat: "Mapping method '{0}' return type must be a class, record, or struct",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.MethodReturnTypeInvalid));

    private static readonly DiagnosticDescriptor TypeNotResolvedDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.TypeNotResolved,
        title: "Type not resolved",
        messageFormat: "Type '{0}' could not be resolved in method '{1}'. If this type is produced by another source generator, note that source generators cannot reference types from sibling generators in the same compilation. Consider moving the type to a referenced project or pre-compiling the other generator's output.",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.TypeNotResolved));

    private static readonly DiagnosticDescriptor DuplicateAttributeAndConfigurationDescriptor = new(
        id: RuleIdentifierConstants.ObjectMappingExtended.DuplicateAttributeAndConfiguration,
        title: "Duplicate attribute and configuration mapping",
        messageFormat: "Type '{0}' has both [MapTo(typeof({1}))] attribute and configuration-based mapping. Attribute takes precedence.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMappingExtended.DuplicateAttributeAndConfiguration));

    private static readonly DiagnosticDescriptor DuplicateConfigurationMappingDescriptor = new(
        id: RuleIdentifierConstants.ObjectMappingExtended.DuplicateConfigurationMapping,
        title: "Duplicate configuration mapping",
        messageFormat: "Type pair '{0}' -> '{1}' is configured multiple times. First configuration is used.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMappingExtended.DuplicateConfigurationMapping));

    private static readonly DiagnosticDescriptor ConfigTargetTypeMustBeClassOrStructDescriptor = new(
        id: RuleIdentifierConstants.ObjectMappingExtended.ConfigTargetTypeMustBeClassOrStruct,
        title: "Target type must be a class or struct",
        messageFormat: "Configuration target type '{0}' must be a class or struct",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMappingExtended.ConfigTargetTypeMustBeClassOrStruct));

    private static readonly DiagnosticDescriptor AutoDetectedEnumPartialMatchDescriptor = new(
        id: RuleIdentifierConstants.EnumMapping.AutoDetectedEnumPartialMatch,
        title: "Partial enum match",
        messageFormat: "Auto-detected enum mapping '{0}' -> '{1}': {2} of {3} values unmatched: {4}",
        category: RuleCategoryConstants.EnumMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.EnumMapping.AutoDetectedEnumPartialMatch));

    private static readonly DiagnosticDescriptor AutoDetectedEnumNoMatchDescriptor = new(
        id: RuleIdentifierConstants.EnumMapping.AutoDetectedEnumNoMatch,
        title: "No enum match",
        messageFormat: "Enum types '{0}' and '{1}' have no matching values; falling back to cast",
        category: RuleCategoryConstants.EnumMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.EnumMapping.AutoDetectedEnumNoMatch));

    private static readonly DiagnosticDescriptor MapCallRequiresTypeArgumentsDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MapCallRequiresTypeArguments,
        title: "Map() requires type arguments",
        messageFormat: "Map() requires exactly two type arguments (source and target type)",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.MapCallRequiresTypeArguments));

    private static readonly DiagnosticDescriptor MethodTooManyParametersDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MethodTooManyParameters,
        title: "Mapping method must have exactly one parameter",
        messageFormat: "Mapping method '{0}' must have exactly one parameter (this TSource source)",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.MethodTooManyParameters));

    private static readonly DiagnosticDescriptor MapCallInvalidArgumentDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.MapCallInvalidArgument,
        title: "Map() argument must be a typeof() expression",
        messageFormat: "Map() arguments must be typeof() expressions such as Map(typeof(Source), typeof(Target)), or use Map<Source, Target>()",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.MapCallInvalidArgument));

    private static readonly DiagnosticDescriptor AddMappingsLambdaRequiredDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.AddMappingsLambdaRequired,
        title: "Lambda required",
        messageFormat: "AddMappings() requires a lambda expression argument",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.AddMappingsLambdaRequired));

    private static readonly DiagnosticDescriptor TargetPropertyNotMappedDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.TargetPropertyNotMapped,
        title: "Target property is not mapped",
        messageFormat: "Target property '{0}' of '{1}' is not mapped from '{2}' and keeps its default value",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.TargetPropertyNotMapped));

    private static readonly DiagnosticDescriptor PropertyTypesNotCompatibleDescriptor = new(
        id: RuleIdentifierConstants.MappingConfiguration.PropertyTypesNotCompatible,
        title: "Property types are not compatible",
        messageFormat: "Property '{0}' is not mapped from '{1}' to '{2}': type '{3}' cannot be converted to '{4}'",
        category: RuleCategoryConstants.MappingConfiguration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.MappingConfiguration.PropertyTypesNotCompatible));

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate fallback attribute definitions
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource("MappingConfigurationAttribute.g.cs", SourceText.From(GenerateMappingConfigurationAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapConfigIgnoreAttribute.g.cs", SourceText.From(GenerateMapConfigIgnoreAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapConfigPropertyAttribute.g.cs", SourceText.From(GenerateMapConfigPropertyAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapConfigOptionsAttribute.g.cs", SourceText.From(GenerateMapConfigOptionsAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapTypesAttribute.g.cs", SourceText.From(GenerateMapTypesAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MappingBuilder.g.cs", SourceText.From(GenerateMappingBuilderSource(), Encoding.UTF8));
        });

        // Find classes with [MappingConfiguration] or class-level [MapTypes] attributes
        // (ForAttributeWithMetadataName filters by attribute name without resolving every attribute)
        var configurationClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{AttributeNamespace}.{MappingConfigurationAttributeName}",
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode)
            .Collect();

        var mapTypesClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{AttributeNamespace}.{MapTypesAttributeName}",
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode)
            .Collect();

        var classDeclarations = configurationClasses
            .Combine(mapTypesClasses)
            .Select(static (pair, _) => pair.Left.AddRange(pair.Right));

        // Find AddMappings() / MappingBuilder.Configure() invocations
        var inlineInvocations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsInlineMappingInvocation(s),
                transform: static (ctx, ct) => GetInlineMappingInvocation(ctx, ct))
            .Where(static m => m is not null)
            .Collect();

        // [MapTo] (source, target) pairs, used to report config mappings that duplicate an attribute mapping.
        // Plain string pairs are equatable, so Roslyn can cache them per file instead of the generator
        // walking every syntax tree of the compilation.
        var attributeMappedPairs = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{AttributeNamespace}.MapToAttribute",
                predicate: static (s, _) => s is TypeDeclarationSyntax,
                transform: static (ctx, _) => GetAttributeMappedPairs(ctx))
            .WithTrackingName("MappingConfiguration.AttributeMappedPairs")
            .SelectMany(static (pairs, _) => pairs)
            .Collect();

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider
            .Combine(classDeclarations)
            .Combine(inlineInvocations)
            .Combine(attributeMappedPairs);

        // Generate source
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) =>
            Execute(source.Left.Left.Left, source.Left.Left.Right, source.Left.Right!, source.Right, spc));
    }

    private static EquatableArray<(string Source, string Target)> GetAttributeMappedPairs(
        GeneratorAttributeSyntaxContext context)
    {
        var sourceName = context.TargetSymbol.ToDisplayString();
        var builder = new List<(string Source, string Target)>();

        foreach (var attribute in context.Attributes)
        {
            if (attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is INamedTypeSymbol targetType)
            {
                builder.Add((sourceName, targetType.ToDisplayString()));
            }
        }

        return new EquatableArray<(string Source, string Target)>(builder);
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
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol methodSymbol)
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

            // Resolved to another method (e.g. an ASP.NET Configure(app => app.Map(...))): not a mapping configuration
            return null;
        }

        // Fallback: if the method is not yet resolved (common for generated methods such as AddMappings),
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
        ImmutableArray<(string Source, string Target)> attributeMappedPairs,
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
                if (semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken) is not { } classSymbol)
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
        DetectDuplicatesWithAttributeMappings(attributeMappedPairs, configuredMappings, context);

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

        // Validate the single source parameter
        if (method.Parameters.Length != 1)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodTooManyParametersDescriptor,
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
        var sourceProperties = MappingTypeAnalyzer.GetAllProperties(sourceType, includePrivateMembers: false);
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
        var targetProperties = MappingTypeAnalyzer.GetAllProperties(targetType, includePrivateMembers: false);
        foreach (var rename in propertyRenames)
        {
            if (ResolveSourcePath(sourceProperties, rename.Source) is null)
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
            context,
            method.Locations.FirstOrDefault(),
            GetUserConversions(containingClass));

        // Find best matching constructor
        var (constructor, constructorParameterNames, constructorDefaultParameters) = FindBestConstructor(sourceType, targetType, propertyRenames, GetUserConversions(containingClass));

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

        // Find all Map() calls in the lambda body, including an expression body that is itself the Map() call
        // (map => map.Map<Source, Target>())
        var mapCalls = lambdaBody.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>();

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
            var sourceTypeInfo = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[0], context.CancellationToken);
            var targetTypeInfo = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[1], context.CancellationToken);

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
                sourceType = ExtractTypeFromTypeOfExpression(positionalArgs[0].Expression, semanticModel, context.CancellationToken);
                targetType = ExtractTypeFromTypeOfExpression(positionalArgs[1].Expression, semanticModel, context.CancellationToken);

                // The types must be known at compile time: report arguments that aren't typeof() expressions
                if (sourceType is null || targetType is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            MapCallInvalidArgumentDescriptor,
                            mapCall.GetLocation()));
                    return null;
                }
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
            context,
            mapCall.GetLocation());

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
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (expression is TypeOfExpressionSyntax typeOfExpression)
        {
            var typeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type, cancellationToken);
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
                    attr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? Location.None,
                    sourceType.Name,
                    "MapTypes"));
            return null;
        }

        if (targetType.TypeKind == TypeKind.Error)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    TypeNotResolvedDescriptor,
                    attr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? Location.None,
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
                    attr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? Location.None,
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
            context,
            attr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation());

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
        sb.AppendLineLf("internal static class AtcMappingServiceCollectionExtensions");
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
        SourceProductionContext context,
        Location? diagnosticLocation = null,
        Dictionary<string, string>? conversions = null)
    {
        var mappings = new List<PropertyMapping>();

        // Configuration mappings use the configuration class's conversion methods, generate the nested mappers of
        // collection elements and dictionary values, auto-detect enum mappings and cast numeric types
        var rules = new PropertyMappingRules
        {
            FindConversionMethod = (source, target) => TryGetUserConversion(conversions, source, target, out var method) ? method : null,
            CanMapDictionaryValue = MappingTypeAnalyzer.IsDictionaryValueNestedMapping,
            UnmappableDictionaryFallsThrough = true,
            ResolveEnumMapping = (source, target) =>
                MappingTypeAnalyzer.HasEnumMappingAttribute(source, target) ||
                (source is INamedTypeSymbol sourceEnum &&
                 target is INamedTypeSymbol targetEnum &&
                 TryAutoDetectEnumMapping(sourceEnum, targetEnum, autoDetectedEnums, context)),
            AllowNumericConversions = true,
        };

        var sourceProperties = MappingTypeAnalyzer.GetAllProperties(sourceType, includePrivateMembers: false);
        var targetProperties = MappingTypeAnalyzer.GetAllProperties(targetType, includePrivateMembers: false);

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
                mappings.Add(PropertyPairClassifier.Create(sourceProp, targetProp));
                continue;
            }

            // Types differ: find the target by name and classify the pair (conversion method, nested, enum, ...)
            targetProp = targetProperties.FirstOrDefault(t =>
                string.Equals(t.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase));
            if (targetProp is null)
            {
                continue;
            }

            var result = PropertyPairClassifier.Classify(sourceProp, targetProp, rules);
            if (result.Mapping is not null)
            {
                mappings.Add(result.Mapping);
            }
            else if (result.Failure == PropertyPairFailure.IncompatibleTypes)
            {
                // Same name, but no conversion between the types: say so instead of
                // leaving the property out silently (which also breaks constructor calls).
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        PropertyTypesNotCompatibleDescriptor,
                        diagnosticLocation ?? Location.None,
                        sourceProp.Name,
                        sourceType.Name,
                        targetType.Name,
                        sourceProp.Type.ToDisplayString(),
                        targetProp.Type.ToDisplayString()));
            }
        }

        // Nested source paths (e.g. "Customer.Address.City" -> "City") from MapConfigProperty / PropertyMap
        foreach (var rename in propertyRenames.Where(r => r.Source.IndexOf('.') >= 0))
        {
            var chain = ResolveSourcePath(sourceProperties, rename.Source);
            var targetProp = targetProperties.FirstOrDefault(t =>
                string.Equals(t.Name, rename.Target, StringComparison.OrdinalIgnoreCase));

            if (chain is null || targetProp is null)
            {
                continue;
            }

            var sourcePath = BuildSourcePath(chain, targetProp.Type, out var isCompatible);
            if (!isCompatible)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        PropertyTypesNotCompatibleDescriptor,
                        diagnosticLocation ?? Location.None,
                        rename.Source,
                        sourceType.Name,
                        targetType.Name,
                        chain[chain.Count - 1].Type.ToDisplayString(),
                        targetProp.Type.ToDisplayString()));
                continue;
            }

            mappings.Add(new PropertyMapping(
                SourceProperty: chain[0],
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
                TargetRequiresUnsafeAccessor: false,
                SourcePath: sourcePath));
        }

        // Handle flattening if enabled
        if (enableFlattening)
        {
            mappings.AddRange(MappingTypeAnalyzer.GetFlattenedMappings(
                sourceProperties,
                targetProperties,
                sourceProp => ignoredProperties.Any(ip => string.Equals(ip, sourceProp.Name, StringComparison.OrdinalIgnoreCase)),
                MappingTypeAnalyzer.CreateFlattenedMapping));
        }

        // Report settable target properties that nothing maps to (Info by default; raise it with
        // "dotnet_diagnostic.ATCMCF016.severity = error" for strict mapping). Only for user-declared mappings.
        if (diagnosticLocation is not null)
        {
            var mappedTargetNames = new HashSet<string>(mappings.Select(m => m.TargetProperty.Name), StringComparer.OrdinalIgnoreCase);
            foreach (var targetProp in targetProperties.Where(t => t.SetMethod is not null && !mappedTargetNames.Contains(t.Name)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        TargetPropertyNotMappedDescriptor,
                        diagnosticLocation,
                        targetProp.Name,
                        targetType.Name,
                        sourceType.Name));
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

    private static (IMethodSymbol? Constructor, List<string> ParameterNames, List<ConstructorDefaultParameter> DefaultParameters) FindBestConstructor(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType,
        List<(string Source, string Target)>? propertyRenames = null,
        Dictionary<string, string>? conversions = null)
    {
        var sourceProperties = MappingTypeAnalyzer.GetAllProperties(sourceType, includePrivateMembers: false);

        // Configuration mappings also convert with user conversion methods and numeric casts, and generate the
        // nested mappers of dictionary values
        return MappingTypeAnalyzer.FindBestConstructor(
            targetType,
            parameterName => FindSourcePropertyForParameter(sourceProperties, parameterName, propertyRenames),
            (sourcePropertyType, parameterType) =>
                MappingTypeAnalyzer.IsConstructorParameterCompatible(sourcePropertyType, parameterType) ||
                TryGetUserConversion(conversions, sourcePropertyType, parameterType, out _) ||
                MappingTypeAnalyzer.IsNumericTypeConversion(sourcePropertyType, parameterType) ||
                IsMappableDictionary(sourcePropertyType, parameterType, out _, out _));
    }

    /// <summary>
    /// Resolves a source property name or a dotted path ("Customer.Address.City") to its chain of properties,
    /// or <c>null</c> when a segment doesn't exist.
    /// </summary>
    private static List<IPropertySymbol>? ResolveSourcePath(
        List<IPropertySymbol> sourceProperties,
        string path)
    {
        var chain = new List<IPropertySymbol>();
        var properties = sourceProperties;

        foreach (var segment in path.Split('.'))
        {
            var property = properties.FirstOrDefault(p => string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));
            if (property is null)
            {
                return null;
            }

            chain.Add(property);
            properties = MappingTypeAnalyzer.UnwrapNullable(property.Type) is INamedTypeSymbol nextType
                ? MappingTypeAnalyzer.GetAllProperties(nextType, includePrivateMembers: false)
                : [];
        }

        return chain;
    }

    /// <summary>
    /// Builds the access expression for a resolved source path, using "?." after every segment that can be null,
    /// and checks that the last property can be assigned to the target type.
    /// </summary>
    private static string BuildSourcePath(
        List<IPropertySymbol> chain,
        ITypeSymbol targetType,
        out bool isCompatible)
    {
        var sb = new StringBuilder(chain[0].Name);
        var usesNullConditional = false;

        for (var i = 1; i < chain.Count; i++)
        {
            var previousType = chain[i - 1].Type;
            var canBeNull = !previousType.IsValueType || MappingTypeAnalyzer.IsNullableValueType(previousType);
            sb.Append(canBeNull ? "?." : ".");
            sb.Append(chain[i].Name);
            usesNullConditional |= canBeNull;
        }

        var lastType = chain[chain.Count - 1].Type;
        isCompatible = SymbolEqualityComparer.Default.Equals(
                           MappingTypeAnalyzer.UnwrapNullable(lastType),
                           MappingTypeAnalyzer.UnwrapNullable(targetType)) ||
                       MappingTypeAnalyzer.IsImplicitlyConvertible(lastType, targetType);

        if (usesNullConditional)
        {
            sb.Append(MappingTypeAnalyzer.GetNullConditionalSuffix(lastType, targetType));
        }
        else if (MappingTypeAnalyzer.IsNullableToNonNullableValueType(lastType, targetType))
        {
            sb.Append(" ?? default");
        }

        return sb.ToString();
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
                return ResolveSourcePath(sourceProperties, rename.Source)?.LastOrDefault();
            }
        }

        return null;
    }

    private static void AddNamespace(
        HashSet<string> namespaces,
        INamespaceSymbol? namespaceSymbol)
    {
        // Types in the global namespace need no using directive
        if (NamespaceHelper.GetNamespaceName(namespaceSymbol) is { } name)
        {
            namespaces.Add(name);
        }
    }

    /// <summary>
    /// Gets the user-implemented conversion methods of a configuration class: static, non-partial methods with one
    /// parameter and a return value, keyed by "source type -> target type". They are used for every property pair
    /// with exactly these types (before the built-in conversions).
    /// </summary>
    private static Dictionary<string, string>? GetUserConversions(
        INamedTypeSymbol? containingClass)
        => MappingTypeAnalyzer.GetConversionMethods(
            containingClass,
            static _ => true, // nested in the same class: any accessibility
            static method => method.Name);

    private static bool TryGetUserConversion(
        Dictionary<string, string>? conversions,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        out string? methodName)
    {
        methodName = MappingTypeAnalyzer.FindConversionMethod(conversions, sourceType, targetType);
        return methodName is not null;
    }

    private static bool IsMappableDictionary(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        out ITypeSymbol? sourceValueType,
        out ITypeSymbol? targetValueType)
        => MappingTypeAnalyzer.TryGetDictionaryMapping(sourceType, targetType, out sourceValueType, out targetValueType) &&
           (MappingTypeAnalyzer.AreDictionaryValuesDirectlyConvertible(sourceValueType!, targetValueType!) ||
            MappingTypeAnalyzer.IsDictionaryValueNestedMapping(sourceValueType!, targetValueType!));

    private static void DetectDuplicatesWithAttributeMappings(
        ImmutableArray<(string Source, string Target)> attributeMappedPairs,
        List<ConfiguredMappingInfo> configuredMappings,
        SourceProductionContext context)
    {
        // [MapTo] (source, target) pairs of the compilation, collected by the incremental pipeline
        var attributeMappedTypes = new HashSet<(string Source, string Target)>(
            attributeMappedPairs,
            new TypePairComparer());

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

            // Collect namespaces (System.Linq for the collection Select/ToList/ToArray calls)
            namespaces.Add("System.Linq");
            foreach (var mapping in classMappings)
            {
                AddNamespace(namespaces, mapping.SourceType.ContainingNamespace);
                AddNamespace(namespaces, mapping.TargetType.ContainingNamespace);
            }

            sb.AppendLineLf("// <auto-generated/>");
            sb.AppendLineLf("#nullable enable");
            sb.AppendLineLf();

            foreach (var ns in namespaces.OrderBy(x => x, StringComparer.Ordinal))
            {
                sb.AppendLineLf($"using {ns};");
            }

            sb.AppendLineLf();

            // A configuration class in the global namespace gets no namespace declaration
            if (NamespaceHelper.GetNamespaceName(containingClass.ContainingNamespace) is { } classNamespace)
            {
                sb.AppendLineLf($"namespace {classNamespace};");
                sb.AppendLineLf();
            }

            sb.AppendLineLf("[global::System.CodeDom.Compiler.GeneratedCode(\"Atc.SourceGenerators.MappingConfiguration\", \"1.0.0\")]");
            sb.AppendLineLf($"public static partial class {containingClass.Name}");
            sb.AppendLineLf("{");

            // Forward and reverse mappings: both can need enum and nested type mapping methods
            var allClassMappings = new List<ConfiguredMappingInfo>(classMappings);

            foreach (var mapping in classMappings)
            {
                GenerateMappingMethodBody(sb, mapping);

                // Generate reverse mapping if bidirectional
                if (mapping.Bidirectional)
                {
                    allClassMappings.Add(GenerateReverseMappingMethod(sb, mapping, autoDetectedEnums, context));
                }
            }

            // Generate auto-detected enum mapping methods
            GenerateAutoDetectedEnumMethods(sb, autoDetectedEnums, allClassMappings);

            // Generate nested type mapping methods for collection element types
            GenerateCollectionElementTypeMappingMethods(sb, allClassMappings, autoDetectedEnums, context);

            sb.AppendLineLf("}");

            // Include the namespace, so same-named configuration classes in different namespaces don't share a file name
            var fileName = $"{NamespaceHelper.GetQualifier(containingClass.ContainingNamespace)}{containingClass.Name}.g.cs";
            context.AddSource(fileName, SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private static void GenerateAssemblyLevelCode(
        List<ConfiguredMappingInfo> mappings,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        var sb = new StringBuilder();
        var namespaces = new HashSet<string>(StringComparer.Ordinal) { "System.Linq" };

        foreach (var mapping in mappings)
        {
            AddNamespace(namespaces, mapping.SourceType.ContainingNamespace);
            AddNamespace(namespaces, mapping.TargetType.ContainingNamespace);
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

        // Forward and reverse mappings: both can need enum and nested type mapping methods
        var allMappings = new List<ConfiguredMappingInfo>(mappings);

        foreach (var mapping in mappings)
        {
            GenerateExtensionMethodBody(sb, mapping);

            if (mapping.Bidirectional)
            {
                allMappings.Add(GenerateReverseExtensionMethod(sb, mapping, autoDetectedEnums, context));
            }
        }

        // Generate auto-detected enum mapping methods
        GenerateAutoDetectedEnumMethods(sb, autoDetectedEnums, allMappings);

        // Generate nested type mapping methods for collection element types
        GenerateCollectionElementTypeMappingMethods(sb, allMappings, autoDetectedEnums, context);

        sb.AppendLineLf("}");

        context.AddSource("ConfiguredMappingExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateMappingMethodBody(
        StringBuilder sb,
        ConfiguredMappingInfo mapping)
    {
        var methodName = mapping.Method!.Name;
        var targetTypeName = mapping.TargetType.ToGlobalDisplayString();
        var sourceTypeName = mapping.SourceType.ToGlobalDisplayString();

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
        var targetTypeName = mapping.TargetType.ToGlobalDisplayString();
        var sourceTypeName = mapping.SourceType.ToGlobalDisplayString();

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
        var targetTypeName = mapping.TargetType.ToGlobalDisplayString();

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
                var comma = isLast ? string.Empty : ",";

                sb.AppendLineLf($"            {value}{comma}");
            }

            if (initializerProps.Count > 0)
            {
                sb.AppendLineLf("        )");
                sb.AppendLineLf("        {");
                MappingCodeEmitter.AppendPropertyInitializers(sb, initializerProps, p => GeneratePropertyMappingValue(p, "source"));
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
            MappingCodeEmitter.AppendPropertyInitializers(sb, mapping.PropertyMappings, p => GeneratePropertyMappingValue(p, "source"));
            sb.AppendLineLf("        };");
        }
    }

    private static ConfiguredMappingInfo GenerateReverseMappingMethod(
        StringBuilder sb,
        ConfiguredMappingInfo mapping,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
    {
        var reverseMethodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(mapping.SourceType.Name)}";
        var sourceTypeName = mapping.TargetType.ToGlobalDisplayString();
        var targetTypeName = mapping.SourceType.ToGlobalDisplayString();

        var reverseMappings = GetPropertyMappingsWithConfig(
            mapping.TargetType,
            mapping.SourceType,
            mapping.EnableFlattening,
            mapping.PropertyNameStrategy,
            [],
            [],
            autoDetectedEnums,
            context,
            mapping.Method?.Locations.FirstOrDefault(),
            GetUserConversions(mapping.ContainingClass));

        var (reverseConstructor, reverseConstructorParams, reverseConstructorDefaults) = FindBestConstructor(mapping.TargetType, mapping.SourceType, conversions: GetUserConversions(mapping.ContainingClass));

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

        return reverseMapping;
    }

    private static ConfiguredMappingInfo GenerateReverseExtensionMethod(
        StringBuilder sb,
        ConfiguredMappingInfo mapping,
        List<AutoDetectedEnumMapping> autoDetectedEnums,
        SourceProductionContext context)
        => GenerateReverseMappingMethod(sb, mapping, autoDetectedEnums, context);

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
                     SymbolEqualityComparer.Default.Equals(p.CollectionElementType, enumMapping.TargetEnum)) ||
                    (p.IsDictionary &&
                     p.HasEnumMapping &&
                     SymbolEqualityComparer.Default.Equals(p.SourceCollectionElementType, enumMapping.SourceEnum) &&
                     SymbolEqualityComparer.Default.Equals(p.CollectionElementType, enumMapping.TargetEnum))));

            if (!isReferenced)
            {
                continue;
            }

            var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(enumMapping.TargetEnum.Name)}";

            sb.AppendLineLf("    /// <summary>");
            sb.AppendLineLf($"    /// Maps <see cref=\"{enumMapping.SourceEnum.ToGlobalDisplayString()}\"/> to <see cref=\"{enumMapping.TargetEnum.ToGlobalDisplayString()}\"/>.");
            sb.AppendLineLf("    /// </summary>");
            sb.AppendLineLf($"    private static {enumMapping.TargetEnum.ToGlobalDisplayString()} {methodName}(");
            sb.AppendLineLf($"        this {enumMapping.SourceEnum.ToGlobalDisplayString()} source)");
            sb.AppendLineLf("        => source switch");
            sb.AppendLineLf("        {");

            foreach (var valueMapping in enumMapping.ValueMappings)
            {
                if (valueMapping.IsMapped && valueMapping.TargetValue is not null)
                {
                    sb.AppendLineLf($"            {enumMapping.SourceEnum.ToGlobalDisplayString()}.{CSharpIdentifier.Escape(valueMapping.SourceValue)} => {enumMapping.TargetEnum.ToGlobalDisplayString()}.{CSharpIdentifier.Escape(valueMapping.TargetValue)},");
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

        // Collect all top-level mapping pairs (source -> target) with their method names, so we don't duplicate them
        var topLevelMappingPairs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var mapping in mappings)
        {
            var pairKey = $"{mapping.SourceType.ToDisplayString()}->{mapping.TargetType.ToDisplayString()}";
            if (!topLevelMappingPairs.ContainsKey(pairKey))
            {
                topLevelMappingPairs[pairKey] = mapping.Method?.Name ??
                                                $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(mapping.TargetType.Name)}";
            }
        }

        // User conversion methods of the configuration class (nested mappers are generated into that class)
        var classConversions = GetUserConversions(mappings.Select(m => m.ContainingClass).FirstOrDefault(c => c is not null));

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

            // Generate a nested mapping method for this type pair
            var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(targetElementType.Name)}";

            // Skip if already generated
            if (!generated.Add(key))
            {
                continue;
            }

            // A top-level mapping exists for this pair. Nested calls use MapTo{TargetName}, so when the declared
            // mapper has another name (e.g. MapToDomainGridSettings), forward to it instead of failing with CS0103.
            if (topLevelMappingPairs.TryGetValue(key, out var declaredMethodName))
            {
                if (!string.Equals(declaredMethodName, methodName, StringComparison.Ordinal))
                {
                    sb.AppendLineLf("    /// <summary>");
                    sb.AppendLineLf($"    /// Maps <see cref=\"{sourceElementType.ToGlobalDisplayString()}\"/> to <see cref=\"{targetElementType.ToGlobalDisplayString()}\"/> with {declaredMethodName}.");
                    sb.AppendLineLf("    /// </summary>");
                    sb.AppendLineLf($"    private static {targetElementType.ToGlobalDisplayString()} {methodName}(");
                    sb.AppendLineLf($"        this {sourceElementType.ToGlobalDisplayString()} source)");
                    sb.AppendLineLf($"        => {declaredMethodName}(source);");
                    sb.AppendLineLf();
                }

                continue;
            }

            var elementPropertyMappings = GetPropertyMappingsWithConfig(
                sourceElementType,
                targetElementType,
                enableFlattening: false,
                PropertyNameStrategy.PascalCase,
                [],
                [],
                autoDetectedEnums,
                context,
                conversions: classConversions);

            var (constructor, constructorParameterNames, constructorDefaultParameters) = FindBestConstructor(sourceElementType, targetElementType, conversions: classConversions);

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
            sb.AppendLineLf($"    /// Maps <see cref=\"{sourceElementType.ToGlobalDisplayString()}\"/> to <see cref=\"{targetElementType.ToGlobalDisplayString()}\"/>.");
            sb.AppendLineLf("    /// </summary>");
            sb.AppendLineLf($"    private static {targetElementType.ToGlobalDisplayString()} {methodName}(");
            sb.AppendLineLf($"        this {sourceElementType.ToGlobalDisplayString()} source)");
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
                // Collection element and dictionary value type pairs
                if ((prop.IsCollection || prop.IsDictionary) &&
                    prop.SourceCollectionElementType is not null &&
                    prop.CollectionElementType is not null &&
                    !SymbolEqualityComparer.Default.Equals(prop.SourceCollectionElementType, prop.CollectionElementType) &&
                    !prop.HasEnumMapping &&
                    prop.SourceCollectionElementType is INamedTypeSymbol sourceElem &&
                    prop.CollectionElementType is INamedTypeSymbol targetElem &&
                    sourceElem.SpecialType == SpecialType.None &&
                    targetElem.SpecialType == SpecialType.None &&
                    sourceElem.TypeKind != TypeKind.Enum &&
                    targetElem.TypeKind != TypeKind.Enum)
                {
                    // Strip nullable reference annotations, so the nested mapper isn't generated for 'T?'
                    queue.Enqueue((
                        (INamedTypeSymbol)sourceElem.WithNullableAnnotation(NullableAnnotation.NotAnnotated),
                        (INamedTypeSymbol)targetElem.WithNullableAnnotation(NullableAnnotation.NotAnnotated)));
                }

                // Non-collection nested property type pairs
                if (prop.IsNested)
                {
                    var unwrappedSource = MappingTypeAnalyzer.UnwrapNullable(prop.SourceProperty.Type)
                        .WithNullableAnnotation(NullableAnnotation.NotAnnotated) as INamedTypeSymbol;
                    var unwrappedTarget = MappingTypeAnalyzer.UnwrapNullable(prop.TargetProperty.Type)
                        .WithNullableAnnotation(NullableAnnotation.NotAnnotated) as INamedTypeSymbol;

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

    private static string GeneratePropertyMappingValue(
        PropertyMapping prop,
        string sourceVariable)
    {
        if (prop.ConversionMethod is not null)
        {
            return $"{prop.ConversionMethod}({sourceVariable}.{prop.SourceProperty.Name})";
        }

        if (prop.SourcePath is not null)
        {
            return $"{sourceVariable}.{prop.SourcePath}";
        }

        if (prop.IsFlattened && prop.FlattenedNestedProperty is not null)
        {
            var isNullable = prop.SourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                             !prop.SourceProperty.Type.IsValueType;

            if (isNullable)
            {
                var suffix = MappingTypeAnalyzer.GetNullConditionalSuffix(prop.FlattenedNestedProperty.Type, prop.TargetProperty.Type);
                return $"{sourceVariable}.{prop.SourceProperty.Name}?.{prop.FlattenedNestedProperty.Name}{suffix}";
            }

            return $"{sourceVariable}.{prop.SourceProperty.Name}.{prop.FlattenedNestedProperty.Name}";
        }

        if (prop.IsBuiltInTypeConversion)
        {
            return MappingCodeEmitter.GetBuiltInTypeConversion(prop, sourceVariable);
        }

        if (prop.IsDictionary)
        {
            return $"{sourceVariable}.{prop.SourceProperty.Name}?.ToDictionary(x => x.Key, x => {MappingCodeEmitter.GetDictionaryValueConversion(prop, static (method, value) => $"{method}({value})")})!";
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
                return MappingCodeEmitter.GetCollectionConversion(sourceAccess, collectionType, prop.CollectionElementType!, elementSelector: null);
            }

            var elementTypeName = prop.CollectionElementType!.Name;
            var mappingMethodName = $"MapTo{elementTypeName}";

            // Element mappers for nested types are generated into the same class and are called with a
            // simple name (see the nested-property case below). Enum mappers can come from the enum
            // mapping generator ([MapTo] on the enum), so they keep extension syntax.
            var elementMapCall = prop.HasEnumMapping
                ? $"x.{mappingMethodName}()"
                : $"{mappingMethodName}(x)";

            return MappingCodeEmitter.GetCollectionConversion(sourceAccess, collectionType, prop.CollectionElementType, $"x => {elementMapCall}");
        }

        if (prop.RequiresConversion)
        {
            if (prop.HasEnumMapping)
            {
                return MappingCodeEmitter.GetEnumMappingCall(prop, $"{sourceVariable}.{prop.SourceProperty.Name}");
            }

            // Numeric conversion - handle nullable wrapping
            return $"({prop.TargetProperty.Type.ToGlobalDisplayString()}){sourceVariable}.{prop.SourceProperty.Name}";
        }

        if (prop.IsNested)
        {
            var unwrappedSourceType = MappingTypeAnalyzer.UnwrapNullable(prop.SourceProperty.Type);
            var unwrappedTargetType = MappingTypeAnalyzer.UnwrapNullable(prop.TargetProperty.Type);
            var nestedMethodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(unwrappedTargetType.Name)}";
            var sourceIsNullable = MappingTypeAnalyzer.IsNullableValueType(prop.SourceProperty.Type);
            var targetIsNullable = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type);

            var sourceAccess = $"{sourceVariable}.{prop.SourceProperty.Name}";

            // Nested mappers are generated into the same class, so they are called with a simple name
            // (not extension syntax). That way a public mapper for the same type pair in another
            // mapping class of the same namespace can't make the call ambiguous (CS0121).
            if (prop.SourceProperty.Type.IsValueType && !sourceIsNullable)
            {
                return $"{nestedMethodName}({sourceAccess})";
            }

            var varName = $"__{char.ToLowerInvariant(prop.SourceProperty.Name[0])}{prop.SourceProperty.Name.Substring(1)}";
            var whenNull = "null!";
            if (targetIsNullable)
            {
                whenNull = "null";
            }
            else if (prop.TargetProperty.Type.IsValueType)
            {
                whenNull = "default";
            }

            return $"{sourceAccess} is {{ }} {varName} ? {nestedMethodName}({varName}) : {whenNull}";
        }

        // T? → T for value types (e.g. Guid? → Guid): fall back to default when null
        if (MappingTypeAnalyzer.IsNullableToNonNullableValueType(prop.SourceProperty.Type, prop.TargetProperty.Type))
        {
            return $"{sourceVariable}.{prop.SourceProperty.Name} ?? default";
        }

        return $"{sourceVariable}.{prop.SourceProperty.Name}";
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
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MappingConfigurationAttribute : global::System.Attribute
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
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapConfigIgnoreAttribute : global::System.Attribute
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
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapConfigPropertyAttribute : global::System.Attribute
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
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapConfigOptionsAttribute : global::System.Attribute
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
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapTypesAttribute : global::System.Attribute
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
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MappingBuilder
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