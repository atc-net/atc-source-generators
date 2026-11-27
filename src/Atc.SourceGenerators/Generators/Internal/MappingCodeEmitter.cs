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
}