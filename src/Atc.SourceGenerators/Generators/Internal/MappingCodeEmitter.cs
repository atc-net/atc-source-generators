namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Code emission shared by the attribute-based (<c>[MapTo]</c>) and configuration-based mapping generators.
/// Where the generators differ, the caller passes the difference in.
/// </summary>
internal static class MappingCodeEmitter
{
    /// <summary>
    /// Appends the object initializer assignments (<c>Target = value,</c>) for the property mappings.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="properties">The property mappings to assign.</param>
    /// <param name="getValue">Gets the value expression of a property mapping.</param>
    public static void AppendPropertyInitializers(
        StringBuilder sb,
        List<PropertyMapping> properties,
        Func<PropertyMapping, string> getValue)
    {
        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var isLast = i == properties.Count - 1;
            var comma = isLast ? string.Empty : ",";

            var value = getValue(prop);
            sb.AppendLineLf($"            {prop.TargetProperty.Name} = {value}{comma}");
        }
    }

    /// <summary>
    /// Gets the value selector of a dictionary copy (<c>x => ...</c> in <c>ToDictionary</c>): the value itself,
    /// the enum mapping method or an enum cast, or a call to the value type's mapper.
    /// </summary>
    /// <param name="prop">The dictionary property mapping.</param>
    /// <param name="callValueMapper">
    /// Formats the call of the value type's mapper from its method name and the value expression, for example
    /// <c>x.Value.MapToItem()</c> for an extension method or <c>MapToItem(x.Value)</c> for a generated nested mapper.
    /// </param>
    public static string GetDictionaryValueConversion(
        PropertyMapping prop,
        Func<string, string, string> callValueMapper)
    {
        var sourceValueType = prop.SourceCollectionElementType;
        var targetValueType = prop.CollectionElementType;

        if (sourceValueType is null ||
            targetValueType is null ||
            SymbolEqualityComparer.Default.Equals(sourceValueType, targetValueType))
        {
            return "x.Value";
        }

        var methodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(targetValueType.Name)}";

        if (MappingTypeAnalyzer.IsDictionaryValueEnumConversion(sourceValueType, targetValueType))
        {
            // Enum mappers come from the enum mapping generator ([MapTo] on the enum) and keep extension syntax;
            // without any enum mapping, fall back to a cast
            return prop.HasEnumMapping
                ? $"x.Value.{methodName}()"
                : $"({targetValueType.ToGlobalDisplayString()})x.Value";
        }

        // Value mappers return default for a null source, so a nullable reference value only needs '!'
        var value = sourceValueType.NullableAnnotation == NullableAnnotation.Annotated && !sourceValueType.IsValueType
            ? "x.Value!"
            : "x.Value";

        return callValueMapper(methodName, value);
    }

    /// <summary>
    /// Gets the call of the enum mapping method (<c>MapTo{TargetEnum}()</c>) for an enum property. A nullable source
    /// is unwrapped with a pattern, and maps null to null for a nullable target or to default otherwise.
    /// </summary>
    /// <param name="prop">The enum property mapping.</param>
    /// <param name="sourceAccess">The source property expression, for example <c>source.Status</c>.</param>
    public static string GetEnumMappingCall(
        PropertyMapping prop,
        string sourceAccess)
    {
        // Name the method after the underlying enum, so Status? -> StatusDto? calls MapToStatusDto (not MapToNullable)
        var unwrappedTarget = MappingTypeAnalyzer.UnwrapNullable(prop.TargetProperty.Type);
        var enumMappingMethodName = $"MapTo{TypeNameSanitizer.SanitizeForIdentifier(unwrappedTarget.Name)}";

        if (MappingTypeAnalyzer.IsNullableValueType(prop.SourceProperty.Type))
        {
            var varName = $"__{char.ToLowerInvariant(prop.SourceProperty.Name[0])}{prop.SourceProperty.Name.Substring(1)}";
            var whenNull = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type) ? "null" : "default";
            return $"{sourceAccess} is {{ }} {varName} ? {varName}.{enumMappingMethodName}() : {whenNull}";
        }

        return $"{sourceAccess}.{enumMappingMethodName}()";
    }

    /// <summary>
    /// Gets the conversion of a collection property to the target collection type: <c>ToArray()</c>,
    /// <c>ToList()</c>, or a <c>Collection&lt;T&gt;</c> / <c>ReadOnlyCollection&lt;T&gt;</c> around a list,
    /// with an optional <c>Select</c> that maps each element.
    /// </summary>
    /// <param name="sourceAccess">The source collection expression, for example <c>source.Items</c>.</param>
    /// <param name="collectionTargetType">The target collection kind: <c>Array</c>, <c>Collection</c>, <c>ReadOnlyCollection</c> or <c>List</c>.</param>
    /// <param name="targetElementType">The target element type.</param>
    /// <param name="elementSelector">The element mapping lambda, for example <c>x =&gt; x.MapToItemDto()</c>, or <c>null</c> to copy the elements.</param>
    public static string GetCollectionConversion(
        string sourceAccess,
        string collectionTargetType,
        ITypeSymbol targetElementType,
        string? elementSelector)
    {
        var elements = elementSelector is null
            ? $"{sourceAccess}?"
            : $"{sourceAccess}?.Select({elementSelector})";

        return collectionTargetType switch
        {
            "Array" => $"{elements}.ToArray()!",
            "Collection" => $"new global::System.Collections.ObjectModel.Collection<{targetElementType.ToGlobalDisplayString()}>({elements}.ToList()!)",
            "ReadOnlyCollection" => $"new global::System.Collections.ObjectModel.ReadOnlyCollection<{targetElementType.ToGlobalDisplayString()}>({elements}.ToList()!)",
            _ => $"{elements}.ToList()!",
        };
    }

    /// <summary>
    /// Gets the conversion expression of a built-in type conversion (DateTime, DateTimeOffset, Guid, numeric and bool
    /// to and from string, DateTime to and from DateTimeOffset, string to and from Uri and enum), including the
    /// nullable variants. Each generator decides with its own predicate which conversions it uses.
    /// </summary>
    [SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "OK - one branch per conversion")]
    public static string GetBuiltInTypeConversion(
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

        // DateTimeOffset <-> DateTime conversions
        if (normalizedSource == "System.DateTimeOffset" && normalizedTarget == "System.DateTime")
        {
            var sourceIsNullableValue = MappingTypeAnalyzer.IsNullableValueType(prop.SourceProperty.Type);
            var targetIsNullableValue = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type);

            if (sourceIsNullableValue && targetIsNullableValue)
            {
                return $"{sourcePropertyAccess}?.DateTime";
            }

            if (sourceIsNullableValue)
            {
                // T? -> T falls back to default when null, like any other nullable value type
                return $"{sourcePropertyAccess}?.DateTime ?? default";
            }

            return $"{sourcePropertyAccess}.DateTime";
        }

        if (normalizedSource == "System.DateTime" && normalizedTarget == "System.DateTimeOffset")
        {
            var sourceIsNullableValue = MappingTypeAnalyzer.IsNullableValueType(prop.SourceProperty.Type);
            var targetIsNullableValue = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type);

            if (sourceIsNullableValue)
            {
                // T? -> T falls back to default when null, like any other nullable value type
                var varName = $"__{char.ToLowerInvariant(prop.SourceProperty.Name[0])}{prop.SourceProperty.Name.Substring(1)}";
                var whenNull = targetIsNullableValue ? "null" : "default";
                return $"{sourcePropertyAccess} is {{ }} {varName} ? new global::System.DateTimeOffset({varName}) : {whenNull}";
            }

            return $"new global::System.DateTimeOffset({sourcePropertyAccess})";
        }

        // string <-> Uri conversions
        if (normalizedSource == "string" && normalizedTarget == "System.Uri")
        {
            if (sourceIsNullableRef || targetIsNullableRef)
            {
                return $"!string.IsNullOrEmpty({sourcePropertyAccess}) ? new global::System.Uri({sourcePropertyAccess}) : null";
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
            var enumType = unwrappedTargetType.ToGlobalDisplayString();
            var sourceIsNullable = sourceIsNullableRef ||
                                   prop.SourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated;
            var targetIsNullableValue = MappingTypeAnalyzer.IsNullableValueType(prop.TargetProperty.Type);

            if (sourceIsNullable && targetIsNullableValue)
            {
                var varName = $"__{char.ToLowerInvariant(prop.SourceProperty.Name[0])}{prop.SourceProperty.Name.Substring(1)}";
                return $"{sourcePropertyAccess} is {{ }} {varName} ? (global::System.Enum.TryParse<{enumType}>({varName}, out var __parsed) ? __parsed : default({enumType}?)) : null";
            }

            return $"global::System.Enum.TryParse<{enumType}>({sourcePropertyAccess}, out var __parsed) ? __parsed : default({enumType})";
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
}