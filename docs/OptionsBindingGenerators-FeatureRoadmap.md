# ⚙️ Feature Roadmap - Options Binding Generator

This document outlines the feature roadmap for the **OptionsBindingGenerator**, based on analysis of Microsoft.Extensions.Options patterns and real-world configuration challenges.

## 🔍 Research Sources

This roadmap is based on comprehensive analysis of:

1. **Microsoft.Extensions.Options** - [Official Documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
   - Options pattern guidance
   - Validation strategies (DataAnnotations, custom, IValidateOptions)
   - Lifetime management (IOptions, IOptionsSnapshot, IOptionsMonitor)
   - Named options support

2. **Configuration Binder Source Generator** - .NET 8+ AOT support
   - [Configuration Generator](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-generator)
   - Replace reflection-based binding with compile-time generation
   - AOT-compatible, trimming-safe

3. **GitHub Issues & Feature Requests**:
   - [Issue #36015](https://github.com/dotnet/runtime/issues/36015) - Better error handling for missing configuration
   - [Issue #83599](https://github.com/dotnet/runtime/issues/83599) - Extensibility for configuration binding
   - Silent failures and hard-to-diagnose misconfigurations

4. **Community Pain Points**:
   - Silent binding failures (missing keys → null properties)
   - Lifetime mismatches (IOptionsSnapshot in singletons)
   - Change detection limitations (only file-based providers)
   - Field binding doesn't work (only properties)

### 📊 Key Insights from Options Pattern

**What Developers Need**:

- **Fail-fast validation** - Catch configuration errors at startup, not runtime
- **Strong typing** - Avoid string-based configuration access
- **Change notifications** - Reload configuration without restarting app
- **Scoped configuration** - Different values per HTTP request
- **Named options** - Multiple configurations for same type
- **Custom validation** - Beyond DataAnnotations

**Common Mistakes**:

- Using `IOptionsSnapshot` in singleton services (lifetime mismatch)
- Expecting fields to bind (only properties work)
- Missing validation (silent failures in production)
- Not using `ValidateOnStart()` for critical configuration

---

## 📊 Current State

### ✅ OptionsBindingGenerator - Implemented Features

- **Section name resolution** - 5-level priority system (explicit → const SectionName → const NameTitle → const Name → auto-inferred)
- **Validation support** - `ValidateDataAnnotations` and `ValidateOnStart` parameters
- **Custom validation** - `IValidateOptions<T>` for complex business rules beyond DataAnnotations
- **Named options** - Multiple configurations of the same options type with different names
- **Error on missing keys** - `ErrorOnMissingKeys` fail-fast validation when configuration sections are missing
- **Configuration change callbacks** - `OnChange` callbacks for Monitor lifetime (auto-generates IHostedService)
- **Post-configuration support** - `PostConfigure` callbacks for normalizing/transforming values after binding (e.g., path normalization, URL lowercase)
- **ConfigureAll support** - Set common defaults for all named options instances before individual binding
- **Child sections** - Simplified syntax for multiple named instances from subsections (e.g., `Email` → Primary/Secondary/Fallback)
- **Nested subsection binding** - Automatic binding of complex properties to configuration subsections (e.g., `Storage:Database:Retry`)
- **Lifetime selection** - Singleton (`IOptions`), Scoped (`IOptionsSnapshot`), Monitor (`IOptionsMonitor`)
- **Multi-project support** - Assembly-specific extension methods with smart naming
- **Transitive registration** - 4 overloads for automatic/selective assembly registration
- **Partial class requirement** - Enforced at compile time
- **Native AOT compatible** - Zero reflection, compile-time generation
- **Compile-time diagnostics** - Validate partial class, section names, OnChange/PostConfigure/ConfigureAll callbacks, ChildSections usage (ATCOPT001-016)

---

## 📋 Feature Status Overview

| Status | Feature | Priority |
|:------:|---------|----------|
| ✅ | [Custom Validation Support (IValidateOptions)](#1-custom-validation-support-ivalidateoptions) | 🔴 High |
| ✅ | [Named Options Support](#2-named-options-support) | 🔴 High |
| ✅ | [Post-Configuration Support](#3-post-configuration-support) | 🟡 Medium-High |
| ✅ | [Error on Missing Configuration Keys](#4-error-on-missing-configuration-keys) | 🔴 High |
| ✅ | [Configuration Change Callbacks](#5-configuration-change-callbacks) | 🟡 Medium |
| ✅ | [Bind Configuration Subsections to Properties](#6-bind-configuration-subsections-to-properties) | 🟡 Medium |
| ✅ | [ConfigureAll Support](#7-configureall-support) | 🟢 Low-Medium |
| ✅ | [Child Sections (Simplified Named Options)](#8-child-sections-simplified-named-options) | 🟢 Low-Medium |
| ❌ | [Compile-Time Section Name Validation](#9-compile-time-section-name-validation) | 🟡 Medium |
| ✅ | [Early Access to Options During Service Registration](#10-early-access-to-options-during-service-registration) | 🔴 High |
| ❌ | [Auto-Generate Options Classes from appsettings.json](#11-auto-generate-options-classes-from-appsettingsjson) | 🟢 Low |
| ❌ | [Environment-Specific Validation](#12-environment-specific-validation) | 🟢 Low |
| ❌ | [Hot Reload Support with Filtering](#13-hot-reload-support-with-filtering) | 🟢 Low |
| 🚫 | [Reflection-Based Binding](#13-reflection-based-binding) | - |
| 🚫 | [JSON Schema Generation](#14-json-schema-generation) | - |
| 🚫 | [Configuration Encryption/Decryption](#15-configuration-encryptiondecryption) | - |
| 🚫 | [Dynamic Configuration Sources](#16-dynamic-configuration-sources) | - |

**Legend:**

- ✅ **Implemented** - Feature is complete and available
- ❌ **Not Implemented** - Feature is planned but not yet developed
- 🚫 **Not Planned** - Feature is out of scope or not aligned with project goals

---

## 🎯 Need to Have (High Priority)

These features address common pain points and align with Microsoft's Options pattern best practices.

### 1. Custom Validation Support (IValidateOptions)

**Priority**: 🔴 **High**
**Status**: ✅ **Implemented**
**Inspiration**: Microsoft.Extensions.Options.IValidateOptions<T>

**Description**: Support complex validation logic beyond DataAnnotations using `IValidateOptions<T>` interface.

**User Story**:
> "As a developer, I want to validate that `MaxConnections` is greater than `MinConnections` using custom business logic that DataAnnotations can't express."

**Example**:

```csharp
// Options class
[OptionsBinding("ConnectionPool", ValidateDataAnnotations = true)]
public partial class ConnectionPoolOptions
{
    [Range(1, 100)]
    public int MinConnections { get; set; } = 1;

    [Range(1, 1000)]
    public int MaxConnections { get; set; } = 10;

    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

// Custom validator
public class ConnectionPoolOptionsValidator : IValidateOptions<ConnectionPoolOptions>
{
    public ValidateOptionsResult Validate(string? name, ConnectionPoolOptions options)
    {
        if (options.MaxConnections <= options.MinConnections)
        {
            return ValidateOptionsResult.Fail(
                "MaxConnections must be greater than MinConnections");
        }

        if (options.ConnectionTimeout < TimeSpan.FromSeconds(1))
        {
            return ValidateOptionsResult.Fail(
                "ConnectionTimeout must be at least 1 second");
        }

        return ValidateOptionsResult.Success;
    }
}

// Generated code should register the validator:
services.AddOptions<ConnectionPoolOptions>()
    .Bind(configuration.GetSection("ConnectionPool"))
    .ValidateDataAnnotations()
    .Services.AddSingleton<IValidateOptions<ConnectionPoolOptions>, ConnectionPoolOptionsValidator>();
```

**Implementation Notes**:

- ✅ Added `Validator` property to `[OptionsBinding]` attribute
- ✅ Generator extracts validator type and registers it as singleton
- ✅ Generated code: `services.AddSingleton<IValidateOptions<TOptions>, TValidator>()`
- ✅ Works with DataAnnotations validation and ValidateOnStart
- ✅ Supports fully qualified type names
- ✅ Tested in sample projects (DatabaseOptions, PetStoreOptions)

---

### 2. Named Options Support

**Priority**: 🔴 **High**
**Status**: ✅ **Implemented**
**Inspiration**: Microsoft.Extensions.Options named options

**Description**: Support multiple configuration sections binding to the same options class with different names.

**User Story**:
> "As a developer building a multi-tenant application, I want to load different database configurations for each tenant using the same `DatabaseOptions` class."

**Example**:

```csharp
// appsettings.json
{
  "Databases": {
    "Primary": {
      "ConnectionString": "Server=primary;...",
      "MaxRetries": 5
    },
    "Reporting": {
      "ConnectionString": "Server=reporting;...",
      "MaxRetries": 3
    },
    "Archive": {
      "ConnectionString": "Server=archive;...",
      "MaxRetries": 10
    }
  }
}

// Options class with named instances
[OptionsBinding("Databases:Primary", Name = "Primary")]
[OptionsBinding("Databases:Reporting", Name = "Reporting")]
[OptionsBinding("Databases:Archive", Name = "Archive")]
public partial class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}

// Generated code:
services.Configure<DatabaseOptions>("Primary",
    configuration.GetSection("Databases:Primary"));
services.Configure<DatabaseOptions>("Reporting",
    configuration.GetSection("Databases:Reporting"));
services.Configure<DatabaseOptions>("Archive",
    configuration.GetSection("Databases:Archive"));

// Usage:
public class DataService
{
    public DataService(IOptionsSnapshot<DatabaseOptions> options)
    {
        var primaryDb = options.Get("Primary");
        var reportingDb = options.Get("Reporting");
        var archiveDb = options.Get("Archive");
    }
}
```

**Implementation Details**:

- ✅ `[OptionsBinding]` attribute supports `AllowMultiple = true`
- ✅ Added `Name` property to distinguish named instances
- ✅ Named options use `Configure<T>(name, section)` pattern
- ✅ Named options accessed via `IOptionsSnapshot<T>.Get(name)`
- ✅ Can mix named and unnamed options on the same class
- ⚠️ Named options do NOT support validation chain (ValidateDataAnnotations, ValidateOnStart, Validator)

**Testing**:

- ✅ 8 comprehensive unit tests covering all scenarios
- ✅ Sample project with EmailOptions demonstrating Primary/Secondary/Fallback servers
- ✅ PetStore.Api sample with NotificationOptions (Email/SMS/Push channels)

---

### 3. Post-Configuration Support

**Priority**: 🟡 **Medium-High**
**Status**: ✅ **Implemented**
**Inspiration**: `IPostConfigureOptions<T>` pattern

**Description**: Support post-configuration actions that run after binding and validation to apply defaults or transformations.

**User Story**:
> "As a developer, I want to apply default values or normalize configuration after binding (e.g., ensure paths end with slash, URLs are lowercase)."

**Example**:

```csharp
[OptionsBinding("Storage", PostConfigure = nameof(NormalizePaths))]
public partial class StorageOptions
{
    public string BasePath { get; set; } = string.Empty;
    public string CachePath { get; set; } = string.Empty;
    public string TempPath { get; set; } = string.Empty;

    // Post-configuration method
    private static void NormalizePaths(StorageOptions options)
    {
        // Ensure all paths end with directory separator
        options.BasePath = EnsureTrailingSlash(options.BasePath);
        options.CachePath = EnsureTrailingSlash(options.CachePath);
        options.TempPath = EnsureTrailingSlash(options.TempPath);
    }

    private static string EnsureTrailingSlash(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}

// Generated code:
services.AddOptions<StorageOptions>()
    .Bind(configuration.GetSection("Storage"))
    .PostConfigure(options => StorageOptions.NormalizePaths(options));
```

**Implementation Details**:

- ✅ Added `PostConfigure` property to `[OptionsBinding]` attribute
- ✅ Generator calls `.PostConfigure()` method on the options builder
- ✅ PostConfigure method must have signature: `static void MethodName(TOptions options)`
- ✅ Runs after binding and validation
- ✅ PostConfigure method can be `internal` or `public` (not `private`)
- ⚠️ Cannot be used with named options
- ✅ Comprehensive compile-time validation with 3 diagnostic codes (ATCOPT008-010)
- ✅ Useful for normalization, defaults, computed properties

**Diagnostics**:

- **ATCOPT008**: PostConfigure callback not supported with named options
- **ATCOPT009**: PostConfigure callback method not found
- **ATCOPT010**: PostConfigure callback method has invalid signature

**Testing**:

- ✅ Unit tests covering all scenarios and error cases
- ✅ Sample project: StoragePathsOptions demonstrates path normalization (trailing slash)
- ✅ PetStore.Api sample: ExternalApiOptions demonstrates URL normalization (lowercase + trailing slash removal)

---

### 4. Error on Missing Configuration Keys

**Priority**: 🔴 **High** ⭐ *Highly requested in GitHub issues*
**Status**: ✅ **Implemented**
**Inspiration**: [GitHub Issue #36015](https://github.com/dotnet/runtime/issues/36015)

**Description**: Throw exceptions when required configuration sections are missing instead of silently binding to null/default values.

**User Story**:
> "As a developer, I want my application to fail at startup if critical configuration like database connection strings is missing, rather than failing in production with NullReferenceException."

**Example**:

```csharp
[OptionsBinding("Database", ErrorOnMissingKeys = true, ValidateOnStart = true)]
public partial class DatabaseOptions
{
    [Required, MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    public int TimeoutSeconds { get; set; } = 30;
}

// Generated code with section existence check:
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .Validate(options =>
    {
        var section = configuration.GetSection("Database");
        if (!section.Exists())
        {
            throw new global::System.InvalidOperationException(
                "Configuration section 'Database' is missing. " +
                "Ensure the section exists in your appsettings.json or other configuration sources.");
        }

        return true;
    })
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

**Implementation Details**:

- ✅ Added `ErrorOnMissingKeys` boolean parameter to `[OptionsBinding]` attribute
- ✅ Generator checks `IConfigurationSection.Exists()` to detect missing sections
- ✅ Throws `InvalidOperationException` with descriptive message including section name
- ✅ Combines with `ValidateOnStart = true` for startup detection (recommended)
- ✅ Works with all validation options (DataAnnotations, custom validators)
- ✅ Section name included in error message for easy troubleshooting
- ⚠️ Named options do NOT support ErrorOnMissingKeys (named options use simpler Configure pattern)

**Testing**:

- ✅ 11 comprehensive unit tests covering all scenarios
- ✅ Sample project updated: DatabaseOptions demonstrates ErrorOnMissingKeys
- ✅ PetStore.Api sample: PetStoreOptions uses ErrorOnMissingKeys for critical configuration

**Best Practices**:

- Always combine with `ValidateOnStart = true` to catch missing configuration at startup
- Use for production-critical configuration (databases, external services, API keys)
- Avoid for optional configuration with reasonable defaults

---

### 5. Configuration Change Callbacks

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented**
**Inspiration**: `IOptionsMonitor<T>.OnChange()` pattern

**Description**: Support registering callbacks that execute when configuration changes are detected.

**User Story**:
> "As a developer, I want to be notified when feature flags change so I can reload caches or update runtime behavior without restarting the application."

**Example**:

```csharp
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
public partial class FeaturesOptions
{
    public bool EnableNewUI { get; set; }
    public bool EnableBetaFeatures { get; set; }
    public int MaxUploadSizeMB { get; set; } = 10;

    // Change callback - signature: static void OnChange(TOptions options, string? name)
    internal static void OnFeaturesChanged(FeaturesOptions options, string? name)
    {
        Console.WriteLine($"Features configuration changed: EnableNewUI={options.EnableNewUI}");
        // Clear caches, notify components, etc.
    }
}

// Generated code:
services.AddOptions<FeaturesOptions>()
    .Bind(configuration.GetSection("Features"));

services.AddHostedService<FeaturesOptionsChangeListener>();

// Generated hosted service
internal sealed class FeaturesOptionsChangeListener : IHostedService
{
    private readonly IOptionsMonitor<FeaturesOptions> _monitor;
    private IDisposable? _changeToken;

    public FeaturesOptionsChangeListener(IOptionsMonitor<FeaturesOptions> monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _changeToken = _monitor.OnChange((options, name) =>
            FeaturesOptions.OnFeaturesChanged(options, name));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _changeToken?.Dispose();
        return Task.CompletedTask;
    }
}
```

**Implementation Details**:

- ✅ Added `OnChange` property to `[OptionsBinding]` attribute
- ✅ Generator creates `IHostedService` that registers the callback via `IOptionsMonitor<T>.OnChange()`
- ✅ Hosted service is automatically registered when application starts
- ✅ Callback signature: `static void MethodName(TOptions options, string? name)`
- ✅ Callback method can be `internal` or `public` (not `private`)
- ✅ Properly disposes change token in `StopAsync` to prevent memory leaks
- ✅ Only applicable when `Lifetime = OptionsLifetime.Monitor`
- ⚠️ Cannot be used with named options
- ✅ Comprehensive compile-time validation with 4 diagnostic codes (ATCOPT004-007)
- **Limitation**: Only works with file-based configuration providers (appsettings.json with reloadOnChange: true)

**Diagnostics**:

- **ATCOPT004**: OnChange callback requires Monitor lifetime
- **ATCOPT005**: OnChange callback not supported with named options
- **ATCOPT006**: OnChange callback method not found
- **ATCOPT007**: OnChange callback method has invalid signature

**Testing**:

- ✅ 20 comprehensive unit tests covering all scenarios and error cases
- ✅ Sample project updated: LoggingOptions demonstrates OnChange callbacks
- ✅ PetStore.Api sample: FeaturesOptions uses OnChange for feature flag changes

---

### 6. Bind Configuration Subsections to Properties

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented**
**Inspiration**: Microsoft.Extensions.Configuration.Binder automatic subsection binding

**Description**: Microsoft's `.Bind()` method automatically handles nested configuration subsections. Complex properties are automatically bound to their corresponding configuration subsections without any additional configuration.

**User Story**:
> "As a developer, I want to bind nested configuration sections to nested properties without manually creating separate options classes or writing additional binding code."

**Example**:

```csharp
// appsettings.json
{
  "Email": {
    "From": "noreply@example.com",
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UseSsl": true
    },
    "Templates": {
      "Welcome": "welcome.html",
      "ResetPassword": "reset.html"
    }
  }
}

// Options class - nested objects automatically bind!
[OptionsBinding("Email")]
public partial class EmailOptions
{
    public string From { get; set; } = string.Empty;

    // Automatically binds to "Email:Smtp" subsection - no special config needed!
    public SmtpSettings Smtp { get; set; } = new();

    // Automatically binds to "Email:Templates" subsection
    public EmailTemplates Templates { get; set; } = new();
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
}

public class EmailTemplates
{
    public string Welcome { get; set; } = string.Empty;
    public string ResetPassword { get; set; } = string.Empty;
}
```

**Real-World Example - Deeply Nested (3 Levels)**:

```csharp
[OptionsBinding("Storage", ValidateDataAnnotations = true)]
public partial class StorageOptions
{
    // Binds to "Storage:Database"
    public DatabaseSettings Database { get; set; } = new();

    // Binds to "Storage:FileStorage"
    public FileStorageSettings FileStorage { get; set; } = new();
}

public class DatabaseSettings
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int MaxConnections { get; set; } = 100;

    // Binds to "Storage:Database:Retry" - 3 levels deep!
    public DatabaseRetryPolicy Retry { get; set; } = new();
}

public class DatabaseRetryPolicy
{
    [Range(0, 10)]
    public int MaxAttempts { get; set; } = 3;

    [Range(100, 10000)]
    public int DelayMilliseconds { get; set; } = 500;
}
```

**Implementation Details**:

- ✅ **Zero configuration required** - Just use complex property types and `.Bind()` handles the rest
- ✅ **Already supported by Microsoft.Extensions.Configuration.Binder** - Our generator leverages this natively
- ✅ **Automatic path construction** - "Parent:Child:GrandChild" paths are built automatically
- ✅ **Works with validation** - DataAnnotations validation applies to all nested levels
- ✅ **Unlimited depth** - Supports deeply nested structures (e.g., CloudStorage → Azure → Blob)
- ✅ **Collections supported** - List<T>, arrays, dictionaries all work automatically
- ✅ **No breaking changes** - This feature works out-of-the-box with existing code

**What Gets Automatically Bound**:

- **Nested objects** - Properties with complex class types
- **Collections** - List<T>, IEnumerable<T>, arrays
- **Dictionaries** - Dictionary<string, string>, Dictionary<string, T>
- **Multiple levels** - As deeply nested as needed

**Testing**:

- ✅ 9 comprehensive unit tests covering all scenarios
- ✅ Sample project: CloudStorageOptions demonstrates Azure/AWS/Blob nested structure
- ✅ PetStore.Api sample: StorageOptions demonstrates Database/FileStorage/Retry 3-level nesting

**Key Benefits**:

- **Cleaner configuration models** - Group related settings without flat structures
- **Better organization** - Mirrors natural hierarchy of configuration
- **Type-safe all the way down** - Compile-time safety for nested properties
- **Works with existing features** - Validation, change detection, all lifetimes

---

### 7. ConfigureAll Support

**Priority**: 🟢 **Low-Medium**
**Status**: ✅ **Implemented**

**Description**: Support configuring all named instances of an options type at once, allowing you to set common defaults that apply to all named configurations before individual settings override them.

**Example**:

```csharp
[OptionsBinding("Email:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
[OptionsBinding("Email:Secondary", Name = "Secondary")]
[OptionsBinding("Email:Fallback", Name = "Fallback")]
public partial class EmailOptions
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public int MaxRetries { get; set; }
    public int TimeoutSeconds { get; set; } = 30;

    internal static void SetDefaults(EmailOptions options)
    {
        // Set common defaults for ALL email configurations
        options.MaxRetries = 3;
        options.TimeoutSeconds = 30;
        options.Port = 587;
    }
}
```

**Generated Code**:

```csharp
// Configure defaults for ALL named instances FIRST
services.ConfigureAll<EmailOptions>(options => EmailOptions.SetDefaults(options));

// Then configure individual instances (can override defaults)
services.Configure<EmailOptions>("Primary", config.GetSection("Email:Primary"));
services.Configure<EmailOptions>("Secondary", config.GetSection("Email:Secondary"));
services.Configure<EmailOptions>("Fallback", config.GetSection("Email:Fallback"));
```

**Implementation Details**:

- ✅ **Requires multiple named instances** - Cannot be used with single unnamed instance (compile-time error)
- ✅ **Method signature validation** - Must be `static void MethodName(TOptions options)`
- ✅ **Execution order** - ConfigureAll runs BEFORE individual Configure calls
- ✅ **Flexible placement** - Can be specified on any one of the `[OptionsBinding]` attributes
- ✅ **Override support** - Individual configurations can override defaults set by ConfigureAll
- ✅ **Compile-time safety** - Diagnostics ATCOPT011-013 validate usage and method signature

**Use Cases**:

- **Baseline settings**: Set common retry, timeout, or connection defaults across all database connections
- **Feature flags**: Enable/disable common features for all tenant configurations
- **Security defaults**: Apply consistent security settings across all API client configurations
- **Notification channels**: Set common rate limits and retry policies for all notification providers

**Testing**:

- ✅ 14 comprehensive unit tests covering all scenarios
- ✅ Sample project: EmailOptions demonstrates default retry/timeout settings
- ✅ PetStore.Api sample: NotificationOptions demonstrates common defaults for Email/SMS/Push channels

---

## 💡 Nice to Have (Medium Priority)

These features would improve usability but are not critical.

### 8. Child Sections (Simplified Named Options)

**Priority**: 🟢 **Low-Medium**
**Status**: ✅ **Implemented**
**Inspiration**: Community feedback on reducing boilerplate for multiple named instances

**Description**: Provide a simplified syntax for creating multiple named options instances from configuration subsections. Instead of writing multiple `[OptionsBinding]` attributes for each named instance, developers can use a single `ChildSections` property.

**User Story**:
> "As a developer, I want to configure multiple related named options (Primary/Secondary/Fallback servers, Email/SMS/Push channels) without repeating multiple attribute declarations."

**Example**:

**Before (Multiple Attributes):**

```csharp
[OptionsBinding("Email:Primary", Name = "Primary", ConfigureAll = nameof(SetDefaults))]
[OptionsBinding("Email:Secondary", Name = "Secondary")]
[OptionsBinding("Email:Fallback", Name = "Fallback")]
public partial class EmailOptions
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;

    internal static void SetDefaults(EmailOptions options)
    {
        options.Port = 587;
        options.UseSsl = true;
        options.MaxRetries = 3;
    }
}
```

**After (With ChildSections):**

```csharp
[OptionsBinding("Email", ChildSections = new[] { "Primary", "Secondary", "Fallback" }, ConfigureAll = nameof(SetDefaults))]
public partial class EmailOptions
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;

    internal static void SetDefaults(EmailOptions options)
    {
        options.Port = 587;
        options.UseSsl = true;
        options.MaxRetries = 3;
    }
}
```

**Generated Code (Identical for Both):**

```csharp
// Configure defaults for ALL instances FIRST
services.ConfigureAll<EmailOptions>(options => EmailOptions.SetDefaults(options));

// Configure individual named instances
services.Configure<EmailOptions>("Primary", configuration.GetSection("Email:Primary"));
services.Configure<EmailOptions>("Secondary", configuration.GetSection("Email:Secondary"));
services.Configure<EmailOptions>("Fallback", configuration.GetSection("Email:Fallback"));
```

**Real-World Example (PetStore Sample):**

```csharp
/// <summary>
/// Notification channel options with support for multiple named configurations.
/// Demonstrates ChildSections + ConfigureAll for common defaults.
/// </summary>
[OptionsBinding("Notifications", ChildSections = new[] { "Email", "SMS", "Push" }, ConfigureAll = nameof(SetCommonDefaults))]
public partial class NotificationOptions
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RateLimitPerMinute { get; set; }

    internal static void SetCommonDefaults(NotificationOptions options)
    {
        options.TimeoutSeconds = 30;
        options.MaxRetries = 3;
        options.RateLimitPerMinute = 60;
        options.Enabled = true;
    }
}
```

**Configuration (appsettings.json):**

```json
{
  "Notifications": {
    "Email": {
      "Enabled": true,
      "Provider": "SendGrid",
      "ApiKey": "your-api-key",
      "SenderId": "noreply@example.com",
      "TimeoutSeconds": 30,
      "MaxRetries": 3
    },
    "SMS": {
      "Enabled": false,
      "Provider": "Twilio",
      "ApiKey": "your-api-key",
      "SenderId": "+1234567890",
      "TimeoutSeconds": 15,
      "MaxRetries": 2
    },
    "Push": {
      "Enabled": true,
      "Provider": "Firebase",
      "ApiKey": "your-server-key",
      "SenderId": "app-id",
      "TimeoutSeconds": 20,
      "MaxRetries": 3
    }
  }
}
```

**Implementation Details**:

- ✅ Added `ChildSections` string array property to `[OptionsBinding]` attribute
- ✅ Generator expands ChildSections into multiple OptionsInfo instances at extraction time
- ✅ Each child section becomes a named instance: `Parent:Child` section path with `Child` as the name
- ✅ **Works with all named options features**: ConfigureAll, validation, ErrorOnMissingKeys, custom validators
- ✅ **Validation support**: Named options with ChildSections can use fluent API for validation
- ✅ **Mutual exclusivity**: Cannot be combined with `Name` property (compile-time error ATCOPT014)
- ✅ **Minimum 2 items**: Requires at least 2 child sections (compile-time error ATCOPT015)
- ✅ **No null/empty items**: All child section names must be non-empty (compile-time error ATCOPT016)
- ✅ **Nested paths supported**: Works with paths like `"App:Services:Cache"` → `"App:Services:Cache:Redis"`

**Diagnostics**:

- **ATCOPT014**: ChildSections cannot be used with Name property
- **ATCOPT015**: ChildSections requires at least 2 items (found X item(s))
- **ATCOPT016**: ChildSections array contains null or empty value at index X

**Testing**:

- ✅ 13 comprehensive unit tests covering all scenarios and error cases
- ✅ Sample project: EmailOptions demonstrates Primary/Secondary/Fallback with ConfigureAll
- ✅ PetStore.Api sample: NotificationOptions demonstrates Email/SMS/Push channels
- ✅ All existing tests pass (275 succeeded, 0 failed, 33 skipped)

**Key Benefits**:

1. **Less Boilerplate**: One attribute instead of 3+ separate declarations
2. **Clearer Intent**: Explicitly shows configurations are grouped under common parent
3. **Easier Maintenance**: Add/remove sections by updating the array
4. **Feature Complete**: Supports all named options capabilities (validation, ConfigureAll, validators)
5. **Same Power**: Generates identical code to multiple attributes approach

**Use Cases**:

- **Notification Channels**: Email, SMS, Push configurations (as shown in PetStore sample)
- **Database Fallback**: Primary, Secondary, Tertiary connections
- **Multi-Region APIs**: USEast, USWest, EUWest, APSouth endpoints
- **Cache Tiers**: L1, L2, L3 cache configurations
- **Multi-Tenant**: Tenant1, Tenant2, Tenant3 configurations

**Design Decision**:

- **Expansion at extraction time**: ChildSections array is expanded into multiple OptionsInfo instances during attribute extraction, not at code generation. This simplifies generator logic and ensures all features work consistently.
- **No OnChange support**: Like regular named options, ChildSections-based instances don't support OnChange callbacks (use `IOptionsMonitor<T>.OnChange()` manually if needed).

---

### 9. Compile-Time Section Name Validation

**Priority**: 🟡 **Medium**
**Status**: ❌ Not Implemented

**Description**: Validate at compile time that specified configuration section paths exist in appsettings.json.

**Challenge**: Requires analyzing appsettings.json files during compilation, which is complex.

**Potential approach**:

- Use MSBuild task to read appsettings.json
- Generate diagnostics if section path doesn't exist
- May have false positives (environment-specific configs)

---

### 10. Early Access to Options During Service Registration

**Priority**: 🔴 **High** ⭐ *Avoids BuildServiceProvider anti-pattern*
**Status**: ✅ **Implemented**
**Inspiration**: [StackOverflow: Avoid BuildServiceProvider](https://stackoverflow.com/questions/66263977/how-to-avoid-using-using-buildserviceprovider-method-at-multiple-places)

> **📝 Implementation Note:** This feature is fully implemented with three APIs:
> 1. `Get[Type]From[Assembly]()` - Reads cache, doesn't populate (efficient, no side effects)
> 2. `GetOrAdd[Type]From[Assembly]()` - Reads AND populates cache (idempotent)
> 3. `GetOptions<T>()` - Smart dispatcher for multi-assembly projects (calls Get internally)
>
> See [OptionsBindingGenerators.md](OptionsBindingGenerators.md#-early-access-to-options-avoid-buildserviceprovider-anti-pattern) for current usage.

**Description**: Enable access to bound and validated options instances **during** service registration without calling `BuildServiceProvider()`, which is a known anti-pattern that causes memory leaks, scope issues, and application instability.

**User Story**:
> "As a developer, I need to access configuration values (like connection strings, API keys, or feature flags) during service registration to conditionally register services or configure dependencies, WITHOUT calling `BuildServiceProvider()` multiple times."

**The Anti-Pattern Problem**:

```csharp
// ❌ ANTI-PATTERN - Multiple BuildServiceProvider calls
var services = new ServiceCollection();
services.AddOptionsFromApp(configuration);

// Need database connection string to configure DbContext
var tempProvider = services.BuildServiceProvider(); // ⚠️ First build
var dbOptions = tempProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(dbOptions.ConnectionString));

// Need API key for HttpClient
var apiOptions = tempProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
services.AddHttpClient("External", client =>
    client.DefaultRequestHeaders.Add("X-API-Key", apiOptions.ApiKey));

// Finally build the real provider
var provider = services.BuildServiceProvider(); // ⚠️ Second build
var app = host.Build();
```

**Why This is Bad**:

1. **Memory Leaks**: First `ServiceProvider` is never disposed properly
2. **Incomplete Services**: Services registered after first build aren't in first provider
3. **Scope/Lifetime Issues**: Transient services may be captured as singletons
4. **Production Failures**: Subtle bugs that only manifest under load
5. **Microsoft's Warning**: Official docs explicitly warn against this pattern

**Proposed Solution - Individual Accessor Methods**:

Generate per-options extension methods that return bound instances for early access:

```csharp
// ✅ SOLUTION - Early access WITHOUT BuildServiceProvider
var services = new ServiceCollection();

// Get options instances during registration (no BuildServiceProvider!)
var dbOptions = services.GetOrAddDatabaseOptions(configuration);
var apiOptions = services.GetOrAddApiOptions(configuration);

// Use options immediately for service configuration
services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(dbOptions.ConnectionString));

services.AddHttpClient("External", client =>
    client.DefaultRequestHeaders.Add("X-API-Key", apiOptions.ApiKey));

// Still call the bulk method for completeness (idempotent - won't duplicate)
services.AddOptionsFromApp(configuration);

// Build provider ONCE at the end
var provider = services.BuildServiceProvider(); // ✅ Only one build!
```

**Generated Code Pattern**:

For each options class, generate an individual accessor method:

```csharp
/// <summary>
/// Gets or adds DatabaseOptions to the service collection with configuration binding.
/// If already registered, returns the existing instance. Otherwise, creates, binds, validates, and registers the instance.
/// This method is idempotent and safe to call multiple times.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The configuration.</param>
/// <returns>The bound and validated DatabaseOptions instance for immediate use during service registration.</returns>
public static DatabaseOptions GetOrAddDatabaseOptions(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Check if already registered (idempotent)
    var existingDescriptor = services.FirstOrDefault(d =>
        d.ServiceType == typeof(DatabaseOptions) &&
        d.ImplementationInstance != null);

    if (existingDescriptor != null)
    {
        return (DatabaseOptions)existingDescriptor.ImplementationInstance!;
    }

    // Create and bind instance
    var options = new DatabaseOptions();
    var section = configuration.GetSection("DatabaseOptions");
    section.Bind(options);

    // Validate immediately (DataAnnotations)
    var validationContext = new ValidationContext(options);
    Validator.ValidateObject(options, validationContext, validateAllProperties: true);

    // Register instance directly (singleton)
    services.AddSingleton(options);

    // Register IOptions wrapper
    services.AddSingleton<IOptions<DatabaseOptions>>(
        new OptionsWrapper<DatabaseOptions>(options));

    // Register for IOptionsSnapshot/IOptionsMonitor (reuses same instance)
    services.AddSingleton<IOptionsSnapshot<DatabaseOptions>>(
        sp => new OptionsWrapper<DatabaseOptions>(options) as IOptionsSnapshot<DatabaseOptions>);

    services.AddSingleton<IOptionsMonitor<DatabaseOptions>>(
        sp => new OptionsMonitorWrapper<DatabaseOptions>(options));

    return options;
}
```

**Transitive Registration Support**:

Just like the existing `AddOptionsFromApp()` supports transitive registration, early access must work across assemblies:

```csharp
// Current transitive registration (4 overloads):
services.AddOptionsFromApp(configuration);                                    // Default
services.AddOptionsFromApp(configuration, includeReferencedAssemblies: true); // Auto-detect all
services.AddOptionsFromApp(configuration, "DataAccess");                      // Specific assembly
services.AddOptionsFromApp(configuration, "DataAccess", "Infrastructure");    // Multiple assemblies

// Early access must support the same pattern:
// Approach 1: Individual accessors from specific assemblies
var dbOptions = services.GetOrAddDatabaseOptionsFromDataAccess(configuration);  // From DataAccess assembly
var apiOptions = services.GetOrAddApiOptionsFromApp(configuration);            // From App assembly

// Approach 2: Generic accessor (works after AddOptionsFrom* called)
services.AddOptionsFromApp(configuration, includeReferencedAssemblies: true);
var dbOptions = services.GetOptionInstanceOf<DatabaseOptions>();  // From any registered assembly
var apiOptions = services.GetOptionInstanceOf<ApiOptions>();
```

**Alternative API - Extension Method Returning from Internal Registry**:

```csharp
// User's suggestion: Retrieve from internal cache after AddOptionsFromApp
services.AddOptionsFromApp(configuration);

// Get instance from internal registry (no BuildServiceProvider)
var dbOptions = services.GetOptionInstanceOf<DatabaseOptions>();
var apiOptions = services.GetOptionInstanceOf<ApiOptions>();

// Works with transitive registration too:
services.AddOptionsFromApp(configuration, includeReferencedAssemblies: true);
var domainOptions = services.GetOptionInstanceOf<PetStoreOptions>();      // From Domain assembly
var dataAccessOptions = services.GetOptionInstanceOf<DatabaseOptions>();  // From DataAccess assembly
```

**Generated Internal Registry** (Shared Across Assemblies):

```csharp
// Generated ONCE in the Atc.OptionsBinding namespace (shared across all assemblies)
namespace Atc.OptionsBinding
{
    using System.Collections.Concurrent;

    internal static class OptionsInstanceCache
    {
        private static readonly ConcurrentDictionary<Type, object> instances = new();

        internal static void Add<T>(T instance) where T : class
            => instances[typeof(T)] = instance;

        internal static T? TryGet<T>() where T : class
            => instances.TryGetValue(typeof(T), out var instance)
                ? (T)instance
                : null;
    }
}

// Design Decision: Use shared static class with ConcurrentDictionary
// ✅ Works across referenced assemblies (DataAccess, Domain, Infrastructure)
// ✅ GetOptionInstanceOf<T>() can retrieve options from ANY registered assembly
// ✅ Single source of truth - no duplicate instances
// ✅ Thread-safe via ConcurrentDictionary (lock-free reads, better performance)
// ✅ Cleaner code - no manual locking required

// Generated extension method
public static T GetOptionInstanceOf<T>(this IServiceCollection services)
    where T : class
{
    var instance = AppOptionsInstanceCache.TryGet<T>();
    if (instance == null)
    {
        throw new InvalidOperationException(
            $"Options instance of type '{typeof(T).Name}' not found. " +
            $"Ensure AddOptionsFromApp() was called before GetOptionInstanceOf<T>().");
    }

    return instance;
}

// Modified AddOptionsFromApp to populate shared cache (supports transitive registration)
public static IServiceCollection AddOptionsFromApp(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Create, bind, and validate DatabaseOptions
    var dbOptions = new DatabaseOptions();
    configuration.GetSection("DatabaseOptions").Bind(dbOptions);

    var validationContext = new ValidationContext(dbOptions);
    Validator.ValidateObject(dbOptions, validationContext, validateAllProperties: true);

    // Add to SHARED cache for early access (accessible via GetOptionInstanceOf)
    OptionsInstanceCache.Add(dbOptions);

    // Register in DI
    services.AddSingleton(dbOptions);
    services.AddSingleton<IOptions<DatabaseOptions>>(
        new OptionsWrapper<DatabaseOptions>(dbOptions));

    // Repeat for all options classes in this assembly...

    return services;
}

// Transitive registration overloads also populate the shared cache:
public static IServiceCollection AddOptionsFromApp(
    this IServiceCollection services,
    IConfiguration configuration,
    bool includeReferencedAssemblies)
{
    // Register current assembly options first
    AddOptionsFromApp(services, configuration);

    if (includeReferencedAssemblies)
    {
        // Call generated methods from referenced assemblies
        // Each one populates the shared OptionsInstanceCache
        services.AddOptionsFromDomain(configuration);      // Domain options added to cache
        services.AddOptionsFromDataAccess(configuration);  // DataAccess options added to cache
        // ... auto-detect and call all referenced assembly methods
    }

    return services;
}
```

**Implementation Recommendations**:

**Approach 1: Individual Accessor Methods (Recommended)**

✅ **Pros**:

- No global state
- Idempotent (safe to call multiple times)
- Works before or after `AddOptionsFromApp`
- Granular control (only get what you need)
- Thread-safe via IServiceCollection
- Clear intent and discoverability

❌ **Cons**:

- Generates more code (one method per options class)
- Two ways to register (could be confusing initially)

**Approach 2: Internal Registry + GetOptionInstanceOf (User's Suggestion)**

✅ **Pros**:

- Clean API matching user's request
- Works well with existing `AddOptionsFromApp`
- Single method for all options types

❌ **Cons**:

- Global static state (testing concerns)
- Order dependency (must call `AddOptionsFromApp` first)
- Less discoverable (generic method)

**Approach 3: Hybrid (Best of Both)**

Generate BOTH approaches:

```csharp
// Approach 1: Individual accessors (primary)
var dbOptions = services.GetOrAddDatabaseOptions(configuration);

// Approach 2: Generic accessor (convenience, requires AddOptionsFromApp called first)
services.AddOptionsFromApp(configuration);
var apiOptions = services.GetOptionInstanceOf<ApiOptions>();
```

**Implementation Details**:

**Key Considerations**:

1. **Validation Strategy**:
   - Eager validation during `GetOrAdd*` (throws immediately on invalid config)
   - Validates ONCE when instance is created
   - No startup validation for early-access instances (would require ServiceProvider)

2. **Lifetime Compatibility**:
   - Early-access instances are ALWAYS registered as Singletons
   - `IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>` all resolve to same instance
   - Scoped lifetime not supported for early-access (singleton registration only)

3. **Named Options**:
   - Named options NOT supported for early access (require specific names)
   - Only unnamed/default options can use `GetOrAdd*` or `GetOptionInstanceOf<T>`

4. **OnChange Callbacks**:
   - Early-access instances do NOT support `OnChange` callbacks
   - Use `IOptionsMonitor<T>.OnChange()` manually if needed after provider built

5. **ErrorOnMissingKeys**:
   - Fully supported - throws during `GetOrAdd*` if section missing
   - Combines with DataAnnotations validation

6. **Idempotency**:
   - Multiple calls to `GetOrAddDatabaseOptions` return same instance
   - Multiple calls to `AddOptionsFromApp` won't duplicate registrations
   - Safe to mix and match approaches

7. **Multi-Assembly Support**:
   - Individual `GetOrAdd*` methods are generated per-assembly with smart naming
   - `GetOrAddDatabaseOptionsFromDataAccess()` - from DataAccess assembly
   - `GetOrAddDatabaseOptionsFromApp()` - from App assembly (if App also has DatabaseOptions)
   - `GetOptionInstanceOf<T>()` works across all registered assemblies
   - Internal registry is shared across all assemblies (static class in generated namespace)

**Diagnostics**:

Potential new diagnostic codes:

- **ATCOPT017**: Early access not supported with named options (Error)
- **ATCOPT018**: Early access requires Singleton lifetime (Warning)
- **ATCOPT019**: OnChange callbacks not supported with early access (Warning)

**Real-World Use Cases**:

**Use Case 1: Conditional Service Registration (Feature Flags)**

```csharp
var features = services.GetOrAddFeaturesOptions(configuration);

if (features.EnableRedisCache)
{
    var redis = services.GetOrAddRedisOptions(configuration);
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redis.ConnectionString;
        options.InstanceName = redis.InstanceName;
    });
}
else
{
    services.AddDistributedMemoryCache();
}
```

**Use Case 2: DbContext Configuration**

```csharp
var dbOptions = services.GetOrAddDatabaseOptions(configuration);

services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(dbOptions.ConnectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: dbOptions.MaxRetries,
                maxRetryDelay: TimeSpan.FromSeconds(dbOptions.RetryDelaySeconds),
                errorNumbersToAdd: null);
        });
});
```

**Use Case 3: HttpClient Configuration**

```csharp
var apiOptions = services.GetOrAddExternalApiOptions(configuration);

services.AddHttpClient("ExternalAPI", client =>
{
    client.BaseAddress = new Uri(apiOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("X-API-Key", apiOptions.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(apiOptions.TimeoutSeconds);
})
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(apiOptions.TimeoutSeconds)));
```

**Use Case 4: Multi-Tenant Routing**

```csharp
var tenants = services.GetOrAddTenantOptions(configuration);

foreach (var tenant in tenants.EnabledTenants)
{
    services.AddScoped<ITenantContext>(sp =>
        new TenantContext(tenant.TenantId, tenant.DatabaseName));
}
```

**Testing Strategy**:

```csharp
[Fact]
public void GetOrAddDatabaseOptions_Should_Return_Same_Instance_When_Called_Multiple_Times()
{
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["DatabaseOptions:ConnectionString"] = "Server=test;Database=test"
        })
        .Build();

    // Act
    var instance1 = services.GetOrAddDatabaseOptions(configuration);
    var instance2 = services.GetOrAddDatabaseOptions(configuration);

    // Assert
    Assert.Same(instance1, instance2); // Idempotent - same instance
    Assert.Single(services.Where(d => d.ServiceType == typeof(DatabaseOptions))); // Only one registration
}

[Fact]
public void GetOrAddDatabaseOptions_Should_Throw_When_Validation_Fails()
{
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["DatabaseOptions:ConnectionString"] = "" // Empty - violates [Required]
        })
        .Build();

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() =>
        services.GetOrAddDatabaseOptions(configuration));

    Assert.Contains("ConnectionString", exception.Message);
}

[Fact]
public void GetOptionInstanceOf_Should_Throw_When_AddOptionsFromApp_Not_Called()
{
    // Arrange
    var services = new ServiceCollection();

    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() =>
        services.GetOptionInstanceOf<DatabaseOptions>());

    Assert.Contains("AddOptionsFromApp", exception.Message);
}
```

**Sample Project - Program.cs** (Multi-Project Scenario):

```csharp
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// ✅ Early access pattern - get options during registration from multiple assemblies
// Approach 1: Individual accessors (type-safe, assembly-specific)
var dbOptions = services.GetOrAddDatabaseOptionsFromDataAccess(configuration);  // From DataAccess assembly
var features = services.GetOrAddFeaturesOptionsFromDomain(configuration);       // From Domain assembly
var apiOptions = services.GetOrAddExternalApiOptionsFromApp(configuration);     // From App assembly

// Use options immediately for conditional registration
if (features.EnableDatabase)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(dbOptions.ConnectionString,
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: dbOptions.MaxRetries,
                maxRetryDelay: TimeSpan.FromSeconds(dbOptions.RetryDelaySeconds),
                errorNumbersToAdd: null)));
}

if (features.EnableRedisCache)
{
    var cacheOptions = services.GetOrAddCacheOptionsFromInfrastructure(configuration);
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheOptions.ConnectionString;
        options.InstanceName = cacheOptions.InstanceName;
    });
}
else
{
    services.AddDistributedMemoryCache();
}

// Configure HTTP clients
services.AddHttpClient("ExternalAPI", client =>
{
    client.BaseAddress = new Uri(apiOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("X-API-Key", apiOptions.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(apiOptions.TimeoutSeconds);
});

// Register remaining options normally with transitive registration (idempotent - won't duplicate)
// This also makes options available via IOptions<T> injection
services.AddOptionsFromApp(configuration, includeReferencedAssemblies: true);

// Alternative Approach 2: Generic accessor (after AddOptionsFrom* called)
// var petStoreOptions = services.GetOptionInstanceOf<PetStoreOptions>();  // From any registered assembly

var app = builder.Build();
app.Run();
```

**Sample Project - 3-Layer Architecture**:

```
PetStore.Api (Program.cs)
├── GetOrAddExternalApiOptionsFromApp()      → Use immediately for HttpClient config
├── GetOrAddFeaturesOptionsFromDomain()      → Use for conditional service registration
└── GetOrAddDatabaseOptionsFromDataAccess()  → Use for DbContext configuration

PetStore.Domain
├── FeaturesOptions [OptionsBinding]
└── PetStoreOptions [OptionsBinding]

PetStore.DataAccess
└── DatabaseOptions [OptionsBinding]
```

**Best Practices**:

1. **Use for Conditional Registration**: Only use early access when you need options DURING registration
2. **Validate Eagerly**: Early access validates immediately - ensure appsettings.json is correct before deployment
3. **Single Provider Build**: Only call `BuildServiceProvider()` ONCE at the end
4. **Combine with AddOptionsFromApp**: Call both for completeness (idempotent)
5. **Avoid for Scoped Options**: Early access uses Singleton lifetime - not suitable for per-request configuration

**Benefits**:

✅ **Eliminates Anti-Pattern**: No more multiple `BuildServiceProvider()` calls
✅ **Production-Safe**: Prevents memory leaks and scope issues
✅ **Type-Safe**: Full compile-time validation and IntelliSense
✅ **Fail-Fast**: Validation errors caught immediately during registration
✅ **Flexible**: Use individual accessors OR generic registry method
✅ **Idempotent**: Safe to call multiple times
✅ **Zero Runtime Cost**: All code generated at compile time

---

### 11. Auto-Generate Options Classes from appsettings.json

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Reverse the process - analyze appsettings.json and generate strongly-typed options classes.

**Example**:

```json
{
  "Database": {
    "ConnectionString": "...",
    "MaxRetries": 5
  }
}
```

Generates:

```csharp
// Auto-generated
public partial class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}
```

**Considerations**:

- Requires JSON schema inference
- Type ambiguity (is "5" an int or string?)
- May conflict with user-defined classes
- Interesting but low priority

---

### 12. Environment-Specific Validation

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Apply different validation rules based on environment (e.g., stricter in production).

**Example**:

```csharp
[OptionsBinding("Features", ValidateOnStart = true)]
public partial class FeaturesOptions
{
    public bool EnableDebugMode { get; set; }

    // Only validate in production
    [RequiredInProduction]
    public string LicenseKey { get; set; } = string.Empty;
}
```

---

### 13. Hot Reload Support with Filtering

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Fine-grained control over which configuration changes trigger reloads.

**Example**:

```csharp
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, ReloadOn = new[] { "EnableNewUI", "MaxUploadSizeMB" })]
public partial class FeaturesOptions
{
    public bool EnableNewUI { get; set; }  // Triggers reload
    public int MaxUploadSizeMB { get; set; }  // Triggers reload
    public string UITheme { get; set; } = "Light";  // Doesn't trigger reload
}
```

---

## ⛔ Do Not Need (Low Priority / Out of Scope)

### 14. Reflection-Based Binding

**Reason**: Defeats the purpose of compile-time source generation and breaks AOT compatibility.

**Status**: ❌ Out of Scope

---

### 15. JSON Schema Generation

**Reason**: Out of scope for options binding. Use dedicated tools like NJsonSchema.

**Status**: ❌ Not Planned

---

### 16. Configuration Encryption/Decryption

**Reason**: Security concern handled by configuration providers (Azure Key Vault, AWS Secrets Manager, etc.), not binding layer.

**Status**: ❌ Out of Scope

---

### 17. Dynamic Configuration Sources

**Reason**: Configuration providers handle this. Options binding focuses on type-safe access.

**Status**: ❌ Out of Scope

---

## 📅 Proposed Implementation Order

Based on priority, user demand, and implementation complexity:

### Phase 1: Validation & Error Handling (v1.1 - Q1 2025) ✅ COMPLETED

**Goal**: Fail-fast and better validation

1. **Error on Missing Configuration Keys** 🔴 High ⭐ - Startup failures instead of silent nulls ✅
2. **Custom Validation Support (IValidateOptions)** 🔴 High - Complex validation beyond DataAnnotations ✅
3. **Post-Configuration Support** 🟡 Medium-High - Normalization and defaults ✅

**Estimated effort**: 3-4 weeks
**Impact**: Prevent production misconfigurations, better developer experience
**Status**: ✅ All features implemented and shipped

---

### Phase 2: Advanced Scenarios (v1.2 - Q2 2025) ✅ COMPLETED

**Goal**: Multi-tenant and dynamic configuration

4. **Named Options Support** 🔴 High - Multiple configurations for same type ✅
5. **Configuration Change Callbacks** 🟡 Medium - React to runtime changes ✅
6. **ConfigureAll Support** 🟢 Low-Medium - Set defaults across named instances ✅

**Estimated effort**: 4-5 weeks
**Impact**: Multi-tenant scenarios, feature flags, runtime configuration
**Status**: ✅ All features implemented and shipped

---

### Phase 3: Developer Experience (v1.3 - Q3 2025) ✅ COMPLETED

**Goal**: Better diagnostics and usability

7. **Bind Configuration Subsections** 🟡 Medium - Nested object binding (already worked out-of-the-box) ✅
8. **Child Sections** 🟢 Low-Medium - Simplified named options syntax ✅
9. **Compile-Time Section Name Validation** 🟡 Medium - Validate section paths exist ❌ Deferred

**Estimated effort**: 3-4 weeks
**Impact**: Catch configuration errors earlier, better IDE support
**Status**: ✅ Core features implemented

---

### Phase 4: Service Registration Integration (v1.4 - 2025)

**Goal**: Eliminate BuildServiceProvider anti-pattern

10. **Early Access to Options During Service Registration** 🔴 High ⭐ - Access options during registration without BuildServiceProvider
    - Generate `GetOrAddDatabaseOptions()` individual accessor methods
    - Generate `GetOptionInstanceOf<T>()` generic accessor method
    - Internal registry for option instances
    - Idempotent registration
    - Eager validation support

**Estimated effort**: 3-4 weeks
**Impact**:

- Prevents memory leaks and scope issues from multiple BuildServiceProvider calls
- Enables conditional service registration based on configuration
- Production-safe pattern for DbContext, HttpClient, and feature flag configuration
- Critical for real-world ASP.NET Core applications

**Priority Justification**: This is a HIGH priority feature because:

- ⚠️ Multiple BuildServiceProvider calls is a **documented anti-pattern** by Microsoft
- 🐛 Causes production bugs (memory leaks, lifetime issues)
- ⭐ Frequently requested in StackOverflow (66k+ views on related questions)
- 🔥 Blocking issue for developers needing configuration-driven service registration

---

### Phase 5: Optional Enhancements (v2.0+ - 2025-2026)

**Goal**: Nice-to-have features based on feedback

11. **Auto-Generate Options from JSON** 🟢 Low - Reverse generation (experimental)
12. **Environment-Specific Validation** 🟢 Low - Production vs. development validation
13. **Hot Reload with Filtering** 🟢 Low - Fine-grained reload control
14. **Compile-Time Section Name Validation** 🟡 Medium - Validate section paths exist (deferred from Phase 3)

**Estimated effort**: Variable
**Impact**: Polish and edge cases

---

### Feature Prioritization Matrix

| Feature | Priority | User Demand | Complexity | Phase | Status |
|---------|----------|-------------|------------|-------|--------|
| Error on Missing Keys | 🔴 High | ⭐⭐⭐ | Medium | 1.1 | ✅ |
| Custom Validation (IValidateOptions) | 🔴 High | ⭐⭐⭐ | Medium | 1.1 | ✅ |
| Post-Configuration | 🟡 Med-High | ⭐⭐ | Low | 1.1 | ✅ |
| Named Options | 🔴 High | ⭐⭐⭐ | Medium | 1.2 | ✅ |
| Change Callbacks | 🟡 Medium | ⭐⭐ | Medium | 1.2 | ✅ |
| ConfigureAll | 🟢 Low-Med | ⭐ | Low | 1.2 | ✅ |
| Nested Object Binding | 🟡 Medium | ⭐⭐ | Low | 1.3 | ✅ |
| Child Sections | 🟢 Low-Med | ⭐⭐ | Low | 1.3 | ✅ |
| **Early Access to Options** | 🔴 **High** | ⭐⭐⭐ | **Medium-High** | **1.4** | ✅ |
| Section Path Validation | 🟡 Medium | ⭐⭐ | High | 2.0+ | ❌ |
| Environment Validation | 🟢 Low | ⭐ | Medium | 2.0+ | ❌ |
| Hot Reload Filtering | 🟢 Low | ⭐ | Medium | 2.0+ | ❌ |
| Auto-Generate from JSON | 🟢 Low | ⭐ | High | 2.0+ | ❌ |

---

## 🎯 Success Metrics

1. **Startup Failure Rate** - Measure configuration errors caught at startup vs. runtime
2. **GitHub Issues** - Track configuration-related bug reports
3. **Validation Coverage** - % of options classes using validation
4. **Adoption Metrics** - NuGet downloads, multi-tenant usage
5. **Community Feedback** - Developer satisfaction with error messages

---

## 📝 Notes

### Design Philosophy

- **Guiding Principle**: **Type-safe**, **fail-fast**, **AOT-compatible** configuration
- **Trade-offs**: We prioritize compile-time safety over runtime flexibility
- **Microsoft.Extensions.Options Alignment**: We follow Microsoft's patterns but generate the boilerplate
- **Configuration Binder Inspiration**: Like .NET 8's source generator, we replace reflection with compile-time code

### Key Differences from Standard Options Pattern

**What we do differently**:

1. **Attribute-based** - `[OptionsBinding]` instead of manual `Configure<T>()` calls
2. **Smart section name resolution** - 5-level priority system vs. manual specification
3. **Transitive registration** - Auto-discover options from referenced assemblies
4. **Assembly-specific methods** - `AddOptionsFromDomain()` vs. manual registration

**What we learn from Microsoft.Extensions.Options**:

- ⭐ **Validation** is critical - DataAnnotations, custom, and startup validation
- ⭐ **Lifetime management** matters - IOptions vs. IOptionsSnapshot vs. IOptionsMonitor
- ⭐ **Named options** enable multi-tenant scenarios
- ⭐ **Silent failures** are the #1 pain point (missing config → null → production crashes)

### Lessons from GitHub Issues

**From dotnet/runtime issues**:

- **Error on missing keys** is one of the most requested features (Issue #36015)
- **Silent binding failures** cause production incidents
- **Change detection** limitations frustrate developers (file-system only)
- **Type converter limitations** in source generator approach

### Updated Priorities Based on Community Insights

**Originally "Nice to Have" → Elevated**:

- ✅ **Error on Missing Keys** - Moved to "Need to Have" due to production incident prevention
- ✅ **Custom Validation (IValidateOptions)** - Essential for complex business rules

**Recognized as Critical**:

- 🔴 **Named Options** - Multi-tenant scenarios are common
- 🔴 **Post-Configuration** - Normalization and defaults are frequently needed

---

## 🔗 Related Resources

- **Microsoft.Extensions.Options**: <https://learn.microsoft.com/en-us/dotnet/core/extensions/options>
- **Configuration Binder Source Generator**: <https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-generator>
- **GitHub Issue #36015**: <https://github.com/dotnet/runtime/issues/36015> (Error on missing keys)
- **Our Documentation**: See `/docs/generators/OptionsBinding.md`
- **Sample Projects**: See `/sample/PetStore.Api` for complete example

---

**Last Updated**: 2025-01-20
**Version**: 1.2
**Research Date**: January 2025 (.NET 8/9 Options Pattern + Service Registration Anti-Patterns)
**Maintained By**: Atc.SourceGenerators Team
