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
        // Generate the attribute definition as fallback
        // If Atc.SourceGenerators.Annotations is referenced, CS0436 warning will be suppressed via project settings
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("MapToAttribute.g.cs", SourceText.From(GenerateAttributeSource(), Encoding.UTF8));
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
        // Check if class is partial
        if (!classSymbol.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is ClassDeclarationSyntax c &&
                                                            c.Modifiers.Any(SyntaxKind.PartialKeyword)))
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

            mappings.Add(new MappingInfo(
                SourceType: classSymbol,
                TargetType: targetType,
                PropertyMappings: propertyMappings,
                Bidirectional: bidirectional));
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
            .Where(p => p.GetMethod is not null)
            .ToList();

        var targetProperties = targetType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod is not null ||
                        targetType.TypeKind == TypeKind.Struct)
            .ToList();

        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = targetProperties.FirstOrDefault(t =>
                t.Name == sourceProp.Name &&
                SymbolEqualityComparer.Default.Equals(t.Type, sourceProp.Type));

            if (targetProp is not null)
            {
                mappings.Add(new PropertyMapping(
                    SourceProperty: sourceProp,
                    TargetProperty: targetProp,
                    RequiresConversion: false,
                    IsNested: false,
                    HasEnumMapping: false));
            }
            else
            {
                // Check if types are different but might be mappable (nested objects or enums)
                targetProp = targetProperties.FirstOrDefault(t => t.Name == sourceProp.Name);
                if (targetProp is not null)
                {
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
                            HasEnumMapping: hasEnumMapping));
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

                var reverseMapping = new MappingInfo(
                    SourceType: mapping.TargetType,
                    TargetType: mapping.SourceType,
                    PropertyMappings: reverseMappings,
                    Bidirectional: false); // Don't generate reverse of reverse

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
        sb.AppendLineLf($"        return new {mapping.TargetType.ToDisplayString()}");
        sb.AppendLineLf("        {");

        for (var i = 0; i < mapping.PropertyMappings.Count; i++)
        {
            var prop = mapping.PropertyMappings[i];
            var isLast = i == mapping.PropertyMappings.Count - 1;
            var comma = isLast ? string.Empty : ",";

            if (prop.RequiresConversion)
            {
                // Enum conversion
                if (prop.HasEnumMapping)
                {
                    // Use EnumMapping extension method (safe mapping with special case handling)
                    var enumMappingMethodName = $"MapTo{prop.TargetProperty.Type.Name}";
                    sb.AppendLineLf($"            {prop.TargetProperty.Name} = source.{prop.SourceProperty.Name}.{enumMappingMethodName}(){comma}");
                }
                else
                {
                    // Fall back to simple cast (less safe but works when no mapping is defined)
                    sb.AppendLineLf($"            {prop.TargetProperty.Name} = ({prop.TargetProperty.Type.ToDisplayString()})source.{prop.SourceProperty.Name}{comma}");
                }
            }
            else if (prop.IsNested)
            {
                // Nested object mapping
                var nestedMethodName = $"MapTo{prop.TargetProperty.Type.Name}";
                sb.AppendLineLf($"            {prop.TargetProperty.Name} = source.{prop.SourceProperty.Name}?.{nestedMethodName}()!{comma}");
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
}