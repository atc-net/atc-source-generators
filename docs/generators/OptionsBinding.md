# ⚙️ Options Binding Source Generator

Automatically bind configuration sections to strongly-typed options classes with compile-time code generation.

## 📑 Table of Contents

- [📖 Overview](#-overview)
- [🚀 Quick Start](#-quick-start)
  - [1️⃣ Install the Package](#️-1-install-the-package)
  - [2️⃣ Create Your Options Class](#️-2-create-your-options-class)
  - [3️⃣ Configure Your appsettings.json](#️-3-configure-your-appsettingsjson)
  - [4️⃣ Register Options in Program.cs](#️-4-register-options-in-programcs)
- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [💡 Usage](#-usage)
  - [🔰 Basic Options Binding](#-basic-options-binding)
  - [📍 Explicit Section Names](#-explicit-section-names)
  - [✅ Validation](#-validation)
  - [⏱️ Options Lifetimes](#️-options-lifetimes)
- [🔧 How It Works](#-how-it-works)
  - [1️⃣ Attribute Detection](#️-attribute-detection)
  - [2️⃣ Section Name Resolution](#️-section-name-resolution)
  - [3️⃣ Code Generation](#️-code-generation)
  - [4️⃣ Compile-Time Safety](#️-compile-time-safety)
- [🎯 Advanced Scenarios](#-advanced-scenarios)
  - [🏢 Multiple Assemblies](#-multiple-assemblies)
  - [✨ Smart Naming](#-smart-naming)
  - [📂 Nested Configuration](#-nested-configuration)
  - [🌍 Environment-Specific Configuration](#-environment-specific-configuration)
- [🛡️ Diagnostics](#️-diagnostics)
  - [❌ ATCOPT001: Options class must be partial](#-atcopt001-options-class-must-be-partial)
  - [❌ ATCOPT002: Section name cannot be null or empty](#-atcopt002-section-name-cannot-be-null-or-empty)
  - [⚠️ ATCOPT003: Invalid options binding configuration](#️-atcopt003-invalid-options-binding-configuration)
  - [❌ ATCOPT003: Const section name cannot be null or empty](#-ATCOPT003-const-section-name-cannot-be-null-or-empty)
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

## ✨ Features

### ✨ Automatic Section Name Inference

The generator resolves section names using the following priority:

1. **Explicit section name** in the attribute constructor
2. **`public const string SectionName`** in the options class
3. **`public const string NameTitle`** in the options class
4. **`public const string Name`** in the options class
5. **Auto-inferred** from class name (uses full class name)

**Examples:**

```csharp
// Auto-inference (uses full class name)
[OptionsBinding]
public partial class DatabaseOptions { }  // Section: "DatabaseOptions"

[OptionsBinding]
public partial class ApiSettings { }       // Section: "ApiSettings"

[OptionsBinding]
public partial class LoggingConfig { }     // Section: "LoggingConfig"

// Using const SectionName (2nd highest priority)
[OptionsBinding(ValidateDataAnnotations = true)]
public partial class DatabaseOptions
{
    public const string SectionName = "CustomDatabase";
    // Section: "CustomDatabase"
}

// Using const NameTitle
[OptionsBinding]
public partial class CacheOptions
{
    public const string NameTitle = "ApplicationCache";
    // Section: "ApplicationCache"
}

// Using const Name
[OptionsBinding]
public partial class EmailOptions
{
    public const string Name = "EmailConfiguration";
    // Section: "EmailConfiguration"
}

// Full priority demonstration
[OptionsBinding]
public partial class LoggingOptions
{
    public const string SectionName = "X1";  // 2nd priority - WINS
    public const string NameTitle = "X2";    // 3rd priority
    public const string Name = "X3";         // 4th priority
    // Section: "X1"
}

// Explicit section name (highest priority)
[OptionsBinding("App:Database")]
public partial class ServiceOptions
{
    public const string SectionName = "Service";  // Ignored
    // Section: "App:Database"
}
```

### 🔒 Built-in Validation

Enable data annotations validation with a single property:

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

### 🎯 Explicit Section Paths

Specify complex configuration paths:

```csharp
[OptionsBinding("App:Services:Database")]
public partial class DatabaseOptions { }
```

```json
{
  "App": {
    "Services": {
      "Database": {
        "ConnectionString": "..."
      }
    }
  }
}
```

### 📦 Multiple Options Classes

Bind multiple configuration sections in a single call:

```csharp
[OptionsBinding("Database")]
public partial class DatabaseOptions { }

[OptionsBinding("Api")]
public partial class ApiOptions { }

[OptionsBinding("Logging")]
public partial class LoggingOptions { }

// In Program.cs - all registered at once
services.AddOptionsFromApp(configuration);
```

### 📦 Multi-Project Support

Just like `DependencyRegistrationGenerator`, each project generates its own `AddOptionsFromXXX()` method:

```csharp
// Domain project - options for business logic layer
// Atc.SourceGenerators.OptionsBinding.Domain
[OptionsBinding("Email")]
public partial class EmailOptions { }

[OptionsBinding] // Section: "CacheOptions" (auto-inferred)
public partial class CacheOptions { }

// Main project - options for application layer
// Atc.SourceGenerators.OptionsBinding
[OptionsBinding("Database")]
public partial class DatabaseOptions { }

// Program.cs - Register options from both projects
services.AddOptionsFromDomain(configuration);
services.AddOptionsFromOptionsBinding(configuration);
```

**Method Naming with Smart Suffixes:** The generator creates methods with **smart naming** - using short suffixes when unique, full names when there are conflicts. For example:
- `PetStore.Domain` (unique suffix) → `AddOptionsFromDomain()`
- `PetStore.Domain` + `AnotherApp.Domain` (conflicting) → `AddOptionsFromPetStoreDomain()` and `AddOptionsFromAnotherAppDomain()`

See [✨ Smart Naming](#-smart-naming) for details.

### 🔗 Transitive Options Registration

The generator supports automatic registration of options from referenced assemblies, making multi-project setups seamless. Each assembly generates **4 overloads** to support different scenarios:

```csharp
// Overload 1: Register only this assembly's options
services.AddOptionsFromApp(configuration);

// Overload 2: Auto-detect ALL referenced assemblies recursively
services.AddOptionsFromApp(configuration, includeReferencedAssemblies: true);

// Overload 3: Register specific referenced assembly (short or full name)
services.AddOptionsFromApp(configuration, "Domain");
services.AddOptionsFromApp(configuration, "MyApp.Domain");

// Overload 4: Register multiple specific assemblies
services.AddOptionsFromApp(configuration, "Domain", "DataAccess", "Infrastructure");
```

#### **Scenario A: Manual Registration (Explicit Control)**

Manually register options from each project:

```csharp
// Register options from main project
services.AddOptionsFromApp(configuration);

// Register options from Domain project
services.AddOptionsFromAppDomain(configuration);

// Register options from DataAccess project
services.AddOptionsFromAppDataAccess(configuration);
```

**When to use:** When you want explicit control over which projects' options are registered.

#### **Scenario B: Transitive Registration (Automatic Discovery)**

Let the generator automatically discover and register options from referenced assemblies:

```csharp
// ✨ Single call registers ALL options from referenced assemblies
services.AddOptionsFromApp(configuration, includeReferencedAssemblies: true);
```

**How it works:**
1. Generator scans all referenced assemblies with matching prefix (e.g., `MyApp.*`)
2. Detects which assemblies contain `[OptionsBinding]` attributes
3. Generates calls to register options from those assemblies
4. Works **recursively** - handles multi-level dependencies (Api → Domain → DataAccess)

**Example Architecture:**
```
MyApp.Api (web project)
  ↓ references
MyApp.Domain (business logic)
  ↓ references
MyApp.DataAccess (database access)
```

**In MyApp.Api Program.cs:**
```csharp
// One call registers options from Api, Domain, AND DataAccess
services.AddOptionsFromAppApi(configuration, includeReferencedAssemblies: true);
```

**Generated code includes:**
```csharp
// From MyApp.Api assembly
public static IServiceCollection AddOptionsFromAppApi(
    this IServiceCollection services,
    IConfiguration configuration,
    bool includeReferencedAssemblies)
{
    services.AddOptionsFromAppApi(configuration);

    if (includeReferencedAssemblies)
    {
        // Auto-detected referenced assemblies with [OptionsBinding]
        AddOptionsFromAppDomain(services, configuration, includeReferencedAssemblies: true);
        AddOptionsFromAppDataAccess(services, configuration, includeReferencedAssemblies: true);
    }

    return services;
}
```

#### **All Available Overloads:**

```csharp
// Overload 1: Default (no transitive registration)
services.AddOptionsFromYourProject(configuration);

// Overload 2: Auto-detect ALL referenced assemblies recursively
services.AddOptionsFromYourProject(configuration, includeReferencedAssemblies: true);

// Overload 3: Register specific referenced assembly (short or full name)
services.AddOptionsFromYourProject(configuration, "Domain");
services.AddOptionsFromYourProject(configuration, "MyApp.Domain");

// Overload 4: Register multiple specific assemblies
services.AddOptionsFromYourProject(configuration, "Domain", "DataAccess", "Infrastructure");
```

**Benefits:**
- ✅ **Clean Architecture:** Main project doesn't need to reference all downstream projects
- ✅ **Zero Boilerplate:** No manual registration of each project's options
- ✅ **Type Safe:** All registration happens at compile time
- ✅ **Recursive:** Automatically handles deep dependency chains
- ✅ **Flexible:** Choose between manual, automatic, or selective registration

### 🚀 Native AOT Compatible

Fully compatible with Native AOT compilation - no reflection or runtime code generation:

```csharp
// All binding code is generated at compile time
[OptionsBinding("Database")]
public partial class DatabaseOptions { }

// Works seamlessly with Native AOT
// ✅ No reflection required
// ✅ Fully trimming-safe
// ✅ All dependencies resolved at compile time
```

**Why this matters:**
- **Faster startup**: No runtime reflection or code generation overhead
- **Smaller deployments**: Trimming removes unused code
- **Better performance**: Native code execution
- **Modern .NET ready**: Full support for Native AOT scenarios

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
