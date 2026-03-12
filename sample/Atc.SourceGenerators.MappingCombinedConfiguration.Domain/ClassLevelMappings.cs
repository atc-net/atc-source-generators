namespace Atc.SourceGenerators.MappingCombinedConfiguration.Domain;

// Approach 3: Class-level [MapTypes] shorthand for simple type mappings
[MapTypes(typeof(DeliveryReport), typeof(DeliveryStatus))]
internal static class ClassLevelMappings;