# 🎯 Atc Source Generators

A collection of Roslyn C# source generators for .NET that eliminate boilerplate code and improve developer productivity. All generators are designed with **Native AOT compatibility** in focus, enabling faster startup times, smaller deployment sizes, and optimal performance for modern cloud-native applications.

**Why Choose Atc Source Generators?**

- 🎯 **Zero boilerplate** - Attribute-based approach eliminates repetitive code
- ⚡ **Compile-time generation** - Catch errors during build, not at runtime
- 🚀 **Native AOT ready** - Zero reflection, fully trimming-safe for modern .NET
- 🧩 **Multi-project architecture** - Smart naming for clean layered applications
- 🛡️ **Type-safe** - Full IntelliSense and compile-time validation
- 📦 **Single package** - Install once, use all generators

## 🚀 Source Generators

- **[⚡ DependencyRegistrationGenerator](#-dependencyregistrationgenerator)** - Automatic DI service registration with attributes
- **[⚙️ OptionsBindingGenerator](#️-optionsbindinggenerator)** - Automatic configuration binding to strongly-typed options classes
- **[🗺️ MappingGenerator](#️-mappinggenerator)** - Automatic object-to-object mapping with type safety
- **[🔄 EnumMappingGenerator](#-enummappinggenerator)** - Automatic enum-to-enum mapping with intelligent matching

## ✨ See It In Action

All four generators work together seamlessly in a typical 3-layer architecture:

```csharp
// 1️⃣ Domain Layer - Your business logic
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatus { Available, Adopted }

[MapTo(typeof(PetDto))]
public partial class Pet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetStatus Status { get; set; }
}

[Registration(Lifetime.Scoped)]
public class PetService : IPetService
{
    public async Task<Pet> GetPetAsync(Guid id) { /* ... */ }
}

[OptionsBinding("PetStore")]
public partial class PetStoreOptions
{
    [Required] public int MaxPetsPerPage { get; set; }
}

// 2️⃣ Program.cs - One line per concern
using Atc.DependencyInjection;
using Atc.Mapping;

// Register all services from Domain layer
builder.Services.AddDependencyRegistrationsFromDomain();

// Bind all options from Domain layer
builder.Services.AddOptionsFromDomain(builder.Configuration);

// 3️⃣ Usage - Clean and type-safe
app.MapGet("/pets/{id}", async (Guid id, IPetService service) =>
{
    var pet = await service.GetPetAsync(id);
    return Results.Ok(pet.MapToPetDto());  // ✨ Generated mapping
});
```

**Result:** Zero boilerplate, full type safety, Native AOT ready! 🚀

## 📦 Installation

All generators are distributed in a single NuGet package. Install once to use all features.

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

### ⚡ DependencyRegistrationGenerator

Stop writing repetitive service registration code. Decorate your services with `[Registration]` and let the generator handle the rest.

#### 📚 Documentation

- **[Complete Guide](docs/DependencyRegistrationGenerators.md)** - In-depth documentation with examples
- **[Quick Start](docs/DependencyRegistrationGenerators.md#get-started---quick-guide)** - PetStore 3-layer architecture tutorial
- **[Multi-Project Setup](docs/DependencyRegistrationGenerators.md#multi-project-setup)** - Working with multiple projects
- **[Auto-Detection](docs/DependencyRegistrationGenerators.md#auto-detection)** - Understanding automatic interface detection
- **[Sample Projects](docs/DependencyRegistrationGenerators-Samples.md)** - Working code examples with architecture diagrams

#### 😫 From This

```csharp
// Program.cs - Manual registration hell 😫
services.AddScoped<IUserService, UserService>();
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IPetRepository, PetRepository>();
services.AddSingleton<ICacheService, CacheService>();
services.AddTransient<ILogger, Logger>();
services.AddScoped<IPetService, PetService>();
services.AddScoped<IEmailService, EmailService>();
// ... 50+ more lines of registration code
// ... spread across multiple files
// ... easy to forget or get wrong
```

#### ✨ To This

```csharp
// Your services - Clean, declarative, self-documenting ✨
[Registration(Lifetime.Scoped)]
public class UserService : IUserService { }

[Registration(Lifetime.Scoped)]
public class OrderService : IOrderService { }

[Registration]
public class CacheService : ICacheService { }

// Program.cs - One line per project (with smart naming!)
using Atc.DependencyInjection;

builder.Services.AddDependencyRegistrationsFromApi();
builder.Services.AddDependencyRegistrationsFromDomain();
builder.Services.AddDependencyRegistrationsFromDataAccess();
```

#### ✨ Key Features

- **🎯 Interface Auto-Detection**: Automatically registers against all implemented interfaces - no `As = typeof(IService)` needed
- **🔷 Generic Interface Registration**: Full support for open generics like `IRepository<T>` and `IHandler<TRequest, TResponse>`
- **🔑 Keyed Service Registration**: Multiple implementations of the same interface with different keys (.NET 8+)
- **🏭 Factory Method Registration**: Custom initialization logic via static factory methods
- **📦 Instance Registration**: Register pre-created singleton instances via static fields, properties, or methods
- **🔄 TryAdd* Registration**: Conditional registration for default implementations (library pattern)
- **⚙️ Conditional Registration**: Register services based on configuration values (feature flags, environment-specific services)
- **🎨 Decorator Pattern Support**: Wrap services with cross-cutting concerns (logging, caching, validation) using `Decorator = true`
- **🚫 Assembly Scanning Filters**: Exclude types by namespace, pattern (wildcards), or interface implementation
- **🎯 Runtime Filtering**: Exclude services when calling registration methods via optional parameters (different apps, different service subsets)
- **🔗 Transitive Registration**: Automatically discover and register services from referenced assemblies (4 overloads: default, auto-detect all, selective by name, selective multiple)
- **🧹 Smart Filtering**: System interfaces (IDisposable, etc.) are automatically excluded
- **🔍 Multi-Interface Registration**: Implementing multiple interfaces? Registers against all of them
- **🏃 Hosted Service Support**: Automatically detects BackgroundService and IHostedService implementations and uses AddHostedService<T>()
- **✨ Smart Naming**: Generates clean method names using suffixes when unique, full names when conflicts exist
- **⚡ Zero Runtime Overhead**: All code generated at compile time
- **🚀 Native AOT Compatible**: No reflection or runtime code generation - fully trimming-safe
- **🏗️ Multi-Project Support**: Works seamlessly across layered architectures
- **🛡️ Compile-Time Validation**: Diagnostics for common errors catch issues before runtime
- **📦 Flexible Lifetimes**: Singleton (default), Scoped, and Transient support

#### 🚀 Quick Example

```csharp
using Atc.DependencyInjection;

// That's it! Auto-detected as IUserService
[Registration(Lifetime.Scoped)]
public class UserService : IUserService
{
    public void CreateUser(string name) { }
}

// Multiple interfaces? No problem - registers against ALL of them
[Registration]
public class EmailService : IEmailService, INotificationService { }

// Need both interface AND concrete type?
[Registration(AsSelf = true)]
public class ReportService : IReportService { }

// Custom initialization logic via factory method
[Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(Create))]
public class EmailSender : IEmailSender
{
    private EmailSender(string apiKey) { }

    public static IEmailSender Create(IServiceProvider sp)
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new EmailSender(config["Email:ApiKey"]);
    }
}

// Default implementation for libraries (can be overridden by consumers)
[Registration(As = typeof(ILogger), TryAdd = true)]
public class DefaultLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
}

// Decorator pattern - wrap services with cross-cutting concerns
[Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
public class LoggingOrderServiceDecorator : IOrderService
{
    private readonly IOrderService inner;
    private readonly ILogger logger;

    public LoggingOrderServiceDecorator(IOrderService inner, ILogger logger)
    {
        this.inner = inner;
        this.logger = logger;
    }

    public async Task PlaceOrderAsync(string orderId)
    {
        logger.Log($"Before placing order {orderId}");
        await inner.PlaceOrderAsync(orderId);
        logger.Log($"After placing order {orderId}");
    }
}
```

#### 🔧 Service Lifetimes

```csharp
[Registration]                          // Singleton (default)
[Registration(Lifetime.Singleton)]      // Explicit singleton
[Registration(Lifetime.Scoped)]         // Per-request (web apps)
[Registration(Lifetime.Transient)]      // New instance every time
```

#### 🛡️ Compile-Time Safety

Get errors at compile time, not runtime:

| ID | Description |
|----|-------------|
| ATCDIR001 | `As` parameter must be an interface or abstract class type |
| ATCDIR002 | Class must implement the specified interface or inherit from abstract class |
| ATCDIR003 | Duplicate registration with different lifetimes |
| ATCDIR004 | Hosted services must use Singleton lifetime |
| ATCDIR005 | Factory method not found |
| ATCDIR006 | Factory method has invalid signature |

---

### ⚙️ OptionsBindingGenerator

Eliminate boilerplate configuration binding code. Decorate your options classes with `[OptionsBinding]` and let the generator create type-safe configuration bindings automatically. Supports DataAnnotations validation, startup validation, fail-fast validation for missing configuration sections (`ErrorOnMissingKeys`), and custom `IValidateOptions<T>` validators for complex business rules.

#### 📚 Documentation

- **[Options Binding Guide](docs/OptionsBindingGenerators.md)** - Full documentation with examples
- **[Sample Projects](docs/OptionsBinding-Samples.md)** - Working examples with architecture diagrams

#### 😫 From This

```csharp
// Manual options binding - repetitive and error-prone 😫
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<ApiOptions>()
    .Bind(configuration.GetSection("App:Api"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<LoggingOptions>()
    .Bind(configuration.GetSection("Logging"))
    .ValidateOnStart();

// ... repeated for every options class
```

#### ✨ To This

```csharp
// Your options classes - Clean and declarative ✨
[OptionsBinding("Database", ValidateDataAnnotations = true, ValidateOnStart = true)]
public partial class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; }
}

[OptionsBinding("App:Api", ValidateDataAnnotations = true)]
public partial class ApiOptions
{
    public string BaseUrl { get; set; }
}

[OptionsBinding]  // Section name auto-inferred as "LoggingOptions"
public partial class LoggingOptions
{
    public string Level { get; set; }
}

// Program.cs - One line binds all options (with smart naming!)
services.AddOptionsFromApp(configuration);
```

#### ✨ Key Features

- **🧠 Automatic Section Name Inference**: Smart resolution from explicit names, const fields (`SectionName`, `NameTitle`, `Name`), or auto-inferred from class names
- **🔒 Built-in Validation**: Integrated DataAnnotations validation (`ValidateDataAnnotations`) and startup validation (`ValidateOnStart`)
- **🎯 Custom Validation**: Support for `IValidateOptions<T>` for complex business rules beyond DataAnnotations
- **🔔 Configuration Change Callbacks**: Auto-generated IHostedService for OnChange notifications with Monitor lifetime - perfect for feature flags and runtime config updates
- **🔧 Post-Configuration Support**: Normalize or transform values after binding with `PostConfigure` callbacks (e.g., ensure paths have trailing slashes, lowercase URLs)
- **📛 Named Options**: Multiple configurations of the same options type with different names (e.g., Primary/Secondary email servers)
- **🎯 Explicit Section Paths**: Support for nested sections like `"App:Database"` or `"Services:Email"`
- **📂 Nested Subsection Binding**: Automatic binding of complex properties to configuration subsections (e.g., `StorageOptions.Database.Retry` → `"Storage:Database:Retry"`)
- **📦 Multiple Options Classes**: Register multiple configuration sections in a single assembly with one method call
- **🏗️ Multi-Project Support**: Smart naming generates assembly-specific extension methods (e.g., `AddOptionsFromDomain()`, `AddOptionsFromDataAccess()`)
- **🔗 Transitive Registration**: Automatically discover and register options from referenced assemblies (4 overloads: default, auto-detect all, selective by name, selective multiple)
- **⏱️ Flexible Lifetimes**: Choose between Singleton (`IOptions<T>`), Scoped (`IOptionsSnapshot<T>`), or Monitor (`IOptionsMonitor<T>`) patterns
- **⚡ Native AOT Ready**: Pure compile-time code generation with zero reflection, fully trimming-safe for modern .NET deployments
- **🛡️ Compile-Time Safety**: Catch configuration errors during build, not at runtime
- **🔧 Partial Class Requirement**: Simple `partial` keyword enables seamless extension method generation

#### 🚀 Quick Example

```csharp
using Atc.SourceGenerators.Annotations;
using System.ComponentModel.DataAnnotations;

// Automatic section name inference
[OptionsBinding]  // Binds to "DatabaseOptions" section (uses full class name)
public partial class DatabaseOptions
{
    public string ConnectionString { get; set; }
}

// Using const SectionName (2nd priority)
[OptionsBinding(ValidateDataAnnotations = true)]
public partial class CacheOptions
{
    public const string SectionName = "ApplicationCache";  // Binds to "ApplicationCache"

    [Range(1, 1000)]
    public int MaxSize { get; set; }
}

// Using const Name (4th priority)
[OptionsBinding]
public partial class EmailOptions
{
    public const string Name = "EmailConfiguration";  // Binds to "EmailConfiguration"
    public string SmtpServer { get; set; }
}

// Full priority demonstration
[OptionsBinding]
public partial class LoggingOptions
{
    public const string SectionName = "X1";  // 2nd prio - WINS
    public const string NameTitle = "X2";    // 3rd prio
    public const string Name = "X3";         // 4th prio
    // Binds to "X1"
}

// Explicit section path (1st priority - highest)
[OptionsBinding("App:Email:Smtp")]
public partial class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
}

// Specify lifetime for different injection patterns
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor)]
public partial class FeatureOptions
{
    public bool EnableNewFeature { get; set; }
}

// Configuration change callbacks - auto-generated IHostedService
[OptionsBinding("Features", Lifetime = OptionsLifetime.Monitor, OnChange = nameof(OnFeaturesChanged))]
public partial class FeaturesOptions
{
    public bool EnableNewUI { get; set; }
    public bool EnableBetaFeatures { get; set; }

    // Called automatically when configuration changes (requires reloadOnChange: true)
    internal static void OnFeaturesChanged(FeaturesOptions options, string? name)
    {
        Console.WriteLine($"[OnChange] EnableNewUI: {options.EnableNewUI}");
        Console.WriteLine($"[OnChange] EnableBetaFeatures: {options.EnableBetaFeatures}");
    }
}

// Usage in your services:
public class MyService
{
    public MyService(IOptions<DatabaseOptions> db)               // Singleton
    public MyService(IOptionsSnapshot<SmtpOptions> smtp)         // Scoped (reloads per request)
    public MyService(IOptionsMonitor<FeatureOptions> features)   // Monitor (change notifications)
}
```

#### 🛡️ Compile-Time Safety

| ID | Description |
|----|-------------|
| ATCOPT001 | Options class must be declared as partial |
| ATCOPT002 | Section name cannot be null or empty |
| ATCOPT003 | Invalid options binding configuration |
| ATCOPT004 | OnChange requires Monitor lifetime |
| ATCOPT005 | OnChange not supported with named options |
| ATCOPT006 | OnChange callback method not found |
| ATCOPT007 | OnChange callback has invalid signature |
| ATCOPT008 | PostConfigure not supported with named options |
| ATCOPT009 | PostConfigure callback method not found |
| ATCOPT010 | PostConfigure callback has invalid signature |

---

### 🗺️ MappingGenerator

Eliminate tedious object-to-object mapping code. Decorate your classes with `[MapTo(typeof(TargetType))]` and let the generator create type-safe mapping extension methods automatically.

#### 📚 Documentation

- **[Object Mapping Guide](docs/ObjectMappingGenerators.md)** - Full documentation with examples
- **[Quick Start](docs/ObjectMappingGenerators.md#get-started---quick-guide)** - UserApp 3-layer architecture tutorial
- **[Advanced Scenarios](docs/ObjectMappingGenerators.md#advanced-scenarios)** - Enums, nested objects, multi-layer mapping
- **[Sample Projects](docs/ObjectMappingGenerators-Samples.md)** - Working code examples with DataAccess → Domain → API

#### 😫 From This

```csharp
// Manual mapping - tedious, repetitive, error-prone 😫
public UserDto MapToDto(User user)
{
    return new UserDto
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Status = (UserStatusDto)user.Status,
        Address = user.Address != null ? new AddressDto
        {
            Street = user.Address.Street,
            City = user.Address.City,
            State = user.Address.State,
            PostalCode = user.Address.PostalCode,
            Country = user.Address.Country
        } : null,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
// ... repeat for every type
// ... across every layer
// ... easy to forget properties
```

#### ✨ To This

```csharp
// Your domain models - Clean, declarative, self-documenting ✨
using Atc.SourceGenerators.Annotations;

[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserStatus Status { get; init; }
    public Address? Address { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

[MapTo(typeof(AddressDto))]
public partial class Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

// Usage - One line per mapping
using Atc.Mapping;

var dto = user.MapToUserDto();
var dtos = users.Select(u => u.MapToUserDto()).ToList();
```

#### ✨ Key Features

- **📦 Collection Mapping**: Automatic mapping for `List<T>`, `IEnumerable<T>`, arrays, and other collection types
- **🏗️ Constructor Mapping**: Automatically detects and uses constructors for records and classes with primary constructors (C# 12+)
- **🚫 Property Exclusion**: Use `[MapIgnore]` to exclude sensitive or internal properties (works on both source and target)
- **🏷️ Custom Property Names**: Use `[MapProperty]` to map properties with different names between source and target
- **📏 Property Flattening**: Opt-in flattening support (e.g., `Address.City` → `AddressCity`)
- **🔄 Built-in Type Conversion**: DateTime ↔ string, Guid ↔ string, numeric ↔ string conversions
- **✅ Required Property Validation**: Compile-time diagnostics (ATCMAP004) for missing required properties (C# 11+)
- **🌳 Polymorphic/Derived Type Mapping**: Runtime type discrimination using switch expressions and `[MapDerivedType]`
- **🪝 Before/After Mapping Hooks**: Custom pre/post-processing logic with `BeforeMap` and `AfterMap` methods
- **🏭 Object Factories**: Custom object creation via factory methods instead of `new TargetType()`
- **♻️ Update Existing Target**: Map to existing instances (EF Core tracked entities) with `UpdateTarget = true`
- **📊 IQueryable Projections**: EF Core server-side query optimization with `GenerateProjection = true`
- **🔷 Generic Mappers**: Type-safe mapping for generic wrapper types like `Result<T>` and `PagedResult<T>`
- **🔐 Private Member Access**: Map to/from private and internal properties using UnsafeAccessor (.NET 8+)
- **🔤 Property Name Casing Strategies**: CamelCase and snake_case support with `PropertyNameStrategy`
- **🧬 Base Class Property Inheritance**: Automatically include properties from base classes (Entity audit fields, etc.)
- **🔁 Bidirectional Mapping**: Generate both forward and reverse mappings with `Bidirectional = true`
- **🔄 Smart Enum Conversion**: Uses safe EnumMapping extension methods when enums have `[MapTo]` attributes, falls back to casts
- **🪆 Nested Object Mapping**: Automatically chains mappings for nested properties
- **🏗️ Multi-Layer Support**: Build Entity → Domain → DTO mapping chains effortlessly
- **⚡ Zero Runtime Cost**: All code generated at compile time
- **🚀 Native AOT Compatible**: No reflection or runtime code generation - fully trimming-safe
- **🛡️ Type-Safe**: Compile-time validation catches mapping errors before runtime
- **📦 Null Safety**: Built-in null checking for nullable reference types
- **🎯 Convention-Based**: Maps properties by name - no configuration needed

#### 🚀 Quick Example

```csharp
using Atc.SourceGenerators.Annotations;
using Atc.Mapping;

// Source with nested object and enum
[MapTo(typeof(PersonDto))]
public partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Status Status { get; set; }
    public Address? Address { get; set; }
}

[MapTo(typeof(AddressDto))]
public partial class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public enum Status { Active = 0, Inactive = 1 }

// Target types
public class PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StatusDto Status { get; set; }
    public AddressDto? Address { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public enum StatusDto { Active = 0, Inactive = 1 }

// ✨ Use generated extension methods
var person = new Person
{
    Id = 1,
    Name = "John Doe",
    Status = Status.Active,
    Address = new Address { Street = "123 Main St", City = "NYC" }
};

var dto = person.MapToPersonDto();
// ✨ Automatic enum conversion
// ✨ Automatic nested object mapping (Address → AddressDto)
// ✨ Null safety built-in
```

#### 🔁 Multi-Layer Architecture

Perfect for 3-layer architectures:

```
Database (Entities) → Domain (Models) → API (DTOs)
```

```csharp
// Data Access Layer
[MapTo(typeof(Domain.Product))]
public partial class ProductEntity
{
    public int DatabaseId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; }  // DB-specific field
}

// Domain Layer
namespace Domain;

[MapTo(typeof(ProductDto))]
public partial class Product
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// ✨ Complete mapping chain
var entity = repository.GetById(1);
var domain = entity.MapToProduct();
var dto = domain.MapToProductDto();
```

#### 🛡️ Compile-Time Safety

Get errors at compile time, not runtime:

| ID | Description |
|----|-------------|
| ATCMAP001 | Mapping class must be declared as partial |
| ATCMAP002 | Target type must be a class or struct |

---

### 🔄 EnumMappingGenerator

Eliminate manual enum conversions with intelligent enum-to-enum mapping. Decorate your enums with `[MapTo(typeof(TargetEnum))]` and let the generator create type-safe switch expression mappings with special case handling automatically.

#### 📚 Documentation

- **[Enum Mapping Guide](docs/EnumMappingGenerators.md)** - Full documentation with examples
- **[Quick Start](docs/EnumMappingGenerators.md#get-started---quick-guide)** - PetStore enum mapping tutorial
- **[Special Case Mappings](docs/EnumMappingGenerators.md#-special-case-mappings)** - None → Unknown, Active → Enabled, etc.
- **[Sample Projects](docs/EnumMappingGenerators-Samples.md)** - Working code examples with bidirectional mapping

#### 😫 From This

```csharp
// Manual enum mapping - tedious, error-prone, inflexible 😫
public PetStatusDto MapToDto(PetStatusEntity status)
{
    return status switch
    {
        PetStatusEntity.None => PetStatusDto.Unknown,
        PetStatusEntity.Pending => PetStatusDto.Pending,
        PetStatusEntity.Available => PetStatusDto.Available,
        PetStatusEntity.Adopted => PetStatusDto.Adopted,
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}

public PetStatusEntity MapToEntity(PetStatusDto status)
{
    return status switch
    {
        PetStatusDto.Unknown => PetStatusEntity.None,
        PetStatusDto.Pending => PetStatusEntity.Pending,
        PetStatusDto.Available => PetStatusEntity.Available,
        PetStatusDto.Adopted => PetStatusEntity.Adopted,
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}
// ... repeat for every enum pair
// ... across every layer
// ... easy to make mistakes
```

#### ✨ To This

```csharp
// Your enums - Clean, declarative, self-documenting ✨
using Atc.SourceGenerators.Annotations;

// Database layer
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatusEntity
{
    None,       // ✨ Auto-maps to PetStatusDto.Unknown (special case)
    Pending,
    Available,
    Adopted,
}

// API layer
public enum PetStatusDto
{
    Unknown,    // ✨ Auto-maps from PetStatusEntity.None
    Available,
    Pending,
    Adopted,
}

// Usage - Generated extension methods
using Atc.Mapping;

var entity = PetStatusEntity.None;
var dto = entity.MapToPetStatusDto();         // PetStatusDto.Unknown
var back = dto.MapToPetStatusEntity();        // PetStatusEntity.None (bidirectional!)
```

#### ✨ Key Features

- **🎯 Intelligent Name Matching**: Maps enum values by name with case-insensitive support
- **🔀 Special Case Detection**: Automatically handles "zero/empty/null" state equivalents:
  - `None` ↔ `Unknown`, `Default`
  - `Unknown` ↔ `None`, `Default`
  - `Default` ↔ `None`, `Unknown`
- **🔁 Bidirectional Mapping**: Generate both forward and reverse mappings with `Bidirectional = true`
- **🔤 Case-Insensitive**: Matches enum values regardless of casing differences
- **⚡ Zero Runtime Cost**: Pure switch expressions, no reflection or runtime code generation
- **🛡️ Type-Safe**: Compile-time validation with diagnostics (ATCENUM002) for unmapped values
- **🚀 Native AOT Compatible**: Fully trimming-safe, works with Native AOT
- **⚠️ Runtime Safety**: `ArgumentOutOfRangeException` thrown for unmapped values

#### 🚀 Quick Example

```csharp
using Atc.SourceGenerators.Annotations;
using Atc.Mapping;

// Database layer enum with special case mapping
[MapTo(typeof(StatusDto), Bidirectional = true)]
public enum StatusEntity
{
    None,        // ✨ Maps to StatusDto.Unknown (special case)
    Active,      // ✨ Exact name match
    Inactive,    // ✨ Exact name match
}

public enum StatusDto
{
    Unknown,     // ✨ Maps from StatusEntity.None (special case)
    Active,      // ✨ Exact name match
    Inactive,    // ✨ Exact name match
}

// ✨ Use generated extension methods
var entity = StatusEntity.None;
var dto = entity.MapToStatusDto();        // StatusDto.Unknown
var back = dto.MapToStatusEntity();       // StatusEntity.None (bidirectional!)
```

#### 🛡️ Compile-Time Safety

Get errors and warnings at compile time, not runtime:

| ID | Description |
|----|-------------|
| ATCENUM001 | Target type must be an enum |
| ATCENUM002 | Enum value has no matching target value (Warning) |

---

## 🔨 Building

```bash
dotnet build
```

## 🧪 Testing

```bash
dotnet test
```

## 📚 Sample Projects

Working code examples demonstrating each generator in realistic scenarios:

### ⚡ [DependencyRegistration Sample](docs/DependencyRegistrationGenerators-Samples.md)

Multi-project console app showing automatic DI registration across layers with auto-detection of interfaces.

```bash
cd sample/Atc.SourceGenerators.DependencyRegistration
dotnet run
```

### ⚙️ [OptionsBinding Sample](docs/OptionsBinding-Samples.md)

Console app demonstrating type-safe configuration binding with validation and multiple options classes.

```bash
cd sample/Atc.SourceGenerators.OptionsBinding
dotnet run
```

### 🗺️ [Mapping Sample](docs/ObjectMappingGenerators-Samples.md)

ASP.NET Core Minimal API showing 3-layer mapping (Entity → Domain → DTO) with automatic enum conversion and nested objects.

```bash
cd sample/Atc.SourceGenerators.Mapping
dotnet run
```

### 🔄 [EnumMapping Sample](docs/EnumMappingGenerators-Samples.md)

Console app demonstrating intelligent enum-to-enum mapping with special case handling (None → Unknown, Active → Enabled), bidirectional mappings, and case-insensitive matching.

```bash
cd sample/Atc.SourceGenerators.EnumMapping
dotnet run
```

### 🎯 [PetStore API - Complete Example](docs/PetStoreApi-Samples.md)

Full-featured ASP.NET Core application using **all four generators** together with OpenAPI/Scalar documentation. This demonstrates production-ready patterns for modern .NET applications.

```bash
cd sample/PetStore.Api
dotnet run
# Open https://localhost:42616/scalar/v1 for API documentation
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

[License information here]
