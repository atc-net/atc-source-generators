namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Combined validation attribute information from various DataAnnotation attributes.
/// </summary>
internal sealed record ValidationAnnotationInfo(
    bool IsRequired,
    bool? AllowEmptyStrings,
    string? RequiredErrorMessage,
    int? MinLength,
    int? MaxLength,
    string? StringLengthErrorMessage,
    string? RangeMinimum,
    string? RangeMaximum,
    string? RangeOperandType,
    string? RangeErrorMessage,
    string? RegularExpressionPattern,
    string? RegularExpressionErrorMessage,
    bool IsEmailAddress,
    bool IsPhone,
    bool IsUrl,
    bool IsCreditCard,
    int? DataType,
    string? CompareOtherProperty,
    bool IsKey,
    bool? IsEditable,
    bool? IsScaffoldColumn,
    bool IsTimestamp);