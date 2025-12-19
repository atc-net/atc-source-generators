namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Builder class for creating ValidationAnnotationInfo incrementally.
/// </summary>
internal sealed class ValidationAnnotationBuilder
{
    public bool IsRequired { get; set; }

    public bool? AllowEmptyStrings { get; set; }

    public string? RequiredErrorMessage { get; set; }

    public int? MinLength { get; set; }

    public int? MaxLength { get; set; }

    public string? StringLengthErrorMessage { get; set; }

    public string? RangeMinimum { get; set; }

    public string? RangeMaximum { get; set; }

    public string? RangeOperandType { get; set; }

    public string? RangeErrorMessage { get; set; }

    public string? RegularExpressionPattern { get; set; }

    public string? RegularExpressionErrorMessage { get; set; }

    public bool IsEmailAddress { get; set; }

    public bool IsPhone { get; set; }

    public bool IsUrl { get; set; }

    public bool IsCreditCard { get; set; }

    public int? DataType { get; set; }

    public string? CompareOtherProperty { get; set; }

    public bool IsKey { get; set; }

    public bool? IsEditable { get; set; }

    public bool? IsScaffoldColumn { get; set; }

    public bool IsTimestamp { get; set; }

    /// <summary>
    /// Gets a value indicating whether any validation annotation has been set.
    /// </summary>
    public bool HasAnyValue =>
        IsRequired ||
        AllowEmptyStrings.HasValue ||
        RequiredErrorMessage is not null ||
        MinLength.HasValue ||
        MaxLength.HasValue ||
        StringLengthErrorMessage is not null ||
        RangeMinimum is not null ||
        RangeMaximum is not null ||
        RangeOperandType is not null ||
        RangeErrorMessage is not null ||
        RegularExpressionPattern is not null ||
        RegularExpressionErrorMessage is not null ||
        IsEmailAddress ||
        IsPhone ||
        IsUrl ||
        IsCreditCard ||
        DataType.HasValue ||
        CompareOtherProperty is not null ||
        IsKey ||
        IsEditable.HasValue ||
        IsScaffoldColumn.HasValue ||
        IsTimestamp;

    /// <summary>
    /// Builds the immutable ValidationAnnotationInfo from accumulated values.
    /// </summary>
    public ValidationAnnotationInfo Build() =>
        new(
            IsRequired: IsRequired,
            AllowEmptyStrings: AllowEmptyStrings,
            RequiredErrorMessage: RequiredErrorMessage,
            MinLength: MinLength,
            MaxLength: MaxLength,
            StringLengthErrorMessage: StringLengthErrorMessage,
            RangeMinimum: RangeMinimum,
            RangeMaximum: RangeMaximum,
            RangeOperandType: RangeOperandType,
            RangeErrorMessage: RangeErrorMessage,
            RegularExpressionPattern: RegularExpressionPattern,
            RegularExpressionErrorMessage: RegularExpressionErrorMessage,
            IsEmailAddress: IsEmailAddress,
            IsPhone: IsPhone,
            IsUrl: IsUrl,
            IsCreditCard: IsCreditCard,
            DataType: DataType,
            CompareOtherProperty: CompareOtherProperty,
            IsKey: IsKey,
            IsEditable: IsEditable,
            IsScaffoldColumn: IsScaffoldColumn,
            IsTimestamp: IsTimestamp);
}