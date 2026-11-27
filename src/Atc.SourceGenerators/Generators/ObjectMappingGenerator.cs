// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
// ReSharper disable DuplicatedStatements
namespace Atc.SourceGenerators.Generators;

/// <summary>
/// Source generator for automatic object-to-object mapping.
/// </summary>
[SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK")]
[Generator]
public class ObjectMappingGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Atc.SourceGenerators.Annotations";
    private const string AttributeName = "MapToAttribute";
    private const string FullAttributeName = $"{AttributeNamespace}.{AttributeName}";

    private static readonly DiagnosticDescriptor MappingClassMustBePartialDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.MappingClassMustBePartial,
        title: "Mapping class must be partial",
        messageFormat: "Class '{0}' must be declared as partial to enable mapping generation",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.MappingClassMustBePartial));

    private static readonly DiagnosticDescriptor TargetTypeMustBeClassOrStructDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.TargetTypeMustBeClassOrStruct,
        title: "Target type must be a class or struct",
        messageFormat: "Target type '{0}' must be a class or struct, but is a {1}",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.TargetTypeMustBeClassOrStruct));

    private static readonly DiagnosticDescriptor MapPropertyTargetNotFoundDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.MapPropertyTargetNotFound,
        title: "MapProperty target property not found",
        messageFormat: "Property '{0}' with [MapProperty(\"{1}\")] specifies target property '{1}' which does not exist on target type '{2}'",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.MapPropertyTargetNotFound));

    private static readonly DiagnosticDescriptor RequiredPropertyNotMappedDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.RequiredPropertyNotMapped,
        title: "Required property on target type has no mapping",
        messageFormat: "Required property '{0}' on target type '{1}' has no mapping from source type '{2}'",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.RequiredPropertyNotMapped));

    private static readonly DiagnosticDescriptor CollectionElementMappingNotFoundDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.CollectionElementMappingNotFound,
        title: "Collection element type has no mapping method",
        messageFormat: "Property '{0}' contains elements of type '{1}' (collection items or dictionary values) but that type has no [MapTo] attribute targeting '{2}'. The property will be skipped.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.CollectionElementMappingNotFound));

    private static readonly DiagnosticDescriptor TargetPropertyNotMappedDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.TargetPropertyNotMapped,
        title: "Target property is not mapped",
        messageFormat: "Target property '{0}' of '{1}' is not mapped from '{2}' and keeps its default value",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.TargetPropertyNotMapped));

    private static readonly DiagnosticDescriptor MapPropertySourcePathInvalidDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.MapPropertySourcePathInvalid,
        title: "MapProperty source path can't be mapped",
        messageFormat: "[MapProperty(\"{0}\", \"{1}\")] on '{2}' can't be mapped: {3}",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.MapPropertySourcePathInvalid));

    private static readonly DiagnosticDescriptor DeepCloneSharesReferenceDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.DeepCloneSharesReference,
        title: "DeepClone copies a property by reference",
        messageFormat: "DeepClone copies property '{0}' by reference: '{1}' has no [MapTo(typeof({1}))] to clone it with",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.DeepCloneSharesReference));

    private static readonly DiagnosticDescriptor NestedMappingNotFoundDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.NestedMappingNotFound,
        title: "Nested property type has no mapping",
        messageFormat: "Property '{0}' is not mapped: there is no mapping from '{1}' to '{2}'. Add [MapTo(typeof({2}))] to '{1}', or declare a '{3}' extension method.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.NestedMappingNotFound));

    private static readonly DiagnosticDescriptor HookMethodNotCallableDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.HookMethodNotCallable,
        title: "Mapping hook method cannot be called",
        messageFormat: "{0} method '{1}' must be a static, internal or public method on '{2}'. The hook is skipped.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.HookMethodNotCallable));

    private static readonly DiagnosticDescriptor DuplicateMappingMethodDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.DuplicateMappingMethod,
        title: "Duplicate mapping method",
        messageFormat: "Mapping '{0}' to '{1}' would generate a second '{2}' method for '{0}'. Target types must have different names; this mapping is skipped.",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: DiagnosticHelpLink.For(RuleIdentifierConstants.ObjectMapping.DuplicateMappingMethod));

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate the attribute definitions as fallback
        // as internal [Embedded] types, so projects referencing each other don't see each other's copies (no CS0436)
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource("MapToAttribute.g.cs", SourceText.From(GenerateAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapIgnoreAttribute.g.cs", SourceText.From(GenerateMapIgnoreAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapPropertyAttribute.g.cs", SourceText.From(GenerateMapPropertyAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapDerivedTypeAttribute.g.cs", SourceText.From(GenerateMapDerivedTypeAttributeSource(), Encoding.UTF8));
        });

        // Find classes and records with [MapTo] (ForAttributeWithMetadataName filters by attribute name
        // without resolving the symbol of every attribute on every class)
        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                FullAttributeName,
                predicate: static (s, _) => s is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.TargetNode)
            .Collect();

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations);

        // Generate source
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<TypeDeclarationSyntax> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var mappingsToGenerate = new List<MappingInfo>();

        // Deduplicate by fully qualified symbol name to handle partial classes across multiple files
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var classDeclaration in classes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken) is not { } classSymbol)
            {
                continue;
            }

            if (!seen.Add(classSymbol.ToDisplayString()))
            {
                continue;
            }

            var mappingInfos = ExtractMappingInfo(classSymbol, context);
            if (mappingInfos is not null)
            {
                mappingsToGenerate.AddRange(mappingInfos);
            }
        }

        if (mappingsToGenerate.Count == 0)
        {
            return;
        }

        // Two mappings from the same source type must not generate the same MapToX method (CS0111)
        var skippedReverseMappings = RemoveDuplicateMappingMethods(mappingsToGenerate, context);

        // Nested properties whose MapTo{Target}() call wouldn't bind are skipped and reported (ATCMAP015)
        var nestedMappers = CreateNestedMapperLookup(compilation, mappingsToGenerate, skippedReverseMappings);
        foreach (var mapping in mappingsToGenerate)
        {
            mapping.PropertyMappings.RemoveAll(p => p.IsNested && !nestedMappers.HasMapper(p));
        }

        // Generate mapping extensions
        var source = GenerateMappingExtensions(mappingsToGenerate, skippedReverseMappings, nestedMappers);
        context.AddSource("ObjectMappingExtensions.g.cs", SourceText.From(source, Encoding.UTF8));

        foreach (var (property, sourceType, targetType) in nestedMappers.Missing)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    NestedMappingNotFoundDescriptor,
                    property.Locations.FirstOrDefault(l => l.IsInSource) ?? Location.None,
                    property.Name,
                    sourceType.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(),
                    MappingTypeAnalyzer.UnwrapNullable(targetType).WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(),
                    NestedMapperLookup.GetMethodName(targetType)));
        }
    }

    private static NestedMapperLookup CreateNestedMapperLookup(
        Compilation compilation,
        List<MappingInfo> mappings,
        HashSet<MappingInfo> skippedReverseMappings)
    {
        // The mappers this generator generates: forward, and reverse for bidirectional mappings
        var generated = new List<(INamedTypeSymbol Source, INamedTypeSymbol Target)>();
        foreach (var mapping in mappings)
        {
            generated.Add((mapping.SourceType, mapping.TargetType));
            if (mapping.Bidirectional && !skippedReverseMappings.Contains(mapping))
            {
                generated.Add((mapping.TargetType, mapping.SourceType));
            }
        }

        return new NestedMapperLookup(compilation, generated, GetGeneratedNamespaces(mappings));
    }

    private static List<MappingInfo>? ExtractMappingInfo(
        INamedTypeSymbol classSymbol,
        SourceProductionContext context)
    {
        // Check if class or record is partial
        if (!classSymbol.DeclaringSyntaxReferences.Any(r =>
            r.GetSyntax(context.CancellationToken) is TypeDeclarationSyntax t && t.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MappingClassMustBePartialDescriptor,
                    classSymbol.Locations.FirstOrDefault() ?? Location.None,
                    classSymbol.Name));
            return null;
        }

        var mappings = new List<MappingInfo>();

        // Get all MapTo attributes
        var attributes = classSymbol
            .GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == FullAttributeName);

        foreach (var attribute in attributes)
        {
            if (attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            var targetTypeValue = attribute.ConstructorArguments[0].Value;
            if (targetTypeValue is not INamedTypeSymbol targetType)
            {
                continue;
            }

            // Validate target type
            if (targetType.TypeKind != TypeKind.Class && targetType.TypeKind != TypeKind.Struct)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        TargetTypeMustBeClassOrStructDescriptor,
                        classSymbol.Locations.FirstOrDefault() ?? Location.None,
                        targetType.Name,
                        targetType
                            .TypeKind
                            .ToString()
                            .ToLowerInvariant()));
                continue;
            }

            // Extract Bidirectional, EnableFlattening, BeforeMap, AfterMap, Factory, UpdateTarget, GenerateProjection, IncludePrivateMembers, and PropertyNameStrategy properties
            var bidirectional = false;
            var enableFlattening = false;
            string? beforeMap = null;
            string? afterMap = null;
            string? factory = null;
            var updateTarget = false;
            var generateProjection = false;
            var includePrivateMembers = false;
            var deepClone = false;
            var propertyNameStrategy = PropertyNameStrategy.PascalCase;
            foreach (var namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "Bidirectional")
                {
                    bidirectional = namedArg.Value.Value as bool? ?? false;
                }
                else if (namedArg.Key == "EnableFlattening")
                {
                    enableFlattening = namedArg.Value.Value as bool? ?? false;
                }
                else if (namedArg.Key == "BeforeMap")
                {
                    beforeMap = namedArg.Value.Value as string;
                }
                else if (namedArg.Key == "AfterMap")
                {
                    afterMap = namedArg.Value.Value as string;
                }
                else if (namedArg.Key == "Factory")
                {
                    factory = namedArg.Value.Value as string;
                }
                else if (namedArg.Key == "UpdateTarget")
                {
                    updateTarget = namedArg.Value.Value as bool? ?? false;
                }
                else if (namedArg.Key == "GenerateProjection")
                {
                    generateProjection = namedArg.Value.Value as bool? ?? false;
                }
                else if (namedArg.Key == "IncludePrivateMembers")
                {
                    includePrivateMembers = namedArg.Value.Value as bool? ?? false;
                }
                else if (namedArg.Key == "DeepClone")
                {
                    deepClone = namedArg.Value.Value as bool? ?? false;
                }
                else if (namedArg.Key == "PropertyNameStrategy")
                {
                    if (namedArg.Value.Value is int strategyValue)
                    {
                        propertyNameStrategy = (PropertyNameStrategy)strategyValue;
                    }
                    else if (namedArg.Value.Value is not null)
                    {
                        // Try to convert to int in case it's a different numeric type
                        try
                        {
                            var value = Convert.ToInt32(namedArg.Value.Value, CultureInfo.InvariantCulture);
                            propertyNameStrategy = (PropertyNameStrategy)value;
                        }
                        catch (FormatException)
                        {
                            // If conversion fails, keep default PascalCase
                        }
                        catch (OverflowException)
                        {
                            // If value is out of range, keep default PascalCase
                        }
                        catch (InvalidCastException)
                        {
                            // If value cannot be cast, keep default PascalCase
                        }
                    }
                }
            }

            // Conversion methods of the [MapTo] class (static, one parameter, callable from the generated class)
            var conversions = GetConversionMethods(classSymbol);

            // Nested source paths of class-level [MapProperty("Customer.Name", "CustomerName")]
            var sourcePaths = GetSourcePaths(classSymbol);

            // The generated mapping is an extension method in another class: hooks must be static and accessible
            beforeMap = GetCallableHook(classSymbol, beforeMap, "BeforeMap", context);
            afterMap = GetCallableHook(classSymbol, afterMap, "AfterMap", context);
            factory = GetCallableHook(classSymbol, factory, "Factory", context);

            // Get property mappings
            var propertyMappings = GetPropertyMappings(classSymbol, targetType, enableFlattening, includePrivateMembers, propertyNameStrategy, context, deepClone, conversions, sourcePaths);

            // Find best matching constructor
            var (constructor, constructorParameterNames, constructorDefaultParameters) = FindBestConstructor(classSymbol, targetType, conversions, sourcePaths);

            // Extract derived type mappings from MapDerivedType attributes
            var derivedTypeMappings = GetDerivedTypeMappings(classSymbol);

            // Detect if this is a generic mapping
            // A generic mapping occurs when:
            // 1. The source type has type parameters (e.g., Result<T>)
            // 2. The target type is an unbound generic type (e.g., typeof(ResultDto<>))
            // 3. The arity (number of type parameters) matches
            var isGenericMapping = classSymbol.IsGenericType &&
                                   targetType.IsUnboundGenericType &&
                                   classSymbol.TypeParameters.Length == targetType.TypeParameters.Length;

            // For generic mappings, construct the target type with source's type parameters
            // This allows property mapping to work correctly
            var targetTypeForMapping = targetType;
            if (isGenericMapping)
            {
                var typeArguments = classSymbol.TypeParameters
                    .Cast<ITypeSymbol>()
                    .ToArray();
                targetTypeForMapping = targetType.ConstructedFrom.Construct(typeArguments);
            }

            // Get property mappings (using constructed target type for generics)
            var mappingsForGeneric = isGenericMapping
                ? GetPropertyMappings(classSymbol, targetTypeForMapping, enableFlattening, includePrivateMembers, propertyNameStrategy, context, deepClone, conversions, sourcePaths)
                : propertyMappings;

            // Find best matching constructor (using constructed target type for generics)
            var (constructorForGeneric, constructorParamsForGeneric, constructorDefaultsForGeneric) = isGenericMapping
                ? FindBestConstructor(classSymbol, targetTypeForMapping, conversions, sourcePaths)
                : (constructor, constructorParameterNames, constructorDefaultParameters);

            mappings.Add(new MappingInfo(
                SourceType: classSymbol,
                TargetType: targetType,
                PropertyMappings: isGenericMapping ? mappingsForGeneric : propertyMappings,
                Bidirectional: bidirectional,
                EnableFlattening: enableFlattening,
                Constructor: isGenericMapping ? constructorForGeneric : constructor,
                ConstructorParameterNames: isGenericMapping ? constructorParamsForGeneric : constructorParameterNames,
                ConstructorDefaultParameters: isGenericMapping ? constructorDefaultsForGeneric : constructorDefaultParameters,
                DerivedTypeMappings: derivedTypeMappings,
                BeforeMap: beforeMap,
                AfterMap: afterMap,
                Factory: factory,
                UpdateTarget: updateTarget,
                GenerateProjection: generateProjection,
                IsGeneric: isGenericMapping,
                IncludePrivateMembers: includePrivateMembers,
                DeepClone: deepClone,
                PropertyNameStrategy: propertyNameStrategy));
        }

        return mappings.Count > 0 ? mappings : null;
    }

    private static List<PropertyMapping> GetPropertyMappings(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType,
        bool enableFlattening,
        bool includePrivateMembers,
        PropertyNameStrategy propertyNameStrategy,
        SourceProductionContext? context = null,
        bool deepClone = false,
        Dictionary<string, string>? conversions = null,
        List<(string SourcePath, string Target)>? sourcePaths = null)
    {
        var mappings = new List<PropertyMapping>();

        // [MapTo] requires a [MapTo] mapping for collection elements and dictionary values (ATCMAP011 otherwise),
        // uses enum mappings from [MapTo] on enums and the conversion methods of the [MapTo] class, and casts numeric types
        var rules = new PropertyMappingRules
        {
            FindConversionMethod = (source, target) => MappingTypeAnalyzer.FindConversionMethod(conversions, source, target),
            CanMapDictionaryValue = HasObjectMappingToType,
            CanMapCollectionElement = HasObjectMappingToType,
            AllowNumericConversions = true,
        };

        // Collect properties from the source type including inherited properties from base classes
        var sourceProperties = MappingTypeAnalyzer.GetAllProperties(sourceType, includePrivateMembers);

        // Collect properties from the target type including inherited properties
        var targetProperties = MappingTypeAnalyzer.GetAllProperties(targetType, includePrivateMembers);

        foreach (var sourceProp in sourceProperties)
        {
            // Check if property has custom mapping via MapProperty attribute
            var customTargetName = GetMapPropertyTargetName(sourceProp);

            // Apply property name casing strategy if no custom mapping is specified
            var targetPropertyName = customTargetName ?? PropertyNameUtility.ConvertPropertyName(sourceProp.Name, propertyNameStrategy);

            // Validate that custom target property exists if MapProperty is used
            if (customTargetName is not null && context.HasValue)
            {
                var targetExists = targetProperties.Any(t =>
                    string.Equals(t.Name, customTargetName, StringComparison.OrdinalIgnoreCase));

                if (!targetExists)
                {
                    context.Value.ReportDiagnostic(
                        Diagnostic.Create(
                            MapPropertyTargetNotFoundDescriptor,
                            sourceProp.Locations.FirstOrDefault() ?? Location.None,
                            sourceProp.Name,
                            customTargetName,
                            targetType.Name));
                    continue; // Skip this property mapping
                }
            }

            // Find target property - use custom name if specified, otherwise use source property name
            var targetProp = targetProperties.FirstOrDefault(t =>
                string.Equals(t.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase) &&
                SymbolEqualityComparer.Default.Equals(t.Type, sourceProp.Type));

            if (targetProp is null)
            {
                // Types differ: find the target by name and classify the pair (nested, enum, collection, ...)
                targetProp = targetProperties.FirstOrDefault(t => string.Equals(t.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase));
                if (targetProp is null)
                {
                    continue;
                }

                var result = PropertyPairClassifier.Classify(sourceProp, targetProp, rules);
                if (result.Mapping is null)
                {
                    if (result.Failure == PropertyPairFailure.ElementHasNoMapping && context is not null)
                    {
                        // No mapping method found for the collection element or dictionary value type - report and skip
                        context.Value.ReportDiagnostic(
                            Diagnostic.Create(
                                CollectionElementMappingNotFoundDescriptor,
                                sourceProp.Locations.FirstOrDefault() ?? Location.None,
                                sourceProp.Name,
                                result.SourceElementType!.ToDisplayString(),
                                result.TargetElementType!.ToDisplayString()));
                    }

                    continue;
                }

                mappings.Add(WithAccessorFlags(result.Mapping, includePrivateMembers));
                continue;
            }

            // Direct type match: an assignment, or a copy for DeepClone
            var directMapping = deepClone
                ? CreateDeepCloneMapping(sourceProp, targetProp, context)
                : PropertyPairClassifier.Create(sourceProp, targetProp);
            mappings.Add(WithAccessorFlags(directMapping, includePrivateMembers));
        }

        // Nested source paths from [MapProperty("Customer.Address.City", "City")] on the class
        foreach (var (path, target) in sourcePaths ?? [])
        {
            AddSourcePathMapping(mappings, sourceProperties, targetProperties, sourceType, targetType, path, target, includePrivateMembers, context);
        }

        // Handle flattening if enabled
        if (enableFlattening)
        {
            mappings.AddRange(MappingTypeAnalyzer.GetFlattenedMappings(
                sourceProperties,
                targetProperties,
                MappingTypeAnalyzer.HasMapIgnoreAttribute,
                (sourceProp, targetProp, nestedProp) => MappingTypeAnalyzer.CreateFlattenedMapping(sourceProp, targetProp, nestedProp) with
                {
                    SourceRequiresUnsafeAccessor = includePrivateMembers && RequiresUnsafeAccessorForReading(sourceProp),
                    TargetRequiresUnsafeAccessor = includePrivateMembers && RequiresUnsafeAccessorForWriting(targetProp),
                },
                mappings));
        }

        // Validate required properties
        if (context.HasValue)
        {
            ValidateRequiredProperties(targetType, sourceType, mappings, context.Value);
        }

        return mappings;
    }

    /// <summary>
    /// DeepClone: a same-type property is copied instead of shared. Collections and dictionaries get new
    /// instances, and nested types, elements and values are cloned with their self-mapping
    /// ([MapTo(typeof(T))] on T). A class without a self-mapping can only be shared (ATCMAP016).
    /// </summary>
    private static PropertyMapping CreateDeepCloneMapping(
        IPropertySymbol sourceProp,
        IPropertySymbol targetProp,
        SourceProductionContext? context)
    {
        var type = sourceProp.Type;
        var mapping = PropertyPairClassifier.Create(sourceProp, targetProp);

        if (MappingTypeAnalyzer.TryGetDictionaryMapping(type, type, out _, out var valueType))
        {
            ReportIfSharedByReference(sourceProp, valueType!, context);
            return mapping with { IsDictionary = true, CollectionElementType = valueType, SourceCollectionElementType = valueType };
        }

        if (MappingTypeAnalyzer.IsCollectionType(type, out var elementType) && elementType is not null)
        {
            ReportIfSharedByReference(sourceProp, elementType, context);
            return mapping with
            {
                IsCollection = true,
                CollectionElementType = elementType,
                SourceCollectionElementType = elementType,
                CollectionTargetType = MappingTypeAnalyzer.GetCollectionTargetType(type),
            };
        }

        if (MappingTypeAnalyzer.IsNestedMapping(type, type) && HasObjectMappingToType(type, type))
        {
            return mapping with { IsNested = true };
        }

        ReportIfSharedByReference(sourceProp, type, context);
        return mapping;
    }

    private static void ReportIfSharedByReference(
        IPropertySymbol sourceProp,
        ITypeSymbol type,
        SourceProductionContext? context)
    {
        var unwrapped = MappingTypeAnalyzer.UnwrapNullable(type);
        if (context is null ||
            unwrapped.TypeKind != TypeKind.Class ||
            MappingTypeAnalyzer.IsSystemType(unwrapped) ||
            HasObjectMappingToType(unwrapped, unwrapped))
        {
            return;
        }

        context.Value.ReportDiagnostic(
            Diagnostic.Create(
                DeepCloneSharesReferenceDescriptor,
                sourceProp.Locations.FirstOrDefault() ?? Location.None,
                sourceProp.Name,
                unwrapped.Name));
    }

    private static bool IsSelfMapped(
        ITypeSymbol? sourceType,
        ITypeSymbol? targetType)
        => sourceType is not null &&
           SymbolEqualityComparer.Default.Equals(sourceType, targetType) &&
           HasObjectMappingToType(sourceType, sourceType);

    /// <summary>
    /// Gets the conversion methods of a [MapTo] class: static one-parameter methods that the generated extension
    /// class can call (internal or public), as global::Namespace.Class.Method.
    /// </summary>
    private static Dictionary<string, string>? GetConversionMethods(
        INamedTypeSymbol classSymbol)
        => MappingTypeAnalyzer.GetConversionMethods(
            classSymbol,
            static method => method.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal,
            method => $"{classSymbol.ToGlobalDisplayString()}.{method.Name}");

    /// <summary>
    /// Gets the class-level [MapProperty("Customer.Address.City", "City")] attributes (source path, target name).
    /// </summary>
    private static List<(string SourcePath, string Target)> GetSourcePaths(
        INamedTypeSymbol classSymbol)
        => classSymbol
            .GetAttributes()
            .Where(a => MappingTypeAnalyzer.IsAttributeMatch(a.AttributeClass, "MapPropertyAttribute") &&
                        a.ConstructorArguments.Length == 2 &&
                        a.ConstructorArguments[0].Value is string &&
                        a.ConstructorArguments[1].Value is string)
            .Select(a => ((string)a.ConstructorArguments[0].Value!, (string)a.ConstructorArguments[1].Value!))
            .ToList();

    /// <summary>
    /// Maps a nested source path null-safely (source.Customer?.Address?.City), replacing a by-name mapping of the
    /// same target property.
    /// </summary>
    private static void AddSourcePathMapping(
        List<PropertyMapping> mappings,
        List<IPropertySymbol> sourceProperties,
        List<IPropertySymbol> targetProperties,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType,
        string path,
        string target,
        bool includePrivateMembers,
        SourceProductionContext? context)
    {
        var location = sourceType.Locations.FirstOrDefault() ?? Location.None;
        var targetProp = targetProperties.FirstOrDefault(t => string.Equals(t.Name, target, StringComparison.OrdinalIgnoreCase));
        if (targetProp is null)
        {
            // A class-level [MapProperty] applies to every [MapTo] of the class: only an error when no target has the property
            var anyTargetHasProperty = sourceType
                .GetAttributes()
                .Where(a => MappingTypeAnalyzer.IsAttributeMatch(a.AttributeClass, "MapToAttribute"))
                .Select(a => a.ConstructorArguments.FirstOrDefault().Value)
                .OfType<INamedTypeSymbol>()
                .Any(t => MappingTypeAnalyzer.GetAllProperties(t, includePrivateMembers: true)
                    .Any(p => string.Equals(p.Name, target, StringComparison.OrdinalIgnoreCase)));
            if (!anyTargetHasProperty)
            {
                context?.ReportDiagnostic(Diagnostic.Create(MapPropertyTargetNotFoundDescriptor, location, path, target, targetType.Name));
            }

            return;
        }

        var chain = MappingTypeAnalyzer.ResolveSourcePath(sourceProperties, path);
        if (chain is null)
        {
            context?.ReportDiagnostic(Diagnostic.Create(
                MapPropertySourcePathInvalidDescriptor,
                location,
                path,
                target,
                sourceType.Name,
                $"'{path}' is not a property path of '{sourceType.Name}'"));
            return;
        }

        var sourcePath = MappingTypeAnalyzer.BuildSourcePath(chain, targetProp.Type, out var isCompatible);
        if (!isCompatible)
        {
            context?.ReportDiagnostic(Diagnostic.Create(
                MapPropertySourcePathInvalidDescriptor,
                location,
                path,
                target,
                sourceType.Name,
                $"'{chain[chain.Count - 1].Type.ToDisplayString()}' can't be converted to '{targetProp.Type.ToDisplayString()}'"));
            return;
        }

        mappings.RemoveAll(m => string.Equals(m.TargetProperty.Name, targetProp.Name, StringComparison.Ordinal));
        mappings.Add(WithAccessorFlags(PropertyPairClassifier.Create(chain[0], targetProp) with { SourcePath = sourcePath }, includePrivateMembers));
    }

    private static PropertyMapping WithAccessorFlags(
        PropertyMapping mapping,
        bool includePrivateMembers)
        => mapping with
        {
            SourceRequiresUnsafeAccessor = includePrivateMembers && RequiresUnsafeAccessorForReading(mapping.SourceProperty),
            TargetRequiresUnsafeAccessor = includePrivateMembers && RequiresUnsafeAccessorForWriting(mapping.TargetProperty),
        };

    /// <summary>
    /// Checks if a source dictionary can be copied into a target dictionary: same key type, and value types
    /// that are the same, two enums, or mapped with [MapTo].
    /// </summary>
    private static bool IsMappableDictionary(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        => MappingTypeAnalyzer.TryGetDictionaryMapping(sourceType, targetType, out var sourceValueType, out var targetValueType) &&
           (MappingTypeAnalyzer.AreDictionaryValuesDirectlyConvertible(sourceValueType!, targetValueType!) ||
            HasObjectMappingToType(sourceValueType!, targetValueType!));

    /// <summary>
    /// Checks if sourceElementType has a [MapTo] attribute targeting targetElementType,
    /// or if targetElementType has a bidirectional [MapTo] back to sourceElementType.
    /// </summary>
    private static bool HasObjectMappingToType(
        ITypeSymbol sourceElementType,
        ITypeSymbol targetElementType)
    {
        // Check if source has [MapTo(typeof(target))]
        foreach (var attr in sourceElementType.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() != FullAttributeName)
            {
                continue;
            }

            if (attr.ConstructorArguments.Length == 0)
            {
                continue;
            }

            if (attr.ConstructorArguments[0].Value is INamedTypeSymbol attrTargetType &&
                SymbolEqualityComparer.Default.Equals(attrTargetType, targetElementType))
            {
                return true;
            }
        }

        // Check if target has [MapTo(typeof(source), Bidirectional = true)]
        foreach (var attr in targetElementType.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() != FullAttributeName)
            {
                continue;
            }

            if (attr.ConstructorArguments.Length == 0)
            {
                continue;
            }

            if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol attrSourceType ||
                !SymbolEqualityComparer.Default.Equals(attrSourceType, sourceElementType))
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

    private static List<DerivedTypeMapping> GetDerivedTypeMappings(
        INamedTypeSymbol sourceType)
    {
        var derivedTypeMappings = new List<DerivedTypeMapping>();

        // Get all MapDerivedType attributes from the source type
        var mapDerivedTypeAttributes = sourceType
            .GetAttributes()
            .Where(attr => attr.AttributeClass?.Name == "MapDerivedTypeAttribute" &&
                           attr.AttributeClass.ContainingNamespace.ToDisplayString() == "Atc.SourceGenerators.Annotations")
            .ToList();

        foreach (var attr in mapDerivedTypeAttributes)
        {
            if (attr.ConstructorArguments.Length != 2)
            {
                continue;
            }

            // Extract source derived type and target derived type from attribute arguments
            var sourceDerivedTypeArg = attr.ConstructorArguments[0];
            var targetDerivedTypeArg = attr.ConstructorArguments[1];

            if (sourceDerivedTypeArg.Value is not INamedTypeSymbol sourceDerivedType ||
                targetDerivedTypeArg.Value is not INamedTypeSymbol targetDerivedType)
            {
                continue;
            }

            derivedTypeMappings.Add(new DerivedTypeMapping(
                SourceDerivedType: sourceDerivedType,
                TargetDerivedType: targetDerivedType));
        }

        return derivedTypeMappings;
    }

    private static void ValidateRequiredProperties(
        INamedTypeSymbol targetType,
        INamedTypeSymbol sourceType,
        List<PropertyMapping> mappings,
        SourceProductionContext context)
    {
        // Get all target properties
        var targetProperties = targetType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => (p.SetMethod is not null && p.SetMethod.DeclaredAccessibility == Accessibility.Public) &&
                        !MappingTypeAnalyzer.HasMapIgnoreAttribute(p))
            .ToList();

        // Get all mapped target property names
        var mappedPropertyNames = new HashSet<string>(
            mappings.Select(m => m.TargetProperty.Name),
            StringComparer.OrdinalIgnoreCase);

        // Check each target property to see if it's required and not mapped
        foreach (var targetProp in targetProperties)
        {
            // Skip if already mapped
            if (mappedPropertyNames.Contains(targetProp.Name))
            {
                continue;
            }

            // Check if property is required (C# 11+ required keyword)
            if (targetProp.IsRequired)
            {
                // Generate warning for unmapped required property
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        RequiredPropertyNotMappedDescriptor,
                        targetType.Locations.FirstOrDefault() ?? Location.None,
                        targetProp.Name,
                        targetType.Name,
                        sourceType.Name));
            }
            else
            {
                // Any other settable target property without a source keeps its default value: report it (Info by
                // default; raise it with "dotnet_diagnostic.ATCMAP014.severity = error" for strict mapping)
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        TargetPropertyNotMappedDescriptor,
                        sourceType.Locations.FirstOrDefault() ?? Location.None,
                        targetProp.Name,
                        targetType.Name,
                        sourceType.Name));
            }
        }
    }

    private static string? GetMapPropertyTargetName(IPropertySymbol property)
    {
        const string mapPropertyAttributeName = "Atc.SourceGenerators.Annotations.MapPropertyAttribute";

        var attributes = property.GetAttributes();
        var mapPropertyAttribute = attributes.FirstOrDefault(attr =>
            attr.AttributeClass?.ToDisplayString() == mapPropertyAttributeName);

        if (mapPropertyAttribute is null)
        {
            return null;
        }

        // Get the target property name from the attribute constructor argument
        if (mapPropertyAttribute.ConstructorArguments.Length > 0)
        {
            var targetPropertyName = mapPropertyAttribute.ConstructorArguments[0].Value as string;
            return targetPropertyName;
        }

        return null;
    }

    private static bool RequiresUnsafeAccessorForReading(
        IPropertySymbol property)
    {
        // Check if the property itself or its getter is not publicly accessible
        if (property.DeclaredAccessibility != Accessibility.Public)
        {
            return true;
        }

        if (property.GetMethod is not null &&
            property.GetMethod.DeclaredAccessibility != Accessibility.Public)
        {
            return true;
        }

        return false;
    }

    private static bool RequiresUnsafeAccessorForWriting(
        IPropertySymbol property)
    {
        // Check if the property itself or its setter is not publicly accessible
        if (property.DeclaredAccessibility != Accessibility.Public)
        {
            return true;
        }

        if (property.SetMethod is not null &&
            property.SetMethod.DeclaredAccessibility != Accessibility.Public)
        {
            return true;
        }

        return false;
    }

    private static (IMethodSymbol? Constructor, List<string> ParameterNames, List<ConstructorDefaultParameter> DefaultParameters) FindBestConstructor(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType,
        Dictionary<string, string>? conversions,
        List<(string SourcePath, string Target)>? sourcePaths = null)
    {
        // Source properties that we can map from, including inherited ones
        var sourceProperties = MappingTypeAnalyzer.GetAllProperties(sourceType, includePrivateMembers: true);

        // A parameter filled from a nested source path is checked against the path's last property; a source property
        // with [MapProperty("Other")] matches the parameter of that target, not one of its own name
        IPropertySymbol? FindSourceProperty(string parameterName)
        {
            var path = (sourcePaths ?? []).FirstOrDefault(p => string.Equals(p.Target, parameterName, StringComparison.OrdinalIgnoreCase));
            return path.SourcePath is not null
                ? MappingTypeAnalyzer.ResolveSourcePath(sourceProperties, path.SourcePath)?.LastOrDefault()
                : sourceProperties.FirstOrDefault(p => string.Equals(
                    GetMapPropertyTargetName(p) ?? p.Name,
                    parameterName,
                    StringComparison.OrdinalIgnoreCase));
        }

        return MappingTypeAnalyzer.FindBestConstructor(
            targetType,
            FindSourceProperty,
            (sourcePropertyType, parameterType) =>
                MappingTypeAnalyzer.IsConstructorParameterCompatible(sourcePropertyType, parameterType) ||
                MappingTypeAnalyzer.FindConversionMethod(conversions, sourcePropertyType, parameterType) is not null ||
                MappingTypeAnalyzer.IsNumericTypeConversion(sourcePropertyType, parameterType) ||
                IsMappableDictionary(sourcePropertyType, parameterType));
    }

    /// <summary>
    /// Checks if sourceType can be implicitly converted to targetType
    /// (e.g., int → long, or derived → base, or nullable unwrapping).
    /// </summary>
    private static void AddNamespaceIfValid(
        HashSet<string> namespaces,
        string? ns)
    {
        if (!string.IsNullOrEmpty(ns) && ns != "<global namespace>")
        {
            namespaces.Add(ns!);
        }
    }

    /// <summary>
    /// Removes mappings whose generated MapTo{TargetName} method would clash with another mapping from the same
    /// source type (reported as ATCMAP012), and returns the bidirectional mappings whose reverse method must not be
    /// generated: either because the target declares the same reverse mapping itself, or because it would clash.
    /// </summary>
    private static HashSet<MappingInfo> RemoveDuplicateMappingMethods(
        List<MappingInfo> mappings,
        SourceProductionContext context)
    {
        var methods = new Dictionary<(string Source, string Method), INamedTypeSymbol>();
        var duplicates = new List<MappingInfo>();

        foreach (var mapping in mappings)
        {
            var key = GetMappingMethodKey(mapping.SourceType, mapping.TargetType);
            if (methods.ContainsKey(key))
            {
                duplicates.Add(mapping);
                ReportDuplicateMappingMethod(context, mapping.SourceType, mapping.TargetType, key.Method);
                continue;
            }

            methods[key] = mapping.TargetType;
        }

        foreach (var duplicate in duplicates)
        {
            mappings.Remove(duplicate);
        }

        var skippedReverseMappings = new HashSet<MappingInfo>();
        foreach (var mapping in mappings.Where(m => m.Bidirectional))
        {
            var reverseKey = GetMappingMethodKey(mapping.TargetType, mapping.SourceType);
            if (methods.TryGetValue(reverseKey, out var existingTarget))
            {
                // The same reverse mapping declared explicitly is fine; a different type with the same name is a clash
                if (!SymbolEqualityComparer.Default.Equals(existingTarget.OriginalDefinition, mapping.SourceType.OriginalDefinition))
                {
                    ReportDuplicateMappingMethod(context, mapping.TargetType, mapping.SourceType, reverseKey.Method);
                }

                skippedReverseMappings.Add(mapping);
                continue;
            }

            methods[reverseKey] = mapping.SourceType;
        }

        return skippedReverseMappings;
    }

    /// <summary>
    /// Returns the hook method name when the class has a static, internal or public method with that name;
    /// otherwise reports ATCMAP013 and returns null, so no uncompilable call is generated.
    /// </summary>
    private static string? GetCallableHook(
        INamedTypeSymbol classSymbol,
        string? methodName,
        string hookKind,
        SourceProductionContext? context)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            return methodName;
        }

        var isCallable = classSymbol
            .GetMembers(methodName!)
            .OfType<IMethodSymbol>()
            .Any(m => m.IsStatic &&
                      m.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal);

        if (isCallable)
        {
            return methodName;
        }

        context?.ReportDiagnostic(
            Diagnostic.Create(
                HookMethodNotCallableDescriptor,
                classSymbol.Locations.FirstOrDefault() ?? Location.None,
                hookKind,
                methodName,
                classSymbol.Name));

        return null;
    }

    private static (string Source, string Method) GetMappingMethodKey(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType)
        => (sourceType.OriginalDefinition.ToDisplayString(), $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(targetType.Name)}");

    private static void ReportDuplicateMappingMethod(
        SourceProductionContext context,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType,
        string methodName)
        => context.ReportDiagnostic(
            Diagnostic.Create(
                DuplicateMappingMethodDescriptor,
                sourceType.Locations.FirstOrDefault() ?? Location.None,
                sourceType.ToDisplayString(),
                targetType.ToDisplayString(),
                methodName));

    /// <summary>
    /// Gets the namespaces the generated file imports.
    /// </summary>
    private static HashSet<string> GetGeneratedNamespaces(
        List<MappingInfo> mappings)
    {
        // System.Linq for the collection and dictionary Select/ToList/ToArray/ToDictionary calls,
        // so the generated code also compiles in projects without implicit usings
        var namespaces = new HashSet<string>(StringComparer.Ordinal) { "System.Linq" };

        // Collect all namespaces (skip global/empty namespaces)
        foreach (var mapping in mappings)
        {
            AddNamespaceIfValid(
                namespaces,
                mapping.SourceType.ContainingNamespace?.ToDisplayString());
            AddNamespaceIfValid(
                namespaces,
                mapping.TargetType.ContainingNamespace?.ToDisplayString());
        }

        return namespaces;
    }

    private static string GenerateMappingExtensions(
        List<MappingInfo> mappings,
        HashSet<MappingInfo> skippedReverseMappings,
        NestedMapperLookup nestedMappers)
    {
        var sb = new StringBuilder();

        var namespaces = GetGeneratedNamespaces(mappings);

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
        sb.AppendLineLf("/// <summary>");
        sb.AppendLineLf("/// Extension methods for object mapping.");
        sb.AppendLineLf("/// </summary>");
        sb.AppendLineLf("public static class ObjectMappingExtensions");
        sb.AppendLineLf("{");

        // Generate UnsafeAccessor methods for all mappings that need them (also the reverse direction of
        // bidirectional mappings), each accessor once
        var accessorsGenerated = new HashSet<string>(StringComparer.Ordinal);
        foreach (var mapping in mappings)
        {
            GenerateUnsafeAccessors(sb, mapping, accessorsGenerated);

            if (mapping.Bidirectional && mapping.IncludePrivateMembers && !skippedReverseMappings.Contains(mapping))
            {
                GenerateUnsafeAccessors(sb, BuildReverseMapping(mapping, nestedMappers), accessorsGenerated);
            }
        }

        foreach (var mapping in mappings)
        {
            GenerateMappingMethod(sb, mapping);

            // Generate projection method if requested
            if (mapping.GenerateProjection)
            {
                GenerateProjectionMethod(sb, mapping);
            }

            // Generate reverse mapping if bidirectional (unless it already exists or would clash)
            if (mapping.Bidirectional && !skippedReverseMappings.Contains(mapping))
            {
                var reverseMapping = BuildReverseMapping(mapping, nestedMappers);

                GenerateMappingMethod(sb, reverseMapping);
            }
        }

        sb.AppendLineLf("}");

        return sb.ToString();
    }

    private static void GenerateMappingMethod(
        StringBuilder sb,
        MappingInfo mapping)
    {
        var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(mapping.TargetType.Name)}";

        // Build type parameter list for generic methods
        var typeParamList = string.Empty;
        var typeConstraints = new List<string>();
        var targetTypeName = mapping.TargetType.ToGlobalDisplayString();

        if (mapping.IsGeneric)
        {
            var typeParams = mapping.SourceType.TypeParameters;
            typeParamList = $"<{string.Join(", ", typeParams.Select(tp => tp.Name))}>";

            // For generic types, get the base name without type parameters
            var targetBaseTypeName = mapping.TargetType.Name;
            targetTypeName = $"global::{NamespaceHelper.GetQualifier(mapping.TargetType.ContainingNamespace)}{targetBaseTypeName}";

            // Collect type parameter constraints
            foreach (var typeParam in typeParams)
            {
                var constraints = new List<string>();

                // Check for class/struct constraint
                if (typeParam.HasReferenceTypeConstraint)
                {
                    constraints.Add("class");
                }
                else if (typeParam.HasValueTypeConstraint)
                {
                    constraints.Add("struct");
                }

                // Add type constraints
                foreach (var constraintType in typeParam.ConstraintTypes)
                {
                    constraints.Add(constraintType.ToGlobalDisplayString());
                }

                // Check for new() constraint
                if (typeParam.HasConstructorConstraint && !typeParam.HasValueTypeConstraint)
                {
                    constraints.Add("new()");
                }

                if (constraints.Count > 0)
                {
                    typeConstraints.Add($"        where {typeParam.Name} : {string.Join(", ", constraints)}");
                }
            }
        }

        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Maps <see cref=\"{mapping.SourceType.ToGlobalDisplayString()}\"/> to <see cref=\"{targetTypeName}{typeParamList}\"/>.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf($"    public static {targetTypeName}{typeParamList} {methodName}{typeParamList}(");
        sb.AppendLineLf($"        this {mapping.SourceType.ToGlobalDisplayString()} source)");

        // Add type parameter constraints
        foreach (var constraint in typeConstraints)
        {
            sb.AppendLineLf(constraint);
        }

        sb.AppendLineLf("    {");
        sb.AppendLineLf("        if (source is null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf($"            return default!;");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();

        // Generate BeforeMap hook call
        if (!string.IsNullOrWhiteSpace(mapping.BeforeMap))
        {
            sb.AppendLineLf($"        {mapping.SourceType.ToGlobalDisplayString()}.{mapping.BeforeMap}(source);");
            sb.AppendLineLf();
        }

        // Check if this is a polymorphic mapping
        if (mapping.DerivedTypeMappings.Count > 0)
        {
            GeneratePolymorphicMapping(sb, mapping);
            sb.AppendLineLf("    }");
            sb.AppendLineLf();
            return;
        }

        // Determine if we need to use a variable for AfterMap hook or Factory
        // Private target members can't be set in an object initializer: they are set through the generated
        // UnsafeAccessor setter after the target is created
        var privateTargetProps = mapping.PropertyMappings.Where(p => p.TargetRequiresUnsafeAccessor).ToList();
        var initializableProps = mapping.PropertyMappings.Where(p => !p.TargetRequiresUnsafeAccessor).ToList();

        var needsTargetVariable = !string.IsNullOrWhiteSpace(mapping.AfterMap) ||
                                  !string.IsNullOrWhiteSpace(mapping.Factory) ||
                                  privateTargetProps.Count > 0;

        // Check if we should use factory method
        var useFactory = !string.IsNullOrWhiteSpace(mapping.Factory);

        if (useFactory)
        {
            // Use factory method to create the object
            sb.AppendLineLf($"        var target = {mapping.SourceType.ToGlobalDisplayString()}.{mapping.Factory}();");
            sb.AppendLineLf();

            // Apply property mappings to the factory-created instance
            foreach (var prop in initializableProps)
            {
                var value = GeneratePropertyMappingValue(prop, "source", mapping.SourceType);
                sb.AppendLineLf($"        target.{prop.TargetProperty.Name} = {value};");
            }
        }

        // Check if we should use constructor-based initialization
        else
        {
            var useConstructor = mapping.Constructor is not null && mapping.ConstructorParameterNames.Count > 0;

            if (useConstructor)
            {
                // Separate properties into constructor parameters and initializer properties
                var constructorParamSet = new HashSet<string>(mapping.ConstructorParameterNames, StringComparer.OrdinalIgnoreCase);
                var constructorProps = new List<PropertyMapping>();
                var initializerProps = new List<PropertyMapping>();

                foreach (var prop in mapping.PropertyMappings)
                {
                    if (constructorParamSet.Contains(prop.TargetProperty.Name))
                    {
                        constructorProps.Add(prop);
                    }
                    else if (!prop.TargetRequiresUnsafeAccessor)
                    {
                        initializerProps.Add(prop);
                    }
                }

                // Arguments in parameter order. A parameter without a source property (positional record) gets its
                // default value; one whose property mapping was skipped (reported, e.g. ATCMAP011 or ATCMAP015)
                // gets default, so the call still has every argument.
                var defaultArguments = (mapping.ConstructorDefaultParameters ?? [])
                    .ToDictionary(d => d.Name, d => d.DefaultExpression, StringComparer.Ordinal);
                var constructorArguments = new List<string>();
                foreach (var parameter in mapping.Constructor!.Parameters)
                {
                    if (defaultArguments.TryGetValue(parameter.Name, out var defaultArgument))
                    {
                        constructorArguments.Add(defaultArgument);
                        continue;
                    }

                    var prop = constructorProps.FirstOrDefault(p =>
                        string.Equals(p.TargetProperty.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
                    constructorArguments.Add(prop is null
                        ? "default!"
                        : GeneratePropertyMappingValue(prop, "source", mapping.SourceType));
                }

                // Generate constructor call
                if (needsTargetVariable)
                {
                    sb.AppendLineLf($"        var target = new {targetTypeName}{typeParamList}(");
                }
                else
                {
                    sb.AppendLineLf($"        return new {targetTypeName}{typeParamList}(");
                }

                for (var i = 0; i < constructorArguments.Count; i++)
                {
                    var comma = i == constructorArguments.Count - 1 ? string.Empty : ",";
                    sb.AppendLineLf($"            {constructorArguments[i]}{comma}");
                }

                if (initializerProps.Count > 0)
                {
                    sb.AppendLineLf("        )");
                    sb.AppendLineLf("        {");
                    MappingCodeEmitter.AppendPropertyInitializers(sb, initializerProps, p => GeneratePropertyMappingValue(p, "source", mapping.SourceType));
                    sb.AppendLineLf("        };");
                }
                else
                {
                    sb.AppendLineLf("        );");
                }
            }
            else
            {
                // Use object initializer syntax
                if (needsTargetVariable)
                {
                    sb.AppendLineLf($"        var target = new {targetTypeName}{typeParamList}");
                }
                else
                {
                    sb.AppendLineLf($"        return new {targetTypeName}{typeParamList}");
                }

                sb.AppendLineLf("        {");
                MappingCodeEmitter.AppendPropertyInitializers(sb, initializableProps, p => GeneratePropertyMappingValue(p, "source", mapping.SourceType));
                sb.AppendLineLf("        };");
            }
        }

        // Set private target members through their UnsafeAccessor setters
        foreach (var prop in privateTargetProps.Where(p => !mapping.ConstructorParameterNames.Contains(p.TargetProperty.Name, StringComparer.OrdinalIgnoreCase) || useFactory))
        {
            var setterName = GetAccessorMethodName(mapping.TargetType, prop.TargetProperty.Name, isGetter: false);
            sb.AppendLineLf($"        {setterName}(target, {GeneratePropertyMappingValue(prop, "source", mapping.SourceType)});");
        }

        // Generate AfterMap hook call and return statement
        if (needsTargetVariable)
        {
            if (!string.IsNullOrWhiteSpace(mapping.AfterMap))
            {
                sb.AppendLineLf();
                sb.AppendLineLf($"        {mapping.SourceType.ToGlobalDisplayString()}.{mapping.AfterMap}(source, target);");
            }

            sb.AppendLineLf();
            sb.AppendLineLf("        return target;");
        }

        sb.AppendLineLf("    }");
        sb.AppendLineLf();

        // Generate additional update target method if requested
        if (mapping.UpdateTarget)
        {
            GenerateUpdateTargetMethod(sb, mapping);
        }
    }

    private static void GenerateUpdateTargetMethod(
        StringBuilder sb,
        MappingInfo mapping)
    {
        var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(mapping.TargetType.Name)}";

        // Build type parameter list for generic methods
        var typeParamList = string.Empty;
        var typeConstraints = new List<string>();
        var targetTypeName = mapping.TargetType.ToGlobalDisplayString();

        if (mapping.IsGeneric)
        {
            var typeParams = mapping.SourceType.TypeParameters;
            typeParamList = $"<{string.Join(", ", typeParams.Select(tp => tp.Name))}>";

            // For generic types, get the base name without type parameters
            var targetBaseTypeName = mapping.TargetType.Name;
            targetTypeName = $"global::{NamespaceHelper.GetQualifier(mapping.TargetType.ContainingNamespace)}{targetBaseTypeName}";

            // Collect type parameter constraints
            foreach (var typeParam in typeParams)
            {
                var constraints = new List<string>();

                // Check for class/struct constraint
                if (typeParam.HasReferenceTypeConstraint)
                {
                    constraints.Add("class");
                }
                else if (typeParam.HasValueTypeConstraint)
                {
                    constraints.Add("struct");
                }

                // Add type constraints
                foreach (var constraintType in typeParam.ConstraintTypes)
                {
                    constraints.Add(constraintType.ToGlobalDisplayString());
                }

                // Check for new() constraint
                if (typeParam.HasConstructorConstraint && !typeParam.HasValueTypeConstraint)
                {
                    constraints.Add("new()");
                }

                if (constraints.Count > 0)
                {
                    typeConstraints.Add($"        where {typeParam.Name} : {string.Join(", ", constraints)}");
                }
            }
        }

        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Maps <see cref=\"{mapping.SourceType.ToGlobalDisplayString()}\"/> to an existing <see cref=\"{targetTypeName}{typeParamList}\"/> instance.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf($"    public static void {methodName}{typeParamList}(");
        sb.AppendLineLf($"        this {mapping.SourceType.ToGlobalDisplayString()} source,");
        sb.AppendLineLf($"        {targetTypeName}{typeParamList} target)");

        // Add type parameter constraints
        foreach (var constraint in typeConstraints)
        {
            sb.AppendLineLf(constraint);
        }

        sb.AppendLineLf("    {");
        sb.AppendLineLf("        if (source is null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            return;");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();
        sb.AppendLineLf("        if (target is null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            return;");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();

        // Generate BeforeMap hook call
        if (!string.IsNullOrWhiteSpace(mapping.BeforeMap))
        {
            sb.AppendLineLf($"        {mapping.SourceType.ToGlobalDisplayString()}.{mapping.BeforeMap}(source);");
            sb.AppendLineLf();
        }

        // Update properties on existing target
        foreach (var prop in mapping.PropertyMappings)
        {
            var value = GeneratePropertyMappingValue(prop, "source", mapping.SourceType);

            if (prop.TargetRequiresUnsafeAccessor)
            {
                // Use UnsafeAccessor setter for private/internal properties
                var setterName = GetAccessorMethodName(mapping.TargetType, prop.TargetProperty.Name, isGetter: false);
                sb.AppendLineLf($"        {setterName}(target, {value});");
            }
            else
            {
                // Direct property assignment for public properties
                sb.AppendLineLf($"        target.{prop.TargetProperty.Name} = {value};");
            }
        }

        // Generate AfterMap hook call
        if (!string.IsNullOrWhiteSpace(mapping.AfterMap))
        {
            sb.AppendLineLf();
            sb.AppendLineLf($"        {mapping.SourceType.ToGlobalDisplayString()}.{mapping.AfterMap}(source, target);");
        }

        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GenerateProjectionMethod(
        StringBuilder sb,
        MappingInfo mapping)
    {
        var methodName = $"ProjectTo{mapping.TargetType.Name}";

        // Filter properties that can be used in projections
        // Only simple properties (no nested objects, no collections or dictionaries, no built-in type conversions)
        var projectionProperties = mapping.PropertyMappings
            .Where(p => !p.IsNested && !p.IsCollection && !p.IsDictionary && !p.IsBuiltInTypeConversion && !p.IsFlattened && p.SourcePath is null)
            .ToList();

        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Creates an Expression projection from <see cref=\"{mapping.SourceType.ToGlobalDisplayString()}\"/> to <see cref=\"{mapping.TargetType.ToGlobalDisplayString()}\"/>.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf("    /// <remarks>");
        sb.AppendLineLf("    /// This projection is designed for use with IQueryable (EF Core server-side projection).");
        sb.AppendLineLf("    /// Only simple property mappings are included. Nested objects, collections, and complex conversions are excluded.");
        sb.AppendLineLf("    /// </remarks>");
        sb.AppendLineLf($"    public static global::System.Linq.Expressions.Expression<global::System.Func<{mapping.SourceType.ToGlobalDisplayString()}, {mapping.TargetType.ToGlobalDisplayString()}>> {methodName}()");
        sb.AppendLineLf("    {");
        sb.AppendLineLf($"        return source => new {mapping.TargetType.ToGlobalDisplayString()}");
        sb.AppendLineLf("        {");

        for (var i = 0; i < projectionProperties.Count; i++)
        {
            var prop = projectionProperties[i];
            var isLast = i == projectionProperties.Count - 1;
            var comma = isLast ? string.Empty : ",";

            // For projections, we only support direct mappings and enum conversions (simple casts)
            if (prop.RequiresConversion)
            {
                // Enum conversion - use simple cast (works in expressions)
                sb.AppendLineLf($"            {prop.TargetProperty.Name} = ({prop.TargetProperty.Type.ToGlobalDisplayString()})source.{prop.SourceProperty.Name}{comma}");
            }
            else if (MappingTypeAnalyzer.IsNullableToNonNullableValueType(prop.SourceProperty.Type, prop.TargetProperty.Type))
            {
                // T? → T for value types (e.g. Guid? → Guid) - coalesce works in expressions
                sb.AppendLineLf($"            {prop.TargetProperty.Name} = source.{prop.SourceProperty.Name} ?? default{comma}");
            }
            else
            {
                // Direct property mapping
                sb.AppendLineLf($"            {prop.TargetProperty.Name} = source.{prop.SourceProperty.Name}{comma}");
            }
        }

        sb.AppendLineLf("        };");
        sb.AppendLineLf("    }");
        sb.AppendLineLf();
    }

    private static void GeneratePolymorphicMapping(
        StringBuilder sb,
        MappingInfo mapping)
    {
        // Add null check
        sb.AppendLineLf("        if (source is null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf("            return default!;");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();

        // Generate switch expression
        sb.AppendLineLf("        return source switch");
        sb.AppendLineLf("        {");

        // Add case for each derived type mapping
        foreach (var derivedMapping in mapping.DerivedTypeMappings)
        {
            var sourceDerivedTypeName = derivedMapping.SourceDerivedType.ToGlobalDisplayString();
            var targetDerivedTypeName = derivedMapping.TargetDerivedType.Name;
            var variableName = CSharpIdentifier.Escape(char.ToLowerInvariant(derivedMapping.SourceDerivedType.Name[0]) + derivedMapping.SourceDerivedType.Name.Substring(1));

            sb.AppendLineLf($"            {sourceDerivedTypeName} {variableName} => {variableName}.MapTo{targetDerivedTypeName}(),");
        }

        // Add default case
        sb.AppendLineLf("            _ => throw new global::System.ArgumentException($\"Unknown derived type: {source.GetType().Name}\")");
        sb.AppendLineLf("        };");
    }

    private static string GeneratePropertyMappingValue(
        PropertyMapping prop,
        string sourceVariable,
        INamedTypeSymbol sourceType)
    {
        if (prop.ConversionMethod is not null)
        {
            // Conversion method of the [MapTo] class
            return $"{prop.ConversionMethod}({GenerateSourcePropertyAccess(prop, sourceVariable, sourceType)})";
        }

        if (prop.SourcePath is not null)
        {
            // Nested source path of a class-level [MapProperty]; a private first segment is read through its accessor
            return prop.SourceRequiresUnsafeAccessor
                ? GenerateSourcePropertyAccess(prop, sourceVariable, sourceType) + prop.SourcePath.Substring(prop.SourceProperty.Name.Length)
                : $"{sourceVariable}.{prop.SourcePath}";
        }

        if (prop.IsFlattened && prop.FlattenedNestedProperty is not null)
        {
            // Flattened property mapping: source.Address.City → target.AddressCity
            // Handle nullable intermediate and nested properties with null-conditional operator
            var isIntermediateNullable = prop.SourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                                         !prop.SourceProperty.Type.IsValueType;

            var isNestedNullable = prop.FlattenedNestedProperty.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                                   (!prop.FlattenedNestedProperty.Type.IsValueType &&
                                    prop.FlattenedNestedProperty.Type.NullableAnnotation != NullableAnnotation.NotAnnotated);

            if (isIntermediateNullable)
            {
                // source.Address?.ZipCode is int? for an int member: coalesce for an int target (not '!')
                var suffix = MappingTypeAnalyzer.GetNullConditionalSuffix(prop.FlattenedNestedProperty.Type, prop.TargetProperty.Type);
                return $"{GenerateSourcePropertyAccess(prop, sourceVariable, sourceType)}?.{prop.FlattenedNestedProperty.Name}{suffix}";
            }

            var nullForgiving = isNestedNullable ? "!" : string.Empty;
            return $"{GenerateSourcePropertyAccess(prop, sourceVariable, sourceType)}.{prop.FlattenedNestedProperty.Name}{nullForgiving}";
        }

        if (prop.IsBuiltInTypeConversion)
        {
            // Built-in type conversion (DateTime ↔ string, Guid ↔ string, numeric ↔ string, bool ↔ string)
            return MappingCodeEmitter.GetBuiltInTypeConversion(prop, GenerateSourcePropertyAccess(prop, sourceVariable, sourceType));
        }

        if (prop.IsDictionary)
        {
            // Dictionary mapping: copy with ToDictionary, converting each value
            var propertyAccess = GenerateSourcePropertyAccess(prop, sourceVariable, sourceType);
            var valueConversion = IsSelfMapped(prop.SourceCollectionElementType, prop.CollectionElementType)
                ? $"x.Value.MapTo{TypeNameSanitizer.SanitizeForIdentifier(prop.CollectionElementType!.Name)}()"
                : MappingCodeEmitter.GetDictionaryValueConversion(prop, static (method, value) => $"{value}.{method}()");
            return $"{propertyAccess}?.ToDictionary(x => x.Key, x => {valueConversion})!";
        }

        if (prop.IsCollection)
        {
            // Collection mapping
            var elementTypeName = prop.CollectionElementType!.Name;
            var mappingMethodName = $"MapTo{elementTypeName}";
            var collectionType = prop.CollectionTargetType!;

            var copyElements = SymbolEqualityComparer.Default.Equals(prop.SourceCollectionElementType, prop.CollectionElementType) &&
                               !IsSelfMapped(prop.SourceCollectionElementType, prop.CollectionElementType);
            return MappingCodeEmitter.GetCollectionConversion(
                GenerateSourcePropertyAccess(prop, sourceVariable, sourceType),
                collectionType,
                prop.CollectionElementType,
                copyElements ? null : $"x => x.{mappingMethodName}()");
        }

        if (prop.RequiresConversion)
        {
            // Enum conversion
            if (prop.HasEnumMapping)
            {
                // Use EnumMapping extension method (safe mapping with special case handling)
                return MappingCodeEmitter.GetEnumMappingCall(prop, GenerateSourcePropertyAccess(prop, sourceVariable, sourceType));
            }

            // Numeric cast, or a simple enum cast when no enum mapping is defined
            return MappingCodeEmitter.GetCastConversion(prop, GenerateSourcePropertyAccess(prop, sourceVariable, sourceType));
        }

        if (prop.IsNested)
        {
            // Nested object mapping
            var nestedMethodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(prop.TargetProperty.Type.Name)}";
            var propertyAccess = GenerateSourcePropertyAccess(prop, sourceVariable, sourceType);

            // For nullable nested objects, we need null-conditional operator
            // UnsafeAccessor returns the value directly via ref, so use a local variable for null check
            if (prop.SourceRequiresUnsafeAccessor)
            {
                return $"{propertyAccess} is {{ }} __nested ? __nested.{nestedMethodName}() : default!";
            }

            if (prop.SourceProperty.Type.IsValueType)
            {
                return $"{propertyAccess}.{nestedMethodName}()";
            }

            if (prop.TargetProperty.Type.IsValueType)
            {
                return $"{propertyAccess} is not null ? {propertyAccess}.{nestedMethodName}() : default";
            }

            return $"{propertyAccess}?.{nestedMethodName}()!";
        }

        // T? → T for value types (e.g. Guid? → Guid): fall back to default when null
        if (MappingTypeAnalyzer.IsNullableToNonNullableValueType(prop.SourceProperty.Type, prop.TargetProperty.Type))
        {
            return $"{GenerateSourcePropertyAccess(prop, sourceVariable, sourceType)} ?? default";
        }

        // Direct property mapping
        return GenerateSourcePropertyAccess(prop, sourceVariable, sourceType);
    }

    private static void GenerateUnsafeAccessors(
        StringBuilder sb,
        MappingInfo mapping,
        HashSet<string> accessorsGenerated)
    {
        if (!mapping.IncludePrivateMembers)
        {
            return;
        }

        foreach (var prop in mapping.PropertyMappings)
        {
            // Generate getter accessor for source property if needed
            if (prop.SourceRequiresUnsafeAccessor)
            {
                var sourceTypeName = mapping.SourceType.ToGlobalDisplayString();
                var accessorName = $"UnsafeGet{mapping.SourceType.Name}_{prop.SourceProperty.Name}";

                if (accessorsGenerated.Add(accessorName))
                {
                    sb.AppendLineLf($"    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = \"get_{prop.SourceProperty.Name}\")]");
                    sb.AppendLineLf($"    private static extern {prop.SourceProperty.Type.ToGlobalDisplayString()} {accessorName}({sourceTypeName} instance);");
                    sb.AppendLineLf();
                }
            }

            // Generate setter accessor for target property if needed
            if (prop.TargetRequiresUnsafeAccessor)
            {
                var targetTypeName = mapping.TargetType.ToGlobalDisplayString();
                var accessorName = $"UnsafeSet{mapping.TargetType.Name}_{prop.TargetProperty.Name}";

                if (accessorsGenerated.Add(accessorName))
                {
                    sb.AppendLineLf($"    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = \"set_{prop.TargetProperty.Name}\")]");
                    sb.AppendLineLf($"    private static extern void {accessorName}({targetTypeName} instance, {prop.TargetProperty.Type.ToGlobalDisplayString()} value);");
                    sb.AppendLineLf();
                }
            }
        }
    }

    /// <summary>
    /// Builds the reverse (target to source) mapping of a bidirectional mapping.
    /// </summary>
    private static MappingInfo BuildReverseMapping(
        MappingInfo mapping,
        NestedMapperLookup nestedMappers)
    {
        // For generic mappings, construct the reverse source type with type parameters
        var reverseSourceType = mapping.TargetType;
        if (mapping.IsGeneric)
        {
            var typeArguments = mapping.SourceType.TypeParameters
                .Cast<ITypeSymbol>()
                .ToArray();
            reverseSourceType = mapping.TargetType.ConstructedFrom.Construct(typeArguments);
        }

        var reverseMappings = GetPropertyMappings(
            sourceType: reverseSourceType,
            targetType: mapping.SourceType,
            enableFlattening: mapping.EnableFlattening,
            includePrivateMembers: mapping.IncludePrivateMembers,
            propertyNameStrategy: mapping.PropertyNameStrategy,
            deepClone: mapping.DeepClone,
            conversions: GetConversionMethods(mapping.SourceType));
        reverseMappings.RemoveAll(p => p.IsNested && !nestedMappers.HasMapper(p));

        // Find best matching constructor for reverse mapping
        var (reverseConstructor, reverseConstructorParams, reverseConstructorDefaults) = FindBestConstructor(reverseSourceType, mapping.SourceType, GetConversionMethods(mapping.SourceType));

        var reverseMapping = new MappingInfo(
            SourceType: reverseSourceType,
            TargetType: mapping.SourceType,
            PropertyMappings: reverseMappings,
            Bidirectional: false, // Don't generate reverse of reverse
            EnableFlattening: mapping.EnableFlattening,
            Constructor: reverseConstructor,
            ConstructorParameterNames: reverseConstructorParams,
            ConstructorDefaultParameters: reverseConstructorDefaults,
            DerivedTypeMappings: [], // No derived type mappings for reverse
            BeforeMap: null, // No hooks for reverse mapping
            AfterMap: null, // No hooks for reverse mapping
            Factory: null, // No factory for reverse mapping
            UpdateTarget: false, // No update target for reverse mapping
            GenerateProjection: false, // No projection for reverse mapping
            IsGeneric: mapping.IsGeneric, // Preserve generic flag for reverse mapping
            IncludePrivateMembers: mapping.IncludePrivateMembers, // Preserve private member access for reverse mapping
            PropertyNameStrategy: mapping.PropertyNameStrategy, // Preserve property name strategy for reverse mapping
            DeepClone: mapping.DeepClone);

        return reverseMapping;
    }

    private static string GetAccessorMethodName(
        INamedTypeSymbol containingType,
        string propertyName,
        bool isGetter) =>
        isGetter
            ? $"UnsafeGet{containingType.Name}_{propertyName}"
            : $"UnsafeSet{containingType.Name}_{propertyName}";

    private static string GenerateSourcePropertyAccess(
        PropertyMapping prop,
        string sourceVariable,
        INamedTypeSymbol sourceType) =>
        prop.SourceRequiresUnsafeAccessor
            ? $"{GetAccessorMethodName(sourceType, prop.SourceProperty.Name, isGetter: true)}({sourceVariable})"
            : $"{sourceVariable}.{prop.SourceProperty.Name}";

    private static string GenerateAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Defines strategies for converting property names during mapping.
               /// Used to map between different naming conventions (PascalCase, camelCase, snake_case, kebab-case).
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.ObjectMapping", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal enum PropertyNameStrategy
               {
                   /// <summary>
                   /// No transformation. Property names must match exactly (case-insensitive comparison).
                   /// This is the default behavior.
                   /// Example: FirstName → FirstName
                   /// </summary>
                   PascalCase = 0,

                   /// <summary>
                   /// Convert PascalCase source properties to camelCase for matching.
                   /// Example: FirstName → firstName
                   /// </summary>
                   CamelCase = 1,

                   /// <summary>
                   /// Convert PascalCase source properties to snake_case for matching.
                   /// Example: FirstName → first_name
                   /// </summary>
                   SnakeCase = 2,

                   /// <summary>
                   /// Convert PascalCase source properties to kebab-case for matching.
                   /// Example: FirstName → first-name
                   /// </summary>
                   KebabCase = 3,
               }

               /// <summary>
               /// Marks a class or enum for automatic mapping code generation.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.ObjectMapping", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapToAttribute : global::System.Attribute
               {
                   /// <summary>
                   /// Initializes a new instance of the <see cref="MapToAttribute"/> class.
                   /// </summary>
                   /// <param name="targetType">The target type to map to.</param>
                   public MapToAttribute(global::System.Type targetType)
                   {
                       TargetType = targetType;
                   }

                   /// <summary>
                   /// Gets the target type to map to.
                   /// </summary>
                   public global::System.Type TargetType { get; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to generate bidirectional mappings
                   /// (both Source → Target and Target → Source).
                   /// Default is false.
                   /// </summary>
                   public bool Bidirectional { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to enable property flattening.
                   /// When enabled, nested object properties are flattened using naming convention {PropertyName}{NestedPropertyName}.
                   /// For example, source.Address.City maps to target.AddressCity.
                   /// Default is false.
                   /// </summary>
                   public bool EnableFlattening { get; set; }

                   /// <summary>
                   /// Gets or sets the name of a static method to call before performing the mapping.
                   /// The method must have the signature: static void MethodName(SourceType source).
                   /// </summary>
                   public string? BeforeMap { get; set; }

                   /// <summary>
                   /// Gets or sets the name of a static method to call after performing the mapping.
                   /// The method must have the signature: static void MethodName(SourceType source, TargetType target).
                   /// </summary>
                   public string? AfterMap { get; set; }

                   /// <summary>
                   /// Gets or sets the name of a static factory method to use for creating the target instance.
                   /// The method must have the signature: static TargetType MethodName().
                   /// </summary>
                   public string? Factory { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to generate an additional method overload
                   /// that updates an existing target instance instead of creating a new one.
                   /// </summary>
                   public bool UpdateTarget { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to generate an Expression projection method
                   /// for use with IQueryable (EF Core server-side projection).
                   /// </summary>
                   public bool GenerateProjection { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether to include private and internal members in the mapping.
                   /// When enabled, uses UnsafeAccessor (NET 8+) for AOT-safe, zero-overhead access to private members.
                   /// Default is false (only public members).
                   /// </summary>
                   public bool IncludePrivateMembers { get; set; }

                   /// <summary>
                   /// Gets or sets a value indicating whether properties of the same type are copied instead of shared:
                   /// collections and dictionaries get new instances, and nested types, elements and values with a
                   /// self-mapping ([MapTo(typeof(T))] on T) are cloned. Use it on a self-mapping
                   /// ([MapTo(typeof(Order), DeepClone = true)] on Order) for a deep copy. Reference cycles are not supported.
                   /// Default is false.
                   /// </summary>
                   public bool DeepClone { get; set; }

                   /// <summary>
                   /// Gets or sets the naming strategy for property name conversion during mapping.
                   /// Allows automatic mapping between different naming conventions (PascalCase, camelCase, snake_case, kebab-case).
                   /// Default is PascalCase (no transformation - exact match).
                   /// </summary>
                   public PropertyNameStrategy PropertyNameStrategy { get; set; } = PropertyNameStrategy.PascalCase;
               }
           }
           """;

    private static string GenerateMapIgnoreAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Marks a property to be excluded from automatic mapping code generation.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.ObjectMapping", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapIgnoreAttribute : global::System.Attribute
               {
               }
           }
           """;

    private static string GenerateMapPropertyAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Specifies a custom target property name for mapping when property names differ between source and target types.
               /// On a property: [MapProperty("TargetName")]. On the [MapTo] class: [MapProperty("Customer.Address.City", "City")]
               /// maps a nested source path (read null-safely) to a target property.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.ObjectMapping", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapPropertyAttribute : global::System.Attribute
               {
                   /// <summary>
                   /// Initializes a new instance of the <see cref="MapPropertyAttribute"/> class.
                   /// </summary>
                   /// <param name="targetPropertyName">The name of the target property to map to.</param>
                   public MapPropertyAttribute(string targetPropertyName)
                   {
                       TargetPropertyName = targetPropertyName;
                   }

                   /// <summary>
                   /// Initializes a new instance of the <see cref="MapPropertyAttribute"/> class for a nested source path
                   /// (on the [MapTo] class).
                   /// </summary>
                   /// <param name="sourcePath">The source property path, e.g. "Customer.Address.City".</param>
                   /// <param name="targetPropertyName">The name of the target property to map to.</param>
                   public MapPropertyAttribute(string sourcePath, string targetPropertyName)
                   {
                       SourcePath = sourcePath;
                       TargetPropertyName = targetPropertyName;
                   }

                   /// <summary>
                   /// Gets the name of the target property to map to.
                   /// </summary>
                   public string TargetPropertyName { get; }

                   /// <summary>
                   /// Gets the nested source path, or <c>null</c> when the attribute is on a source property.
                   /// </summary>
                   public string? SourcePath { get; }
               }
           }
           """;

    private static string GenerateMapDerivedTypeAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Specifies a derived type mapping for polymorphic object mapping.
               /// Used in conjunction with <see cref="MapToAttribute"/> to define how derived types should be mapped.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.ObjectMapping", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
               [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
               internal sealed class MapDerivedTypeAttribute : global::System.Attribute
               {
                   /// <summary>
                   /// Initializes a new instance of the <see cref="MapDerivedTypeAttribute"/> class.
                   /// </summary>
                   /// <param name="sourceType">The source derived type to match.</param>
                   /// <param name="targetType">The target derived type to map to.</param>
                   public MapDerivedTypeAttribute(global::System.Type sourceType, global::System.Type targetType)
                   {
                       SourceType = sourceType;
                       TargetType = targetType;
                   }

                   /// <summary>
                   /// Gets the source derived type to match during polymorphic mapping.
                   /// </summary>
                   public global::System.Type SourceType { get; }

                   /// <summary>
                   /// Gets the target derived type to map to when the source type matches.
                   /// </summary>
                   public global::System.Type TargetType { get; }
               }
           }
           """;
}