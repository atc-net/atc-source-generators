// ReSharper disable InconsistentNaming
namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// Atc-specific annotation information extracted from Atc attributes.
/// </summary>
internal sealed record AtcAnnotationInfo(
    bool IsIgnoreDisplay, // IgnoreDisplayAttribute
    string? EnumGuid, // EnumGuidAttribute
    string? CasingStyleDefault, // CasingStyleDescriptionAttribute
    string? CasingStylePrefix,
    bool IsIPAddress, // IPAddressAttribute
    bool? IPAddressRequired,
    bool IsIsoCurrencySymbol, // IsoCurrencySymbolAttribute
    bool? IsoCurrencySymbolRequired,
    EquatableArray<string> IsoCurrencySymbols,
    bool IsAtcString, // StringAttribute (and KeyStringAttribute)
    bool IsKeyString,
    bool? StringRequired,
    uint? StringMinLength,
    uint? StringMaxLength,
    EquatableArray<char> StringInvalidCharacters,
    EquatableArray<string> StringInvalidPrefixStrings,
    string? StringRegularExpression,
    bool IsAtcUri, // UriAttribute
    bool? UriRequired,
    bool? UriAllowHttp,
    bool? UriAllowHttps,
    bool? UriAllowFtp,
    bool? UriAllowFtps,
    bool? UriAllowFile,
    bool? UriAllowOpcTcp);