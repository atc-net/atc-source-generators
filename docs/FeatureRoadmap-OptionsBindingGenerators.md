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
- **Lifetime selection** - Singleton (`IOptions`), Scoped (`IOptionsSnapshot`), Monitor (`IOptionsMonitor`)
- **Multi-project support** - Assembly-specific extension methods with smart naming
- **Transitive registration** - 4 overloads for automatic/selective assembly registration
- **Partial class requirement** - Enforced at compile time
- **Native AOT compatible** - Zero reflection, compile-time generation
- **Compile-time diagnostics** - Validate partial class, section names

---

## 🎯 Need to Have (High Priority)

These features address common pain points and align with Microsoft's Options pattern best practices.

### 1. Custom Validation Support (IValidateOptions)

**Priority**: 🔴 **High**
**Status**: ❌ Not Implemented
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

- Detect classes implementing `IValidateOptions<T>` in the same assembly
- Auto-register validators when corresponding options class has `[OptionsBinding]`
- Support multiple validators for same options type
- Consider adding `Validator = typeof(ConnectionPoolOptionsValidator)` parameter to `[OptionsBinding]`

---

### 2. Named Options Support

**Priority**: 🔴 **High**
**Status**: ❌ Not Implemented
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

**Implementation Notes**:

- Allow multiple `[OptionsBinding]` attributes on same class
- Add `Name` parameter to distinguish named instances
- Use `Configure<T>(string name, ...)` for named options
- Generate helper properties/methods for easy access

---

### 3. Post-Configuration Support

**Priority**: 🟡 **Medium-High**
**Status**: ❌ Not Implemented
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

**Implementation Notes**:

- Add `PostConfigure` parameter pointing to static method
- Method signature: `static void Configure(TOptions options)`
- Runs after binding and validation
- Useful for normalization, defaults, computed properties

---

### 4. Error on Missing Configuration Keys

**Priority**: 🔴 **High** ⭐ *Highly requested in GitHub issues*
**Status**: ❌ Not Implemented
**Inspiration**: [GitHub Issue #36015](https://github.com/dotnet/runtime/issues/36015)

**Description**: Throw exceptions when required configuration keys are missing instead of silently setting properties to null/default.

**User Story**:
> "As a developer, I want my application to fail at startup if critical configuration like database connection strings is missing, rather than failing in production with NullReferenceException."

**Example**:

```csharp
[OptionsBinding("Database", ErrorOnMissingKeys = true, ValidateOnStart = true)]
public partial class DatabaseOptions
{
    // If "Database:ConnectionString" is missing in appsettings.json,
    // throw exception at startup instead of silently setting to null
    public string ConnectionString { get; set; } = string.Empty;

    public int MaxRetries { get; set; } = 5;
}

// Generated code with error checking:
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .Validate(options =>
    {
        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            throw new OptionsValidationException(
                nameof(DatabaseOptions),
                typeof(DatabaseOptions),
                new[] { "ConnectionString is required but was not found in configuration" });
        }
        return true;
    })
    .ValidateOnStart();
```

**Implementation Notes**:

- Add `ErrorOnMissingKeys` boolean parameter
- Generate validation delegate that checks for null/default values
- Combine with `ValidateOnStart = true` for startup failure
- Consider making this opt-in per-property with attribute: `[Required]` from DataAnnotations

---

### 5. Configuration Change Callbacks

**Priority**: 🟡 **Medium**
**Status**: ❌ Not Implemented
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
    private static void OnFeaturesChanged(FeaturesOptions options, string? name)
    {
        Console.WriteLine($"Features configuration changed: EnableNewUI={options.EnableNewUI}");
        // Clear caches, notify components, etc.
    }
}

// Generated code:
var monitor = services.BuildServiceProvider().GetRequiredService<IOptionsMonitor<FeaturesOptions>>();
monitor.OnChange((options, name) => FeaturesOptions.OnFeaturesChanged(options, name));
```

**Implementation Notes**:

- Only applicable when `Lifetime = OptionsLifetime.Monitor`
- Callback signature: `static void OnChange(TOptions options, string? name)`
- Useful for feature flags, dynamic configuration
- **Limitation**: Only works with file-based configuration providers (appsettings.json)

---

### 6. Bind Configuration Subsections to Properties

**Priority**: 🟡 **Medium**
**Status**: ❌ Not Implemented

**Description**: Support binding nested configuration sections to complex property types.

**User Story**:
> "As a developer, I want to bind nested configuration sections to nested properties without manually creating separate options classes."

**Example**:

```csharp
// appsettings.json
{
  "Email": {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UseSsl": true
    },
    "From": "noreply@example.com",
    "Templates": {
      "Welcome": "welcome.html",
      "ResetPassword": "reset.html"
    }
  }
}

[OptionsBinding("Email")]
public partial class EmailOptions
{
    public string From { get; set; } = string.Empty;

    // Nested object - should automatically bind "Email:Smtp" section
    public SmtpSettings Smtp { get; set; } = new();

    // Nested object - should automatically bind "Email:Templates" section
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

**Implementation Notes**:

- Automatically bind complex properties using `Bind()`
- No special attribute required for nested types
- Already supported by Microsoft.Extensions.Configuration.Binder
- Our generator should leverage this automatically

---

### 7. ConfigureAll Support

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Support configuring all named instances of an options type at once (e.g., setting defaults).

**Example**:

```csharp
// Configure defaults for ALL named DatabaseOptions instances
services.ConfigureAll<DatabaseOptions>(options =>
{
    options.MaxRetries = 3;  // Default for all instances
    options.CommandTimeout = TimeSpan.FromSeconds(30);
});

// Named instances override specific values
services.Configure<DatabaseOptions>("Primary", config.GetSection("Databases:Primary"));
```

**Implementation Notes**:

- Generate `ConfigureAll<T>()` call when multiple named instances exist
- Useful for setting defaults across all instances

---

## 💡 Nice to Have (Medium Priority)

These features would improve usability but are not critical.

### 8. Options Snapshots for Specific Sections

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Support binding multiple sections dynamically at runtime using `IOptionsSnapshot`.

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

### 10. Auto-Generate Options Classes from appsettings.json

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

### 11. Environment-Specific Validation

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

### 12. Hot Reload Support with Filtering

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

### 13. Reflection-Based Binding

**Reason**: Defeats the purpose of compile-time source generation and breaks AOT compatibility.

**Status**: ❌ Out of Scope

---

### 14. JSON Schema Generation

**Reason**: Out of scope for options binding. Use dedicated tools like NJsonSchema.

**Status**: ❌ Not Planned

---

### 15. Configuration Encryption/Decryption

**Reason**: Security concern handled by configuration providers (Azure Key Vault, AWS Secrets Manager, etc.), not binding layer.

**Status**: ❌ Out of Scope

---

### 16. Dynamic Configuration Sources

**Reason**: Configuration providers handle this. Options binding focuses on type-safe access.

**Status**: ❌ Out of Scope

---

## 📅 Proposed Implementation Order

Based on priority, user demand, and implementation complexity:

### Phase 1: Validation & Error Handling (v1.1 - Q1 2025)

**Goal**: Fail-fast and better validation

1. **Error on Missing Configuration Keys** 🔴 High ⭐ - Startup failures instead of silent nulls
2. **Custom Validation Support (IValidateOptions)** 🔴 High - Complex validation beyond DataAnnotations
3. **Post-Configuration Support** 🟡 Medium-High - Normalization and defaults

**Estimated effort**: 3-4 weeks
**Impact**: Prevent production misconfigurations, better developer experience

---

### Phase 2: Advanced Scenarios (v1.2 - Q2 2025)

**Goal**: Multi-tenant and dynamic configuration

4. **Named Options Support** 🔴 High - Multiple configurations for same type
5. **Configuration Change Callbacks** 🟡 Medium - React to runtime changes
6. **ConfigureAll Support** 🟢 Low-Medium - Set defaults across named instances

**Estimated effort**: 4-5 weeks
**Impact**: Multi-tenant scenarios, feature flags, runtime configuration

---

### Phase 3: Developer Experience (v1.3 - Q3 2025)

**Goal**: Better diagnostics and usability

7. **Compile-Time Section Name Validation** 🟡 Medium - Validate section paths exist
8. **Bind Configuration Subsections** 🟡 Medium - Nested object binding (may already work)
9. **Environment-Specific Validation** 🟢 Low - Production vs. development validation

**Estimated effort**: 3-4 weeks
**Impact**: Catch configuration errors earlier, better IDE support

---

### Phase 4: Optional Enhancements (v2.0+ - 2025-2026)

**Goal**: Nice-to-have features based on feedback

10. **Hot Reload with Filtering** 🟢 Low - Fine-grained reload control
11. **Auto-Generate Options from JSON** 🟢 Low - Reverse generation (experimental)
12. **Options Snapshots for Sections** 🟢 Low-Medium - Dynamic section binding

**Estimated effort**: Variable
**Impact**: Polish and edge cases

---

### Feature Prioritization Matrix

| Feature | Priority | User Demand | Complexity | Phase |
|---------|----------|-------------|------------|-------|
| Error on Missing Keys | 🔴 High | ⭐⭐⭐ | Medium | 1.1 |
| Custom Validation (IValidateOptions) | 🔴 High | ⭐⭐⭐ | Medium | 1.1 |
| Post-Configuration | 🟡 Med-High | ⭐⭐ | Low | 1.1 |
| Named Options | 🔴 High | ⭐⭐⭐ | Medium | 1.2 |
| Change Callbacks | 🟡 Medium | ⭐⭐ | Medium | 1.2 |
| ConfigureAll | 🟢 Low-Med | ⭐ | Low | 1.2 |
| Section Path Validation | 🟡 Medium | ⭐⭐ | High | 1.3 |
| Nested Object Binding | 🟡 Medium | ⭐⭐ | Low | 1.3 |
| Environment Validation | 🟢 Low | ⭐ | Medium | 1.3 |
| Hot Reload Filtering | 🟢 Low | ⭐ | Medium | 2.0+ |
| Auto-Generate from JSON | 🟢 Low | ⭐ | High | 2.0+ |

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

**Last Updated**: 2025-01-17
**Version**: 1.0
**Research Date**: January 2025 (.NET 8/9 Options Pattern)
**Maintained By**: Atc.SourceGenerators Team
