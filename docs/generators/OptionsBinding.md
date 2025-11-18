# ⚙️ Options Binding Source Generator

Automatically bind configuration sections to strongly-typed options classes with compile-time code generation.

**Key Benefits:**
- 🎯 **Zero boilerplate** - No manual `AddOptions<T>().Bind()` calls needed
- 🧠 **Smart section inference** - Auto-detects section names from class names or constants
- 🛡️ **Built-in validation** - Automatic DataAnnotations validation and startup checks
- 🔧 **Multi-project support** - Smart naming for assembly-specific registration methods
- ⚡ **Native AOT ready** - Pure compile-time generation with zero reflection

**Quick Example:**
```csharp
// Input: Decorate your options class
[OptionsBinding("Database")]
public partial class DatabaseOptions
{
    [Required] public string ConnectionString { get; set; } = string.Empty;
}

// Generated: Registration extension method
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

## 📑 Table of Contents

- [⚙️ Options Binding Source Generator](#️-options-binding-source-generator)
  - [📑 Table of Contents](#-table-of-contents)
  - [📖 Overview](#-overview)
    - [😫 Before (Manual Approach)](#-before-manual-approach)
    - [✨ After (With Source Generator)](#-after-with-source-generator)
  - [🚀 Quick Start](#-quick-start)
    - [1️⃣ Install the Package](#1️⃣-install-the-package)
    - [2️⃣ Create Your Options Class](#2️⃣-create-your-options-class)
    - [3️⃣ Configure Your appsettings.json](#3️⃣-configure-your-appsettingsjson)
    - [4️⃣ Register Options in Program.cs](#4️⃣-register-options-in-programcs)
  - [📋 Configuration Examples](#-configuration-examples)
    - [🎯 Base JSON Configuration](#-base-json-configuration)
    - [📚 All Configuration Patterns](#-all-configuration-patterns)
      - [1️⃣ Explicit Section Name (Highest Priority)](#1️⃣-explicit-section-name-highest-priority)
      - [2️⃣ Using `const string SectionName` (2nd Priority)](#2️⃣-using-const-string-sectionname-2nd-priority)
      - [3️⃣ Using `const string NameTitle` (3rd Priority)](#3️⃣-using-const-string-nametitle-3rd-priority)
      - [4️⃣ Using `const string Name` (4th Priority)](#4️⃣-using-const-string-name-4th-priority)
      - [5️⃣ Auto-Inferred from Class Name (Lowest Priority)](#5️⃣-auto-inferred-from-class-name-lowest-priority)
    - [🔒 Validation Examples](#-validation-examples)
      - [With Data Annotations Only](#with-data-annotations-only)
      - [With Validation On Start](#with-validation-on-start)
      - [With Both Validations (Recommended)](#with-both-validations-recommended)
    - [⏱️ Lifetime Examples](#️-lifetime-examples)
      - [Singleton (Default - IOptions)](#singleton-default---ioptions)
      - [Scoped (IOptionsSnapshot)](#scoped-ioptionssnapshot)
      - [Monitor (IOptionsMonitor)](#monitor-ioptionsmonitor)
    - [🎯 Complete Example - All Features Combined](#-complete-example---all-features-combined)
    - [📊 Priority Summary Table](#-priority-summary-table)
    - [🔄 Mapping Both Base JSON Examples](#-mapping-both-base-json-examples)
  - [✨ Features](#-features)
  - [📦 Installation](#-installation)
    - [📋 Package Reference](#-package-reference)
  - [💡 Usage](#-usage)
    - [🔰 Basic Options Binding](#-basic-options-binding)
    - [📍 Explicit Section Names](#-explicit-section-names)
    - [✅ Validation](#-validation)
      - [🏷️ Data Annotations Validation](#️-data-annotations-validation)
      - [🚀 Validate on Startup](#-validate-on-startup)
      - [🔗 Combined Validation](#-combined-validation)
    - [⏱️ Options Lifetimes](#️-options-lifetimes)
  - [🔧 How It Works](#-how-it-works)
    - [1️⃣ Attribute Detection](#1️⃣-attribute-detection)
    - [2️⃣ Section Name Resolution](#2️⃣-section-name-resolution)
    - [3️⃣ Code Generation](#3️⃣-code-generation)
    - [4️⃣ Compile-Time Safety](#4️⃣-compile-time-safety)
  - [🎯 Advanced Scenarios](#-advanced-scenarios)
    - [🏢 Multiple Assemblies](#-multiple-assemblies)
    - [✨ Smart Naming](#-smart-naming)
    - [📂 Nested Configuration](#-nested-configuration)
    - [🌍 Environment-Specific Configuration](#-environment-specific-configuration)
  - [🛡️ Diagnostics](#️-diagnostics)
    - [❌ ATCOPT001: Options class must be partial](#-atcopt001-options-class-must-be-partial)
    - [❌ ATCOPT002: Section name cannot be null or empty](#-atcopt002-section-name-cannot-be-null-or-empty)
    - [⚠️ ATCOPT003: Invalid options binding configuration](#️-atcopt003-invalid-options-binding-configuration)
    - [❌ ATCOPT003: Const section name cannot be null or empty](#-atcopt003-const-section-name-cannot-be-null-or-empty)
  - [🚀 Native AOT Compatibility](#-native-aot-compatibility)
  - [📚 Examples](#-examples)
    - [📝 Example 1: Simple Configuration](#-example-1-simple-configuration)
    - [🔒 Example 2: Validated Database Options](#-example-2-validated-database-options)
    - [🏗️ Example 3: Multi-Layer Application](#️-example-3-multi-layer-application)
  - [🔗 Additional Resources](#-additional-resources)
  - [❓ FAQ](#-faq)
  - [📄 License](#-license)

---

## 📖 Overview

The Options Binding Source Generator eliminates the boilerplate code required to bind configuration sections to options classes. Simply decorate your options class with `[OptionsBinding]`, and the generator creates the necessary registration code at compile time.

### 😫 Before (Manual Approach)

```csharp
// appsettings.json
{
  "Database": {
    "ConnectionString": "Server=localhost;...",
    "MaxRetries": 5
  }
}

// DatabaseOptions.cs
public class DatabaseOptions
{
    public string ConnectionString { get; set; }
    public int MaxRetries { get; set; }
}

// Program.cs - Manual binding
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### ✨ After (With Source Generator)

```csharp
// DatabaseOptions.cs
[OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; }

    [Range(1, 10)]
    public int MaxRetries { get; set; }
}

// Program.cs - Generated extension method
services.AddOptionsFromApp(configuration);
```

---

## 🚀 Quick Start

### 1️⃣ Install the Package

```bash
dotnet add package Atc.SourceGenerators
```

### 2️⃣ Create Your Options Class

```csharp
using Atc.SourceGenerators.Annotations;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Configuration;

[OptionsBinding("Database", ValidateDataAnnotations = true)]
public partial class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;
}
```

### 3️⃣ Configure Your appsettings.json

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyDb;",
    "MaxRetries": 5
  }
}
```

### 4️⃣ Register Options in Program.cs

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

// Use the generated extension method
services.AddOptionsFromApp(configuration);

var serviceProvider = services.BuildServiceProvider();

// Access your options
var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
Console.WriteLine(dbOptions.Value.ConnectionString);
```

---

## 📋 Configuration Examples

This section demonstrates all possible ways to create options classes and map them to `appsettings.json` sections.

### 🎯 Base JSON Configuration

We'll use these two JSON sections throughout the examples:

**appsettings.json:**

```json
{
  "PetMaintenanceService": {
    "RepeatIntervalInSeconds": 10,
    "EnableAutoCleanup": true,
    "MaxPetsPerBatch": 50
  },

  "PetOtherServiceOptions": {
    "RepeatIntervalInSeconds": 10,
    "EnableAutoCleanup": true,
    "MaxPetsPerBatch": 50
  }
}
```

**Two different scenarios:**

- **`"PetMaintenanceService"`** - Section name that doesn't match any class name (requires explicit mapping)
- **`"PetOtherServiceOptions"`** - Section name that exactly matches a class name (can use auto-inference)

### 📚 All Configuration Patterns

#### 1️⃣ Explicit Section Name (Highest Priority)

Use when you want full control over the section name:

```csharp
// Maps to "PetMaintenanceService" section
[OptionsBinding("PetMaintenanceService")]
public partial class PetMaintenanceServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}
```

**When to use:**
- ✅ When section name doesn't match class name
- ✅ When using nested configuration paths (e.g., `"App:Services:Database"`)
- ✅ When you want explicit, readable code

#### 2️⃣ Using `const string SectionName` (2nd Priority)

Use when you want the section name defined as a constant in the class:

```csharp
// Maps to "PetMaintenanceService" section
[OptionsBinding]
public partial class PetMaintenanceServiceOptions
{
    public const string SectionName = "PetMaintenanceService";

    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}
```

**When to use:**
- ✅ When you want the section name accessible as a constant
- ✅ When other code needs to reference the same section name
- ✅ When building configuration paths dynamically

#### 3️⃣ Using `const string NameTitle` (3rd Priority)

Use as an alternative to `SectionName`:

```csharp
// Maps to "PetMaintenanceService" section
[OptionsBinding]
public partial class PetMaintenanceServiceOptions
{
    public const string NameTitle = "PetMaintenanceService";

    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}
```

**When to use:**
- ✅ When following specific naming conventions
- ✅ When `SectionName` is not preferred in your codebase

#### 4️⃣ Using `const string Name` (4th Priority)

Another alternative for section name definition:

```csharp
// Maps to "PetMaintenanceService" section
[OptionsBinding]
public partial class PetMaintenanceServiceOptions
{
    public const string Name = "PetMaintenanceService";

    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}
```

**When to use:**
- ✅ When following specific naming conventions
- ✅ When `Name` fits your code style better

#### 5️⃣ Auto-Inferred from Class Name (Lowest Priority)

The generator uses the full class name as-is:

```csharp
// Maps to "PetOtherServiceOptions" section (full class name)
[OptionsBinding]
public partial class PetOtherServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}
```

**When to use:**
- ✅ When section name matches class name exactly
- ✅ When you want minimal code
- ✅ When following convention-over-configuration

**Important:** The class name is used **as-is** - no suffix removal or transformation:
- `DatabaseOptions` → `"DatabaseOptions"` (NOT `"Database"`)
- `ApiSettings` → `"ApiSettings"` (NOT `"Api"`)
- `CacheConfig` → `"CacheConfig"` (NOT `"Cache"`)
- `PetOtherServiceOptions` → `"PetOtherServiceOptions"` ✅ (Matches our JSON section!)

### 🔒 Validation Examples

#### With Data Annotations Only

```csharp
using System.ComponentModel.DataAnnotations;

// Maps to "PetMaintenanceService" section
[OptionsBinding("PetMaintenanceService", ValidateDataAnnotations = true)]
public partial class PetMaintenanceServiceOptions
{
    [Range(1, 3600)]
    public int RepeatIntervalInSeconds { get; set; }

    public bool EnableAutoCleanup { get; set; }

    [Range(1, 1000)]
    public int MaxPetsPerBatch { get; set; }
}
```

**Generated code includes:**
```csharp
services.AddOptions<PetMaintenanceServiceOptions>()
    .Bind(configuration.GetSection("PetMaintenanceService"))
    .ValidateDataAnnotations();
```

#### With Validation On Start

```csharp
// Maps to "PetMaintenanceService" section
[OptionsBinding("PetMaintenanceService", ValidateOnStart = true)]
public partial class PetMaintenanceServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}
```

**Generated code includes:**
```csharp
services.AddOptions<PetMaintenanceServiceOptions>()
    .Bind(configuration.GetSection("PetMaintenanceService"))
    .ValidateOnStart();
```

#### With Both Validations (Recommended)

```csharp
using System.ComponentModel.DataAnnotations;

// Maps to "PetMaintenanceService" section
[OptionsBinding("PetMaintenanceService",
    ValidateDataAnnotations = true,
    ValidateOnStart = true)]
public partial class PetMaintenanceServiceOptions
{
    [Required]
    [Range(1, 3600, ErrorMessage = "Interval must be between 1 and 3600 seconds")]
    public int RepeatIntervalInSeconds { get; set; }

    public bool EnableAutoCleanup { get; set; }

    [Range(1, 1000)]
    public int MaxPetsPerBatch { get; set; }
}
```

**Generated code includes:**
```csharp
services.AddOptions<PetMaintenanceServiceOptions>()
    .Bind(configuration.GetSection("PetMaintenanceService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### ⏱️ Lifetime Examples

#### Singleton (Default - IOptions<T>)

Best for options that don't change during application lifetime:

```csharp
// Default: Lifetime = OptionsLifetime.Singleton
[OptionsBinding("PetMaintenanceService")]
public partial class PetMaintenanceServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
}

// Usage:
public class PetMaintenanceService
{
    public PetMaintenanceService(IOptions<PetMaintenanceServiceOptions> options)
    {
        var config = options.Value; // Cached singleton value
    }
}
```

**Generated code comment:**
```csharp
// Configure PetMaintenanceServiceOptions - Inject using IOptions<T>
```

#### Scoped (IOptionsSnapshot<T>)

Best for options that may change per request/scope:

```csharp
[OptionsBinding("PetMaintenanceService", Lifetime = OptionsLifetime.Scoped)]
public partial class PetMaintenanceServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
}

// Usage:
public class PetRequestHandler
{
    public PetRequestHandler(IOptionsSnapshot<PetMaintenanceServiceOptions> options)
    {
        var config = options.Value; // Fresh value per scope/request
    }
}
```

**Generated code comment:**
```csharp
// Configure PetMaintenanceServiceOptions - Inject using IOptionsSnapshot<T>
```

#### Monitor (IOptionsMonitor<T>)

Best for options that need change notifications and hot-reload:

```csharp
[OptionsBinding("PetMaintenanceService", Lifetime = OptionsLifetime.Monitor)]
public partial class PetMaintenanceServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
}

// Usage:
public class PetMaintenanceService
{
    public PetMaintenanceService(IOptionsMonitor<PetMaintenanceServiceOptions> options)
    {
        var config = options.CurrentValue; // Always current value

        // Subscribe to configuration changes
        options.OnChange(newConfig =>
        {
            Console.WriteLine($"Configuration changed! New interval: {newConfig.RepeatIntervalInSeconds}");
        });
    }
}
```

**Generated code comment:**
```csharp
// Configure PetMaintenanceServiceOptions - Inject using IOptionsMonitor<T>
```

### 🎯 Complete Example - All Features Combined

Here's an example using all features together:

**appsettings.json:**
```json
{
  "PetMaintenanceService": {
    "RepeatIntervalInSeconds": 10,
    "EnableAutoCleanup": true,
    "MaxPetsPerBatch": 50,
    "NotificationEmail": "admin@petstore.com"
  }
}
```

**Options class:**
```csharp
using System.ComponentModel.DataAnnotations;
using Atc.SourceGenerators.Annotations;

namespace PetStore.Domain.Options;

/// <summary>
/// Configuration options for the pet maintenance service.
/// </summary>
[OptionsBinding("PetMaintenanceService",
    ValidateDataAnnotations = true,
    ValidateOnStart = true,
    Lifetime = OptionsLifetime.Monitor)]
public partial class PetMaintenanceServiceOptions
{
    /// <summary>
    /// The interval in seconds between maintenance runs.
    /// </summary>
    [Required]
    [Range(1, 3600, ErrorMessage = "Interval must be between 1 and 3600 seconds")]
    public int RepeatIntervalInSeconds { get; set; }

    /// <summary>
    /// Whether to enable automatic cleanup of old records.
    /// </summary>
    public bool EnableAutoCleanup { get; set; }

    /// <summary>
    /// Maximum number of pets to process in a single batch.
    /// </summary>
    [Range(1, 1000)]
    public int MaxPetsPerBatch { get; set; } = 50;

    /// <summary>
    /// Email address for maintenance notifications.
    /// </summary>
    [Required]
    [EmailAddress]
    public string NotificationEmail { get; set; } = string.Empty;
}
```

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register all options from Domain assembly
builder.Services.AddOptionsFromDomain(builder.Configuration);

var app = builder.Build();
app.Run();
```

**Usage in service:**
```csharp
public class PetMaintenanceService : BackgroundService
{
    private readonly IOptionsMonitor<PetMaintenanceServiceOptions> _options;
    private readonly ILogger<PetMaintenanceService> _logger;

    public PetMaintenanceService(
        IOptionsMonitor<PetMaintenanceServiceOptions> options,
        ILogger<PetMaintenanceService> logger)
    {
        _options = options;
        _logger = logger;

        // React to configuration changes
        _options.OnChange(newOptions =>
        {
            _logger.LogInformation(
                "Configuration updated! New interval: {Interval}s",
                newOptions.RepeatIntervalInSeconds);
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var config = _options.CurrentValue;

            _logger.LogInformation(
                "Running maintenance with interval {Interval}s, batch size {BatchSize}",
                config.RepeatIntervalInSeconds,
                config.MaxPetsPerBatch);

            // Perform maintenance...

            await Task.Delay(
                TimeSpan.FromSeconds(config.RepeatIntervalInSeconds),
                stoppingToken);
        }
    }
}
```

### 📊 Priority Summary Table

When multiple section name sources are present, the generator uses this priority:

| Priority | Source | Example |
|----------|--------|---------|
| 1️⃣ **Highest** | Attribute parameter | `[OptionsBinding("Database")]` |
| 2️⃣ | `const string SectionName` | `public const string SectionName = "DB";` |
| 3️⃣ | `const string NameTitle` | `public const string NameTitle = "DB";` |
| 4️⃣ | `const string Name` | `public const string Name = "DB";` |
| 5️⃣ **Lowest** | Auto-inferred from class name | Class `DatabaseOptions` → `"DatabaseOptions"` |

**Example showing priority:**
```csharp
// This maps to "ExplicitSection" (priority 1 wins)
[OptionsBinding("ExplicitSection")]
public partial class MyOptions
{
    public const string SectionName = "SectionNameConst";  // Ignored (priority 2)
    public const string NameTitle = "NameTitleConst";      // Ignored (priority 3)
    public const string Name = "NameConst";                // Ignored (priority 4)
    // Class name "MyOptions" would be used if no explicit section (priority 5)
}
```

### 🔄 Mapping Both Base JSON Examples

Here's how to map both JSON sections from our base configuration:

**appsettings.json:**
```json
{
  "PetMaintenanceService": {
    "RepeatIntervalInSeconds": 10,
    "EnableAutoCleanup": true,
    "MaxPetsPerBatch": 50
  },
  "PetOtherServiceOptions": {
    "RepeatIntervalInSeconds": 10,
    "EnableAutoCleanup": true,
    "MaxPetsPerBatch": 50
  }
}
```

**Options classes:**
```csharp
// Case 1: Section name doesn't match class name - Use explicit mapping
[OptionsBinding("PetMaintenanceService")]  // ✅ Explicit section name required
public partial class PetMaintenanceServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}

// Case 2: Section name matches class name exactly - Auto-inference works!
[OptionsBinding]  // ✅ No section name needed - infers "PetOtherServiceOptions"
public partial class PetOtherServiceOptions
{
    public int RepeatIntervalInSeconds { get; set; }
    public bool EnableAutoCleanup { get; set; }
    public int MaxPetsPerBatch { get; set; }
}
```

**Program.cs:**
```csharp
// Both registered with a single call
services.AddOptionsFromYourProject(configuration);

// Use the options
var maintenanceOptions = provider.GetRequiredService<IOptions<PetMaintenanceServiceOptions>>();
var otherOptions = provider.GetRequiredService<IOptions<PetOtherServiceOptions>>();

Console.WriteLine($"Maintenance interval: {maintenanceOptions.Value.RepeatIntervalInSeconds}s");
Console.WriteLine($"Other interval: {otherOptions.Value.RepeatIntervalInSeconds}s");
```

---

## ✨ Features

- **🧠 Automatic section name inference** - Smart resolution from explicit names, const fields (`SectionName`, `NameTitle`, `Name`), or auto-inferred from class names
- **🔒 Built-in validation** - Integrated DataAnnotations validation (`ValidateDataAnnotations`) and startup validation (`ValidateOnStart`)
- **🎯 Explicit section paths** - Support for nested sections like `"App:Database"` or `"Services:Email"`
- **📦 Multiple options classes** - Register multiple configuration sections in a single assembly with one method call
- **🏗️ Multi-project support** - Smart naming generates assembly-specific extension methods (e.g., `AddOptionsFromDomain()`, `AddOptionsFromDataAccess()`)
- **🔗 Transitive registration** - Automatically discover and register options from referenced assemblies (4 overloads: default, auto-detect all, selective by name, selective multiple)
- **⏱️ Flexible lifetimes** - Choose between Singleton (`IOptions<T>`), Scoped (`IOptionsSnapshot<T>`), or Monitor (`IOptionsMonitor<T>`) patterns
- **⚡ Native AOT ready** - Pure compile-time code generation with zero reflection, fully trimming-safe for modern .NET deployments
- **🛡️ Compile-time safety** - Catch configuration errors during build, not at runtime
- **🔧 Partial class requirement** - Simple `partial` keyword enables seamless extension method generation

---

**Section Name Resolution Priority:**
1. Explicit attribute parameter: `[OptionsBinding("SectionName")]`
2. Const field: `public const string SectionName = "...";`
3. Const field: `public const string NameTitle = "...";`
4. Const field: `public const string Name = "...";`
5. Auto-inferred from class name

---

**Transitive Registration Overloads:**
```csharp
// Overload 1: Base (current assembly only)
services.AddOptionsFrom{Assembly}(configuration);

// Overload 2: Auto-detect all referenced assemblies
services.AddOptionsFrom{Assembly}(configuration, includeReferencedAssemblies: true);

// Overload 3: Register specific referenced assembly
services.AddOptionsFrom{Assembly}(configuration, "DataAccess");

// Overload 4: Register multiple specific assemblies
services.AddOptionsFrom{Assembly}(configuration, "DataAccess", "Infrastructure");
```

---

## 📦 Installation

### 📋 Package Reference

**Required:**
```bash
dotnet add package Atc.SourceGenerators
```

**Optional (recommended for better IntelliSense):**
```bash
dotnet add package Atc.SourceGenerators.Annotations
```

Or in your `.csproj`:

```xml
<ItemGroup>
  <!-- Required: Source generator -->
  <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />

  <!-- Optional: Attribute definitions with XML documentation -->
  <PackageReference Include="Atc.SourceGenerators.Annotations" Version="1.0.0" />
</ItemGroup>
```

**Note:** The generator emits fallback attributes automatically, so the Annotations package is optional. However, it provides better XML documentation and IntelliSense. If you include it, suppress the expected CS0436 warning: `<NoWarn>$(NoWarn);CS0436</NoWarn>`

---

## 💡 Usage

### 🔰 Basic Options Binding

The simplest usage with automatic section name inference:

```csharp
using Atc.SourceGenerators.Annotations;

namespace MyApp.Options;

[OptionsBinding]  // Section name inferred as "Database"
public partial class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}
```

**appsettings.json:**

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyDb;",
    "MaxRetries": 5,
    "TimeoutSeconds": 60
  }
}
```

**Generated Code:**

```csharp
public static class OptionsBindingExtensions
{
    public static IServiceCollection AddOptionsFromApp(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<global::MyApp.Options.DatabaseOptions>()
            .Bind(configuration.GetSection("Database"))
            ;

        return services;
    }
}
```

### 📍 Explicit Section Names

Specify the exact configuration path:

```csharp
[OptionsBinding("App:ExternalServices:PaymentGateway")]
public partial class PaymentOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}
```

**appsettings.json:**

```json
{
  "App": {
    "ExternalServices": {
      "PaymentGateway": {
        "ApiKey": "your-api-key",
        "BaseUrl": "https://payment-api.com"
      }
    }
  }
}
```

### ✅ Validation

#### 🏷️ Data Annotations Validation

```csharp
using System.ComponentModel.DataAnnotations;

[OptionsBinding("Email", ValidateDataAnnotations = true)]
public partial class EmailOptions
{
    [Required, EmailAddress]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    [Required]
    public string Username { get; set; } = string.Empty;
}
```

**Generated Code:**

```csharp
services.AddOptions<global::MyApp.Options.EmailOptions>()
    .Bind(configuration.GetSection("Email"))
    .ValidateDataAnnotations()
    ;
```

#### 🚀 Validate on Startup

Ensure options are valid when the application starts:

```csharp
[OptionsBinding("Database", ValidateOnStart = true)]
public partial class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}
```

**Generated Code:**

```csharp
services.AddOptions<global::MyApp.Options.DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateOnStart()
    ;
```

#### 🔗 Combined Validation

```csharp
[OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class DatabaseOptions
{
    [Required, MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;
}
```

### ⏱️ Options Lifetimes

Control which options interface consumers should inject. **All three interfaces are always available**, but the `Lifetime` property indicates the recommended interface for your use case:

```csharp
// Singleton lifetime - Use IOptions<T>
// Best for: Options that don't change during app lifetime
[OptionsBinding("Logging", Lifetime = OptionsLifetime.Singleton)]
public partial class LoggingOptions { }

// Scoped lifetime - Use IOptionsSnapshot<T>
// Best for: Options that may change per request/scope, supports reloading
[OptionsBinding("Request", Lifetime = OptionsLifetime.Scoped)]
public partial class RequestOptions { }

// Monitor lifetime - Use IOptionsMonitor<T>
// Best for: Options that need change notifications and hot-reload support
[OptionsBinding("Feature", Lifetime = OptionsLifetime.Monitor)]
public partial class FeatureOptions { }
```

**How to consume:**

```csharp
// Singleton - IOptions<T> (default, most common)
public class MyService
{
    public MyService(IOptions<LoggingOptions> options)
    {
        var logOptions = options.Value;
    }
}

// Scoped - IOptionsSnapshot<T> (reloads per request)
public class RequestHandler
{
    public RequestHandler(IOptionsSnapshot<RequestOptions> options)
    {
        var reqOptions = options.Value;  // Fresh value per request
    }
}

// Monitor - IOptionsMonitor<T> (supports change notifications)
public class FeatureManager
{
    public FeatureManager(IOptionsMonitor<FeatureOptions> options)
    {
        var features = options.CurrentValue;  // Current value

        // Subscribe to changes
        options.OnChange(newOptions =>
        {
            // Handle configuration changes
        });
    }
}
```

**Important Notes:**
- `AddOptions<T>()` registers **all three interfaces** automatically
- The `Lifetime` property is a **recommendation** for which interface to inject
- Default is `Singleton` (IOptions<T>) if not specified
- The generated code includes comments indicating the recommended interface

---

## 🔧 How It Works

### 1️⃣ Attribute Detection

The generator scans your code for classes decorated with `[OptionsBinding]`:

```csharp
[OptionsBinding("Database")]
public partial class DatabaseOptions { }
```

### 2️⃣ Section Name Resolution

The generator resolves section names in the following priority order:

1. **Explicit section name** - Provided in the attribute constructor parameter
   ```csharp
   [OptionsBinding("App:Database")]
   public partial class DatabaseOptions { }  // Uses "App:Database"
   ```

2. **`public const string SectionName`** - Defined in the options class (2nd highest priority)
   ```csharp
   [OptionsBinding]
   public partial class DatabaseOptions
   {
       public const string SectionName = "CustomDatabase";  // Uses "CustomDatabase"
   }
   ```

3. **`public const string NameTitle`** - Defined in the options class (takes priority over `Name`)
   ```csharp
   [OptionsBinding]
   public partial class CacheOptions
   {
       public const string NameTitle = "MyCache";  // Uses "MyCache"
   }
   ```

4. **`public const string Name`** - Defined in the options class
   ```csharp
   [OptionsBinding]
   public partial class EmailOptions
   {
       public const string Name = "EmailConfig";  // Uses "EmailConfig"
   }
   ```

5. **Auto-inferred** - Uses the full class name as-is:
   - `DatabaseOptions` → `"DatabaseOptions"`
   - `ApiSettings` → `"ApiSettings"`
   - `LoggingConfig` → `"LoggingConfig"`
   - `CacheConfiguration` → `"CacheConfiguration"`

### 3️⃣ Code Generation

Generates an extension method for your assembly:

```csharp
public static IServiceCollection AddOptionsFrom{AssemblyName}(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Registration code for each options class
}
```

### 4️⃣ Compile-Time Safety

All code is generated at compile time, ensuring:
- ✅ Type safety
- ✅ No runtime reflection
- ✅ IntelliSense support
- ✅ Easy debugging

---

## 🎯 Advanced Scenarios

### 🏢 Multiple Assemblies

Each assembly gets its own extension method:

**MyApp.Core:**

```csharp
[OptionsBinding("Database")]
public partial class DatabaseOptions { }

// Generated: AddOptionsFromAppCore(configuration)
```

**MyApp.Api:**

```csharp
[OptionsBinding("Api")]
public partial class ApiOptions { }

// Generated: AddOptionsFromAppApi(configuration)
```

**Program.cs:**

```csharp
services.AddOptionsFromCore(configuration);
services.AddOptionsFromApi(configuration);
```

### ✨ Smart Naming

The generator uses **smart suffix-based naming** to create cleaner, more readable method names:

**How it works:**
- ✅ If the assembly suffix (last segment after final dot) is **unique** among all assemblies → use short suffix
- ⚠️ If multiple assemblies have the **same suffix** → use full sanitized name to avoid conflicts

**Examples:**

```csharp
// ✅ Unique suffixes (cleaner names)
PetStore.Domain     → AddOptionsFromDomain(configuration)
PetStore.DataAccess → AddOptionsFromDataAccess(configuration)
PetStore.Api        → AddOptionsFromApi(configuration)

// ⚠️ Conflicting suffixes (full names prevent collisions)
PetStore.Domain     → AddOptionsFromPetStoreDomain(configuration)
AnotherApp.Domain   → AddOptionsFromAnotherAppDomain(configuration)
```

**Benefits:**
- 🎯 **Cleaner API**: Shorter method names when there are no conflicts
- 🛡️ **Automatic Conflict Prevention**: Fallback to full names prevents naming collisions
- ⚡ **Zero Configuration**: Works automatically based on compilation context
- 🔄 **Context-Aware**: Method names adapt to the assemblies in your solution

### 📂 Nested Configuration

```csharp
[OptionsBinding("App:Services:Email:Smtp")]
public partial class SmtpOptions { }
```

```json
{
  "App": {
    "Services": {
      "Email": {
        "Smtp": {
          "Host": "smtp.example.com",
          "Port": 587
        }
      }
    }
  }
}
```

### 🌍 Environment-Specific Configuration

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

services.AddOptionsFromApp(configuration);
```

---

## 🛡️ Diagnostics

The generator provides helpful compile-time diagnostics:

### ❌ ATCOPT001: Options class must be partial

**Error:**

```csharp
[OptionsBinding("Database")]
public class DatabaseOptions { }  // ❌ Missing 'partial' keyword
```

**Fix:**

```csharp
[OptionsBinding("Database")]
public partial class DatabaseOptions { }  // ✅ Correct
```

### ❌ ATCOPT002: Section name cannot be null or empty

**Error:**

```csharp
[OptionsBinding("")]  // ❌ Empty section name
public partial class DatabaseOptions { }
```

**Fix:**

```csharp
[OptionsBinding("Database")]  // ✅ Provide section name
public partial class DatabaseOptions { }

// Or let it be inferred
[OptionsBinding]  // ✅ Inferred as "Database"
public partial class DatabaseOptions { }
```

### ⚠️ ATCOPT003: Invalid options binding configuration

**Warning:**

General warning for invalid configuration scenarios.

### ❌ ATCOPT003: Const section name cannot be null or empty

**Error:**

```csharp
[OptionsBinding]
public partial class DatabaseOptions
{
    public const string Name = "";  // ❌ Empty const value
}
```

```csharp
[OptionsBinding]
public partial class ApiOptions
{
    public const string NameTitle = null;  // ❌ Null const value
}
```

**Fix:**

```csharp
// Provide a valid const value
[OptionsBinding]
public partial class DatabaseOptions
{
    public const string Name = "MyDatabase";  // ✅ Valid const value
}

// Or remove the const field to use auto-inference
[OptionsBinding]
public partial class DatabaseOptions  // ✅ Inferred as "Database"
{
    // No const Name/NameTitle field
}
```

---

## 🚀 Native AOT Compatibility

The Options Binding Generator is **fully compatible with Native AOT** compilation, producing code that meets all AOT requirements:

### ✅ AOT-Safe Features

- **Zero reflection** - All options binding uses `IConfiguration.Bind()` without reflection-based discovery
- **Compile-time generation** - Binding code is generated during build, not at runtime
- **Trimming-safe** - No dynamic type discovery or metadata dependencies
- **Static method calls** - All registration uses concrete extension method calls
- **Static analysis friendly** - All code paths are visible to the AOT compiler

### 🏗️ How It Works

1. **Build-time analysis**: The generator scans classes with `[OptionsBinding]` attributes during compilation
2. **Method generation**: Creates static extension methods with concrete `IConfiguration.GetSection()` and `Bind()` calls
3. **Options API integration**: Uses standard .NET Options pattern (`AddOptions<T>()`, `Bind()`, `Validate()`)
4. **AOT compilation**: The generated code compiles to native machine code with full optimizations

### 📋 Example Generated Code

```csharp
// Source: [OptionsBinding("Database")] public partial class DatabaseOptions { ... }

// Generated AOT-safe code:
public static IServiceCollection AddOptionsFromYourProject(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddOptions<DatabaseOptions>()
        .Bind(configuration.GetSection("Database"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    return services;
}
```

**Why This Is AOT-Safe:**
- No `Activator.CreateInstance()` calls (reflection)
- No dynamic assembly scanning
- All types resolved at compile time via generic parameters
- Configuration binding uses built-in AOT-compatible `IConfiguration.Bind()`
- Validation uses standard DataAnnotations attributes

### 🎯 Multi-Project AOT Support

Even transitive options registration remains fully AOT-compatible:

```csharp
// Auto-detect and register referenced assemblies - still AOT-safe!
services.AddOptionsFromApp(configuration, includeReferencedAssemblies: true);
```

The generator produces concrete method calls to each referenced assembly's registration method, ensuring the entire dependency chain compiles to efficient native code.

---

## 📚 Examples

### 📝 Example 1: Simple Configuration

```csharp
// Options class
[OptionsBinding]
public partial class AppOptions
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

// appsettings.json
{
  "App": {
    "Name": "My Application",
    "Version": "1.0.0"
  }
}

// Program.cs
services.AddOptionsFromApp(configuration);
var appOptions = serviceProvider.GetRequiredService<IOptions<AppOptions>>();
Console.WriteLine($"{appOptions.Value.Name} v{appOptions.Value.Version}");
```

### 🔒 Example 2: Validated Database Options

```csharp
[OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class DatabaseOptions
{
    [Required, MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}
```

### 🏗️ Example 3: Multi-Layer Application

**appsettings.json:**

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyDb;"
  },
  "Api": {
    "BaseUrl": "https://api.example.com",
    "Timeout": 30
  },
  "Logging": {
    "Level": "Information",
    "EnableConsole": true
  }
}
```

**Options Classes:**

```csharp
[OptionsBinding("Database")]
public partial class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}

[OptionsBinding("Api")]
public partial class ApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
}

[OptionsBinding("Logging")]
public partial class LoggingOptions
{
    public string Level { get; set; } = "Information";
    public bool EnableConsole { get; set; } = true;
}
```

**Program.cs:**

```csharp
// Single call registers all options
services.AddOptionsFromApp(configuration);

// Use options
var dbOpts = provider.GetRequiredService<IOptions<DatabaseOptions>>();
var apiOpts = provider.GetRequiredService<IOptions<ApiOptions>>();
var logOpts = provider.GetRequiredService<IOptions<LoggingOptions>>();
```

---

## 🔗 Additional Resources

- **[Sample Project](../sample/Atc.SourceGenerators.OptionsBinding)** - Working example
- **[Source Generator Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)** - Microsoft Docs
- **[Options Pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)** - Microsoft Docs

---

## ❓ FAQ

**Q: Do I need to make my options class partial?**

A: Yes, the `partial` keyword is required for source generators to add generated code to your class.

**Q: Can I use this with ASP.NET Core?**

A: Absolutely! It works with any .NET application that uses `Microsoft.Extensions.Options`.

**Q: What if I don't specify a section name?**

A: The generator infers the section name by removing common suffixes (`Options`, `Settings`, `Config`, `Configuration`) from your class name.

**Q: Can I validate options at startup?**

A: Yes, use `ValidateOnStart = true` in the attribute. Combined with `ValidateDataAnnotations = true`, your options will be validated when the application starts.

**Q: Does this work with reloadable configuration?**

A: Yes, when you use `reloadOnChange: true` with your configuration source, the bound options will reflect changes automatically.

---

## 📄 License

[License information here]
