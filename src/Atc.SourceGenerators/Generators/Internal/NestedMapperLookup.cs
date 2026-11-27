namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Decides whether the <c>MapTo{Target}()</c> call that the [MapTo] generator emits for a nested property will
/// bind, and records the nested properties for which it won't (reported as ATCMAP015).
/// </summary>
/// <remarks>
/// A nested mapper is found when:
/// <list type="bullet">
/// <item>the [MapTo] generator generates it itself in this compilation (also for a base class of the source type,
/// a generic definition, or the reverse of a bidirectional mapping);</item>
/// <item>the compilation has configuration mappings ([MappingConfiguration], [MapTypes], AddMappings or
/// MappingBuilder.Configure), whose generated mappers this generator can't see - then every call is kept;</item>
/// <item>a probe call with the usings of the generated file binds: hand-written mappers, and mappers in
/// referenced assemblies.</item>
/// </list>
/// The lookups only run for nested properties, and the probe only when the first two don't apply.
/// </remarks>
internal sealed class NestedMapperLookup
{
    private static readonly HashSet<string> ConfigurationAttributeNames = new(StringComparer.Ordinal)
    {
        "MappingConfiguration",
        "MappingConfigurationAttribute",
        "MapTypes",
        "MapTypesAttribute",
    };

    private readonly Compilation compilation;
    private readonly List<(INamedTypeSymbol Source, string MethodName)> generatedMappers;
    private readonly IReadOnlyCollection<string> namespaces;
    private readonly Dictionary<string, bool> cache = new(StringComparer.Ordinal);
    private readonly HashSet<string> reported = new(StringComparer.Ordinal);
    private bool? hasConfigurationMappings;

    public NestedMapperLookup(
        Compilation compilation,
        IEnumerable<(INamedTypeSymbol Source, INamedTypeSymbol Target)> generatedMappings,
        IReadOnlyCollection<string> namespaces)
    {
        this.compilation = compilation;
        this.namespaces = namespaces;
        generatedMappers = generatedMappings
            .Select(m => (m.Source, GetMethodName(m.Target)))
            .ToList();
    }

    /// <summary>
    /// Gets the nested properties without a mapper: the source property, its type and the target type.
    /// </summary>
    public List<(IPropertySymbol Property, ITypeSymbol SourceType, ITypeSymbol TargetType)> Missing { get; } = [];

    /// <summary>
    /// Gets the name of the mapping method that maps to the type, as the [MapTo] generator names it.
    /// </summary>
    public static string GetMethodName(ITypeSymbol targetType)
        => $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(targetType.Name)}";

    /// <summary>
    /// Returns <c>true</c> when the nested mapping of the property has a mapper; otherwise records the property
    /// as missing (once) and returns <c>false</c>.
    /// </summary>
    public bool HasMapper(PropertyMapping mapping)
    {
        var sourceType = MappingTypeAnalyzer.UnwrapNullable(mapping.SourceProperty.Type);
        var methodName = GetMethodName(mapping.TargetProperty.Type);

        var key = $"{sourceType.ToDisplayString()}|{methodName}";
        if (!cache.TryGetValue(key, out var found))
        {
            found = Resolve(sourceType, methodName);
            cache[key] = found;
        }

        if (!found &&
            reported.Add($"{mapping.SourceProperty.ContainingType.ToDisplayString()}.{mapping.SourceProperty.Name}|{methodName}"))
        {
            Missing.Add((mapping.SourceProperty, sourceType, mapping.TargetProperty.Type));
        }

        return found;
    }

    private bool Resolve(
        ITypeSymbol sourceType,
        string methodName)
    {
        // Open generic or unresolved types can't be checked reliably: keep the call
        if (sourceType is not INamedTypeSymbol namedSource ||
            sourceType.TypeKind == TypeKind.Error ||
            ContainsTypeParameter(namedSource))
        {
            return true;
        }

        if (generatedMappers.Any(m => m.MethodName == methodName && IsAssignableTo(namedSource, m.Source)))
        {
            return true;
        }

        hasConfigurationMappings ??= HasConfigurationMappings();
        return hasConfigurationMappings.Value || ProbeBinds(namedSource, methodName);
    }

    private static bool ContainsTypeParameter(INamedTypeSymbol type)
        => type.TypeArguments.Any(a => a.TypeKind == TypeKind.TypeParameter ||
                                       (a is INamedTypeSymbol named && ContainsTypeParameter(named)));

    private static bool IsAssignableTo(
        INamedTypeSymbol type,
        INamedTypeSymbol mapperSource)
    {
        var expected = mapperSource.OriginalDefinition;
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, expected))
            {
                return true;
            }
        }

        return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, expected));
    }

    private bool HasConfigurationMappings()
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            // Skip generated files (the fallback attribute definitions mention the attribute names)
            if (tree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                switch (node)
                {
                    case AttributeSyntax attribute when ConfigurationAttributeNames.Contains(GetRightmostName(attribute.Name)):
                        return true;
                    case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess }
                        when memberAccess.Name.Identifier.Text == "AddMappings" ||
                             (memberAccess.Name.Identifier.Text == "Configure" &&
                              memberAccess.Expression.ToString().EndsWith("MappingBuilder", StringComparison.Ordinal)):
                        return true;
                }
            }
        }

        return false;
    }

    private static string GetRightmostName(NameSyntax name)
        => name switch
        {
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            AliasQualifiedNameSyntax alias => alias.Name.Identifier.Text,
            SimpleNameSyntax simple => simple.Identifier.Text,
            _ => name.ToString(),
        };

    private bool ProbeBinds(
        INamedTypeSymbol sourceType,
        string methodName)
    {
        var sb = new StringBuilder();
        foreach (var ns in namespaces.OrderBy(n => n, StringComparer.Ordinal))
        {
            sb.AppendLineLf($"using {ns};");
        }

        sb.AppendLineLf("namespace Atc.Mapping");
        sb.AppendLineLf("{");
        sb.AppendLineLf("    internal static class AtcNestedMapperProbe");
        sb.AppendLineLf("    {");
        sb.AppendLineLf($"        private static void Probe({sourceType.ToGlobalDisplayString()} source)");
        sb.AppendLineLf("        {");
        sb.AppendLineLf($"            _ = source.{methodName}();");
        sb.AppendLineLf("        }");
        sb.AppendLineLf("    }");
        sb.AppendLineLf("}");

        var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
        var probeTree = CSharpSyntaxTree.ParseText(sb.ToString(), options);
        var probeCompilation = compilation.AddSyntaxTrees(probeTree);
        var invocation = probeTree
            .GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First();

        var symbolInfo = probeCompilation
            .GetSemanticModel(probeTree)
            .GetSymbolInfo(invocation);

        // An ambiguous call (several mappers) also means that a mapper exists; any other failure doesn't
        return symbolInfo.Symbol is not null || symbolInfo.CandidateReason == CandidateReason.Ambiguous;
    }
}