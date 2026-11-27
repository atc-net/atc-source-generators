// ReSharper disable CommentTypo
namespace Atc.SourceGenerators;

/// <summary>
/// Defines diagnostic identifiers for source generators.
/// </summary>
internal static class RuleIdentifierConstants
{
    /// <summary>
    /// Dependency Injection - Rule identifiers from ATCDIR001 to ATCDIR099.
    /// </summary>
    internal static class DependencyInjection
    {
        /// <summary>
        /// ATCDIR001: Service 'As' type must be an interface or abstract class.
        /// </summary>
        internal const string AsTypeMustBeInterface = "ATCDIR001";

        /// <summary>
        /// ATCDIR002: Class does not implement specified interface or inherit from abstract class.
        /// </summary>
        internal const string ClassDoesNotImplementInterface = "ATCDIR002";

        /// <summary>
        /// ATCDIR003: Duplicate service registration with different lifetime.
        /// </summary>
        internal const string DuplicateRegistration = "ATCDIR003";

        /// <summary>
        /// ATCDIR004: Hosted services must use Singleton lifetime.
        /// </summary>
        internal const string HostedServiceMustBeSingleton = "ATCDIR004";

        /// <summary>
        /// ATCDIR005: Factory method not found.
        /// </summary>
        internal const string FactoryMethodNotFound = "ATCDIR005";

        /// <summary>
        /// ATCDIR006: Factory method has invalid signature.
        /// </summary>
        internal const string FactoryMethodInvalidSignature = "ATCDIR006";

        /// <summary>
        /// ATCDIR007: Instance member not found.
        /// </summary>
        internal const string InstanceMemberNotFound = "ATCDIR007";

        /// <summary>
        /// ATCDIR008: Instance member must be static.
        /// </summary>
        internal const string InstanceMemberMustBeStatic = "ATCDIR008";

        /// <summary>
        /// ATCDIR009: Instance and Factory are mutually exclusive.
        /// </summary>
        internal const string InstanceAndFactoryMutuallyExclusive = "ATCDIR009";

        /// <summary>
        /// ATCDIR010: Instance registration requires Singleton lifetime.
        /// </summary>
        internal const string InstanceRequiresSingletonLifetime = "ATCDIR010";

        /// <summary>
        /// ATCDIR011: Replace cannot be combined with TryAdd, Key, Decorator or a hosted service.
        /// </summary>
        internal const string ReplaceNotSupportedWithOption = "ATCDIR011";
    }

    /// <summary>
    /// Options Binding - Rule identifiers from ATCOPT001 to ATCOPT099.
    /// </summary>
    internal static class OptionsBinding
    {
        /// <summary>
        /// ATCOPT001: Options class must be partial.
        /// </summary>
        internal const string OptionsClassMustBePartial = "ATCOPT001";

        /// <summary>
        /// ATCOPT002: Section name cannot be null or empty.
        /// </summary>
        internal const string SectionNameCannotBeEmpty = "ATCOPT002";

        /// <summary>
        /// ATCOPT003: Const section name cannot be null or empty.
        /// </summary>
        internal const string ConstSectionNameCannotBeEmpty = "ATCOPT003";

        /// <summary>
        /// ATCOPT004: OnChange callback requires Monitor lifetime.
        /// </summary>
        internal const string OnChangeRequiresMonitorLifetime = "ATCOPT004";

        /// <summary>
        /// ATCOPT005: OnChange callback not supported with named options.
        /// </summary>
        internal const string OnChangeNotSupportedWithNamedOptions = "ATCOPT005";

        /// <summary>
        /// ATCOPT006: OnChange callback method not found.
        /// </summary>
        internal const string OnChangeCallbackNotFound = "ATCOPT006";

        /// <summary>
        /// ATCOPT007: OnChange callback method has invalid signature.
        /// </summary>
        internal const string OnChangeCallbackInvalidSignature = "ATCOPT007";

        /// <summary>
        /// ATCOPT008: PostConfigure callback not supported with named options.
        /// </summary>
        internal const string PostConfigureNotSupportedWithNamedOptions = "ATCOPT008";

        /// <summary>
        /// ATCOPT009: PostConfigure callback method not found.
        /// </summary>
        internal const string PostConfigureCallbackNotFound = "ATCOPT009";

        /// <summary>
        /// ATCOPT010: PostConfigure callback method has invalid signature.
        /// </summary>
        internal const string PostConfigureCallbackInvalidSignature = "ATCOPT010";

        /// <summary>
        /// ATCOPT011: ConfigureAll requires multiple named options.
        /// </summary>
        internal const string ConfigureAllRequiresMultipleNamedOptions = "ATCOPT011";

        /// <summary>
        /// ATCOPT012: ConfigureAll callback method not found.
        /// </summary>
        internal const string ConfigureAllCallbackNotFound = "ATCOPT012";

        /// <summary>
        /// ATCOPT013: ConfigureAll callback method has invalid signature.
        /// </summary>
        internal const string ConfigureAllCallbackInvalidSignature = "ATCOPT013";

        /// <summary>
        /// ATCOPT014: ChildSections cannot be used with Name property.
        /// </summary>
        internal const string ChildSectionsCannotBeUsedWithName = "ATCOPT014";

        /// <summary>
        /// ATCOPT015: ChildSections requires at least 2 items.
        /// </summary>
        internal const string ChildSectionsRequiresAtLeastTwoItems = "ATCOPT015";

        /// <summary>
        /// ATCOPT016: ChildSections items cannot be null or empty.
        /// </summary>
        internal const string ChildSectionsItemsCannotBeNullOrEmpty = "ATCOPT016";
    }

    /// <summary>
    /// Object Mapping - Rule identifiers from ATCMAP001 to ATCMAP099.
    /// </summary>
    internal static class ObjectMapping
    {
        /// <summary>
        /// ATCMAP001: Mapping class must be partial.
        /// </summary>
        internal const string MappingClassMustBePartial = "ATCMAP001";

        /// <summary>
        /// ATCMAP002: Target type must be a class or struct.
        /// </summary>
        internal const string TargetTypeMustBeClassOrStruct = "ATCMAP002";

        /// <summary>
        /// ATCMAP003: MapProperty target property not found.
        /// </summary>
        internal const string MapPropertyTargetNotFound = "ATCMAP003";

        /// <summary>
        /// ATCMAP004: Required property on target type has no mapping.
        /// </summary>
        internal const string RequiredPropertyNotMapped = "ATCMAP004";

        /// <summary>
        /// ATCMAP011: Collection element type has no mapping method.
        /// </summary>
        internal const string CollectionElementMappingNotFound = "ATCMAP011";

        /// <summary>
        /// ATCMAP012: Two mappings from the same source type would generate a mapping method with the same name.
        /// </summary>
        internal const string DuplicateMappingMethod = "ATCMAP012";

        /// <summary>
        /// ATCMAP013: BeforeMap, AfterMap or Factory method is missing, not static, or not accessible from generated code.
        /// </summary>
        internal const string HookMethodNotCallable = "ATCMAP013";

        /// <summary>
        /// ATCMAP014: Target property is not mapped (no matching source property).
        /// </summary>
        internal const string TargetPropertyNotMapped = "ATCMAP014";

        /// <summary>
        /// ATCMAP015: Nested property is not mapped because its type has no mapping to the target property type.
        /// </summary>
        internal const string NestedMappingNotFound = "ATCMAP015";

        /// <summary>
        /// ATCMAP016: DeepClone copies a property by reference because its type has no self-mapping.
        /// </summary>
        internal const string DeepCloneSharesReference = "ATCMAP016";

        /// <summary>
        /// ATCMAP017: The source path of a class-level [MapProperty] can't be mapped.
        /// </summary>
        internal const string MapPropertySourcePathInvalid = "ATCMAP017";
    }

    /// <summary>
    /// Enum Mapping - Rule identifiers from ATCENUM001 to ATCENUM099.
    /// </summary>
    internal static class EnumMapping
    {
        /// <summary>
        /// ATCENUM001: Target type must be an enum.
        /// </summary>
        internal const string TargetTypeMustBeEnum = "ATCENUM001";

        /// <summary>
        /// ATCENUM002: Source enum value has no matching target value.
        /// </summary>
        internal const string UnmappedEnumValue = "ATCENUM002";

        /// <summary>
        /// ATCENUM004: Auto-detected enum mapping has partial match.
        /// </summary>
        internal const string AutoDetectedEnumPartialMatch = "ATCENUM004";

        /// <summary>
        /// ATCENUM005: Enum types have no matching values; falling back to cast.
        /// </summary>
        internal const string AutoDetectedEnumNoMatch = "ATCENUM005";

        /// <summary>
        /// ATCENUM006: Flag enum mapping may produce incorrect results for combined values.
        /// </summary>
        internal const string FlagEnumMappingWarning = "ATCENUM006";
    }

    /// <summary>
    /// Mapping Configuration - Rule identifiers from ATCMCF001 to ATCMCF099.
    /// </summary>
    internal static class MappingConfiguration
    {
        /// <summary>
        /// ATCMCF001: Mapping configuration class must be declared as static.
        /// </summary>
        internal const string ConfigClassMustBeStatic = "ATCMCF001";

        /// <summary>
        /// ATCMCF002: Mapping configuration class must be declared as partial.
        /// </summary>
        internal const string ConfigClassMustBePartial = "ATCMCF002";

        /// <summary>
        /// ATCMCF004: Mapping method must be an extension method.
        /// </summary>
        internal const string MethodMustBeExtensionMethod = "ATCMCF004";

        /// <summary>
        /// ATCMCF005: Ignored property not found on source type.
        /// </summary>
        internal const string IgnoredPropertyNotFound = "ATCMCF005";

        /// <summary>
        /// ATCMCF006: Renamed source property not found on type.
        /// </summary>
        internal const string RenamedSourcePropertyNotFound = "ATCMCF006";

        /// <summary>
        /// ATCMCF007: Renamed target property not found on type.
        /// </summary>
        internal const string RenamedTargetPropertyNotFound = "ATCMCF007";

        /// <summary>
        /// ATCMCF008: Mapping configuration class has no partial extension methods.
        /// </summary>
        internal const string EmptyConfigurationClass = "ATCMCF008";

        /// <summary>
        /// ATCMCF009: Mapping method must have exactly one parameter (the source type).
        /// </summary>
        internal const string MethodTooManyParameters = "ATCMCF009";

        /// <summary>
        /// ATCMCF010: Mapping method return type must be a class, record, or struct.
        /// </summary>
        internal const string MethodReturnTypeInvalid = "ATCMCF010";

        /// <summary>
        /// ATCMCF011: Map() requires exactly two type arguments (source and target type).
        /// </summary>
        internal const string MapCallRequiresTypeArguments = "ATCMCF011";

        /// <summary>
        /// ATCMCF012: Map() argument must be a typeof() expression or compile-time constant.
        /// </summary>
        internal const string MapCallInvalidArgument = "ATCMCF012";

        /// <summary>
        /// ATCMCF013: AddMappings() requires a lambda expression argument.
        /// </summary>
        internal const string AddMappingsLambdaRequired = "ATCMCF013";

        /// <summary>
        /// ATCMCF014: Type could not be resolved (possibly produced by another source generator).
        /// </summary>
        internal const string TypeNotResolved = "ATCMCF014";

        /// <summary>
        /// ATCMCF015: Property with a matching name is not mapped because the types are not compatible.
        /// </summary>
        internal const string PropertyTypesNotCompatible = "ATCMCF015";

        /// <summary>
        /// ATCMCF016: Target property is not mapped (no matching source property).
        /// </summary>
        internal const string TargetPropertyNotMapped = "ATCMCF016";
    }

    /// <summary>
    /// Extended Object Mapping - Rule identifiers from ATCMAP005 onwards.
    /// </summary>
    internal static class ObjectMappingExtended
    {
        /// <summary>
        /// ATCMAP005: Type has both attribute and configuration-based mapping.
        /// </summary>
        internal const string DuplicateAttributeAndConfiguration = "ATCMAP005";

        /// <summary>
        /// ATCMAP006: Type pair is configured multiple times.
        /// </summary>
        internal const string DuplicateConfigurationMapping = "ATCMAP006";

        /// <summary>
        /// ATCMAP010: Configuration target type must be a class or struct.
        /// </summary>
        internal const string ConfigTargetTypeMustBeClassOrStruct = "ATCMAP010";
    }
}