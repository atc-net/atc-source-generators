# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Roslyn C# Source Generators** project that provides compile-time code generation for .NET applications. The solution contains three main source generators:

1. **DependencyRegistrationGenerator** - Automatically generates dependency injection service registrations
2. **OptionsBindingGenerator** - Automatically generates configuration options binding code
3. **MappingGenerator** - Automatically generates type-safe object-to-object mapping code

All generators eliminate boilerplate code and improve developer productivity while maintaining Native AOT compatibility.

## Project Structure

```
src/
  Atc.SourceGenerators/           # Main generator implementations
    DependencyRegistrationGenerator.cs
    OptionsBindingGenerator.cs
    ObjectMappingGenerator.cs
    RuleIdentifierConstants.cs    # Diagnostic ID constants
    RuleCategoryConstants.cs       # Diagnostic category constants
  Atc.SourceGenerators.Annotations/ # Shared attribute definitions (published as separate package)
    RegistrationAttribute.cs
    OptionsBindingAttribute.cs
    MapToAttribute.cs
    Lifetime.cs
    OptionsLifetime.cs

test/
  Atc.SourceGenerators.Tests/     # Unit tests using Roslyn testing infrastructure
    DependencyRegistrationGeneratorTests.cs
    OptionsBindingGeneratorTests.cs
    ObjectMappingGeneratorTests.cs

sample/
  Atc.SourceGenerators.DependencyRegistration/        # DI registration sample
  Atc.SourceGenerators.DependencyRegistration.Domain/ # Multi-project DI sample
  Atc.SourceGenerators.OptionsBinding/                # Options binding sample
  Atc.SourceGenerators.OptionsBinding.Domain/         # Multi-project options sample
  Atc.SourceGenerators.Mapping/                       # Object mapping API sample
  Atc.SourceGenerators.Mapping.Domain/                # Domain models with mappings (includes BaseEntity/AuditableEntity/Book for inheritance demo)
  Atc.SourceGenerators.Mapping.DataAccess/            # Database entities with mappings
  PetStore.Api/                                       # Complete 3-layer ASP.NET Core API with OpenAPI/Scalar
  PetStore.Api.Contract/                              # API contracts (DTOs)
  PetStore.Domain/                                    # Domain layer using all generators
  PetStore.DataAccess/                                # Data access layer with repositories
```

## Build and Test Commands

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~Generator_Should_Auto_Detect_Multiple_Interfaces"
```

### Run Sample Projects
```bash
# DependencyRegistration sample
dotnet run --project sample/Atc.SourceGenerators.DependencyRegistration

# OptionsBinding sample
dotnet run --project sample/Atc.SourceGenerators.OptionsBinding

# Mapping sample (minimal API with 3-layer architecture)
dotnet run --project sample/Atc.SourceGenerators.Mapping

# PetStore API (complete 3-layer app with all generators + OpenAPI/Scalar)
dotnet run --project sample/PetStore.Api
# Open browser to https://localhost:42616/scalar/v1 for API documentation
```

### Clean Build Artifacts
```bash
dotnet clean
```

## Architecture

### Source Generator Lifecycle

Both generators follow the **Incremental Generator** pattern (IIncrementalGenerator):

1. **PostInitialization** - Generate attribute definitions as fallback (for projects that don't reference Atc.SourceGenerators.Annotations)
2. **Syntax Provider** - Filter candidate classes with attributes (predicate + transform)
3. **Compilation Combination** - Combine filtered classes with compilation context
4. **Source Output** - Validate, analyze, and generate extension methods

### DependencyRegistrationGenerator

**Key Features:**
- Auto-detects all implemented interfaces (excluding System.* and Microsoft.* namespaces)
- **Generic interface registration** - Full support for open generic types like `IRepository<T>` and `IHandler<TRequest, TResponse>`
- **Keyed service registration** - Multiple implementations of the same interface with different keys (.NET 8+)
- **Factory method registration** - Custom initialization logic via static factory methods
- **Instance registration** - Register pre-created singleton instances via static fields, properties, or methods
- **TryAdd registration** - Conditional registration for default implementations (library pattern)
- **Decorator pattern support** - Wrap services with cross-cutting concerns (logging, caching, validation) using `Decorator = true`
- **Conditional registration** - Register services based on configuration values (feature flags, environment-specific services)
- **Assembly scanning filters** - Exclude types by namespace, pattern (wildcards), or interface implementation
- **Runtime filtering** - Exclude services when calling registration methods via optional parameters (different apps, different service subsets)
- Supports explicit `As` parameter to override auto-detection
- Generates `AddDependencyRegistrationsFrom{SmartSuffix}()` extension methods with 4 overloads
- **Smart naming** - uses short suffix if unique, full name if conflicts exist
- **Transitive dependency registration** - automatically registers services from referenced assemblies
- **Hosted service detection** - automatically uses `AddHostedService<T>()` for `BackgroundService` or `IHostedService` implementations
- Default lifetime: Singleton (can specify Scoped or Transient)

**Generated Code Pattern:**
```csharp
// Input: [Registration] public class UserService : IUserService { }
// Output: services.AddSingleton<IUserService, UserService>();

// Generic Input: [Registration(Lifetime.Scoped)] public class Repository<T> : IRepository<T> where T : class { }
// Generic Output: services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Keyed Input: [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Stripe")]
// Keyed Output: services.AddKeyedScoped<IPaymentProcessor, StripePaymentProcessor>("Stripe");

// Factory Input: [Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(Create))]
//                public static IEmailSender Create(IServiceProvider sp) => new EmailSender();
// Factory Output: services.AddScoped<IEmailSender>(sp => EmailSender.Create(sp));

// Instance Input: [Registration(As = typeof(IAppConfiguration), Instance = nameof(DefaultInstance))]
//                 public static readonly AppConfiguration DefaultInstance = new();
// Instance Output: services.AddSingleton<IAppConfiguration>(AppConfiguration.DefaultInstance);

// TryAdd Input: [Registration(As = typeof(ILogger), TryAdd = true)]
// TryAdd Output: services.TryAddSingleton<ILogger, DefaultLogger>();

// Hosted Service Input: [Registration] public class MaintenanceService : BackgroundService { }
// Hosted Service Output: services.AddHostedService<MaintenanceService>();

// Decorator Input: [Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
//                   public class LoggingOrderServiceDecorator : IOrderService { }
// Decorator Output: services.Decorate<IOrderService>((provider, inner) =>
//                       ActivatorUtilities.CreateInstance<LoggingOrderServiceDecorator>(provider, inner));

// Conditional Input: [Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
//                    public class RedisCache : ICache { }
// Conditional Output: if (configuration.GetValue<bool>("Features:UseRedisCache"))
//                    {
//                        services.AddSingleton<ICache, RedisCache>();
//                    }

// Negated Conditional: [Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
//                      public class MemoryCache : ICache { }
// Negated Output: if (!configuration.GetValue<bool>("Features:UseRedisCache"))
//                {
//                    services.AddSingleton<ICache, MemoryCache>();
//                }
```

**Smart Naming:**
```csharp
// If "Domain" suffix is unique in the compilation context:
PetStore.Domain → AddDependencyRegistrationsFromDomain()

// If multiple assemblies have "Domain" suffix:
PetStore.Domain + AnotherApp.Domain → AddDependencyRegistrationsFromPetStoreDomain()
```

**Conditional Registration Configuration:**
When an assembly contains services with `Condition` parameter, an `IConfiguration` parameter is added to all generated extension method signatures:

```csharp
// Without conditional services:
services.AddDependencyRegistrationsFromDomain();

// With conditional services (IConfiguration required):
services.AddDependencyRegistrationsFromDomain(configuration);
services.AddDependencyRegistrationsFromDomain(configuration, includeReferencedAssemblies: true);
```

**Transitive Registration (4 Overloads):**
```csharp
// Overload 1: Default (no transitive registration)
services.AddDependencyRegistrationsFromDomain();

// Overload 2: Auto-detect ALL referenced assemblies recursively
services.AddDependencyRegistrationsFromDomain(includeReferencedAssemblies: true);

// Overload 3: Register specific referenced assembly (short or full name)
services.AddDependencyRegistrationsFromDomain("DataAccess");
services.AddDependencyRegistrationsFromDomain("MyApp.DataAccess");

// Overload 4: Register multiple specific assemblies
services.AddDependencyRegistrationsFromDomain("DataAccess", "Infrastructure");

// Note: Configuration is only passed to the calling assembly, not transitively to referenced assemblies
// Each assembly with conditional services should be called directly with configuration if needed
```

**How Transitive Registration Works:**
- **Auto-detect mode**: Scans ALL referenced assemblies for `[Registration]` attributes, recursively
- **Manual mode**: Only includes assemblies with matching prefix (e.g., "MyApp.*")
- **Prefix filtering**: When using assembly names, only same-prefix assemblies are registered
- **Silent skip**: Non-existent assemblies or assemblies without registrations are silently skipped

**Assembly Scanning Filters:**
Assembly-level filters allow excluding types from automatic registration during assembly scanning. Apply multiple `[RegistrationFilter]` attributes to exclude specific namespaces, naming patterns, or interface implementations.

```csharp
// AssemblyInfo.cs - Exclude by namespace
[assembly: RegistrationFilter(
    ExcludeNamespaces = new[] { "MyApp.Internal", "MyApp.Tests" })]

// Exclude by pattern (wildcards: * = any characters, ? = single character)
[assembly: RegistrationFilter(
    ExcludePatterns = new[] { "*Mock*", "*Test*", "*Fake*" })]

// Exclude types implementing specific interfaces
[assembly: RegistrationFilter(
    ExcludeImplementing = new[] { typeof(ITestUtility), typeof(IInternalService) })]

// Multiple filters can be combined
[assembly: RegistrationFilter(ExcludeNamespaces = new[] { "MyApp.Legacy" })]
[assembly: RegistrationFilter(ExcludePatterns = new[] { "*Deprecated*" })]
```

**How Assembly Scanning Filters Work:**
- **Namespace filtering**: Exact match or sub-namespace match (e.g., "MyApp.Internal" excludes "MyApp.Internal.Deep.Nested")
- **Pattern matching**: Case-insensitive wildcard matching on both short type name and full type name
- **Interface filtering**: Uses `SymbolEqualityComparer` for proper generic type comparison
- **Multiple filters**: All filter attributes are combined (union of all exclusions)
- **Applied globally**: Filters apply to both current assembly and referenced assemblies during transitive registration

**Runtime Filtering:**
Runtime filters allow excluding services when calling the registration methods, rather than at compile time. All generated methods support three optional filter parameters:

```csharp
// Exclude specific types
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[] { typeof(EmailService), typeof(SmsService) });

// Exclude by namespace (including sub-namespaces)
services.AddDependencyRegistrationsFromDomain(
    excludedNamespaces: new[] { "MyApp.Domain.Internal" });

// Exclude by pattern (wildcards: * and ?)
services.AddDependencyRegistrationsFromDomain(
    excludedPatterns: new[] { "*Mock*", "*Test*" });

// Combine all three
services.AddDependencyRegistrationsFromDomain(
    excludedNamespaces: new[] { "MyApp.Internal" },
    excludedPatterns: new[] { "*Test*" },
    excludedTypes: new[] { typeof(LegacyService) });

// Works with transitive registration too
services.AddDependencyRegistrationsFromDomain(
    includeReferencedAssemblies: true,
    excludedTypes: new[] { typeof(EmailService) });
```

**How Runtime Filtering Works:**
- **Applied at registration**: Filters are evaluated when services are being added to the container
- **Application-specific**: Different applications can exclude different services from the same library
- **Propagated**: Filters are automatically passed to referenced assembly calls
- **Generic type support**: Properly handles generic types using `typeof(Repository<>)` syntax
- **Complement to compile-time**: Use compile-time filters for global exclusions, runtime for application-specific

**Runtime vs Compile-Time Filtering:**
- Compile-time (assembly-level): Fixed at build time, applies to ALL registrations from that assembly
- Runtime (method parameters): Flexible per application, allows different apps to exclude different services

**Diagnostics:**
- `ATCDIR001` - Service 'As' type must be an interface (Error)
- `ATCDIR002` - Class does not implement specified interface (Error)
- `ATCDIR003` - Duplicate registration with different lifetimes (Warning)
- `ATCDIR004` - Hosted services must use Singleton lifetime (Error)
- `ATCDIR005` - Factory method not found (Error)
- `ATCDIR006` - Factory method has invalid signature (Error)
- `ATCDIR007` - Instance member not found (Error)
- `ATCDIR008` - Instance member must be static (Error)
- `ATCDIR009` - Instance and Factory are mutually exclusive (Error)
- `ATCDIR010` - Instance registration requires Singleton lifetime (Error)

### OptionsBindingGenerator

**Key Features:**
- Section name resolution priority:
  1. Explicit constructor parameter
  2. `public const string SectionName`
  3. `public const string NameTitle`
  4. `public const string Name`
  5. Auto-inferred from class name
- Supports validation: `ValidateDataAnnotations`, `ValidateOnStart`, `ErrorOnMissingKeys` (fail-fast for missing sections), Custom validators (`IValidateOptions<T>`)
- **Configuration change callbacks**: Auto-generated IHostedService for OnChange notifications with Monitor lifetime - perfect for feature flags and runtime configuration updates
- **Post-configuration support**: `PostConfigure` callbacks for normalizing/transforming values after binding (e.g., path normalization, URL lowercase)
- **ConfigureAll support**: Set common default values for all named options instances before individual binding with `ConfigureAll` callbacks (e.g., baseline retry/timeout settings)
- **Named options support**: Multiple configurations of the same options type with different names (e.g., Primary/Secondary email servers)
- **Nested subsection binding**: Automatic binding of complex properties to configuration subsections (e.g., `StorageOptions.Database.Retry` → `"Storage:Database:Retry"`) - supported out-of-the-box by Microsoft's `.Bind()` method
- Supports lifetime selection: Singleton (IOptions), Scoped (IOptionsSnapshot), Monitor (IOptionsMonitor)
- Requires classes to be declared `partial`
- **Smart naming** - uses short suffix if unique, full name if conflicts exist
- **Transitive registration**: Generates 4 overloads for each assembly to support automatic or selective registration of referenced assemblies

**Generated Code Pattern:**
```csharp
// Input: [OptionsBinding("Database")] public partial class DatabaseOptions { }
// Output:
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Input with custom validator:
[OptionsBinding("Database", ValidateDataAnnotations = true, Validator = typeof(DatabaseOptionsValidator))]
public partial class DatabaseOptions { }

// Output with custom validator:
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

// Input with ErrorOnMissingKeys (fail-fast for missing configuration):
[OptionsBinding("Database", ErrorOnMissingKeys = true, ValidateOnStart = true)]
public partial class DatabaseOptions { }

// Output with ErrorOnMissingKeys:
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
    .ValidateOnStart();

// Input with named options (multiple configurations):
[OptionsBinding("Email:Primary", Name = "Primary")]
[OptionsBinding("Email:Secondary", Name = "Secondary")]
[OptionsBinding("Email:Fallback", Name = "Fallback")]
public partial class EmailOptions { }

// Output with named options:
services.Configure<EmailOptions>("Primary", configuration.GetSection("Email:Primary"));
services.Configure<EmailOptions>("Secondary", configuration.GetSection("Email:Secondary"));
services.Configure<EmailOptions>("Fallback", configuration.GetSection("Email:Fallback"));

// Usage: Access via IOptionsSnapshot<T>.Get(name)
var emailSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<EmailOptions>>();
var primaryEmail = emailSnapshot.Get("Primary");
var secondaryEmail = emailSnapshot.Get("Secondary");

// Input with OnChange callback (requires Monitor lifetime):
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
public partial class FeaturesOptions
{
    public bool EnableNewUI { get; set; }
    public bool EnableBetaFeatures { get; set; }

    internal static void OnFeaturesChanged(FeaturesOptions options, string? name)
    {
        Console.WriteLine($"[OnChange] EnableNewUI: {options.EnableNewUI}");
        Console.WriteLine($"[OnChange] EnableBetaFeatures: {options.EnableBetaFeatures}");
    }
}

// Output with OnChange callback (auto-generated IHostedService):
// Generates internal IHostedService class:
internal sealed class FeaturesOptionsMonitorService : IHostedService, IDisposable
{
    private readonly IOptionsMonitor<FeaturesOptions> _monitor;
    private IDisposable? _changeToken;

    public FeaturesOptionsMonitorService(IOptionsMonitor<FeaturesOptions> monitor) => _monitor = monitor;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _changeToken = _monitor.OnChange(FeaturesOptions.OnFeaturesChanged);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose() => _changeToken?.Dispose();
}

// Generates registration code:
services.AddHostedService<FeaturesOptionsMonitorService>();
services.AddSingleton<IOptionsChangeTokenSource<FeaturesOptions>>(
    new ConfigurationChangeTokenSource<FeaturesOptions>(
        configuration.GetSection("Features")));
services.Configure<FeaturesOptions>(configuration.GetSection("Features"));

// Input with PostConfigure (path normalization):
[OptionsBinding("Storage", PostConfigure = nameof(NormalizePaths))]
public partial class StorageOptions
{
    public string BasePath { get; set; } = string.Empty;
    public string CachePath { get; set; } = string.Empty;

    private static void NormalizePaths(StorageOptions options)
    {
        options.BasePath = EnsureTrailingSlash(options.BasePath);
        options.CachePath = EnsureTrailingSlash(options.CachePath);
    }

    private static string EnsureTrailingSlash(string path)
        => string.IsNullOrWhiteSpace(path) || path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}

// Output with PostConfigure:
services.AddOptions<StorageOptions>()
    .Bind(configuration.GetSection("Storage"))
    .PostConfigure(options => StorageOptions.NormalizePaths(options));

// Input with ConfigureAll (set defaults for all named instances):
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
        options.MaxRetries = 3;
        options.TimeoutSeconds = 30;
        options.Port = 587;
    }
}

// Output with ConfigureAll (runs BEFORE individual configurations):
services.ConfigureAll<EmailOptions>(options => EmailOptions.SetDefaults(options));
services.Configure<EmailOptions>("Primary", configuration.GetSection("Email:Primary"));
services.Configure<EmailOptions>("Secondary", configuration.GetSection("Email:Secondary"));
services.Configure<EmailOptions>("Fallback", configuration.GetSection("Email:Fallback"));
```

**Smart Naming:**
```csharp
// If "Domain" suffix is unique in the compilation context:
PetStore.Domain → AddOptionsFromDomain()

// If multiple assemblies have "Domain" suffix:
PetStore.Domain + AnotherApp.Domain → AddOptionsFromPetStoreDomain()
```

**Transitive Registration:**
```csharp
// Overload 1: Base registration
services.AddOptionsFromDomain(configuration);

// Overload 2: Auto-detect all referenced assemblies
services.AddOptionsFromDomain(configuration, includeReferencedAssemblies: true);

// Overload 3: Register specific assembly
services.AddOptionsFromDomain(configuration, "DataAccess");

// Overload 4: Register multiple assemblies
services.AddOptionsFromDomain(configuration, "DataAccess", "Infrastructure");
```

**Diagnostics:**
- `ATCOPT001` - Options class must be partial (Error)
- `ATCOPT002` - Section name cannot be null or empty (Error)
- `ATCOPT003` - Const section name cannot be null or empty (Error)
- `ATCOPT004` - OnChange requires Monitor lifetime (Error)
- `ATCOPT005` - OnChange not supported with named options (Error)
- `ATCOPT006` - OnChange callback method not found (Error)
- `ATCOPT007` - OnChange callback has invalid signature (Error)
- `ATCOPT008` - PostConfigure not supported with named options (Error)
- `ATCOPT009` - PostConfigure callback method not found (Error)
- `ATCOPT010` - PostConfigure callback has invalid signature (Error)
- `ATCOPT011` - ConfigureAll requires multiple named options (Error)
- `ATCOPT012` - ConfigureAll callback method not found (Error)
- `ATCOPT013` - ConfigureAll callback has invalid signature (Error)

### MappingGenerator

**Key Features:**
- Automatic property-to-property mapping by name (case-insensitive)
- **Property exclusion** - Use `[MapIgnore]` attribute to exclude sensitive or internal properties from mapping (works on both source and target properties)
- **Custom property names** - Use `[MapProperty("TargetName")]` attribute to map properties with different names between source and target types
- **Constructor mapping** - Automatically detects and uses constructors when mapping to records or classes with primary constructors:
  - Prefers constructor calls over object initializers when available
  - Supports records with positional parameters (C# 9+)
  - Supports classes with primary constructors (C# 12+)
  - **Mixed initialization** - Uses constructor for required parameters and object initializer for remaining properties
  - **Case-insensitive parameter matching** - Matches property names to constructor parameter names regardless of casing
- **Smart enum conversion**:
  - Uses EnumMapping extension methods when enums have `[MapTo]` attributes (safe, with special case handling)
  - Falls back to simple casts for enums without `[MapTo]` attributes
- **Collection mapping support** - Automatically maps collections with LINQ `.Select()`:
  - Supports `List<T>`, `IList<T>`, `IEnumerable<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `T[]`
  - Generates appropriate `.ToList()`, `.ToArray()`, or collection constructor calls
  - Automatically chains element mappings (e.g., `source.Items?.Select(x => x.MapToItemDto()).ToList()!`)
- **Base class property inheritance** - Automatically includes properties from base classes:
  - Traverses entire inheritance hierarchy (Entity → AuditableEntity → ConcreteEntity)
  - Handles property overrides correctly (no duplicates)
  - Respects `[MapIgnore]` on base class properties
  - Works with all mapping features (PropertyNameStrategy, Bidirectional, etc.)
  - Perfect for entity base classes with audit fields (Id, CreatedAt, UpdatedAt, etc.)
- Nested object mapping (automatically chains mappings)
- Null safety (null checks for nullable properties)
- Multi-layer support (Entity → Domain → DTO chains)
- **Bidirectional mapping support** - Generate both forward and reverse mappings with `Bidirectional = true`
- **Record support** - Works with classes, records, and structs
- Requires types to be declared `partial`

**Generated Code Pattern (Object Initializer):**
```csharp
// Input:
[MapTo(typeof(UserDto))]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public Address? Address { get; set; }
}

// Output (extension method in Atc.Mapping namespace):
public static UserDto MapToUserDto(this User source)
{
    if (source is null)
    {
        return default!;
    }

    return new UserDto
    {
        Id = source.Id,
        Name = source.Name,
        Status = source.Status.MapToUserStatusDto(),  // ✨ Safe enum mapping (if UserStatus has [MapTo])
        // OR: Status = (UserStatusDto)source.Status,  // ⚠️ Fallback cast (if no [MapTo])
        Address = source.Address?.MapToAddress()!  // Automatic nested mapping
    };
}
```

**Generated Code Pattern (Constructor Mapping):**
```csharp
// Input - Record with constructor:
public record OrderDto(Guid Id, string CustomerName, decimal Total, DateTimeOffset OrderDate);

[MapTo(typeof(OrderDto))]
public partial record Order(Guid Id, string CustomerName, decimal Total, DateTimeOffset OrderDate);

// Output - Constructor call:
public static OrderDto MapToOrderDto(this Order source)
{
    if (source is null)
    {
        return default!;
    }

    return new OrderDto(
        source.Id,
        source.CustomerName,
        source.Total,
        source.OrderDate);
}
```

**Generated Code Pattern (Mixed Constructor + Initializer):**
```csharp
// Input - Record with constructor and extra properties:
public record ProductDto(Guid Id, string Name, decimal Price)
{
    public string Description { get; set; } = string.Empty;
    public bool InStock { get; set; }
}

[MapTo(typeof(ProductDto))]
public partial class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool InStock { get; set; }
}

// Output - Mixed constructor + initializer:
public static ProductDto MapToProductDto(this Product source)
{
    if (source is null)
    {
        return default!;
    }

    return new ProductDto(
        source.Id,
        source.Name,
        source.Price)
    {
        Description = source.Description,
        InStock = source.InStock
    };
}
```

**Generated Code Pattern (Base Class Property Inheritance):**
```csharp
// Input - Base entity with common properties:
public abstract partial class BaseEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public abstract partial class AuditableEntity : BaseEntity
{
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

[MapTo(typeof(BookDto))]
public partial class Book : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class BookDto
{
    public Guid Id { get; set; }                    // From BaseEntity
    public DateTimeOffset CreatedAt { get; set; }    // From BaseEntity
    public DateTimeOffset? UpdatedAt { get; set; }   // From AuditableEntity
    public string? UpdatedBy { get; set; }           // From AuditableEntity
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Output - All properties from entire hierarchy included:
public static BookDto MapToBookDto(this Book source)
{
    if (source is null)
    {
        return default!;
    }

    return new BookDto
    {
        Id = source.Id,                    // ✨ From BaseEntity (2 levels up)
        CreatedAt = source.CreatedAt,      // ✨ From BaseEntity
        UpdatedAt = source.UpdatedAt,      // ✨ From AuditableEntity (1 level up)
        UpdatedBy = source.UpdatedBy,      // ✨ From AuditableEntity
        Title = source.Title,
        Author = source.Author,
        Price = source.Price
    };
}
```

**Mapping Rules:**
1. **Base Class Property Collection**: Generator traverses the entire inheritance hierarchy:
   - Walks up from most derived class to `System.Object`
   - Collects properties from each level (respecting accessibility and `[MapIgnore]`)
   - Handles property overrides correctly (keeps most derived version, no duplicates)
   - Works with unlimited inheritance depth
2. **Constructor Detection**: Generator automatically detects suitable constructors:
   - Finds public constructors where ALL parameters match source properties (case-insensitive)
   - Prefers constructors with more parameters
   - Uses constructor call syntax when a suitable constructor is found
   - Falls back to object initializer syntax when no matching constructor exists
3. **Property Matching**: Properties are matched by name (case-insensitive):
   - `Id` matches `id`, `ID`, `Id` (supports different casing conventions)
   - Enables mapping between PascalCase properties and camelCase constructor parameters
4. **Direct Mapping**: Properties with same name and type are mapped directly
5. **Smart Enum Conversion**:
   - If source enum has `[MapTo(typeof(TargetEnum))]`, uses `.MapToTargetEnum()` extension method (safe)
   - If target enum has `[MapTo(typeof(SourceEnum), Bidirectional = true)]`, uses reverse mapping method (safe)
   - Otherwise, falls back to `(TargetEnum)source.Enum` cast (less safe)
6. **Collection Mapping**: If both source and target properties are collections:
   - Extracts element types and generates `.Select(x => x.MapToXxx())` code
   - Uses `.ToList()` for most collection types (List, IEnumerable, ICollection, IList, IReadOnlyList)
   - Uses `.ToArray()` for array types
   - Uses collection constructors for `Collection<T>` and `ReadOnlyCollection<T>`
7. **Nested Objects**: If a property type has a `MapToXxx()` method, it's used automatically
8. **Null Safety**: Nullable properties use `?.` and `!` for proper null handling

**3-Layer Architecture Support:**
```
UserEntity (DataAccess) [MapTo(typeof(User))]
    ↓ .MapToUser()
User (Domain) [MapTo(typeof(UserDto))]
    ↓ .MapToUserDto()
UserDto (API)
```

**Diagnostics:**
- `ATCMAP001` - Mapping class must be partial (Error)
- `ATCMAP002` - Target type must be a class or struct (Error)
- `ATCMAP003` - MapProperty target property not found (Error)

### EnumMappingGenerator

**Key Features:**
- Intelligent name-based enum value matching (case-insensitive)
- Automatic special case detection (None → Unknown, Active → Enabled, etc.)
- Bidirectional mapping support with `Bidirectional = true`
- Zero runtime cost - pure switch expressions
- Type-safe with compile-time diagnostics
- No "partial" requirement (enums can't be partial)

**Generated Code Pattern:**
```csharp
// Input:
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatusEntity
{
    None,       // Special case: maps to PetStatusDto.Unknown
    Pending,
    Available,
    Adopted,
}

public enum PetStatusDto
{
    Unknown,    // Special case: maps from PetStatusEntity.None
    Available,
    Pending,
    Adopted,
}

// Output (extension method in Atc.Mapping namespace):
public static PetStatusDto MapToPetStatusDto(this PetStatusEntity source)
{
    return source switch
    {
        PetStatusEntity.None => PetStatusDto.Unknown,        // Special case mapping
        PetStatusEntity.Pending => PetStatusDto.Pending,
        PetStatusEntity.Available => PetStatusDto.Available,
        PetStatusEntity.Adopted => PetStatusDto.Adopted,
        _ => throw new global::System.ArgumentOutOfRangeException(
            nameof(source), source, "Unmapped enum value"),
    };
}

// Reverse mapping (Bidirectional = true):
public static PetStatusEntity MapToPetStatusEntity(this PetStatusDto source)
{
    return source switch
    {
        PetStatusDto.Unknown => PetStatusEntity.None,        // Special case mapping
        PetStatusDto.Available => PetStatusEntity.Available,
        PetStatusDto.Pending => PetStatusEntity.Pending,
        PetStatusDto.Adopted => PetStatusEntity.Adopted,
        _ => throw new global::System.ArgumentOutOfRangeException(
            nameof(source), source, "Unmapped enum value"),
    };
}
```

**Mapping Rules:**
1. **Exact Match**: Enum values with same name (case-sensitive) map directly
2. **Case-Insensitive Match**: Falls back to case-insensitive comparison
3. **Special Cases**: Automatically detects common patterns:
   - `None` ↔ `Unknown`, `Default`, `NotSet`
   - `Active` ↔ `Enabled`, `On`, `Running`
   - `Inactive` ↔ `Disabled`, `Off`, `Stopped`
   - `Deleted` ↔ `Removed`, `Archived`
   - `Pending` ↔ `InProgress`, `Processing`
   - `Completed` ↔ `Done`, `Finished`
4. **Unmapped Values**: Generate warnings at compile time, throw at runtime

**Special Case Detection:**
The generator uses `EnumMappingUtility` which contains a dictionary of common enum naming patterns. This eliminates the need for manual configuration when dealing with standard patterns like database "None" values mapping to API "Unknown" values.

**3-Layer Architecture Support:**
```
PetStatusEntity (DataAccess) [MapTo(typeof(Domain.PetStatus))]
    ↓ .MapToPetStatus()
PetStatus (Domain) [MapTo(typeof(Api.PetStatus))]
    ↓ .MapToPetStatus()
PetStatus (API)
```

**Diagnostics:**
- `ATCENUM001` - Target type must be an enum (Error)
- `ATCENUM002` - Source enum value has no matching target value (Warning)

## PetStore Sample - Complete Example

The `PetStore.Api` sample demonstrates all four generators working together in a realistic 3-layer ASP.NET Core application with OpenAPI/Scalar documentation.

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ PetStore.Api (ASP.NET Core 10.0 Minimal API)               │
│ - GenerateDocumentationFile=true (for OpenAPI)             │
│ - Endpoints: POST /pets, GET /pets/{id}                    │
│ - OpenAPI/Scalar integration                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ PetStore.Domain                                             │
│ - [Registration] PetService, ValidationService              │
│ - [Registration] PetMaintenanceService (BackgroundService)  │
│ - [OptionsBinding] PetStoreOptions, PetMaintenanceOptions   │
│ - [MapTo] Pet → PetDto, Pet → PetEntity                    │
│ - GenerateDocumentationFile=false                           │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ PetStore.DataAccess                                         │
│ - [Registration] PetRepository                              │
│ - [MapTo] PetEntity → Pet                                   │
│ - GenerateDocumentationFile=false                           │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ PetStore.Api.Contract                                       │
│ - DTOs: CreatePetRequest, PetDto                            │
│ - GenerateDocumentationFile=false                           │
└─────────────────────────────────────────────────────────────┘
```

### Generated Extension Methods

The generators create these registration methods per assembly (with smart naming):

```csharp
// From PetStore.Domain (suffix "Domain" is unique → AddDependencyRegistrationsFromDomain)
services.AddDependencyRegistrationsFromDomain();
services.AddOptionsFromDomain(configuration);

// From PetStore.DataAccess (suffix "DataAccess" is unique → AddDependencyRegistrationsFromDataAccess)
services.AddDependencyRegistrationsFromDataAccess();

// Mapping extensions available via 'using Atc.Mapping'
var dto = pet.MapToPetResponse();
var entity = pet.MapToPetEntity();
var domain = entity.MapToPet();
```

### Request Flow Example

```
POST /pets { "name": "Buddy", "species": "Dog" }
    ↓
API receives CreatePetRequest
    ↓
IPetService.CreatePetAsync() [injected via DI generator]
    ↓
Validation with PetStoreOptions [bound via Options generator]
    ↓
CreatePetRequest.MapToPet() [Mapping generator]
    ↓
IPetRepository.SaveAsync() [injected via DI generator]
    ↓
Pet.MapToPetEntity() [Mapping generator]
    ↓
Save to storage
    ↓
PetEntity.MapToPet().MapToPetDto() [Mapping generator chain]
    ↓
Return PetDto to client
```

### Key Features Demonstrated

- **Zero boilerplate DI registration**: All services auto-registered, including hosted services
- **Background service support**: `PetMaintenanceService` automatically registered with `AddHostedService<T>()`
- **Type-safe configuration**: Options validated and bound automatically
- **Automatic mapping chains**: Entity ↔ Domain ↔ DTO conversions
- **OpenAPI integration**: Full API documentation with Scalar UI
- **Multi-project architecture**: Shows how generators work across project boundaries

## Development Guidelines

### Diagnostic Constants Pattern

The project follows the pattern from `atc-analyzer` for managing diagnostic identifiers and categories:

**RuleIdentifierConstants.cs:**
- Defines all diagnostic IDs as constants organized by category
- `ATCDIR001-099` - Dependency Injection diagnostics
- `ATCOPT001-099` - Options Binding diagnostics
- `ATCMAP001-099` - Object Mapping diagnostics

**RuleCategoryConstants.cs:**
- Defines diagnostic categories: `DependencyInjection`, `OptionsBinding`, `ObjectMapping`
- Used in DiagnosticDescriptor creation for proper categorization

**AnalyzerReleases.Unshipped.md:**
- Track all changes to diagnostic IDs, categories, or severities
- Update when modifying existing diagnostics

### When Modifying Generators

1. **Target Framework**: Generators must target `netstandard2.0` for Roslyn compatibility
2. **Attribute Generation**: All generators ALWAYS emit fallback attribute definitions in `PostInitialization` to ensure attributes are available early in compilation. This is required even if projects reference `Atc.SourceGenerators.Annotations`. CS0436 warnings are expected and should be suppressed via `<NoWarn>$(NoWarn);CS0436</NoWarn>`
3. **Generated Code Attributes**: Always include best-practice attributes on generated types:
   - `[GeneratedCode]` - Marks code as auto-generated
   - `[EditorBrowsable(Never)]` - Hides from IntelliSense
   - `[CompilerGenerated]` - Marks as compiler-generated
   - `[DebuggerNonUserCode]` - Improves debugging (classes/methods only)
   - `[ExcludeFromCodeCoverage]` - Excludes from coverage (classes/methods only)
4. **Testing**: Use the test helper pattern in `DependencyRegistrationGeneratorTests.cs` - create syntax trees, run generator, validate diagnostics and output
5. **Incremental Generation**: Leverage Roslyn's incremental generation pipeline for performance
6. **Diagnostics**: Always use constants from `RuleIdentifierConstants` and `RuleCategoryConstants` - never hardcode diagnostic IDs or categories

### Testing Pattern

```csharp
private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput(string source)
{
    var syntaxTree = CSharpSyntaxTree.ParseText(source);
    var compilation = CSharpCompilation.Create("TestAssembly", [syntaxTree], references, options);
    var generator = new DependencyRegistrationGenerator();
    var driver = CSharpGeneratorDriver.Create(generator);
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
    // Extract and validate generated code
}
```

### String Generation Utilities

Both generators use `StringBuilderExtensions.AppendLineLf()` for consistent LF-only line endings in generated code (see `src/Atc.SourceGenerators/StringBuilderExtensions.cs`).

### Multi-Project Support

Both generators generate assembly-specific extension methods with **smart naming**:
- DI: `AddDependencyRegistrationsFrom{SmartSuffix}()`
- Options: `AddOptionsFrom{SmartSuffix}()`

**Smart Naming Rules:**
- If the assembly suffix (last segment) is unique in the compilation context, use just the suffix
- If multiple assemblies share the same suffix, use the full sanitized assembly name
- Assembly names are sanitized to create valid C# identifiers (dots, dashes, spaces removed)

**Examples:**
- `PetStore.Domain` with no other "Domain" → `AddDependencyRegistrationsFromDomain()`
- `PetStore.Domain` + `AnotherApp.Domain` → `AddDependencyRegistrationsFromPetStoreDomain()` and `AddDependencyRegistrationsFromAnotherAppDomain()`

## XML Documentation Configuration

### Best Practices for OpenAPI/Swagger Integration

When using these source generators with OpenAPI/Swagger documentation (e.g., `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`), follow these XML documentation settings:

**Library Projects** (Domain, DataAccess, Contracts):
```xml
<PropertyGroup>
  <GenerateDocumentationFile>false</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);CS0436;SA0001;CS1591;IDE0005</NoWarn>
</PropertyGroup>
```

**API Projects** (Web API, Minimal API):
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);CS0436;SA0001;CS1591;IDE0005</NoWarn>
</PropertyGroup>
```

**Annotations Assembly**:
```xml
<PropertyGroup>
  <GenerateDocumentationFile>false</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);EnableGenerateDocumentationFile</NoWarn>
</PropertyGroup>
```

### Why This Configuration?

1. **Fallback Attributes**: Each generator emits attribute definitions in `PostInitialization` for projects that don't reference `Atc.SourceGenerators.Annotations`
2. **Duplicate Prevention**: If multiple projects generate XML docs for the same types (via fallback attributes), OpenAPI will throw `ArgumentException: An item with the same key has already been added`
3. **Standard Practice**: Library projects typically don't need XML documentation files; only the final API project needs them for OpenAPI/Swagger

### Warning Suppressions Explained

- **CS0436**: "The type 'X' in 'Y' conflicts with the imported type 'X' in 'Z'" - Expected when fallback attributes are generated
- **SA0001**: StyleCop warning - typically disabled for generated code
- **CS1591**: "Missing XML comment for publicly visible type" - Common to suppress in projects without XML docs
- **IDE0005**: "Remove unnecessary usings" - May conflict with generated code
- **EnableGenerateDocumentationFile**: Roslyn analyzer suggestion to enable XML docs - can be ignored for library/annotation projects

### Generated Code Best Practices

All generators emit best-practice attributes on generated types:

```csharp
[global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.{GeneratorName}", "1.0.0")]
[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
[global::System.Runtime.CompilerServices.CompilerGenerated]
[global::System.Diagnostics.DebuggerNonUserCode]  // Classes/methods only
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]  // Classes/methods only
```

These attributes:
- Hide generated code from IntelliSense
- Exclude from code coverage
- Mark as compiler-generated
- Improve debugging experience

## Common Issues

### Generator Not Running
- Clean and rebuild: `dotnet clean && dotnet build`
- Check that the class has the correct attribute and accessibility (public)
- For options: verify class is declared `partial`
- Restart IDE/OmniSharp if using VS Code

### Attribute CS0436 Warnings
This is **expected and by design**. All generators emit fallback attribute definitions in `PostInitialization` to ensure attributes are available early in the compilation pipeline. This happens even if projects reference `Atc.SourceGenerators.Annotations`.

**Always suppress this warning:**
```xml
<NoWarn>$(NoWarn);CS0436</NoWarn>
```

### XML Documentation Duplicate Key Errors
If you encounter `System.ArgumentException: An item with the same key has already been added` when using OpenAPI/Swagger:

1. Disable XML documentation for all **library projects** (Domain, DataAccess, Contracts):
   ```xml
   <GenerateDocumentationFile>false</GenerateDocumentationFile>
   ```

2. Enable XML documentation **only** for the final **API project**:
   ```xml
   <GenerateDocumentationFile>true</GenerateDocumentationFile>
   ```

3. Ensure the Annotations project has the warning suppressed:
   ```xml
   <NoWarn>$(NoWarn);EnableGenerateDocumentationFile</NoWarn>
   ```

See the **XML Documentation Configuration** section above for detailed explanation.

### Testing Generators
Run tests with verbose output to see generated source:
```bash
dotnet test --logger "console;verbosity=detailed"
```

## NuGet Package Structure

The `Atc.SourceGenerators.csproj` uses a special analyzer package structure:
- DLL placed in `analyzers/dotnet/cs/` path (not standard lib folder)
- `DevelopmentDependency=true` - not included in consuming project outputs
- PDBs are embedded in the DLL (`DebugType=embedded`)
- `IncludeBuildOutput=false` - analyzer packages don't follow standard structure
