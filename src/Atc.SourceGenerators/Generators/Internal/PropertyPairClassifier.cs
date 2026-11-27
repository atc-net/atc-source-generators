namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Decides how a source property maps to a target property of a different type (the same type is a direct
/// assignment): a user conversion method, a dictionary copy, a collection, or an enum, nested, built-in, numeric
/// or nullable conversion. Shared by the attribute-based and configuration-based mapping generators, which pass
/// their differences as <see cref="PropertyMappingRules"/>.
/// </summary>
internal static class PropertyPairClassifier
{
    public static PropertyPairResult Classify(
        IPropertySymbol sourceProp,
        IPropertySymbol targetProp,
        PropertyMappingRules rules)
    {
        var conversionMethod = rules.FindConversionMethod(sourceProp.Type, targetProp.Type);
        if (conversionMethod is not null)
        {
            return Mapped(Create(sourceProp, targetProp) with { ConversionMethod = conversionMethod });
        }

        return TryClassifyDictionary(sourceProp, targetProp, rules) ??
               TryClassifyCollection(sourceProp, targetProp, rules) ??
               ClassifyConversion(sourceProp, targetProp, rules);
    }

    private static PropertyPairResult? TryClassifyDictionary(
        IPropertySymbol sourceProp,
        IPropertySymbol targetProp,
        PropertyMappingRules rules)
    {
        if (!MappingTypeAnalyzer.TryGetDictionaryMapping(sourceProp.Type, targetProp.Type, out var sourceValueType, out var targetValueType))
        {
            return null;
        }

        if (MappingTypeAnalyzer.AreDictionaryValuesDirectlyConvertible(sourceValueType!, targetValueType!) ||
            rules.CanMapDictionaryValue(sourceValueType!, targetValueType!))
        {
            // Copied with ToDictionary, converting each value
            var hasEnumMapping = MappingTypeAnalyzer.IsDictionaryValueEnumConversion(sourceValueType!, targetValueType!) &&
                                 rules.ResolveEnumMapping(sourceValueType!, targetValueType!);

            return Mapped(Create(sourceProp, targetProp) with
            {
                HasEnumMapping = hasEnumMapping,
                CollectionElementType = targetValueType,
                SourceCollectionElementType = sourceValueType,
                IsDictionary = true,
            });
        }

        return rules.UnmappableDictionaryFallsThrough
            ? null
            : new PropertyPairResult(null, PropertyPairFailure.ElementHasNoMapping, sourceValueType, targetValueType);
    }

    private static PropertyPairResult? TryClassifyCollection(
        IPropertySymbol sourceProp,
        IPropertySymbol targetProp,
        PropertyMappingRules rules)
    {
        if (!MappingTypeAnalyzer.IsCollectionType(sourceProp.Type, out var sourceElementType) ||
            !MappingTypeAnalyzer.IsCollectionType(targetProp.Type, out var targetElementType) ||
            sourceElementType is null ||
            targetElementType is null)
        {
            return null;
        }

        if (!rules.CanMapCollectionElement(sourceElementType, targetElementType))
        {
            return new PropertyPairResult(null, PropertyPairFailure.ElementHasNoMapping, sourceElementType, targetElementType);
        }

        var hasEnumMapping = MappingTypeAnalyzer.IsEnumConversion(sourceElementType, targetElementType) &&
                             rules.ResolveEnumMapping(sourceElementType, targetElementType);

        return Mapped(Create(sourceProp, targetProp) with
        {
            HasEnumMapping = hasEnumMapping,
            IsCollection = true,
            CollectionElementType = targetElementType,
            SourceCollectionElementType = sourceElementType,
            CollectionTargetType = MappingTypeAnalyzer.GetCollectionTargetType(targetProp.Type),
        });
    }

    private static PropertyPairResult ClassifyConversion(
        IPropertySymbol sourceProp,
        IPropertySymbol targetProp,
        PropertyMappingRules rules)
    {
        var sourceType = sourceProp.Type;
        var targetType = targetProp.Type;

        // Enum, nested, built-in, numeric or nullable conversion. The analyzer unwraps Nullable<T>, so
        // Status? -> StatusDto? is an enum conversion (not a nested mapping of Nullable<T>).
        var isEnumConversion = MappingTypeAnalyzer.IsEnumConversion(sourceType, targetType);
        var isBuiltInTypeConversion = MappingTypeAnalyzer.IsBuiltInTypeConversion(sourceType, targetType);
        var isNumericConversion = rules.AllowNumericConversions && MappingTypeAnalyzer.IsNumericTypeConversion(sourceType, targetType);
        var isNullableValueTypeWrapping = MappingTypeAnalyzer.IsNullableValueTypeWrapping(sourceType, targetType);

        // Same value type with only nullability differing (Guid? -> Guid) is a direct assignment, and built-in
        // conversions take precedence over nested mapping detection
        var isNested = MappingTypeAnalyzer.IsNestedMapping(sourceType, targetType) &&
                       !isNullableValueTypeWrapping &&
                       !isBuiltInTypeConversion;

        if (!isEnumConversion && !isNested && !isBuiltInTypeConversion && !isNumericConversion && !isNullableValueTypeWrapping)
        {
            return new PropertyPairResult(null, PropertyPairFailure.IncompatibleTypes);
        }

        var hasEnumMapping = isEnumConversion &&
                             rules.ResolveEnumMapping(
                                 MappingTypeAnalyzer.UnwrapNullable(sourceType),
                                 MappingTypeAnalyzer.UnwrapNullable(targetType));

        return Mapped(Create(sourceProp, targetProp) with
        {
            RequiresConversion = isEnumConversion || isNumericConversion,
            IsNested = isNested,
            HasEnumMapping = hasEnumMapping,
            IsBuiltInTypeConversion = isBuiltInTypeConversion,
        });
    }

    /// <summary>
    /// Creates a direct property mapping (same type, plain assignment).
    /// </summary>
    public static PropertyMapping Create(
        IPropertySymbol sourceProp,
        IPropertySymbol targetProp)
        => new(
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
            SourceRequiresUnsafeAccessor: false,
            TargetRequiresUnsafeAccessor: false);

    private static PropertyPairResult Mapped(PropertyMapping mapping)
        => new(mapping, PropertyPairFailure.None);
}