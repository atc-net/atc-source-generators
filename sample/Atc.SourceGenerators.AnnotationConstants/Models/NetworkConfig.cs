namespace Atc.SourceGenerators.AnnotationConstants.Models;

/// <summary>
/// Example model demonstrating Atc validation attributes.
/// These attributes are from the Atc package and extend System.ComponentModel.DataAnnotations.
/// </summary>
public class NetworkConfig
{
    [Display(Name = "Server IP Address")]
    [IPAddress(Required = true)]
    public string ServerAddress { get; set; } = string.Empty;

    [Display(Name = "Backup Server IP")]
    [IPAddress]
    public string? BackupServerAddress { get; set; }

    [Display(Name = "API Endpoint")]
    [Uri(Required = true, AllowHttp = false, AllowHttps = true, AllowFtp = false, AllowFtps = false, AllowFile = false, AllowOpcTcp = false)]
    public string ApiEndpoint { get; set; } = string.Empty;

    [Display(Name = "FTP Server")]
    [Uri(AllowHttp = false, AllowHttps = false, AllowFtp = true, AllowFtps = true, AllowFile = false, AllowOpcTcp = false)]
    public string? FtpServer { get; set; }

    [Display(Name = "Config File Path")]
    [Uri(AllowHttp = false, AllowHttps = false, AllowFtp = false, AllowFtps = false, AllowFile = true, AllowOpcTcp = false)]
    public string? ConfigFilePath { get; set; }

    [Display(Name = "Currency Code")]
    [IsoCurrencySymbol(Required = true)]
    public string CurrencyCode { get; set; } = "USD";

    [Display(Name = "Allowed Currencies")]
    [IsoCurrencySymbol(IsoCurrencySymbols = new[] { "USD", "EUR", "GBP", "JPY" })]
    public string? AllowedCurrency { get; set; }

    [Display(Name = "Service Key")]
    [KeyString]
    public string ServiceKey { get; set; } = string.Empty;

    [Display(Name = "Custom Key")]
    [KeyString(true, 5, 50)]
    public string CustomKey { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [String(false, 0, 500)]
    public string? Description { get; set; }

    [Display(Name = "Identifier")]
    [String(true, 3, 20, new[] { ' ', '@', '#' }, new[] { "_", "-" })]
    public string Identifier { get; set; } = string.Empty;
}