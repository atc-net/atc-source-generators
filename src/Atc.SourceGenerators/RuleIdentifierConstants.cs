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
        /// ATCDIR001: Service 'As' type must be an interface.
        /// </summary>
        internal const string AsTypeMustBeInterface = "ATCDIR001";

        /// <summary>
        /// ATCDIR002: Class does not implement specified interface.
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
    }
}