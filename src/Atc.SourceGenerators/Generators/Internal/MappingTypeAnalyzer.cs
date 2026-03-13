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

    internal static bool IsEnumConversion(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        => sourceType.TypeKind == TypeKind.Enum &&
           targetType.TypeKind == TypeKind.Enum;

    internal static bool IsNestedMapping(
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

    internal static bool IsBuiltInTypeConversion(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var sourceTypeName = sourceType.ToDisplayString();
        var targetTypeName = targetType.ToDisplayString();

        // DateTime/DateTimeOffset -> string
        if ((sourceTypeName is "System.DateTime" or "System.DateTimeOffset") &&
            targetTypeName == "string")
        {
            return true;
        }

        // string -> DateTime/DateTimeOffset
        if (sourceTypeName == "string" &&
            (targetTypeName is "System.DateTime" or "System.DateTimeOffset"))
        {
            return true;
        }

        // Guid -> string
        if (sourceTypeName == "System.Guid" && targetTypeName == "string")
        {
            return true;
        }

        // string -> Guid
        if (sourceTypeName == "string" && targetTypeName == "System.Guid")
        {
            return true;
        }

        // Numeric types -> string
        if (IsNumericType(sourceTypeName) && targetTypeName == "string")
        {
            return true;
        }

        // string -> Numeric types
        if (sourceTypeName == "string" && IsNumericType(targetTypeName))
        {
            return true;
        }

        // bool -> string
        if (sourceTypeName == "bool" && targetTypeName == "string")
        {
            return true;
        }

        // string -> bool
        if (sourceTypeName == "string" && targetTypeName == "bool")
        {
            return true;
        }

        return false;
    }

    internal static bool IsNumericType(string typeName)
        => typeName is "int" or "long" or "short" or "byte" or "sbyte" or
           "uint" or "ulong" or "ushort" or
           "decimal" or "double" or "float";

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
}