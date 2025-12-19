namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Builder class for creating AtcAnnotationInfo incrementally.
/// </summary>
internal sealed class AtcAnnotationBuilder
{
    // IgnoreDisplayAttribute
    public bool IsIgnoreDisplay { get; set; }

    // EnumGuidAttribute
    public string? EnumGuid { get; set; }

    // CasingStyleDescriptionAttribute
    public string? CasingStyleDefault { get; set; }

    public string? CasingStylePrefix { get; set; }

    // IPAddressAttribute
    public bool IsIPAddress { get; set; }

    public bool? IPAddressRequired { get; set; }

    // IsoCurrencySymbolAttribute
    public bool IsIsoCurrencySymbol { get; set; }

    public bool? IsoCurrencySymbolRequired { get; set; }

    public string[]? IsoCurrencySymbols { get; set; }

    // StringAttribute (and KeyStringAttribute)
    public bool IsAtcString { get; set; }

    public bool IsKeyString { get; set; }

    public bool? StringRequired { get; set; }

    public uint? StringMinLength { get; set; }

    public uint? StringMaxLength { get; set; }

    public char[]? StringInvalidCharacters { get; set; }

    public string[]? StringInvalidPrefixStrings { get; set; }

    public string? StringRegularExpression { get; set; }

    // UriAttribute
    public bool IsAtcUri { get; set; }

    public bool? UriRequired { get; set; }

    public bool? UriAllowHttp { get; set; }

    public bool? UriAllowHttps { get; set; }

    public bool? UriAllowFtp { get; set; }

    public bool? UriAllowFtps { get; set; }

    public bool? UriAllowFile { get; set; }

    public bool? UriAllowOpcTcp { get; set; }

    /// <summary>
    /// Gets a value indicating whether any Atc annotation has been set.
    /// </summary>
    public bool HasAnyValue =>
        IsIgnoreDisplay ||
        EnumGuid is not null ||
        CasingStyleDefault is not null ||
        CasingStylePrefix is not null ||
        IsIPAddress ||
        IsIsoCurrencySymbol ||
        IsAtcString ||
        IsKeyString ||
        IsAtcUri;

    /// <summary>
    /// Builds the immutable AtcAnnotationInfo from accumulated values.
    /// </summary>
    public AtcAnnotationInfo Build() =>
        new(
            IsIgnoreDisplay: IsIgnoreDisplay,
            EnumGuid: EnumGuid,
            CasingStyleDefault: CasingStyleDefault,
            CasingStylePrefix: CasingStylePrefix,
            IsIPAddress: IsIPAddress,
            IPAddressRequired: IPAddressRequired,
            IsIsoCurrencySymbol: IsIsoCurrencySymbol,
            IsoCurrencySymbolRequired: IsoCurrencySymbolRequired,
            IsoCurrencySymbols: IsoCurrencySymbols,
            IsAtcString: IsAtcString,
            IsKeyString: IsKeyString,
            StringRequired: StringRequired,
            StringMinLength: StringMinLength,
            StringMaxLength: StringMaxLength,
            StringInvalidCharacters: StringInvalidCharacters,
            StringInvalidPrefixStrings: StringInvalidPrefixStrings,
            StringRegularExpression: StringRegularExpression,
            IsAtcUri: IsAtcUri,
            UriRequired: UriRequired,
            UriAllowHttp: UriAllowHttp,
            UriAllowHttps: UriAllowHttps,
            UriAllowFtp: UriAllowFtp,
            UriAllowFtps: UriAllowFtps,
            UriAllowFile: UriAllowFile,
            UriAllowOpcTcp: UriAllowOpcTcp);
}