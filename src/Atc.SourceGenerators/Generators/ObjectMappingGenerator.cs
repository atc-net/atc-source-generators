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

            // The generated mapping is an extension method in another class: hooks must be static and accessible
            beforeMap = GetCallableHook(classSymbol, beforeMap, "BeforeMap", context);
            afterMap = GetCallableHook(classSymbol, afterMap, "AfterMap", context);
            factory = GetCallableHook(classSymbol, factory, "Factory", context);

            // Get property mappings
            var propertyMappings = GetPropertyMappings(classSymbol, targetType, enableFlattening, includePrivateMembers, propertyNameStrategy, context);

            // Find best matching constructor
            var (constructor, constructorParameterNames) = FindBestConstructor(classSymbol, targetType);

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
                ? GetPropertyMappings(classSymbol, targetTypeForMapping, enableFlattening, includePrivateMembers, propertyNameStrategy, context)
                : propertyMappings;

            // Find best matching constructor (using constructed target type for generics)
            var (constructorForGeneric, constructorParamsForGeneric) = isGenericMapping
                ? FindBestConstructor(classSymbol, targetTypeForMapping)
                : (constructor, constructorParameterNames);

            mappings.Add(new MappingInfo(
                SourceType: classSymbol,
                TargetType: targetType,
                PropertyMappings: isGenericMapping ? mappingsForGeneric : propertyMappings,
                Bidirectional: bidirectional,
                EnableFlattening: enableFlattening,
                Constructor: isGenericMapping ? constructorForGeneric : constructor,
                ConstructorParameterNames: isGenericMapping ? constructorParamsForGeneric : constructorParameterNames,
                DerivedTypeMappings: derivedTypeMappings,
                BeforeMap: beforeMap,
                AfterMap: afterMap,
                Factory: factory,
                UpdateTarget: updateTarget,
                GenerateProjection: generateProjection,
                IsGeneric: isGenericMapping,
                IncludePrivateMembers: includePrivateMembers,
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
        SourceProductionContext? context = null)
    {
        var mappings = new List<PropertyMapping>();

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
                    SourceRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForReading(sourceProp),
                    TargetRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForWriting(targetProp)));
            }
            else
            {
                // Check if types are different but might be mappable (nested objects, enums, or collections)
                targetProp = targetProperties.FirstOrDefault(t => string.Equals(t.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase));
                if (targetProp is not null &&
                    MappingTypeAnalyzer.TryGetDictionaryMapping(sourceProp.Type, targetProp.Type, out var sourceValueType, out var targetValueType))
                {
                    // Dictionary with the same key type: copied with ToDictionary, converting each value
                    if (MappingTypeAnalyzer.AreDictionaryValuesDirectlyConvertible(sourceValueType!, targetValueType!) ||
                        HasObjectMappingToType(sourceValueType!, targetValueType!))
                    {
                        var hasDictionaryEnumMapping = MappingTypeAnalyzer.IsDictionaryValueEnumConversion(sourceValueType!, targetValueType!) &&
                                                       MappingTypeAnalyzer.HasEnumMappingAttribute(sourceValueType!, targetValueType!);

                        mappings.Add(new PropertyMapping(
                            SourceProperty: sourceProp,
                            TargetProperty: targetProp,
                            RequiresConversion: false,
                            IsNested: false,
                            HasEnumMapping: hasDictionaryEnumMapping,
                            IsCollection: false,
                            CollectionElementType: targetValueType,
                            SourceCollectionElementType: sourceValueType,
                            CollectionTargetType: null,
                            IsFlattened: false,
                            FlattenedNestedProperty: null,
                            IsBuiltInTypeConversion: false,
                            SourceRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForReading(sourceProp),
                            TargetRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForWriting(targetProp),
                            IsDictionary: true));
                    }
                    else if (context is not null)
                    {
                        // No mapping method found for the dictionary value type - report diagnostic and skip
                        context.Value.ReportDiagnostic(
                            Diagnostic.Create(
                                CollectionElementMappingNotFoundDescriptor,
                                sourceProp.Locations.FirstOrDefault() ?? Location.None,
                                sourceProp.Name,
                                sourceValueType!.ToDisplayString(),
                                targetValueType!.ToDisplayString()));
                    }
                }
                else if (targetProp is not null)
                {
                    // Check for collection mapping
                    var isSourceCollection = MappingTypeAnalyzer.IsCollectionType(sourceProp.Type, out var sourceElementType);
                    var isTargetCollection = MappingTypeAnalyzer.IsCollectionType(targetProp.Type, out var targetElementType);

                    if (isSourceCollection && isTargetCollection && sourceElementType is not null && targetElementType is not null)
                    {
                        // Validate that the source element type has a [MapTo] mapping to the target element type
                        if (HasObjectMappingToType(sourceElementType, targetElementType))
                        {
                            // Collection mapping with validated element mapping
                            mappings.Add(new PropertyMapping(
                                SourceProperty: sourceProp,
                                TargetProperty: targetProp,
                                RequiresConversion: false,
                                IsNested: false,
                                HasEnumMapping: false,
                                IsCollection: true,
                                CollectionElementType: targetElementType,
                                SourceCollectionElementType: sourceElementType,
                                CollectionTargetType: MappingTypeAnalyzer.GetCollectionTargetType(targetProp.Type),
                                IsFlattened: false,
                                FlattenedNestedProperty: null,
                                IsBuiltInTypeConversion: false,
                                SourceRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForReading(sourceProp),
                                TargetRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForWriting(targetProp)));
                        }
                        else if (context is not null)
                        {
                            // No mapping method found for collection element type - report diagnostic and skip
                            context.Value.ReportDiagnostic(
                                Diagnostic.Create(
                                    CollectionElementMappingNotFoundDescriptor,
                                    sourceProp.Locations.FirstOrDefault() ?? Location.None,
                                    sourceProp.Name,
                                    sourceElementType.ToDisplayString(),
                                    targetElementType.ToDisplayString()));
                        }
                    }
                    else
                    {
                        // Check for enum conversion, nested mapping, or built-in type conversion
                        // The shared analyzer unwraps Nullable<T>, so Status? -> StatusDto? is an enum conversion
                        // (not a nested mapping of Nullable<T>)
                        var requiresConversion = MappingTypeAnalyzer.IsEnumConversion(sourceProp.Type, targetProp.Type);
                        var isNested = MappingTypeAnalyzer.IsNestedMapping(sourceProp.Type, targetProp.Type);
                        var hasEnumMapping = requiresConversion &&
                                             MappingTypeAnalyzer.HasEnumMappingAttribute(
                                                 MappingTypeAnalyzer.UnwrapNullable(sourceProp.Type),
                                                 MappingTypeAnalyzer.UnwrapNullable(targetProp.Type));
                        var isBuiltInTypeConversion = MappingTypeAnalyzer.IsBuiltInTypeConversion(sourceProp.Type, targetProp.Type);
                        var isNullableValueTypeWrapping = MappingTypeAnalyzer.IsNullableValueTypeWrapping(sourceProp.Type, targetProp.Type);

                        // Same value type with only nullability differing (e.g. Guid? → Guid) is a direct assignment
                        if (isNullableValueTypeWrapping)
                        {
                            isNested = false;
                        }

                        if (requiresConversion || isNested || isBuiltInTypeConversion || isNullableValueTypeWrapping)
                        {
                            mappings.Add(new PropertyMapping(
                                SourceProperty: sourceProp,
                                TargetProperty: targetProp,
                                RequiresConversion: requiresConversion,
                                IsNested: isNested,
                                HasEnumMapping: hasEnumMapping,
                                IsCollection: false,
                                CollectionElementType: null,
                                SourceCollectionElementType: null,
                                CollectionTargetType: null,
                                IsFlattened: false,
                                FlattenedNestedProperty: null,
                                IsBuiltInTypeConversion: isBuiltInTypeConversion,
                                SourceRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForReading(sourceProp),
                                TargetRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForWriting(targetProp)));
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
                // Skip properties that are already mapped or ignored
                if (MappingTypeAnalyzer.HasMapIgnoreAttribute(sourceProp))
                {
                    continue;
                }

                // Only flatten class/struct types (nested objects)
                if (sourceProp.Type is not INamedTypeSymbol namedType ||
                    (namedType.TypeKind != TypeKind.Class && namedType.TypeKind != TypeKind.Struct))
                {
                    continue;
                }

                // Skip BCL types (string, DateTime, Guid, DateTimeOffset, List<T>, ...)
                if (MappingTypeAnalyzer.IsSystemType(namedType))
                {
                    continue;
                }

                // Get properties from the nested object
                var nestedProperties = namedType
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => p.GetMethod is not null && !MappingTypeAnalyzer.HasMapIgnoreAttribute(p))
                    .ToList();

                // Try to match flattened properties: {PropertyName}{NestedPropertyName}
                foreach (var nestedProp in nestedProperties)
                {
                    var flattenedName = $"{sourceProp.Name}{nestedProp.Name}";
                    var targetProp = targetProperties.FirstOrDefault(t =>
                        string.Equals(t.Name, flattenedName, StringComparison.OrdinalIgnoreCase) &&
                        SymbolEqualityComparer.Default.Equals(t.Type, nestedProp.Type));

                    if (targetProp is not null)
                    {
                        // Add flattened mapping
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
                            SourceRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForReading(sourceProp),
                            TargetRequiresUnsafeAccessor: includePrivateMembers && RequiresUnsafeAccessorForWriting(targetProp)));
                    }
                }
            }
        }

        // Validate required properties
        if (context.HasValue)
        {
            ValidateRequiredProperties(targetType, sourceType, mappings, context.Value);
        }

        return mappings;
    }

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

    private static (IMethodSymbol? Constructor, List<string> ParameterNames) FindBestConstructor(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType)
    {
        // Get all public constructors
        var constructors = targetType
            .Constructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic)
            .ToList();

        if (constructors.Count == 0)
        {
            return (null, []);
        }

        // Get source properties that we can map from, including inherited ones
        var sourceProperties = MappingTypeAnalyzer.GetAllProperties(sourceType, includePrivateMembers: true);

        // Find constructor where all parameters match source properties (case-insensitive)
        foreach (var constructor in constructors.OrderByDescending(c => c.Parameters.Length))
        {
            var parameterNames = new List<string>();
            var allParametersMatch = true;

            foreach (var parameter in constructor.Parameters)
            {
                // Check if we have a matching source property (case-insensitive name AND compatible type)
                var matchingSourceProperty = sourceProperties.FirstOrDefault(p =>
                    string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

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
                    !MappingTypeAnalyzer.IsNullableValueTypeWrapping(sourcePropertyType, parameterType) &&
                    !MappingTypeAnalyzer.AreBothCustomOrEnumTypes(sourcePropertyType, parameterType) &&
                    !MappingTypeAnalyzer.AreCollectionsWithCompatibleElements(sourcePropertyType, parameterType, MappingTypeAnalyzer.AreBothCustomOrEnumTypes) &&
                    !IsMappableDictionary(sourcePropertyType, parameterType))
                {
                    allParametersMatch = false;
                    break;
                }

                parameterNames.Add(parameter.Name);
            }

            if (allParametersMatch && constructor.Parameters.Length > 0)
            {
                return (constructor, parameterNames);
            }
        }

        return (null, []);
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

                // Order constructor props by parameter order. A parameter whose property mapping was skipped
                // (reported, e.g. ATCMAP011 or ATCMAP015) gets default, so the call still has every argument.
                var orderedConstructorProps = new List<PropertyMapping?>();
                foreach (var paramName in mapping.ConstructorParameterNames)
                {
                    orderedConstructorProps.Add(constructorProps.FirstOrDefault(p =>
                        string.Equals(p.TargetProperty.Name, paramName, StringComparison.OrdinalIgnoreCase)));
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

                for (var i = 0; i < orderedConstructorProps.Count; i++)
                {
                    var prop = orderedConstructorProps[i];
                    var isLast = i == orderedConstructorProps.Count - 1;
                    var comma = isLast ? string.Empty : ",";

                    var value = prop is null
                        ? "default!"
                        : GeneratePropertyMappingValue(prop, "source", mapping.SourceType);
                    sb.AppendLineLf($"            {value}{comma}");
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
            .Where(p => !p.IsNested && !p.IsCollection && !p.IsDictionary && !p.IsBuiltInTypeConversion && !p.IsFlattened)
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
                return $"{sourceVariable}.{prop.SourceProperty.Name}?.{prop.FlattenedNestedProperty.Name}{suffix}";
            }

            var nullForgiving = isNestedNullable ? "!" : string.Empty;
            return $"{sourceVariable}.{prop.SourceProperty.Name}.{prop.FlattenedNestedProperty.Name}{nullForgiving}";
        }

        if (prop.IsBuiltInTypeConversion)
        {
            // Built-in type conversion (DateTime ↔ string, Guid ↔ string, numeric ↔ string, bool ↔ string)
            return MappingCodeEmitter.GetBuiltInTypeConversion(prop, sourceVariable);
        }

        if (prop.IsDictionary)
        {
            // Dictionary mapping: copy with ToDictionary, converting each value
            var propertyAccess = GenerateSourcePropertyAccess(prop, sourceVariable, sourceType);
            return $"{propertyAccess}?.ToDictionary(x => x.Key, x => {MappingCodeEmitter.GetDictionaryValueConversion(prop, static (method, value) => $"{value}.{method}()")})!";
        }

        if (prop.IsCollection)
        {
            // Collection mapping
            var elementTypeName = prop.CollectionElementType!.Name;
            var mappingMethodName = $"MapTo{elementTypeName}";
            var collectionType = prop.CollectionTargetType!;

            return MappingCodeEmitter.GetCollectionConversion(
                $"{sourceVariable}.{prop.SourceProperty.Name}",
                collectionType,
                prop.CollectionElementType,
                $"x => x.{mappingMethodName}()");
        }

        if (prop.RequiresConversion)
        {
            // Enum conversion
            if (prop.HasEnumMapping)
            {
                // Use EnumMapping extension method (safe mapping with special case handling)
                return MappingCodeEmitter.GetEnumMappingCall(prop, GenerateSourcePropertyAccess(prop, sourceVariable, sourceType));
            }

            // Fall back to simple cast (less safe but works when no mapping is defined)
            return $"({prop.TargetProperty.Type.ToGlobalDisplayString()}){sourceVariable}.{prop.SourceProperty.Name}";
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
            propertyNameStrategy: mapping.PropertyNameStrategy);
        reverseMappings.RemoveAll(p => p.IsNested && !nestedMappers.HasMapper(p));

        // Find best matching constructor for reverse mapping
        var (reverseConstructor, reverseConstructorParams) = FindBestConstructor(reverseSourceType, mapping.SourceType);

        var reverseMapping = new MappingInfo(
            SourceType: reverseSourceType,
            TargetType: mapping.SourceType,
            PropertyMappings: reverseMappings,
            Bidirectional: false, // Don't generate reverse of reverse
            EnableFlattening: mapping.EnableFlattening,
            Constructor: reverseConstructor,
            ConstructorParameterNames: reverseConstructorParams,
            DerivedTypeMappings: [], // No derived type mappings for reverse
            BeforeMap: null, // No hooks for reverse mapping
            AfterMap: null, // No hooks for reverse mapping
            Factory: null, // No factory for reverse mapping
            UpdateTarget: false, // No update target for reverse mapping
            GenerateProjection: false, // No projection for reverse mapping
            IsGeneric: mapping.IsGeneric, // Preserve generic flag for reverse mapping
            IncludePrivateMembers: mapping.IncludePrivateMembers, // Preserve private member access for reverse mapping
            PropertyNameStrategy: mapping.PropertyNameStrategy); // Preserve property name strategy for reverse mapping

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
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.ObjectMapping", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
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
                   /// Gets the name of the target property to map to.
                   /// </summary>
                   public string TargetPropertyName { get; }
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