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