#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable MA0048 // File name must match type name
namespace Atc.SourceGenerators.OptionsBinding.Options;

/// <summary>
/// Cloud storage configuration options.
/// Demonstrates Feature #6: Binding nested configuration subsections to complex properties.
/// The Microsoft.Extensions.Configuration.Binder automatically binds nested objects:
/// - Azure property binds to "CloudStorage:Azure" section
/// - Aws property binds to "CloudStorage:Aws" section
/// - RetryPolicy property binds to "CloudStorage:RetryPolicy" section
/// </summary>
[OptionsBinding("CloudStorage", ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class CloudStorageOptions
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Azure storage settings - automatically binds to "CloudStorage:Azure" section.
    /// </summary>
    public AzureStorageSettings Azure { get; set; } = new();

    /// <summary>
    /// AWS S3 settings - automatically binds to "CloudStorage:Aws" section.
    /// </summary>
    public AwsS3Settings Aws { get; set; } = new();

    /// <summary>
    /// Retry policy - automatically binds to "CloudStorage:RetryPolicy" section.
    /// </summary>
    public RetryPolicy RetryPolicy { get; set; } = new();
}

/// <summary>
/// Azure Blob Storage settings.
/// </summary>
public class AzureStorageSettings
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = string.Empty;

    /// <summary>
    /// Blob-specific settings - demonstrates deeply nested binding ("CloudStorage:Azure:Blob").
    /// </summary>
    public BlobSettings Blob { get; set; } = new();
}

/// <summary>
/// Azure Blob-specific settings.
/// </summary>
public class BlobSettings
{
    public int MaxBlockSize { get; set; } = 4194304; // 4 MB

    public int ParallelOperations { get; set; } = 8;
}

/// <summary>
/// AWS S3 storage settings.
/// </summary>
public class AwsS3Settings
{
    [Required]
    public string AccessKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";

    public string BucketName { get; set; } = string.Empty;
}

/// <summary>
/// Retry policy configuration.
/// </summary>
public class RetryPolicy
{
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(100, 60000)]
    public int DelayMilliseconds { get; set; } = 1000;

    public bool UseExponentialBackoff { get; set; } = true;
}
