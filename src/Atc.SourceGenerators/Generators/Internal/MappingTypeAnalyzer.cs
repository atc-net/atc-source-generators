namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Shared helper methods for analyzing types during mapping code generation.
/// Used by both ObjectMappingGenerator and MappingConfigurationGenerator.
/// </summary>
internal static class MappingTypeAnalyzer
{
    /// <summary>
    /// Checks if an attribute class matches the expected name and namespace
    /// using MetadataName comparison instead of expensive ToDisplayString().
    /// </summary>
    internal static bool IsAttributeMatch(
        INamedTypeSymbol? attributeClass,
        string expectedName,
        string expectedNamespace = "Atc.SourceGenerators.Annotations")
    {
        if (attributeClass is null)
        {
            return false;
        }

        return attributeClass.MetadataName == expectedName &&
               attributeClass.ContainingNamespace?.ToDisplayString() == expectedNamespace;
    }

    internal static ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            return nullable.TypeArguments[0];
        }

        return type;
    }

    internal static bool IsNullableValueType(ITypeSymbol type)
        => type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

    internal static bool IsEnumConversion(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var unwrappedSource = UnwrapNullable(sourceType);
        var unwrappedTarget = UnwrapNullable(targetType);
        return unwrappedSource.TypeKind == TypeKind.Enum &&
               unwrappedTarget.TypeKind == TypeKind.Enum;
    }

    /// <summary>
    /// Gets what to append to a null-conditional member access (<c>source.A?.B</c>) that is assigned to a target:
    /// for a non-nullable value-type member the expression is <c>T?</c>, so a non-nullable target needs
    /// <c> ?? default</c> and a nullable target needs nothing; a reference member gets <c>!</c>.
    /// </summary>
    internal static string GetNullConditionalSuffix(
        ITypeSymbol memberType,
        ITypeSymbol targetType)
    {
        if (memberType.IsValueType && !IsNullableValueType(memberType))
        {
            return IsNullableValueType(targetType) ? string.Empty : " ?? default";
        }

        return "!";
    }

    internal static bool IsNestedMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var unwrappedSource = UnwrapNullable(sourceType);
        var unwrappedTarget = UnwrapNullable(targetType);

        return unwrappedSource.TypeKind is TypeKind.Class or TypeKind.Struct &&
               unwrappedTarget.TypeKind is TypeKind.Class or TypeKind.Struct &&
               !IsSystemType(unwrappedSource) &&
               !IsSystemType(unwrappedTarget);
    }

    /// <summary>
    /// Checks if a type (after unwrapping <see cref="Nullable{T}"/>) is a BCL type, i.e. a special type
    /// (int, string, DateTime, ...) or a type declared in the System namespace or one of its sub-namespaces
    /// (Guid, DateTimeOffset, TimeSpan, DateOnly, ...). Such types are never treated as nested mappings.
    /// </summary>
    internal static bool IsSystemType(ITypeSymbol type)
    {
        var unwrapped = UnwrapNullable(type);
        if (unwrapped.SpecialType != SpecialType.None)
        {
            return true;
        }

        var ns = unwrapped.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return ns == "System" ||
               ns.StartsWith("System.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks if the source and target are the same non-enum value type where exactly one side is
    /// <see cref="Nullable{T}"/> (e.g. <c>Guid?</c> → <c>Guid</c> or <c>Guid</c> → <c>Guid?</c>).
    /// </summary>
    internal static bool IsNullableValueTypeWrapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (IsNullableValueType(sourceType) == IsNullableValueType(targetType))
        {
            return false;
        }

        var unwrappedSource = UnwrapNullable(sourceType);
        return unwrappedSource.TypeKind != TypeKind.Enum &&
               SymbolEqualityComparer.Default.Equals(unwrappedSource, UnwrapNullable(targetType));
    }

    /// <summary>
    /// Checks if the source is <c>T?</c> and the target is <c>T</c> for the same value type,
    /// meaning the generated assignment needs <c>?? default</c>.
    /// </summary>
    internal static bool IsNullableToNonNullableValueType(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        => IsNullableValueType(sourceType) &&
           !IsNullableValueType(targetType) &&
           SymbolEqualityComparer.Default.Equals(UnwrapNullable(sourceType), targetType);

    internal static bool IsBuiltInTypeConversion(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var sourceTypeName = UnwrapNullable(sourceType).ToDisplayString();
        var targetTypeName = UnwrapNullable(targetType).ToDisplayString();

        return IsStringToFromSystemTypeConversion(sourceTypeName, targetTypeName) ||
               IsStringToFromUriConversion(sourceTypeName, targetTypeName) ||
               IsStringToFromEnumConversion(sourceType, targetType, sourceTypeName, targetTypeName) ||
               IsDateTimeOffsetToFromDateTimeConversion(sourceTypeName, targetTypeName);
    }

    private static bool IsStringToFromSystemTypeConversion(
        string sourceTypeName,
        string targetTypeName)
    {
        // Compare without the nullable annotation (string? -> int, int -> string?)
        sourceTypeName = sourceTypeName.TrimEnd('?');
        targetTypeName = targetTypeName.TrimEnd('?');

        // DateTime/DateTimeOffset <-> string
        if ((sourceTypeName is "System.DateTime" or "System.DateTimeOffset") &&
            targetTypeName == "string")
        {
            return true;
        }

        if (sourceTypeName == "string" &&
            (targetTypeName is "System.DateTime" or "System.DateTimeOffset"))
        {
            return true;
        }

        // Guid <-> string
        if ((sourceTypeName == "System.Guid" && targetTypeName == "string") ||
            (sourceTypeName == "string" && targetTypeName == "System.Guid"))
        {
            return true;
        }

        // Numeric types <-> string
        if ((IsNumericType(sourceTypeName) && targetTypeName == "string") ||
            (sourceTypeName == "string" && IsNumericType(targetTypeName)))
        {
            return true;
        }

        // bool <-> string
        if ((sourceTypeName == "bool" && targetTypeName == "string") ||
            (sourceTypeName == "string" && targetTypeName == "bool"))
        {
            return true;
        }

        return false;
    }

    private static bool IsStringToFromUriConversion(
        string sourceTypeName,
        string targetTypeName)
    {
        var normalizedSource = sourceTypeName.TrimEnd('?');
        var normalizedTarget = targetTypeName.TrimEnd('?');

        return (normalizedSource == "string" && normalizedTarget == "System.Uri") ||
               (normalizedSource == "System.Uri" && normalizedTarget == "string");
    }

    private static bool IsDateTimeOffsetToFromDateTimeConversion(
        string sourceTypeName,
        string targetTypeName)
    {
        var normalizedSource = sourceTypeName.TrimEnd('?');
        var normalizedTarget = targetTypeName.TrimEnd('?');

        return (normalizedSource == "System.DateTimeOffset" && normalizedTarget == "System.DateTime") ||
               (normalizedSource == "System.DateTime" && normalizedTarget == "System.DateTimeOffset");
    }

    private static bool IsStringToFromEnumConversion(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        string sourceTypeName,
        string targetTypeName)
    {
        var normalizedSource = sourceTypeName.TrimEnd('?');
        var normalizedTarget = targetTypeName.TrimEnd('?');

        if (normalizedSource == "string" && UnwrapNullable(targetType).TypeKind == TypeKind.Enum)
        {
            return true;
        }

        if (UnwrapNullable(sourceType).TypeKind == TypeKind.Enum && normalizedTarget == "string")
        {
            return true;
        }

        return false;
    }

    internal static bool IsNumericType(string typeName)
        => typeName is "int" or "long" or "short" or "byte" or "sbyte" or
           "uint" or "ulong" or "ushort" or
           "decimal" or "double" or "float";

    internal static bool IsNumericTypeConversion(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var sourceName = UnwrapNullable(sourceType).ToDisplayString();
        var targetName = UnwrapNullable(targetType).ToDisplayString();
        return sourceName != targetName && IsNumericType(sourceName) && IsNumericType(targetName);
    }

    internal static bool IsCollectionType(
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
        if (namedType is { IsGenericType: true, TypeArguments.Length: 1 } &&
            IsKnownCollectionType(namedType.ConstructedFrom))
        {
            elementType = namedType.TypeArguments[0];
            return true;
        }

        return false;
    }

    internal static bool IsKnownCollectionType(
        INamedTypeSymbol originalDefinition)
    {
        var name = originalDefinition.MetadataName;
        var ns = originalDefinition.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        return (ns == "System.Collections.Generic" &&
                name is "List`1" or "IEnumerable`1" or "ICollection`1" or
                         "IList`1" or "IReadOnlyList`1" or "IReadOnlyCollection`1")
               || (ns == "System.Collections.ObjectModel" &&
                   name is "Collection`1" or "ReadOnlyCollection`1");
    }

    /// <summary>
    /// Checks whether a dictionary on the source side can be copied into a dictionary on the target side
    /// with <c>ToDictionary(x =&gt; x.Key, x =&gt; ...)</c>: the target is <c>Dictionary&lt;K, V&gt;</c>,
    /// <c>IDictionary&lt;K, V&gt;</c> or <c>IReadOnlyDictionary&lt;K, V&gt;</c>, the source is an
    /// <c>IEnumerable&lt;KeyValuePair&lt;K, V&gt;&gt;</c>, and the key types are the same on both sides
    /// (nullable reference annotations are ignored). The value types are returned so the caller can decide
    /// how to convert them.
    /// </summary>
    internal static bool TryGetDictionaryMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        out ITypeSymbol? sourceValueType,
        out ITypeSymbol? targetValueType)
    {
        sourceValueType = null;
        targetValueType = null;

        if (!IsKnownDictionaryTargetType(targetType, out var targetKey, out var targetValue) ||
            !TryGetKeyValuePairTypes(sourceType, out var sourceKey, out var sourceValue) ||
            !SymbolEqualityComparer.Default.Equals(sourceKey, targetKey))
        {
            return false;
        }

        sourceValueType = sourceValue;
        targetValueType = targetValue;
        return true;
    }

    /// <summary>
    /// Checks whether dictionary values can be converted without a mapping method: the same type, or
    /// two enums (mapped with an enum mapping method or a cast).
    /// </summary>
    internal static bool AreDictionaryValuesDirectlyConvertible(
        ITypeSymbol sourceValueType,
        ITypeSymbol targetValueType)
        => SymbolEqualityComparer.Default.Equals(sourceValueType, targetValueType) ||
           IsDictionaryValueEnumConversion(sourceValueType, targetValueType);

    /// <summary>
    /// Checks whether dictionary values are two different non-nullable enums.
    /// </summary>
    internal static bool IsDictionaryValueEnumConversion(
        ITypeSymbol sourceValueType,
        ITypeSymbol targetValueType)
        => sourceValueType.TypeKind == TypeKind.Enum &&
           targetValueType.TypeKind == TypeKind.Enum &&
           !SymbolEqualityComparer.Default.Equals(sourceValueType, targetValueType);

    /// <summary>
    /// Checks whether dictionary values are two different user types (classes, records or structs, not
    /// <see cref="Nullable{T}"/>) that need a nested mapping method.
    /// </summary>
    internal static bool IsDictionaryValueNestedMapping(
        ITypeSymbol sourceValueType,
        ITypeSymbol targetValueType)
        => !IsNullableValueType(sourceValueType) &&
           !IsNullableValueType(targetValueType) &&
           !SymbolEqualityComparer.Default.Equals(sourceValueType, targetValueType) &&
           IsNestedMapping(sourceValueType, targetValueType);

    private static bool IsKnownDictionaryTargetType(
        ITypeSymbol type,
        out ITypeSymbol? keyType,
        out ITypeSymbol? valueType)
    {
        keyType = null;
        valueType = null;

        if (type is not INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 2 } namedType)
        {
            return false;
        }

        var originalDefinition = namedType.ConstructedFrom;
        if (originalDefinition.ContainingNamespace?.ToDisplayString() != "System.Collections.Generic" ||
            originalDefinition.MetadataName is not ("Dictionary`2" or "IDictionary`2" or "IReadOnlyDictionary`2"))
        {
            return false;
        }

        keyType = namedType.TypeArguments[0];
        valueType = namedType.TypeArguments[1];
        return true;
    }

    private static bool TryGetKeyValuePairTypes(
        ITypeSymbol type,
        out ITypeSymbol? keyType,
        out ITypeSymbol? valueType)
    {
        keyType = null;
        valueType = null;

        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        var candidates = namedType.TypeKind == TypeKind.Interface
            ? namedType.AllInterfaces.Add(namedType)
            : namedType.AllInterfaces;

        foreach (var candidate in candidates)
        {
            if (candidate is not { IsGenericType: true, TypeArguments.Length: 1 } ||
                candidate.ConstructedFrom.SpecialType != SpecialType.System_Collections_Generic_IEnumerable_T ||
                candidate.TypeArguments[0] is not INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 2 } pair ||
                pair.ConstructedFrom.MetadataName != "KeyValuePair`2" ||
                pair.ConstructedFrom.ContainingNamespace?.ToDisplayString() != "System.Collections.Generic")
            {
                continue;
            }

            keyType = pair.TypeArguments[0];
            valueType = pair.TypeArguments[1];
            return true;
        }

        return false;
    }

    internal static string GetCollectionTargetType(
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

        var originalDef = namedType.ConstructedFrom;
        var name = originalDef.MetadataName;
        var ns = originalDef.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        if (ns == "System.Collections.ObjectModel")
        {
            if (name == "Collection`1")
            {
                return "Collection";
            }

            if (name == "ReadOnlyCollection`1")
            {
                return "ReadOnlyCollection";
            }
        }

        return "List";
    }

    internal static bool HasMapIgnoreAttribute(IPropertySymbol property)
    {
        var attributes = property.GetAttributes();
        return attributes.Any(attr =>
            IsAttributeMatch(attr.AttributeClass, "MapIgnoreAttribute"));
    }

    internal static bool IsImplicitlyConvertible(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (targetType.TypeKind == TypeKind.Interface)
        {
            return sourceType.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i, targetType));
        }

        var current = sourceType.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, targetType))
            {
                return true;
            }

            current = current.BaseType;
        }

        if (targetType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedTarget)
        {
            return SymbolEqualityComparer.Default.Equals(sourceType, namedTarget.TypeArguments[0]);
        }

        return false;
    }

    internal static List<IPropertySymbol> GetAllProperties(
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

    internal static bool HasEnumMappingAttribute(
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

    /// <summary>
    /// Returns true when both types are collections whose element types are the same, or can be mapped
    /// according to the generator's own rule.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="areElementsMappable">The generator's rule for element types that a mapping can bridge.</param>
    internal static bool AreCollectionsWithCompatibleElements(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        Func<ITypeSymbol, ITypeSymbol, bool> areElementsMappable)
    {
        if (!IsCollectionType(sourceType, out var sourceElement) ||
            !IsCollectionType(targetType, out var targetElement) ||
            sourceElement is null ||
            targetElement is null)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(sourceElement, targetElement) ||
               areElementsMappable(sourceElement, targetElement);
    }

    /// <summary>
    /// Returns true when a mapping can bridge the two types: both are enums (enum mapping), or both are classes or
    /// structs outside the BCL (nested mapping). The same rule decides how a property is mapped, so a constructor
    /// parameter is only accepted when its property can be mapped.
    /// </summary>
    internal static bool AreBothCustomOrEnumTypes(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        => (UnwrapNullable(sourceType).TypeKind == TypeKind.Enum && UnwrapNullable(targetType).TypeKind == TypeKind.Enum) ||
           IsNestedMapping(sourceType, targetType);
}