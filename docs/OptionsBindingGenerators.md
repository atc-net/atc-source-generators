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

## 📖 Documentation Navigation

- **[📋 Feature Roadmap](OptionsBindingGenerators-FeatureRoadmap.md)** - See all implemented and planned features
- **[🎯 Sample Projects](OptionsBindingGenerators-Samples.md)** - Working code examples with architecture diagrams

## 📑 Table of Contents

- [⚙️ Options Binding Source Generator](#️-options-binding-source-generator)
  - [� Documentation Navigation](#-documentation-navigation)
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
      - [🎯 Custom Validation (IValidateOptions)](#-custom-validation-ivalidateoptions)
      - [🚨 Error on Missing Configuration Keys](#-error-on-missing-configuration-keys)
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
    - [✅ AOT-Safe Features](#-aot-safe-features)
    - [🏗️ How It Works](#️-how-it-works)
    - [📋 Example Generated Code](#-example-generated-code)
    - [🎯 Multi-Project AOT Support](#-multi-project-aot-support)
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
- **🎯 Custom validation** - Support for `IValidateOptions<T>` for complex business rules beyond DataAnnotations
- **🚨 Error on missing keys** - Fail-fast validation when configuration sections are missing (`ErrorOnMissingKeys`) to catch deployment issues at startup
- **🔔 Configuration change callbacks** - Automatically respond to configuration changes at runtime with `OnChange` callbacks (requires Monitor lifetime)
- **🔧 Post-configuration support** - Normalize or transform values after binding with `PostConfigure` callbacks (e.g., ensure paths have trailing slashes, lowercase URLs)
- **📛 Named options** - Multiple configurations of the same options type with different names (e.g., Primary/Secondary email servers)
- **🎯 Explicit section paths** - Support for nested sections like `"App:Database"` or `"Services:Email"`
- **📂 Nested subsection binding** - Automatically bind complex properties to configuration subsections (e.g., `StorageOptions.Database.Retry` → `"Storage:Database:Retry"`)
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

#### 🎯 Custom Validation (IValidateOptions)

For complex validation logic that goes beyond DataAnnotations, use custom validators implementing `IValidateOptions<T>`:

```csharp
using Microsoft.Extensions.Options;

// Options class with custom validator
[OptionsBinding("Database",
    ValidateDataAnnotations = true,
    ValidateOnStart = true,
    Validator = typeof(DatabaseOptionsValidator))]
public partial class DatabaseOptions
{
    [Required, MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    public int TimeoutSeconds { get; set; } = 30;
}

// Custom validator with complex business rules
public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        var failures = new List<string>();

        // Custom validation: timeout must be at least 10 seconds
        if (options.TimeoutSeconds < 10)
        {
            failures.Add("TimeoutSeconds must be at least 10 seconds for reliable operations");
        }

        // Custom validation: connection string must contain Server or Data Source
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            var connStr = options.ConnectionString.ToLowerInvariant();
            if (!connStr.Contains("server=") && !connStr.Contains("data source="))
            {
                failures.Add("ConnectionString must contain 'Server=' or 'Data Source=' parameter");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
```

**Generated Code:**

```csharp
services.AddOptions<global::MyApp.Options.DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddSingleton<global::Microsoft.Extensions.Options.IValidateOptions<global::MyApp.Options.DatabaseOptions>,
    global::MyApp.Options.DatabaseOptionsValidator>();
```

**Key Features:**
- Supports complex validation logic beyond DataAnnotations
- Validator is automatically registered as a singleton
- Runs during options validation pipeline
- Can validate cross-property dependencies
- Returns detailed failure messages

#### 🚨 Error on Missing Configuration Keys

The `ErrorOnMissingKeys` feature provides fail-fast validation when configuration sections are missing, preventing runtime errors from invalid or missing configuration.

**When to use:**
- Critical configuration that must be present (database connections, API keys, etc.)
- Detect configuration issues at application startup instead of later at runtime
- Ensure deployment validation catches missing configuration files or sections

**Example:**

```csharp
using System.ComponentModel.DataAnnotations;

[OptionsBinding("Database",
    ValidateDataAnnotations = true,
    ValidateOnStart = true,
    ErrorOnMissingKeys = true)]
public partial class DatabaseOptions
{
    [Required, MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    public int TimeoutSeconds { get; set; } = 30;
}
```

**Generated Code:**

```csharp
services.AddOptions<global::MyApp.Options.DatabaseOptions>()
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

**Behavior:**
- Validates that the configuration section exists using `IConfigurationSection.Exists()`
- Throws `InvalidOperationException` with descriptive message if section is missing
- Combines with `ValidateOnStart = true` to fail at startup (recommended)
- Error message includes the section name for easy troubleshooting

**Best Practices:**
- Always combine with `ValidateOnStart = true` to catch missing configuration at startup
- Use for production-critical configuration (databases, external services, etc.)
- Avoid for optional configuration with reasonable defaults
- Ensure deployment processes validate configuration files exist

**Example Error Message:**
```
System.InvalidOperationException: Configuration section 'Database' is missing.
Ensure the section exists in your appsettings.json or other configuration sources.
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

### 🔔 Configuration Change Callbacks

Automatically respond to configuration changes at runtime using the `OnChange` property. This feature enables hot-reload of configuration without restarting your application.

**Requirements:**
- Must use `Lifetime = OptionsLifetime.Monitor`
- Requires appsettings.json with `reloadOnChange: true`
- Cannot be used with named options
- Callback method must have signature: `static void MethodName(TOptions options, string? name)`

**Basic Example:**

```csharp
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
public partial class FeaturesOptions
{
    public bool EnableNewUI { get; set; }
    public bool EnableBetaFeatures { get; set; }

    internal static void OnFeaturesChanged(FeaturesOptions options, string? name)
    {
        Console.WriteLine("[OnChange] Feature flags changed:");
        Console.WriteLine($"  EnableNewUI: {options.EnableNewUI}");
        Console.WriteLine($"  EnableBetaFeatures: {options.EnableBetaFeatures}");
    }
}
```

**Generated Code:**

The generator automatically creates an `IHostedService` that registers the callback:

```csharp
// Registration in extension method
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

**Usage Scenarios:**

```csharp
// ✅ Feature toggles that change without restart
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
public partial class FeaturesOptions
{
    public bool EnableNewUI { get; set; }

    internal static void OnFeaturesChanged(FeaturesOptions options, string? name)
    {
        // Update feature flag cache, notify observers, etc.
    }
}

// ✅ Logging configuration changes
[OptionsBinding("Logging", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnLoggingChanged))]
public partial class LoggingOptions
{
    public string Level { get; set; } = "Information";

    internal static void OnLoggingChanged(LoggingOptions options, string? name)
    {
        // Reconfigure logging providers with new level
    }
}

// ✅ Combined with validation
[OptionsBinding("Database",
    Lifetime = OptionsLifetime.Monitor,
    ValidateDataAnnotations = true,
    ValidateOnStart = true,
    OnChange = nameof(OnDatabaseChanged))]
public partial class DatabaseOptions
{
    [Required] public string ConnectionString { get; set; } = string.Empty;

    internal static void OnDatabaseChanged(DatabaseOptions options, string? name)
    {
        // Refresh connection pools, update database context, etc.
    }
}
```

**Validation Errors:**

The generator performs compile-time validation of OnChange callbacks:

- **ATCOPT004**: OnChange callback requires Monitor lifetime
  ```csharp
  // ❌ Error: Must use Lifetime = OptionsLifetime.Monitor
  [OptionsBinding("Settings", OnChange = nameof(OnChanged))]
  public partial class Settings { }
  ```

- **ATCOPT005**: OnChange callback not supported with named options
  ```csharp
  // ❌ Error: Named options don't support OnChange
  [OptionsBinding("Email", Name = "Primary", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnChanged))]
  public partial class EmailOptions { }
  ```

- **ATCOPT006**: OnChange callback method not found
  ```csharp
  // ❌ Error: Method 'OnSettingsChanged' does not exist
  [OptionsBinding("Settings", Lifetime = OptionsLifetime.Monitor, OnChange = "OnSettingsChanged")]
  public partial class Settings { }
  ```

- **ATCOPT007**: OnChange callback method has invalid signature
  ```csharp
  // ❌ Error: Must be static void with (TOptions, string?) parameters
  [OptionsBinding("Settings", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnChanged))]
  public partial class Settings
  {
      private void OnChanged(Settings options) { }  // Wrong: not static, missing 2nd parameter
  }
  ```

**Important Notes:**

- Change detection only works with file-based configuration providers (e.g., appsettings.json with `reloadOnChange: true`)
- The callback is invoked whenever the configuration file changes and is reloaded
- The hosted service is automatically registered when the application starts
- Callback method can be `internal` or `public` (not `private`)
- The `name` parameter is useful when dealing with named options in other scenarios (always null for unnamed options)

---

### 🔧 Post-Configuration Support

Automatically normalize, validate, or transform configuration values after binding using the `PostConfigure` property. This feature enables applying defaults, normalizing paths, lowercasing URLs, or computing derived properties.

**Requirements:**
- Cannot be used with named options
- Callback method must have signature: `static void MethodName(TOptions options)`
- Runs after binding and validation

**Basic Example:**

```csharp
[OptionsBinding("Storage", PostConfigure = nameof(NormalizePaths))]
public partial class StoragePathsOptions
{
    public string BasePath { get; set; } = string.Empty;
    public string CachePath { get; set; } = string.Empty;
    public string TempPath { get; set; } = string.Empty;

    private static void NormalizePaths(StoragePathsOptions options)
    {
        // Ensure all paths end with directory separator
        options.BasePath = EnsureTrailingSlash(options.BasePath);
        options.CachePath = EnsureTrailingSlash(options.CachePath);
        options.TempPath = EnsureTrailingSlash(options.TempPath);
    }

    private static string EnsureTrailingSlash(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        return path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}
```

**Generated Code:**

The generator automatically calls `.PostConfigure()` after binding:

```csharp
services.AddOptions<StoragePathsOptions>()
    .Bind(configuration.GetSection("Storage"))
    .PostConfigure(options => StoragePathsOptions.NormalizePaths(options));
```

**Usage Scenarios:**

```csharp
// Path normalization - ensure trailing slashes
[OptionsBinding("Storage", PostConfigure = nameof(NormalizePaths))]
public partial class StoragePathsOptions
{
    public string BasePath { get; set; } = string.Empty;
    public string CachePath { get; set; } = string.Empty;

    private static void NormalizePaths(StoragePathsOptions options)
    {
        options.BasePath = EnsureTrailingSlash(options.BasePath);
        options.CachePath = EnsureTrailingSlash(options.CachePath);
    }

    private static string EnsureTrailingSlash(string path)
        => string.IsNullOrWhiteSpace(path) || path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}

// URL normalization - lowercase and remove trailing slashes
[OptionsBinding("ExternalApi", PostConfigure = nameof(NormalizeUrls))]
public partial class ExternalApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;

    private static void NormalizeUrls(ExternalApiOptions options)
    {
        options.BaseUrl = NormalizeUrl(options.BaseUrl);
        options.CallbackUrl = NormalizeUrl(options.CallbackUrl);
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        // Lowercase and remove trailing slash
        return url.ToLowerInvariant().TrimEnd('/');
    }
}

// Combined with validation
[OptionsBinding("Database",
    ValidateDataAnnotations = true,
    ValidateOnStart = true,
    PostConfigure = nameof(ApplyDefaults))]
public partial class DatabaseOptions
{
    [Required] public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; }

    private static void ApplyDefaults(DatabaseOptions options)
    {
        // Apply default timeout if not set
        if (options.CommandTimeout <= 0)
        {
            options.CommandTimeout = 30;
        }
    }
}
```

**Validation Errors:**

The generator performs compile-time validation of PostConfigure callbacks:

- **ATCOPT008**: PostConfigure callback not supported with named options
  ```csharp
  // Error: Named options don't support PostConfigure
  [OptionsBinding("Email", Name = "Primary", PostConfigure = nameof(Normalize))]
  public partial class EmailOptions { }
  ```

- **ATCOPT009**: PostConfigure callback method not found
  ```csharp
  // Error: Method 'ApplyDefaults' does not exist
  [OptionsBinding("Settings", PostConfigure = "ApplyDefaults")]
  public partial class Settings { }
  ```

- **ATCOPT010**: PostConfigure callback method has invalid signature
  ```csharp
  // Error: Must be static void with (TOptions) parameter
  [OptionsBinding("Settings", PostConfigure = nameof(Configure))]
  public partial class Settings
  {
      private void Configure() { }  // Wrong: not static, missing parameter
  }
  ```

**Important Notes:**

- PostConfigure runs **after** binding and validation
- Callback method can be `internal` or `public` (not `private`)
- Cannot be combined with named options (use manual `.PostConfigure()` if needed)
- Perfect for normalizing user input, applying business rules, or computed properties
- Order of execution: Bind → Validate → PostConfigure

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

### 📂 Nested Configuration (Feature #6: Bind Configuration Subsections to Properties)

The generator automatically handles nested configuration subsections through Microsoft's `.Bind()` method. Complex properties are automatically bound to their corresponding configuration subsections.

#### 🎯 How It Works

When you have properties that are complex types (not primitives like string, int, etc.), the configuration binder automatically:
1. Detects the property is a complex type
2. Looks for a subsection with the same name
3. Recursively binds that subsection to the property

This works for:
- **Nested objects** - Properties with custom class types
- **Collections** - List<T>, IEnumerable<T>, arrays
- **Dictionaries** - Dictionary<string, string>, Dictionary<string, T>
- **Multiple levels** - Deeply nested structures (e.g., CloudStorage → Azure → Blob)

#### 📋 Example 1: Simple Nested Objects

```csharp
[OptionsBinding("Email")]
public partial class EmailOptions
{
    public string From { get; set; } = string.Empty;

    // Automatically binds to "Email:Smtp" subsection
    public SmtpSettings Smtp { get; set; } = new();
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
}
```

```json
{
  "Email": {
    "From": "noreply@example.com",
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "UseSsl": true
    }
  }
}
```

#### 📋 Example 2: Deeply Nested Objects (3 Levels)

```csharp
[OptionsBinding("Storage", ValidateDataAnnotations = true)]
public partial class StorageOptions
{
    // Automatically binds to "Storage:Database" subsection
    public DatabaseSettings Database { get; set; } = new();
}

public class DatabaseSettings
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int MaxConnections { get; set; } = 100;

    // Automatically binds to "Storage:Database:Retry" subsection (3 levels deep!)
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

```json
{
  "Storage": {
    "Database": {
      "ConnectionString": "Server=localhost;Database=PetStoreDb;",
      "MaxConnections": 100,
      "Retry": {
        "MaxAttempts": 3,
        "DelayMilliseconds": 500
      }
    }
  }
}
```

#### 📋 Example 3: Real-World Scenario (Cloud Storage)

```csharp
[OptionsBinding("CloudStorage", ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class CloudStorageOptions
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    // Binds to "CloudStorage:Azure"
    public AzureStorageSettings Azure { get; set; } = new();

    // Binds to "CloudStorage:Aws"
    public AwsS3Settings Aws { get; set; } = new();

    // Binds to "CloudStorage:RetryPolicy"
    public RetryPolicy RetryPolicy { get; set; } = new();
}

public class AzureStorageSettings
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = string.Empty;

    // Binds to "CloudStorage:Azure:Blob" (deeply nested!)
    public BlobSettings Blob { get; set; } = new();
}

public class BlobSettings
{
    public int MaxBlockSize { get; set; } = 4194304; // 4 MB
    public int ParallelOperations { get; set; } = 8;
}
```

```json
{
  "CloudStorage": {
    "Provider": "Azure",
    "Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=myaccount;",
      "ContainerName": "my-container",
      "Blob": {
        "MaxBlockSize": 4194304,
        "ParallelOperations": 8
      }
    },
    "Aws": {
      "AccessKey": "AKIAIOSFODNN7EXAMPLE",
      "SecretKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
      "Region": "us-west-2",
      "BucketName": "my-bucket"
    },
    "RetryPolicy": {
      "MaxRetries": 3,
      "DelayMilliseconds": 1000,
      "UseExponentialBackoff": true
    }
  }
}
```

#### 🎯 Key Points

- **Zero extra configuration** - Just declare properties with complex types
- **Automatic path construction** - "Parent:Child:GrandChild" paths are built automatically
- **Works with validation** - DataAnnotations validation applies to all nested levels
- **Unlimited depth** - Support for deeply nested structures
- **Collections supported** - List<T>, arrays, dictionaries all work automatically

#### 📍 Explicit Nested Paths

You can also explicitly specify the full nested path in the attribute:

```csharp
[OptionsBinding("App:Services:Email:Smtp")]
public partial class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
}
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

### 📛 Named Options (Multiple Configurations)

**Named Options** allow you to have multiple configurations of the same options type with different names. This is useful when you need different configurations for the same logical service (e.g., Primary/Secondary email servers, Production/Staging databases).

#### ✨ Use Cases

- **🔄 Fallback Servers**: Primary, Secondary, and Fallback email/database servers
- **🌍 Multi-Region**: Different API endpoints for different regions (US, EU, Asia)
- **🎯 Multi-Tenant**: Tenant-specific configurations
- **🔧 Environment Tiers**: Production, Staging, Development endpoints

#### 🎯 Basic Example

**Define options with multiple named instances:**

```csharp
[OptionsBinding("Email:Primary", Name = "Primary")]
[OptionsBinding("Email:Secondary", Name = "Secondary")]
[OptionsBinding("Email:Fallback", Name = "Fallback")]
public partial class EmailOptions
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
}
```

**Configure appsettings.json:**

```json
{
  "Email": {
    "Primary": {
      "SmtpServer": "smtp.primary.example.com",
      "Port": 587,
      "UseSsl": true,
      "FromAddress": "noreply@primary.example.com"
    },
    "Secondary": {
      "SmtpServer": "smtp.secondary.example.com",
      "Port": 587,
      "UseSsl": true,
      "FromAddress": "noreply@secondary.example.com"
    },
    "Fallback": {
      "SmtpServer": "smtp.fallback.example.com",
      "Port": 25,
      "UseSsl": false,
      "FromAddress": "noreply@fallback.example.com"
    }
  }
}
```

**Access named options using IOptionsSnapshot:**

```csharp
public class EmailService
{
    private readonly IOptionsSnapshot<EmailOptions> _emailOptionsSnapshot;

    public EmailService(IOptionsSnapshot<EmailOptions> emailOptionsSnapshot)
    {
        _emailOptionsSnapshot = emailOptionsSnapshot;
    }

    public async Task SendAsync(string to, string body)
    {
        // Try primary first
        var primaryOptions = _emailOptionsSnapshot.Get("Primary");
        if (await TrySendAsync(primaryOptions, to, body))
            return;

        // Fallback to secondary
        var secondaryOptions = _emailOptionsSnapshot.Get("Secondary");
        if (await TrySendAsync(secondaryOptions, to, body))
            return;

        // Last resort: fallback server
        var fallbackOptions = _emailOptionsSnapshot.Get("Fallback");
        await TrySendAsync(fallbackOptions, to, body);
    }
}
```

#### 🔧 Generated Code

```csharp
// Generated registration methods
services.Configure<EmailOptions>("Primary", configuration.GetSection("Email:Primary"));
services.Configure<EmailOptions>("Secondary", configuration.GetSection("Email:Secondary"));
services.Configure<EmailOptions>("Fallback", configuration.GetSection("Email:Fallback"));
```

#### ⚠️ Important Notes

- **📝 Use `IOptionsSnapshot<T>`**: Named options are accessed via `IOptionsSnapshot<T>.Get(name)`, not `IOptions<T>.Value`
- **🚫 No Validation Chain**: Named options use the simpler `Configure<T>(name, section)` pattern without validation support
- **🔄 AllowMultiple**: The `[OptionsBinding]` attribute supports `AllowMultiple = true` to enable multiple configurations

#### 🎯 Mixing Named and Unnamed Options

You can have both named and unnamed options on the same class:

```csharp
// Default unnamed instance
[OptionsBinding("Email")]

// Named instances for specific use cases
[OptionsBinding("Email:Backup", Name = "Backup")]
public partial class EmailOptions
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
}
```

```csharp
// Access default (unnamed) instance
var defaultEmail = serviceProvider.GetRequiredService<IOptions<EmailOptions>>();

// Access named instances
var emailSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<EmailOptions>>();
var backupEmail = emailSnapshot.Get("Backup");
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

### ❌ ATCOPT004-007: OnChange Callback Diagnostics

See [Configuration Change Callbacks](#-configuration-change-callbacks) section for details.

### ❌ ATCOPT008-010: PostConfigure Callback Diagnostics

See [Post-Configuration Support](#-post-configuration-support) section for details.

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
