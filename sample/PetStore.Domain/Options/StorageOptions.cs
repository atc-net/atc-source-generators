#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable MA0048 // File name must match type name
namespace PetStore.Domain.Options;

/// <summary>
/// Storage configuration options for the PetStore application.
/// Demonstrates Feature #6: Binding nested configuration subsections to complex properties.
/// The nested Database and FileStorage properties are automatically bound to their respective subsections.
/// </summary>
[OptionsBinding("Storage", ValidateDataAnnotations = true)]
public partial class StorageOptions
{
    /// <summary>
    /// Gets or sets the database configuration.
    /// Automatically binds to the "Storage:Database" configuration section.
    /// </summary>
    public DatabaseSettings Database { get; set; } = new();

    /// <summary>
    /// Gets or sets the file storage configuration.
    /// Automatically binds to the "Storage:FileStorage" configuration section.
    /// </summary>
    public FileStorageSettings FileStorage { get; set; } = new();
}

/// <summary>
/// Database storage settings.
/// </summary>
public class DatabaseSettings
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int MaxConnections { get; set; } = 100;

    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Retry policy for database operations.
    /// Demonstrates deeply nested binding ("Storage:Database:Retry").
    /// </summary>
    public DatabaseRetryPolicy Retry { get; set; } = new();
}

/// <summary>
/// Database retry policy settings.
/// </summary>
public class DatabaseRetryPolicy
{
    [Range(0, 10)]
    public int MaxAttempts { get; set; } = 3;

    [Range(100, 10000)]
    public int DelayMilliseconds { get; set; } = 500;
}

/// <summary>
/// File storage settings for pet images and documents.
/// </summary>
public class FileStorageSettings
{
    [Required]
    public string BasePath { get; set; } = string.Empty;

    [Range(1, 100)]
    public int MaxFileSizeMB { get; set; } = 10;

    public IList<string> AllowedExtensions { get; set; } = new List<string> { ".jpg", ".png", ".pdf" };
}
