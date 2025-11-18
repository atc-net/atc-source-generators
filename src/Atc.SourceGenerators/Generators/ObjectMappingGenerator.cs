// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
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
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TargetTypeMustBeClassOrStructDescriptor = new(
        id: RuleIdentifierConstants.ObjectMapping.TargetTypeMustBeClassOrStruct,
        title: "Target type must be a class or struct",
        messageFormat: "Target type '{0}' must be a class or struct, but is a {1}",
        category: RuleCategoryConstants.ObjectMapping,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate the attribute definitions as fallback
        // If Atc.SourceGenerators.Annotations is referenced, CS0436 warning will be suppressed via project settings
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("MapToAttribute.g.cs", SourceText.From(GenerateAttributeSource(), Encoding.UTF8));
            ctx.AddSource("MapIgnoreAttribute.g.cs", SourceText.From(GenerateMapIgnoreAttributeSource(), Encoding.UTF8));
        });

        // Find classes with MapTo attribute
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
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } or
           RecordDeclarationSyntax { AttributeLists.Count: > 0 };

    private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in typeDeclaration.AttributeLists)
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
                    return typeDeclaration;
                }
            }
        }

        return null;
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

        foreach (var classDeclaration in classes.Distinct())
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not { } classSymbol)
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

        // Generate mapping extensions
        var source = GenerateMappingExtensions(mappingsToGenerate);
        context.AddSource("ObjectMappingExtensions.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static List<MappingInfo>? ExtractMappingInfo(
        INamedTypeSymbol classSymbol,
        SourceProductionContext context)
    {
        // Check if class or record is partial
        if (!classSymbol.DeclaringSyntaxReferences.Any(r =>
            r.GetSyntax() is TypeDeclarationSyntax t && t.Modifiers.Any(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MappingClassMustBePartialDescriptor,
                    classSymbol.Locations.First(),
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
                        classSymbol.Locations.First(),
                        targetType.Name,
                        targetType
                            .TypeKind
                            .ToString()
                            .ToLowerInvariant()));
                continue;
            }

            // Extract Bidirectional property
            var bidirectional = false;
            foreach (var namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "Bidirectional")
                {
                    bidirectional = namedArg.Value.Value as bool? ?? false;
                    break;
                }
            }

            // Get property mappings
            var propertyMappings = GetPropertyMappings(classSymbol, targetType);

            // Find best matching constructor
            var (constructor, constructorParameterNames) = FindBestConstructor(classSymbol, targetType);

            mappings.Add(new MappingInfo(
                SourceType: classSymbol,
                TargetType: targetType,
                PropertyMappings: propertyMappings,
                Bidirectional: bidirectional,
                Constructor: constructor,
                ConstructorParameterNames: constructorParameterNames));
        }

        return mappings.Count > 0 ? mappings : null;
    }

    private static List<PropertyMapping> GetPropertyMappings(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol targetType)
    {
        var mappings = new List<PropertyMapping>();

        var sourceProperties = sourceType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod is not null && !HasMapIgnoreAttribute(p))
            .ToList();

        var targetProperties = targetType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => (p.SetMethod is not null || targetType.TypeKind == TypeKind.Struct) &&
                        !HasMapIgnoreAttribute(p))
            .ToList();

        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = targetProperties.FirstOrDefault(t =>
                string.Equals(t.Name, sourceProp.Name, StringComparison.OrdinalIgnoreCase) &&
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
                    CollectionTargetType: null));
            }
            else
            {
                // Check if types are different but might be mappable (nested objects, enums, or collections)
                targetProp = targetProperties.FirstOrDefault(t => string.Equals(t.Name, sourceProp.Name, StringComparison.OrdinalIgnoreCase));
                if (targetProp is not null)
                {
                    // Check for collection mapping
                    var isSourceCollection = IsCollectionType(sourceProp.Type, out var sourceElementType);
                    var isTargetCollection = IsCollectionType(targetProp.Type, out var targetElementType);

                    if (isSourceCollection && isTargetCollection && sourceElementType is not null && targetElementType is not null)
                    {
                        // Collection mapping
                        mappings.Add(new PropertyMapping(
                            SourceProperty: sourceProp,
                            TargetProperty: targetProp,
                            RequiresConversion: false,
                            IsNested: false,
                            HasEnumMapping: false,
                            IsCollection: true,
                            CollectionElementType: targetElementType,
                            CollectionTargetType: GetCollectionTargetType(targetProp.Type)));
                    }
                    else
                    {
                        // Check for enum conversion or nested mapping
                        var requiresConversion = IsEnumConversion(sourceProp.Type, targetProp.Type);
                        var isNested = IsNestedMapping(sourceProp.Type, targetProp.Type);
                        var hasEnumMapping = requiresConversion && HasEnumMappingAttribute(sourceProp.Type, targetProp.Type);

                        if (requiresConversion || isNested)
                        {
                            mappings.Add(new PropertyMapping(
                                SourceProperty: sourceProp,
                                TargetProperty: targetProp,
                                RequiresConversion: requiresConversion,
                                IsNested: isNested,
                                HasEnumMapping: hasEnumMapping,
                                IsCollection: false,
                                CollectionElementType: null,
                                CollectionTargetType: null));
                        }
                    }
                }
            }
        }

        return mappings;
    }

    private static bool IsEnumConversion(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        => sourceType.TypeKind == TypeKind.Enum &&
           targetType.TypeKind == TypeKind.Enum;

    private static bool HasEnumMappingAttribute(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (sourceType is not INamedTypeSymbol sourceEnum || targetType is not INamedTypeSymbol targetEnum)
        {
            return false;
        }

        // Check if source enum has [MapTo(typeof(TargetEnum))]
        var sourceAttributes = sourceEnum.GetAttributes();
        foreach (var attr in sourceAttributes)
        {
            if (attr.AttributeClass?.ToDisplayString() != FullAttributeName)
            {
                continue;
            }

            if (attr.ConstructorArguments.Length == 0)
            {
                continue;
            }

            var targetTypeArg = attr.ConstructorArguments[0].Value;
            if (targetTypeArg is INamedTypeSymbol attrTargetType &&
                SymbolEqualityComparer.Default.Equals(attrTargetType, targetEnum))
            {
                return true;
            }
        }

        // Check if target enum has [MapTo(typeof(SourceEnum), Bidirectional = true)]
        var targetAttributes = targetEnum.GetAttributes();
        foreach (var attr in targetAttributes)
        {
            if (attr.AttributeClass?.ToDisplayString() != FullAttributeName)
            {
                continue;
            }

            if (attr.ConstructorArguments.Length == 0)
            {
                continue;
            }

            var sourceTypeArg = attr.ConstructorArguments[0].Value;
            if (sourceTypeArg is not INamedTypeSymbol attrSourceType ||
                !SymbolEqualityComparer.Default.Equals(attrSourceType, sourceEnum))
            {
                continue;
            }

            // Check if Bidirectional is true
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "Bidirectional" && namedArg.Value.Value is bool bidirectional && bidirectional)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsNestedMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var st = sourceType.SpecialType.ToString();
        var tt = targetType.SpecialType.ToString();

        return sourceType.TypeKind is TypeKind.Class or TypeKind.Struct &&
               targetType.TypeKind is TypeKind.Class or TypeKind.Struct &&
               !st.StartsWith("System", StringComparison.Ordinal) &&
               !tt.StartsWith("System", StringComparison.Ordinal);
    }

    private static bool IsCollectionType(
        ITypeSymbol type,
        out ITypeSymbol? elementType)
    {
        elementType = null;

        // Handle arrays first
        if (type is IArrayTypeSymbol arrayType)
        {
            elementType = arrayType.ElementType;
            return true;
        }

        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        // Handle generic collections: List<T>, IEnumerable<T>, ICollection<T>, IReadOnlyList<T>, etc.
        if (namedType.IsGenericType && namedType.TypeArguments.Length == 1)
        {
            var typeName = namedType.ConstructedFrom.ToDisplayString();

            if (typeName.StartsWith("System.Collections.Generic.List<", StringComparison.Ordinal) ||
                typeName.StartsWith("System.Collections.Generic.IEnumerable<", StringComparison.Ordinal) ||
                typeName.StartsWith("System.Collections.Generic.ICollection<", StringComparison.Ordinal) ||
                typeName.StartsWith("System.Collections.Generic.IList<", StringComparison.Ordinal) ||
                typeName.StartsWith("System.Collections.Generic.IReadOnlyList<", StringComparison.Ordinal) ||
                typeName.StartsWith("System.Collections.Generic.IReadOnlyCollection<", StringComparison.Ordinal) ||
                typeName.StartsWith("System.Collections.ObjectModel.Collection<", StringComparison.Ordinal) ||
                typeName.StartsWith("System.Collections.ObjectModel.ReadOnlyCollection<", StringComparison.Ordinal))
            {
                elementType = namedType.TypeArguments[0];
                return true;
            }
        }

        return false;
    }

    private static string GetCollectionTargetType(
        ITypeSymbol targetPropertyType)
    {
        if (targetPropertyType is IArrayTypeSymbol)
        {
            return "Array";
        }

        if (targetPropertyType is not INamedTypeSymbol namedType)
        {
            return "List";
        }

        var typeName = namedType.ConstructedFrom.ToDisplayString();

        if (typeName.StartsWith("System.Collections.Generic.List<", StringComparison.Ordinal))
        {
            return "List";
        }

        if (typeName.StartsWith("System.Collections.Generic.IEnumerable<", StringComparison.Ordinal))
        {
            return "List";
        }

        if (typeName.StartsWith("System.Collections.Generic.ICollection<", StringComparison.Ordinal))
        {
            return "List";
        }

        if (typeName.StartsWith("System.Collections.Generic.IList<", StringComparison.Ordinal))
        {
            return "List";
        }

        if (typeName.StartsWith("System.Collections.Generic.IReadOnlyList<", StringComparison.Ordinal))
        {
            return "List";
        }

        if (typeName.StartsWith("System.Collections.Generic.IReadOnlyCollection<", StringComparison.Ordinal))
        {
            return "List";
        }

        if (typeName.StartsWith("System.Collections.ObjectModel.Collection<", StringComparison.Ordinal))
        {
            return "Collection";
        }

        if (typeName.StartsWith("System.Collections.ObjectModel.ReadOnlyCollection<", StringComparison.Ordinal))
        {
            return "ReadOnlyCollection";
        }

        return "List";
    }

    private static bool HasMapIgnoreAttribute(IPropertySymbol property)
    {
        const string mapIgnoreAttributeName = "Atc.SourceGenerators.Annotations.MapIgnoreAttribute";

        var attributes = property.GetAttributes();
        return attributes.Any(attr =>
            attr.AttributeClass?.ToDisplayString() == mapIgnoreAttributeName);
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
            return (null, new List<string>());
        }

        // Get source properties that we can map from
        var sourceProperties = sourceType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod is not null && !HasMapIgnoreAttribute(p))
            .ToList();

        // Find constructor where all parameters match source properties (case-insensitive)
        foreach (var constructor in constructors.OrderByDescending(c => c.Parameters.Length))
        {
            var parameterNames = new List<string>();
            var allParametersMatch = true;

            foreach (var parameter in constructor.Parameters)
            {
                // Check if we have a matching source property (case-insensitive)
                var matchingSourceProperty = sourceProperties.FirstOrDefault(p =>
                    string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (matchingSourceProperty is null)
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

        return (null, new List<string>());
    }

    private static string GenerateMappingExtensions(List<MappingInfo> mappings)
    {
        var sb = new StringBuilder();
        var namespaces = new HashSet<string>(StringComparer.Ordinal);

        // Collect all namespaces
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
        sb.AppendLineLf("/// <summary>");
        sb.AppendLineLf("/// Extension methods for object mapping.");
        sb.AppendLineLf("/// </summary>");
        sb.AppendLineLf("public static class ObjectMappingExtensions");
        sb.AppendLineLf("{");

        foreach (var mapping in mappings)
        {
            GenerateMappingMethod(sb, mapping);

            // Generate reverse mapping if bidirectional
            if (mapping.Bidirectional)
            {
                var reverseMappings = GetPropertyMappings(
                    sourceType: mapping.TargetType,
                    targetType: mapping.SourceType);

                // Find best matching constructor for reverse mapping
                var (reverseConstructor, reverseConstructorParams) = FindBestConstructor(mapping.TargetType, mapping.SourceType);

                var reverseMapping = new MappingInfo(
                    SourceType: mapping.TargetType,
                    TargetType: mapping.SourceType,
                    PropertyMappings: reverseMappings,
                    Bidirectional: false, // Don't generate reverse of reverse
                    Constructor: reverseConstructor,
                    ConstructorParameterNames: reverseConstructorParams);

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
        var methodName = $"MapTo{mapping.TargetType.Name}";

        sb.AppendLineLf("    /// <summary>");
        sb.AppendLineLf($"    /// Maps <see cref=\"{mapping.SourceType.ToDisplayString()}\"/> to <see cref=\"{mapping.TargetType.ToDisplayString()}\"/>.");
        sb.AppendLineLf("    /// </summary>");
        sb.AppendLineLf($"    public static {mapping.TargetType.ToDisplayString()} {methodName}(");
        sb.AppendLineLf($"        this {mapping.SourceType.ToDisplayString()} source)");
        sb.AppendLineLf("    {");
        sb.AppendLineLf("        if (source is null)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf($"            return default!;");
        sb.AppendLineLf("        }");
        sb.AppendLineLf();

        // Check if we should use constructor-based initialization
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
                else
                {
                    initializerProps.Add(prop);
                }
            }

            // Order constructor props by parameter order
            var orderedConstructorProps = new List<PropertyMapping>();
            foreach (var paramName in mapping.ConstructorParameterNames)
            {
                var prop = constructorProps.FirstOrDefault(p =>
                    string.Equals(p.TargetProperty.Name, paramName, StringComparison.OrdinalIgnoreCase));
                if (prop is not null)
                {
                    orderedConstructorProps.Add(prop);
                }
            }

            // Generate constructor call
            sb.AppendLineLf($"        return new {mapping.TargetType.ToDisplayString()}(");

            for (var i = 0; i < orderedConstructorProps.Count; i++)
            {
                var prop = orderedConstructorProps[i];
                var isLast = i == orderedConstructorProps.Count - 1;
                var comma = isLast && initializerProps.Count == 0 ? string.Empty : ",";

                var value = GeneratePropertyMappingValue(prop, "source");
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
            // Use object initializer syntax
            sb.AppendLineLf($"        return new {mapping.TargetType.ToDisplayString()}");
            sb.AppendLineLf("        {");
            GeneratePropertyInitializers(sb, mapping.PropertyMappings);
            sb.AppendLineLf("        };");
        }

        sb.AppendLineLf("    }");
        sb.AppendLineLf();
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
        if (prop.IsCollection)
        {
            // Collection mapping
            var elementTypeName = prop.CollectionElementType!.Name;
            var mappingMethodName = $"MapTo{elementTypeName}";
            var collectionType = prop.CollectionTargetType!;

            if (collectionType == "Array")
            {
                return $"{sourceVariable}.{prop.SourceProperty.Name}?.Select(x => x.{mappingMethodName}()).ToArray()!";
            }

            if (collectionType == "Collection")
            {
                return $"new global::System.Collections.ObjectModel.Collection<{prop.CollectionElementType.ToDisplayString()}>({sourceVariable}.{prop.SourceProperty.Name}?.Select(x => x.{mappingMethodName}()).ToList()!)";
            }

            if (collectionType == "ReadOnlyCollection")
            {
                return $"new global::System.Collections.ObjectModel.ReadOnlyCollection<{prop.CollectionElementType.ToDisplayString()}>({sourceVariable}.{prop.SourceProperty.Name}?.Select(x => x.{mappingMethodName}()).ToList()!)";
            }

            // Default to List (handles List, IEnumerable, ICollection, IList, IReadOnlyList, IReadOnlyCollection)
            return $"{sourceVariable}.{prop.SourceProperty.Name}?.Select(x => x.{mappingMethodName}()).ToList()!";
        }

        if (prop.RequiresConversion)
        {
            // Enum conversion
            if (prop.HasEnumMapping)
            {
                // Use EnumMapping extension method (safe mapping with special case handling)
                var enumMappingMethodName = $"MapTo{prop.TargetProperty.Type.Name}";
                return $"{sourceVariable}.{prop.SourceProperty.Name}.{enumMappingMethodName}()";
            }

            // Fall back to simple cast (less safe but works when no mapping is defined)
            return $"({prop.TargetProperty.Type.ToDisplayString()}){sourceVariable}.{prop.SourceProperty.Name}";
        }

        if (prop.IsNested)
        {
            // Nested object mapping
            var nestedMethodName = $"MapTo{prop.TargetProperty.Type.Name}";
            return $"{sourceVariable}.{prop.SourceProperty.Name}?.{nestedMethodName}()!";
        }

        // Direct property mapping
        return $"{sourceVariable}.{prop.SourceProperty.Name}";
    }

    private static string GenerateAttributeSource()
        => """
           // <auto-generated/>
           #nullable enable

           namespace Atc.SourceGenerators.Annotations
           {
               /// <summary>
               /// Marks a class or enum for automatic mapping code generation.
               /// </summary>
               [global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.ObjectMapping", "1.0.0")]
               [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
               [global::System.Diagnostics.DebuggerNonUserCode]
               [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
               [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
               public sealed class MapToAttribute : global::System.Attribute
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
               public sealed class MapIgnoreAttribute : global::System.Attribute
               {
               }
           }
           """;
}