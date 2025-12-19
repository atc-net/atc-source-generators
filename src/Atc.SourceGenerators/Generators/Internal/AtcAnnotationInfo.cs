// ReSharper disable InconsistentNaming
namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Atc-specific annotation information extracted from Atc attributes.
/// </summary>
internal sealed record AtcAnnotationInfo(

    // IgnoreDisplayAttribute
    bool IsIgnoreDisplay,

    // EnumGuidAttribute
    string? EnumGuid,

    // CasingStyleDescriptionAttribute
    string? CasingStyleDefault,
    string? CasingStylePrefix,

    // IPAddressAttribute
    bool IsIPAddress,
    bool? IPAddressRequired,

    // IsoCurrencySymbolAttribute
    bool IsIsoCurrencySymbol,
    bool? IsoCurrencySymbolRequired,
    string[]? IsoCurrencySymbols,

    // StringAttribute (and KeyStringAttribute)
    bool IsAtcString,
    bool IsKeyString,
    bool? StringRequired,
    uint? StringMinLength,
    uint? StringMaxLength,
    char[]? StringInvalidCharacters,
    string[]? StringInvalidPrefixStrings,
    string? StringRegularExpression,

    // UriAttribute
    bool IsAtcUri,
    bool? UriRequired,
    bool? UriAllowHttp,
    bool? UriAllowHttps,
    bool? UriAllowFtp,
    bool? UriAllowFtps,
    bool? UriAllowFile,
    bool? UriAllowOpcTcp);