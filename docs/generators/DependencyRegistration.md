# 🎯 Dependency Registration Generator

Automatically register services in the dependency injection container using attributes instead of manual registration code. The generator creates type-safe registration code at compile time, eliminating boilerplate and reducing errors.

## 📑 Table of Contents

- [🎯 Dependency Registration Generator](#-dependency-registration-generator)
  - [📑 Table of Contents](#-table-of-contents)
  - [🚀 Get Started - Quick Guide](#-get-started---quick-guide)
    - [📂 Project Structure](#-project-structure)
    - [1️⃣ Setup Projects](#1️⃣-setup-projects)
    - [2️⃣ Data Access Layer (PetStore.DataAccess) 💾](#2️⃣-data-access-layer-petstoredataaccess-)
    - [3️⃣ Domain Layer (PetStore.Domain) 🧠](#3️⃣-domain-layer-petstoredomain-)
    - [4️⃣ API Layer (PetStore.Api) 🌐](#4️⃣-api-layer-petstoreapi-)
    - [5️⃣ Program.cs (Minimal API Setup) ⚡](#5️⃣-programcs-minimal-api-setup-)
    - [🎨 What Gets Generated](#-what-gets-generated)
    - [6️⃣ Testing the Application 🧪](#6️⃣-testing-the-application-)
    - [🔍 Viewing Generated Code (Optional)](#-viewing-generated-code-optional)
    - [🎯 Key Takeaways](#-key-takeaways)
  - [✨ Features](#-features)
  - [📦 Installation](#-installation)
  - [💡 Basic Usage](#-basic-usage)
    - [1️⃣ Add Using Directives](#1️⃣-add-using-directives)
    - [2️⃣ Decorate Your Services](#2️⃣-decorate-your-services)
    - [3️⃣ Register in DI Container](#3️⃣-register-in-di-container)
  - [🏗️ Multi-Project Setup](#️-multi-project-setup)
    - [📁 Example Structure](#-example-structure)
    - [⚡ Program.cs Registration](#-programcs-registration)
    - [🔄 Transitive Dependency Registration](#-transitive-dependency-registration)
    - [🏷️ Method Naming Convention](#️-method-naming-convention)
    - [✨ Smart Naming](#-smart-naming)
  - [🔍 Auto-Detection](#-auto-detection)
    - [1️⃣ Single Interface](#1️⃣-single-interface)
    - [🔢 Multiple Interfaces](#-multiple-interfaces)
    - [🧹 System Interfaces Filtered](#-system-interfaces-filtered)
    - [🎯 Explicit Override](#-explicit-override)
    - [🔀 Register As Both Interface and Concrete Type](#-register-as-both-interface-and-concrete-type)
  - [⏱️ Service Lifetimes](#️-service-lifetimes)
    - [🔒 Singleton (Default)](#-singleton-default)
    - [🔄 Scoped](#-scoped)
    - [⚡ Transient](#-transient)
  - [🚀 Native AOT Compatibility](#-native-aot-compatibility)
    - [✅ Why It Works](#-why-it-works)
    - [🎯 Key Benefits](#-key-benefits)
    - [🚀 Native AOT Example](#-native-aot-example)
  - [⚙️ RegistrationAttribute Parameters](#️-registrationattribute-parameters)
    - [📝 Examples](#-examples)
  - [🛡️ Diagnostics](#️-diagnostics)
    - [❌ ATCDIR001: As Type Must Be Interface](#-atcdir001-as-type-must-be-interface)
    - [❌ ATCDIR002: Class Does Not Implement Interface](#-atcdir002-class-does-not-implement-interface)
    - [⚠️ ATCDIR003: Duplicate Registration with Different Lifetime](#️-atcdir003-duplicate-registration-with-different-lifetime)
    - [❌ ATCDIR004: Hosted Services Must Use Singleton Lifetime](#-atcdir004-hosted-services-must-use-singleton-lifetime)
  - [🔷 Generic Interface Registration](#-generic-interface-registration)
    - [Single Type Parameter](#single-type-parameter)
    - [Multiple Type Parameters](#multiple-type-parameters)
    - [Complex Constraints](#complex-constraints)
    - [Explicit Generic Registration](#explicit-generic-registration)
  - [🔑 Keyed Service Registration](#-keyed-service-registration)
    - [String Keys](#string-keys)
    - [Generic Keyed Services](#generic-keyed-services)
  - [🏭 Factory Method Registration](#-factory-method-registration)
    - [Basic Factory Method](#basic-factory-method)
    - [Factory Method Requirements](#factory-method-requirements)
    - [Factory Method with Multiple Interfaces](#factory-method-with-multiple-interfaces)
    - [Factory Method Best Practices](#factory-method-best-practices)
    - [Factory Method Diagnostics](#factory-method-diagnostics)
  - [📦 Instance Registration](#-instance-registration)
    - [Basic Instance Registration (Static Field)](#basic-instance-registration-static-field)
    - [Instance Registration with Static Property](#instance-registration-with-static-property)
    - [Instance Registration with Static Method](#instance-registration-with-static-method)
    - [Instance Registration Requirements](#instance-registration-requirements)
    - [Instance Registration with Multiple Interfaces](#instance-registration-with-multiple-interfaces)
    - [Instance Registration Best Practices](#instance-registration-best-practices)
    - [Instance Registration Diagnostics](#instance-registration-diagnostics)
    - [Instance vs Factory Method](#instance-vs-factory-method)
  - [🔄 TryAdd\* Registration](#-tryadd-registration)
    - [Basic TryAdd Registration](#basic-tryadd-registration)
    - [How TryAdd Works](#how-tryadd-works)
    - [Library Author Pattern](#library-author-pattern)
    - [TryAdd with Different Lifetimes](#tryadd-with-different-lifetimes)
    - [TryAdd with Factory Methods](#tryadd-with-factory-methods)
    - [TryAdd with Generic Types](#tryadd-with-generic-types)
    - [TryAdd with Multiple Interfaces](#tryadd-with-multiple-interfaces)
    - [TryAdd Best Practices](#tryadd-best-practices)
    - [Important Notes](#important-notes)
  - [🚫 Assembly Scanning Filters](#-assembly-scanning-filters)
    - [Basic Filter Usage](#basic-filter-usage)
    - [Namespace Exclusion](#namespace-exclusion)
    - [Pattern Exclusion](#pattern-exclusion)
    - [Interface Exclusion](#interface-exclusion)
    - [Combining Multiple Filters](#combining-multiple-filters)
    - [Multiple Filter Attributes](#multiple-filter-attributes)
    - [Real-World Example](#real-world-example)
    - [Filter Priority and Behavior](#filter-priority-and-behavior)
    - [Verification](#verification)
    - [Best Practices](#best-practices)
  - [🎯 Runtime Filtering](#-runtime-filtering)
    - [Basic Usage](#basic-usage)
    - [🔹 Filter by Type](#-filter-by-type)
    - [🔹 Filter by Namespace](#-filter-by-namespace)
    - [🔹 Filter by Pattern](#-filter-by-pattern)
    - [🔹 Combining Filters](#-combining-filters)
    - [🔹 Filters with Transitive Registration](#-filters-with-transitive-registration)
    - [Runtime vs. Compile-Time Filtering](#runtime-vs-compile-time-filtering)
    - [Complete Example: Multi-Application Scenario](#complete-example-multi-application-scenario)
    - [Best Practices](#best-practices-1)
    - [Verification](#verification-1)
  - [🎨 Decorator Pattern](#-decorator-pattern)
    - [✨ How It Works](#-how-it-works)
    - [📝 Basic Example](#-basic-example)
    - [Generated Code](#generated-code)
    - [🔄 Multiple Decorators](#-multiple-decorators)
    - [🎯 Common Use Cases](#-common-use-cases)
      - [1. Logging/Auditing](#1-loggingauditing)
      - [2. Caching](#2-caching)
      - [3. Validation](#3-validation)
      - [4. Retry Logic](#4-retry-logic)
    - [⚠️ Important Notes](#️-important-notes)
    - [🔍 Complete Example](#-complete-example)
  - [📚 Additional Examples](#-additional-examples)

---

## 🚀 Get Started - Quick Guide

This guide demonstrates a realistic 3-layer architecture for a PetStore application using minimal APIs.

### 📂 Project Structure

```
PetStore.sln
├── PetStore.Api/              (Presentation layer)
├── PetStore.Domain/           (Business logic layer)
└── PetStore.DataAccess/       (Data access layer)
```

### 1️⃣ Setup Projects

**PetStore.DataAccess.csproj** (Base layer):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
  </ItemGroup>
</Project>
```

**PetStore.Domain.csproj** (Middle layer):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />

    <ProjectReference Include="..\PetStore.DataAccess\PetStore.DataAccess.csproj" />
  </ItemGroup>
</Project>
```

**PetStore.Api.csproj** (Top layer):

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />

    <ProjectReference Include="..\PetStore.Domain\PetStore.Domain.csproj" />
  </ItemGroup>
</Project>
```

> **Note:** The `Microsoft.Extensions.DependencyInjection.Abstractions` package is needed for projects that use the `IServiceCollection` extension methods. Logging requires `Microsoft.Extensions.Logging.Abstractions`.

### 2️⃣ Data Access Layer (PetStore.DataAccess) 💾

**Models/Pet.cs**:

```csharp
namespace PetStore.DataAccess.Models;

public class Pet
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Species { get; set; }
    public int Age { get; set; }
}
```

**Repositories/IPetRepository.cs**:

```csharp
namespace PetStore.DataAccess.Repositories;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(int id);
    Task<IEnumerable<Pet>> GetAllAsync();
    Task<int> CreateAsync(Pet pet);
    Task<bool> UpdateAsync(Pet pet);
    Task<bool> DeleteAsync(int id);
}
```

**Repositories/PetRepository.cs**:

```csharp
using Atc.DependencyInjection;
using PetStore.DataAccess.Models;

namespace PetStore.DataAccess.Repositories;

[Registration(Lifetime.Scoped)]
public class PetRepository : IPetRepository
{
    // In-memory storage for demo purposes
    private static readonly List<Pet> InMemoryPets = new()
    {
        new Pet { Id = 1, Name = "Buddy", Species = "Dog", Age = 3 },
        new Pet { Id = 2, Name = "Whiskers", Species = "Cat", Age = 5 },
        new Pet { Id = 3, Name = "Goldie", Species = "Fish", Age = 1 }
    };

    private static int _nextId = 4;

    public Task<Pet?> GetByIdAsync(int id)
    {
        var pet = InMemoryPets.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(pet);
    }

    public Task<IEnumerable<Pet>> GetAllAsync()
    {
        return Task.FromResult(InMemoryPets.AsEnumerable());
    }

    public Task<int> CreateAsync(Pet pet)
    {
        pet.Id = _nextId++;
        InMemoryPets.Add(pet);
        return Task.FromResult(pet.Id);
    }

    public Task<bool> UpdateAsync(Pet pet)
    {
        var existing = InMemoryPets.FirstOrDefault(p => p.Id == pet.Id);
        if (existing is null) return Task.FromResult(false);

        existing.Name = pet.Name;
        existing.Species = pet.Species;
        existing.Age = pet.Age;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var pet = InMemoryPets.FirstOrDefault(p => p.Id == id);
        if (pet is null) return Task.FromResult(false);

        InMemoryPets.Remove(pet);
        return Task.FromResult(true);
    }
}
```

### 3️⃣ Domain Layer (PetStore.Domain) 🧠

**Models/PetDto.cs**:

```csharp
namespace PetStore.Domain.Models;

public record PetDto(int Id, string Name, string Species, int Age);

public record CreatePetRequest(string Name, string Species, int Age);

public record UpdatePetRequest(string Name, string Species, int Age);

public record ValidationResult(bool IsValid, List<string> Errors);
```

**Services/IPetService.cs**:

```csharp
namespace PetStore.Domain.Services;

public interface IPetService
{
    Task<PetDto?> GetPetAsync(int id);
    Task<IEnumerable<PetDto>> GetAllPetsAsync();
    Task<int> CreatePetAsync(CreatePetRequest request);
    Task<bool> UpdatePetAsync(int id, UpdatePetRequest request);
    Task<bool> DeletePetAsync(int id);
}
```

**Services/PetService.cs**:

```csharp
using Atc.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetStore.DataAccess.Models;
using PetStore.DataAccess.Repositories;
using PetStore.Domain.Models;

namespace PetStore.Domain.Services;

[Registration(Lifetime.Scoped)]
public class PetService : IPetService
{
    private readonly IPetRepository _repository;
    private readonly ILogger<PetService> _logger;

    public PetService(IPetRepository repository, ILogger<PetService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PetDto?> GetPetAsync(int id)
    {
        _logger.LogInformation("Getting pet with ID {PetId}", id);
        var pet = await _repository.GetByIdAsync(id);
        return pet is not null ? MapToDto(pet) : null;
    }

    public async Task<IEnumerable<PetDto>> GetAllPetsAsync()
    {
        _logger.LogInformation("Getting all pets");
        var pets = await _repository.GetAllAsync();
        return pets.Select(MapToDto);
    }

    public async Task<int> CreatePetAsync(CreatePetRequest request)
    {
        _logger.LogInformation("Creating pet: {PetName}", request.Name);
        var pet = new Pet
        {
            Name = request.Name,
            Species = request.Species,
            Age = request.Age
        };
        return await _repository.CreateAsync(pet);
    }

    public async Task<bool> UpdatePetAsync(int id, UpdatePetRequest request)
    {
        _logger.LogInformation("Updating pet with ID {PetId}", id);
        var pet = new Pet
        {
            Id = id,
            Name = request.Name,
            Species = request.Species,
            Age = request.Age
        };
        return await _repository.UpdateAsync(pet);
    }

    public async Task<bool> DeletePetAsync(int id)
    {
        _logger.LogInformation("Deleting pet with ID {PetId}", id);
        return await _repository.DeleteAsync(id);
    }

    private static PetDto MapToDto(Pet pet) => new(pet.Id, pet.Name, pet.Species, pet.Age);
}
```

**Validators/IPetValidator.cs**:

```csharp
using PetStore.Domain.Models;

namespace PetStore.Domain.Validators;

public interface IPetValidator
{
    ValidationResult Validate(CreatePetRequest request);
}
```

**Validators/PetValidator.cs**:

```csharp
using Atc.DependencyInjection;
using PetStore.Domain.Models;

namespace PetStore.Domain.Validators;

[Registration]  // Default Singleton lifetime
public class PetValidator : IPetValidator
{
    public ValidationResult Validate(CreatePetRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Pet name is required");

        if (request.Age < 0)
            errors.Add("Pet age must be positive");

        return new ValidationResult(errors.Count == 0, errors);
    }
}
```

### 4️⃣ API Layer (PetStore.Api) 🌐

**Handlers/IPetHandlers.cs**:

```csharp
namespace PetStore.Api.Handlers;

public interface IPetHandlers
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
```

**Handlers/PetHandlers.cs**:

```csharp
using Atc.DependencyInjection;
using PetStore.Domain.Models;
using PetStore.Domain.Services;
using PetStore.Domain.Validators;

namespace PetStore.Api.Handlers;

[Registration]  // Default Singleton lifetime
public class PetHandlers : IPetHandlers
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var pets = app.MapGroup("/api/pets").WithTags("Pets");

        pets.MapGet("/", GetAllPets)
            .WithName("GetAllPets");

        pets.MapGet("/{id:int}", GetPet)
            .WithName("GetPet");

        pets.MapPost("/", CreatePet)
            .WithName("CreatePet");

        pets.MapPut("/{id:int}", UpdatePet)
            .WithName("UpdatePet");

        pets.MapDelete("/{id:int}", DeletePet)
            .WithName("DeletePet");
    }

    private static async Task<IResult> GetAllPets(IPetService petService)
    {
        var pets = await petService.GetAllPetsAsync();
        return Results.Ok(pets);
    }

    private static async Task<IResult> GetPet(int id, IPetService petService)
    {
        var pet = await petService.GetPetAsync(id);
        return pet is not null ? Results.Ok(pet) : Results.NotFound();
    }

    private static async Task<IResult> CreatePet(
        CreatePetRequest request,
        IPetService petService,
        IPetValidator validator)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
            return Results.BadRequest(new { errors = validation.Errors });

        var id = await petService.CreatePetAsync(request);
        return Results.Created($"/api/pets/{id}", new { id });
    }

    private static async Task<IResult> UpdatePet(
        int id,
        UpdatePetRequest request,
        IPetService petService)
    {
        var success = await petService.UpdatePetAsync(id, request);
        return success ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeletePet(int id, IPetService petService)
    {
        var success = await petService.DeletePetAsync(id);
        return success ? Results.NoContent() : Results.NotFound();
    }
}
```

### 5️⃣ Program.cs (Minimal API Setup) ⚡

**Option 1 - Manual Registration (Scenario A):**

```csharp
using Atc.DependencyInjection;
using PetStore.Api.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register all services from all three layers manually
builder.Services.AddDependencyRegistrationsFromDataAccess();
builder.Services.AddDependencyRegistrationsFromDomain();
builder.Services.AddDependencyRegistrationsFromApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints using handler
var petHandlers = app.Services.GetRequiredService<IPetHandlers>();
petHandlers.MapEndpoints(app);

app.Run();
```

**Option 2 - Transitive Registration (Scenario B - Recommended):**

```csharp
using Atc.DependencyInjection;
using PetStore.Api.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Single call registers all services from Domain + DataAccess automatically!
builder.Services.AddDependencyRegistrationsFromDomain(includeReferencedAssemblies: true);

// Only need to register API layer services separately
builder.Services.AddDependencyRegistrationsFromApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints using handler
var petHandlers = app.Services.GetRequiredService<IPetHandlers>();
petHandlers.MapEndpoints(app);

app.Run();
```

### 🎨 What Gets Generated

For each project, the generator creates an assembly-specific extension method (with smart naming):

**PetStore.DataAccess** → `AddDependencyRegistrationsFromDataAccess()` (suffix "DataAccess" is unique)

```csharp
services.AddScoped<IPetRepository, PetRepository>();
```

**PetStore.Domain** → `AddDependencyRegistrationsFromDomain()` (suffix "Domain" is unique)

```csharp
services.AddScoped<IPetService, PetService>();
services.AddSingleton<IPetValidator, PetValidator>();
```

**PetStore.Api** → `AddDependencyRegistrationsFromApi()` (suffix "Api" is unique)

```csharp
services.AddSingleton<IPetHandlers, PetHandlers>();
```

### 6️⃣ Testing the Application 🧪

**Build and run:**

```bash
dotnet run --project PetStore.Api
```

**Test the endpoints:**

```bash
# Get all pets
curl http://localhost:5265/api/pets/

# Get specific pet
curl http://localhost:5265/api/pets/1

# Create a new pet
curl -X POST http://localhost:5265/api/pets/ \
  -H "Content-Type: application/json" \
  -d '{"name":"Max","species":"Dog","age":2}'

# Test validation (should return errors)
curl -X POST http://localhost:5265/api/pets/ \
  -H "Content-Type: application/json" \
  -d '{"name":"","species":"Cat","age":-1}'

# Update a pet
curl -X PUT http://localhost:5265/api/pets/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"Buddy Updated","species":"Dog","age":4}'

# Delete a pet
curl -X DELETE http://localhost:5265/api/pets/1
```

**Expected output:**

- All endpoints work with proper dependency injection
- Validation errors return structured JSON: `{"errors":["Pet name is required","Pet age must be positive"]}`
- Logging appears in console for all service operations

### 🔍 Viewing Generated Code (Optional)

To see what code the generator creates, add this to your `.csproj` files:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files will appear in `obj/Generated/Atc.SourceGenerators/`:

- `RegistrationAttribute.g.cs` - The `[Registration]` attribute definition
- `ServiceCollectionExtensions.g.cs` - The `AddDependencyRegistrationsFrom...()` method

### 🎯 Key Takeaways

This quick guide demonstrated:

✅ **Zero boilerplate** - No manual `services.AddScoped<IFoo, Foo>()` calls
✅ **Type safety** - Compile-time validation of registrations
✅ **Auto-detection** - Automatically registers against implemented interfaces
✅ **Multi-project support** - Each assembly gets its own registration method
✅ **Flexible lifetimes** - Singleton, Scoped, and Transient support
✅ **Clean architecture** - Clear separation between layers

Compare this to manual registration:

```csharp
// ❌ Without Source Generator (manual, error-prone)
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddSingleton<IPetValidator, PetValidator>();
builder.Services.AddSingleton<IPetHandlers, PetHandlers>();
// ... and so on for every service

// ✅ With Source Generator (automatic, type-safe, smart naming)
builder.Services.AddDependencyRegistrationsFromDataAccess();
builder.Services.AddDependencyRegistrationsFromDomain();
builder.Services.AddDependencyRegistrationsFromApi();
```

---

## ✨ Features

- **Automatic Service Registration**: Decorate classes with `[Registration]` attribute for automatic DI registration
- **Generic Interface Registration**: Full support for open generic types like `IRepository<T>` and `IHandler<TRequest, TResponse>` 🆕
- **Keyed Service Registration**: Multiple implementations of the same interface with different keys (.NET 8+)
- **Factory Method Registration**: Custom initialization logic via static factory methods
- **TryAdd* Registration**: Conditional registration for default implementations (library pattern)
- **Conditional Registration**: Register services based on configuration values (feature flags, environment-specific services) 🆕
- **Assembly Scanning Filters**: Exclude types by namespace, pattern (wildcards), or interface implementation
- **Runtime Filtering**: Exclude services at registration time with method parameters (different apps, different service subsets) 🆕
- **Hosted Service Support**: Automatically detects `BackgroundService` and `IHostedService` implementations and uses `AddHostedService<T>()`
- **Interface Auto-Detection**: Automatically registers against all implemented interfaces (no `As` parameter needed!)
- **Smart Filtering**: System interfaces (IDisposable, etc.) are automatically excluded
- **Multiple Interface Support**: Services implementing multiple interfaces are registered against all of them
- **Flexible Lifetimes**: Support for Singleton, Scoped, and Transient service lifetimes
- **Explicit Override**: Optional `As` parameter to override auto-detection when needed
- **Dual Registration**: Register services as both interface and concrete type with `AsSelf`
- **Compile-time Validation**: Diagnostics for common errors (invalid interface types, missing implementations, incorrect hosted service lifetimes)
- **Zero Runtime Overhead**: All code is generated at compile time
- **Native AOT Compatible**: No reflection or runtime code generation - fully trimming-safe and AOT-ready
- **Multi-Project Support**: Each project generates its own registration method

---

## 📦 Installation

Add the NuGet package to each project that contains services to register:

**Required:**

```bash
dotnet add package Atc.SourceGenerators
```

**Optional (recommended for better IntelliSense):**

```bash
dotnet add package Atc.SourceGenerators.Annotations
```

Or add to your `.csproj` file:

```xml
<ItemGroup>
  <!-- Required: Source generator -->
  <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />

  <!-- Optional: Attribute definitions with XML documentation -->
  <PackageReference Include="Atc.SourceGenerators.Annotations" Version="1.0.0" />
</ItemGroup>
```

**Note:** The generator emits fallback attributes automatically, so the Annotations package is optional. However, it provides better XML documentation and IntelliSense. If you include it, suppress the expected CS0436 warning: `<NoWarn>$(NoWarn);CS0436</NoWarn>`

The generator runs at compile time and has zero runtime overhead.

---

## 💡 Basic Usage

### 1️⃣ Add Using Directives

```csharp
using Atc.DependencyInjection;
```

### 2️⃣ Decorate Your Services

```csharp
// Simple registration with default singleton lifetime
[Registration]
public class CacheService
{
    public void Store(string key, string value) { }
}

// Interface auto-detection
[Registration]
public class UserService : IUserService
{
    public void CreateUser(string name) { }
}

// Scoped lifetime
[Registration(Lifetime.Scoped)]
public class OrderService : IOrderService
{
    public void ProcessOrder(int orderId) { }
}

// Transient lifetime
[Registration(Lifetime.Transient)]
public class LoggerService
{
    public void Log(string message) { }
}
```

### 3️⃣ Register in DI Container

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Call the generated extension method
services.AddDependencyRegistrationsFromYourAssemblyName();

var serviceProvider = services.BuildServiceProvider();
```

---

## 🏗️ Multi-Project Setup

When using the generator across multiple projects, each project generates its own extension method with a unique name based on the assembly name.

### 📁 Example Structure

```
Solution/
├── MyApp.Api/          → AddDependencyRegistrationsFromApi()
├── MyApp.Domain/       → AddDependencyRegistrationsFromDomain()
└── MyApp.DataAccess/   → AddDependencyRegistrationsFromDataAccess()
```

### ⚡ Program.cs Registration

**Scenario A - Manual Registration (Traditional):**

Call all registration methods explicitly in your startup:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services from all projects manually
builder.Services.AddDependencyRegistrationsFromDataAccess();
builder.Services.AddDependencyRegistrationsFromDomain();
builder.Services.AddDependencyRegistrationsFromApi();

var app = builder.Build();
```

**Scenario B - Transitive Registration (Recommended):**

Let the generator automatically register services from referenced assemblies:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Single call automatically registers:
//   - Services from MyApp.Domain
//   - Services from MyApp.DataAccess (referenced by Domain)
builder.Services.AddDependencyRegistrationsFromDomain(includeReferencedAssemblies: true);

var app = builder.Build();
```

### 🔄 Transitive Dependency Registration

Each generated registration method has **4 overloads** to support different registration scenarios:

```csharp
// Overload 1: Default (no transitive registration)
builder.Services.AddDependencyRegistrationsFromDomain();

// Overload 2: Auto-detect ALL referenced assemblies recursively
builder.Services.AddDependencyRegistrationsFromDomain(includeReferencedAssemblies: true);

// Overload 3: Register specific referenced assembly by name (short or full name)
builder.Services.AddDependencyRegistrationsFromDomain("DataAccess");
builder.Services.AddDependencyRegistrationsFromDomain("MyApp.DataAccess");

// Overload 4: Register multiple specific referenced assemblies
builder.Services.AddDependencyRegistrationsFromDomain("DataAccess", "Infrastructure");
```

**How It Works:**

1. **Auto-detect mode** (`includeReferencedAssemblies: true`):
   - Scans ALL referenced assemblies that contain `[Registration]` attributes
   - Recursively registers services from the entire dependency chain
   - Example: Domain → DataAccess → Infrastructure (all registered with ONE call)

2. **Manual mode** (assembly name parameters):
   - Only registers assemblies with matching prefix (e.g., "MyApp.*")
   - Supports both full names ("MyApp.DataAccess") and short names ("DataAccess")
   - Allows fine-grained control over which assemblies to include

3. **Silent skip**:
   - If a specified assembly doesn't exist or has no registrations, it's silently skipped
   - No compile-time or runtime errors

**Benefits:**

✅ **Clean Architecture**: Api → Domain → DataAccess (no need for Api to reference DataAccess directly)
✅ **Less Boilerplate**: One registration call instead of many
✅ **Maintainable**: Adding new projects doesn't require updating Program.cs
✅ **Type-Safe**: All registrations validated at compile time

### 🏷️ Method Naming Convention

The generator creates extension methods based on assembly names with **smart naming**:

| Assembly Name | Smart Naming Result |
|---------------|---------------------|
| `PetStore.Domain` (unique suffix) | `AddDependencyRegistrationsFromDomain()` |
| `PetStore.DataAccess` (unique suffix) | `AddDependencyRegistrationsFromDataAccess()` |
| `PetStore.Domain` + `AnotherApp.Domain` | `AddDependencyRegistrationsFromPetStoreDomain()` and `AddDependencyRegistrationsFromAnotherAppDomain()` |

Assembly names are sanitized to create valid C# identifiers (dots, dashes, spaces removed).

### ✨ Smart Naming

The generator uses **smart suffix-based naming** to create cleaner, more readable method names:

**How it works:**

- ✅ If the assembly suffix (last segment after final dot) is **unique** among all assemblies → use short suffix
- ⚠️ If multiple assemblies have the **same suffix** → use full sanitized name to avoid conflicts

**Examples:**

```csharp
// ✅ Unique suffixes (cleaner names)
PetStore.Domain     → AddDependencyRegistrationsFromDomain()
PetStore.DataAccess → AddDependencyRegistrationsFromDataAccess()
PetStore.Api        → AddDependencyRegistrationsFromApi()

// ⚠️ Conflicting suffixes (full names prevent collisions)
PetStore.Domain     → AddDependencyRegistrationsFromPetStoreDomain()
AnotherApp.Domain   → AddDependencyRegistrationsFromAnotherAppDomain()
```

**Benefits:**

- 🎯 **Cleaner API**: Shorter method names when there are no conflicts
- 🛡️ **Automatic Conflict Prevention**: Fallback to full names prevents naming collisions
- ⚡ **Zero Configuration**: Works automatically based on compilation context
- 🔄 **Context-Aware**: Method names adapt to the assemblies in your solution

---

## 🔍 Auto-Detection

The generator automatically detects and registers services against their implemented interfaces:

### 1️⃣ Single Interface

```csharp
public interface IUserService { }

[Registration]
public class UserService : IUserService { }
```

**Generated:**

```csharp
services.AddSingleton<IUserService, UserService>();
```

### 🔢 Multiple Interfaces

```csharp
public interface IEmailService { }
public interface INotificationService { }

[Registration]
public class EmailService : IEmailService, INotificationService { }
```

**Generated:**

```csharp
services.AddSingleton<IEmailService, EmailService>();
services.AddSingleton<INotificationService, EmailService>();
```

### 🧹 System Interfaces Filtered

System and Microsoft namespace interfaces are automatically excluded:

```csharp
[Registration]
public class CacheService : IDisposable
{
    public void Dispose() { }
}
```

**Generated:**

```csharp
services.AddSingleton<CacheService>(); // IDisposable ignored
```

### 🎯 Explicit Override

Use `As` parameter to register against a specific interface only:

```csharp
[Registration(As = typeof(IUserService))]
public class UserService : IUserService, INotificationService { }
```

**Generated:**

```csharp
services.AddSingleton<IUserService, UserService>(); // Only IUserService
```

### 🔀 Register As Both Interface and Concrete Type

Use `AsSelf = true` to register both ways:

```csharp
[Registration(AsSelf = true)]
public class EmailService : IEmailService { }
```

**Generated:**

```csharp
services.AddSingleton<IEmailService, EmailService>();
services.AddSingleton<EmailService>();
```

---

## ⏱️ Service Lifetimes

### 🔒 Singleton (Default)

Single instance for the entire application lifetime:

```csharp
[Registration] // or [Registration(Lifetime.Singleton)]
public class CacheService { }
```

**Use for:** Stateless services, shared state, expensive-to-create objects

### 🔄 Scoped

New instance per scope (e.g., per HTTP request):

```csharp
[Registration(Lifetime.Scoped)]
public class OrderService : IOrderService { }
```

**Use for:** Database contexts, request-specific services, unit of work pattern

### ⚡ Transient

New instance every time it's requested:

```csharp
[Registration(Lifetime.Transient)]
public class LoggerService { }
```

**Use for:** Lightweight, stateless services, services that shouldn't be shared

---

## 🚀 Native AOT Compatibility

The Dependency Registration Generator is **fully compatible with Native AOT** compilation, making it ideal for modern .NET applications that require fast startup times and minimal deployment sizes.

### ✅ Why It Works

All registration code is generated at compile time with **no reflection or runtime code generation**:

```csharp
// Your attributed service
[Registration(Lifetime.Scoped)]
public class UserService : IUserService { }

// Generated code (compile-time, no reflection!)
public static class DependencyRegistrationExtensions
{
    public static IServiceCollection AddDependencyRegistrationsFromApp(
        this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
```

### 🎯 Key Benefits

- **✅ No reflection required**: All service registration happens through direct method calls
- **✅ Fully trimming-safe**: No dynamic code means the trimmer can safely remove unused code
- **✅ AOT-ready**: All dependencies are resolved at compile time
- **✅ Faster startup**: No runtime scanning or reflection overhead
- **✅ Smaller deployments**: Dead code elimination works perfectly
- **✅ Better performance**: Native code execution with zero runtime overhead

### 🚀 Native AOT Example

```xml
<!-- YourProject.csproj -->
<PropertyGroup>
  <PublishAot>true</PublishAot>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />
</ItemGroup>
```

```csharp
// Your services work seamlessly with Native AOT
[Registration(Lifetime.Singleton)]
public class CacheService : ICacheService { }

[Registration(Lifetime.Scoped)]
public class UserService : IUserService { }

// Program.cs
var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddDependencyRegistrationsFromApp();
var app = builder.Build();
```

**Result:** Fast startup, minimal memory footprint, and production-ready native binaries.

---

## ⚙️ RegistrationAttribute Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `lifetime` | `Lifetime` | `Singleton` | Service lifetime (Singleton, Scoped, or Transient) |
| `As` | `Type?` | `null` | Explicit interface type to register against (overrides auto-detection) |
| `AsSelf` | `bool` | `false` | Also register the concrete type when interfaces are detected/specified |
| `Key` | `object?` | `null` | Service key for keyed service registration (.NET 8+) |
| `Factory` | `string?` | `null` | Name of static factory method for custom initialization |
| `TryAdd` | `bool` | `false` | Use TryAdd* methods for conditional registration (library pattern) |
| `Decorator` | `bool` | `false` | Mark this service as a decorator that wraps the previous registration of the same interface |
| `Condition` | `string?` | `null` | Configuration key path for conditional registration (feature flags). Prefix with "!" for negation |

### 📝 Examples

```csharp
// Default singleton, auto-detect interfaces
[Registration]

// Explicit lifetime
[Registration(Lifetime.Scoped)]

// Explicit interface
[Registration(As = typeof(IUserService))]

// Combination
[Registration(Lifetime.Transient, As = typeof(ILogger))]

// Also register concrete type
[Registration(AsSelf = true)]

// Keyed service
[Registration(Key = "Primary", As = typeof(ICache))]

// Factory method
[Registration(Factory = nameof(Create))]

// TryAdd registration
[Registration(TryAdd = true)]

// Decorator pattern
[Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]

// Conditional registration
[Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]

// Negated conditional
[Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]

// All parameters
[Registration(Lifetime.Scoped, As = typeof(IService), AsSelf = true, TryAdd = true)]
```

---

## 🛡️ Diagnostics

The generator provides compile-time diagnostics to catch common errors:

### ❌ ATCDIR001: As Type Must Be Interface

**Severity:** Error

**Description:** The type specified in `As` parameter must be an interface.

```csharp
// ❌ Error: BaseService is a class, not an interface
[Registration(As = typeof(BaseService))]
public class UserService : BaseService { }
```

**Fix:** Use an interface type or remove the `As` parameter.

### ❌ ATCDIR002: Class Does Not Implement Interface

**Severity:** Error

**Description:** Class does not implement the interface specified in `As` parameter.

```csharp
public interface IUserService { }

// ❌ Error: UserService doesn't implement IUserService
[Registration(As = typeof(IUserService))]
public class UserService { }
```

**Fix:** Implement the interface or remove the `As` parameter.

### ⚠️ ATCDIR003: Duplicate Registration with Different Lifetime

**Severity:** Warning

**Description:** Service is registered multiple times with different lifetimes.

```csharp
public interface IUserService { }

// ⚠️ Warning: Same interface registered with two different lifetimes
[Registration(Lifetime.Singleton)]
public class UserServiceSingleton : IUserService { }

[Registration(Lifetime.Scoped)]
public class UserServiceScoped : IUserService { }
```

**Fix:** Ensure consistent lifetimes or use different interfaces.

### ❌ ATCDIR004: Hosted Services Must Use Singleton Lifetime

**Severity:** Error

**Description:** Hosted services (BackgroundService or IHostedService implementations) must use Singleton lifetime.

```csharp
// ❌ Error: Hosted services cannot use Scoped or Transient lifetime
[Registration(Lifetime.Scoped)]
public class MyBackgroundService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
```

**Fix:** Use Singleton lifetime (or default [Registration]) for hosted services:

```csharp
// ✅ Correct: Singleton lifetime (explicit)
[Registration(Lifetime.Singleton)]
public class MyBackgroundService : BackgroundService { }

// ✅ Correct: Default lifetime is Singleton
[Registration]
public class MyBackgroundService : BackgroundService { }
```

**Generated Registration:**

```csharp
services.AddHostedService<MyBackgroundService>();
```

---

## 🔷 Generic Interface Registration

The generator supports open generic types, enabling the repository pattern and other generic service patterns.

### Single Type Parameter

```csharp
// Generic interface
public interface IRepository<T> where T : class
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
}

// Generic implementation
[Registration(Lifetime.Scoped)]
public class Repository<T> : IRepository<T> where T : class
{
    public T? GetById(int id) => /* implementation */;
    public IEnumerable<T> GetAll() => /* implementation */;
    public void Add(T entity) => /* implementation */;
}
```

**Generated Code:**

```csharp
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

**Usage:**

```csharp
// Resolve for specific types
var userRepository = serviceProvider.GetRequiredService<IRepository<User>>();
var productRepository = serviceProvider.GetRequiredService<IRepository<Product>>();
```

### Multiple Type Parameters

```csharp
// Handler interface with two type parameters
public interface IHandler<TRequest, TResponse>
{
    TResponse Handle(TRequest request);
}

[Registration(Lifetime.Transient)]
public class Handler<TRequest, TResponse> : IHandler<TRequest, TResponse>
{
    public TResponse Handle(TRequest request) => /* implementation */;
}
```

**Generated Code:**

```csharp
services.AddTransient(typeof(IHandler<,>), typeof(Handler<,>));
```

### Complex Constraints

```csharp
// Interface with multiple constraints
public interface IRepository<T>
    where T : class, IEntity, new()
{
    T Create();
    void Save(T entity);
}

[Registration(Lifetime.Scoped)]
public class Repository<T> : IRepository<T>
    where T : class, IEntity, new()
{
    public T Create() => new T();
    public void Save(T entity) => /* implementation */;
}
```

**Generated Code:**

```csharp
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

### Explicit Generic Registration

You can also use explicit `As` parameter with open generic types:

```csharp
[Registration(Lifetime.Scoped, As = typeof(IRepository<>))]
public class Repository<T> : IRepository<T> where T : class
{
    // Implementation
}
```

---

## 🔑 Keyed Service Registration

Register multiple implementations of the same interface and resolve them by key (.NET 8+).

### String Keys

```csharp
[Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Stripe")]
public class StripePaymentProcessor : IPaymentProcessor
{
    public Task ProcessPaymentAsync(decimal amount) { /* Stripe implementation */ }
}

[Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "PayPal")]
public class PayPalPaymentProcessor : IPaymentProcessor
{
    public Task ProcessPaymentAsync(decimal amount) { /* PayPal implementation */ }
}
```

**Generated Code:**

```csharp
services.AddKeyedScoped<IPaymentProcessor, StripePaymentProcessor>("Stripe");
services.AddKeyedScoped<IPaymentProcessor, PayPalPaymentProcessor>("PayPal");
```

**Usage:**

```csharp
// Constructor injection with [FromKeyedServices]
public class CheckoutService(
    [FromKeyedServices("Stripe")] IPaymentProcessor stripeProcessor,
    [FromKeyedServices("PayPal")] IPaymentProcessor paypalProcessor)
{
    // Use specific implementations
}

// Manual resolution
var stripeProcessor = serviceProvider.GetRequiredKeyedService<IPaymentProcessor>("Stripe");
var paypalProcessor = serviceProvider.GetRequiredKeyedService<IPaymentProcessor>("PayPal");
```

### Generic Keyed Services

Keyed services work with generic types:

```csharp
[Registration(Lifetime.Scoped, As = typeof(IRepository<>), Key = "Primary")]
public class PrimaryRepository<T> : IRepository<T> where T : class
{
    public T? GetById(int id) => /* Primary database */;
}

[Registration(Lifetime.Scoped, As = typeof(IRepository<>), Key = "ReadOnly")]
public class ReadOnlyRepository<T> : IRepository<T> where T : class
{
    public T? GetById(int id) => /* Read-only replica */;
}
```

**Generated Code:**

```csharp
services.AddKeyedScoped(typeof(IRepository<>), "Primary", typeof(PrimaryRepository<>));
services.AddKeyedScoped(typeof(IRepository<>), "ReadOnly", typeof(ReadOnlyRepository<>));
```

---

## 🏭 Factory Method Registration

Factory methods allow custom initialization logic for services that require configuration values, conditional setup, or complex dependencies.

### Basic Factory Method

```csharp
[Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
public class EmailSender : IEmailSender
{
    private readonly string smtpHost;
    private readonly int smtpPort;

    private EmailSender(string smtpHost, int smtpPort)
    {
        this.smtpHost = smtpHost;
        this.smtpPort = smtpPort;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Implementation...
    }

    /// <summary>
    /// Factory method for creating EmailSender instances.
    /// Must be static and accept IServiceProvider as parameter.
    /// </summary>
    public static IEmailSender CreateEmailSender(IServiceProvider serviceProvider)
    {
        // Resolve configuration from DI container
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var smtpHost = config["Email:SmtpHost"] ?? "smtp.example.com";
        var smtpPort = int.Parse(config["Email:SmtpPort"] ?? "587");

        return new EmailSender(smtpHost, smtpPort);
    }
}
```

**Generated Code:**

```csharp
services.AddScoped<IEmailSender>(sp => EmailSender.CreateEmailSender(sp));
```

### Factory Method Requirements

- ✅ Must be `static`
- ✅ Must accept `IServiceProvider` as the single parameter
- ✅ Must return the service type (interface specified in `As` parameter, or class type if no `As` specified)
- ✅ Can be `public`, `internal`, or `private`

### Factory Method with Multiple Interfaces

```csharp
[Registration(Lifetime.Singleton, Factory = nameof(CreateService))]
public class CacheService : ICacheService, IHealthCheck
{
    private readonly string connectionString;

    private CacheService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public static ICacheService CreateService(IServiceProvider sp)
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var connString = config.GetConnectionString("Redis");
        return new CacheService(connString);
    }

    // ICacheService members...
    // IHealthCheck members...
}
```

**Generated Code:**

```csharp
// Registers against both interfaces using the same factory
services.AddSingleton<ICacheService>(sp => CacheService.CreateService(sp));
services.AddSingleton<IHealthCheck>(sp => CacheService.CreateService(sp));
```

### Factory Method Best Practices

**When to Use Factory Methods:**

- ✅ Service requires configuration values from `IConfiguration`
- ✅ Conditional initialization based on runtime environment
- ✅ Complex dependency resolution beyond constructor injection
- ✅ Services with private constructors that require initialization

**When NOT to Use Factory Methods:**

- ❌ Simple services with no special initialization - use regular constructor injection
- ❌ Services that can use `IOptions<T>` pattern instead
- ❌ When factory logic is overly complex - consider using a dedicated factory class

### Factory Method Diagnostics

The generator provides compile-time validation:

**ATCDIR005: Factory method not found**

```csharp
// ❌ Error: Factory method doesn't exist
[Registration(Factory = "NonExistentMethod")]
public class MyService : IMyService { }
```

**ATCDIR006: Invalid factory method signature**

```csharp
// ❌ Error: Factory method must be static
[Registration(Factory = nameof(Create))]
public class MyService : IMyService
{
    public IMyService Create(IServiceProvider sp) => this;  // Not static!
}

// ❌ Error: Wrong parameter type
public static IMyService Create(string config) => new MyService();

// ❌ Error: Wrong return type
public static string Create(IServiceProvider sp) => "wrong";
```

**Correct signature:**

```csharp
// ✅ Correct: static, accepts IServiceProvider, returns service type
public static IMyService Create(IServiceProvider sp) => new MyService();
```

---

## 📦 Instance Registration

Instance registration allows you to register pre-created singleton instances via static fields, properties, or methods. This is ideal for immutable configuration objects or singleton instances that are initialized at startup.

### Basic Instance Registration (Static Field)

```csharp
[Registration(As = typeof(IAppConfiguration), Instance = nameof(DefaultInstance))]
public class AppConfiguration : IAppConfiguration
{
    // Static field providing the pre-created instance
    public static readonly AppConfiguration DefaultInstance = new()
    {
        ApplicationName = "My Application",
        Environment = "Production",
        MaxConnections = 100,
        IsDebugMode = false,
    };

    // Private constructor enforces singleton pattern
    private AppConfiguration() { }

    public string ApplicationName { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public int MaxConnections { get; init; }
    public bool IsDebugMode { get; init; }
}
```

**Generated Code:**

```csharp
services.AddSingleton<IAppConfiguration>(AppConfiguration.DefaultInstance);
```

### Instance Registration with Static Property

```csharp
[Registration(As = typeof(ICache), Instance = nameof(Instance))]
public class MemoryCache : ICache
{
    // Static property providing the singleton instance
    public static MemoryCache Instance { get; } = new MemoryCache();

    private MemoryCache() { }

    public void Set(string key, object value) { /* implementation */ }
    public object? Get(string key) { /* implementation */ }
}
```

**Generated Code:**

```csharp
services.AddSingleton<ICache>(MemoryCache.Instance);
```

### Instance Registration with Static Method

```csharp
[Registration(As = typeof(ILogger), Instance = nameof(GetDefaultLogger))]
public class DefaultLogger : ILogger
{
    private static readonly DefaultLogger instance = new();

    private DefaultLogger() { }

    // Static method returning the singleton instance
    public static DefaultLogger GetDefaultLogger() => instance;

    public void Log(string message) => Console.WriteLine($"[{DateTime.UtcNow:O}] {message}");
}
```

**Generated Code:**

```csharp
services.AddSingleton<ILogger>(DefaultLogger.GetDefaultLogger());
```

### Instance Registration Requirements

- ✅ The `Instance` parameter must reference a **static** field, property, or parameterless method
- ✅ Instance registration **requires Singleton lifetime** (enforced at compile-time)
- ✅ The member can be `public`, `internal`, or `private`
- ✅ Works with `TryAdd`: `services.TryAddSingleton<T>(ClassName.Instance)`
- ❌ **Cannot** be used with `Factory` parameter (mutually exclusive)
- ❌ **Cannot** be used with Scoped or Transient lifetimes

### Instance Registration with Multiple Interfaces

When a class implements multiple interfaces, the same instance is registered for each interface:

```csharp
[Registration(Instance = nameof(DefaultInstance))]
public class ServiceHub : IServiceA, IServiceB, IHealthCheck
{
    public static readonly ServiceHub DefaultInstance = new();

    private ServiceHub() { }

    // IServiceA members...
    // IServiceB members...
    // IHealthCheck members...
}
```

**Generated Code:**

```csharp
// Same instance registered for all interfaces
services.AddSingleton<IServiceA>(ServiceHub.DefaultInstance);
services.AddSingleton<IServiceB>(ServiceHub.DefaultInstance);
services.AddSingleton<IHealthCheck>(ServiceHub.DefaultInstance);
```

### Instance Registration Best Practices

**When to Use Instance Registration:**

- ✅ Immutable configuration objects with default values
- ✅ Pre-initialized singleton services (caches, registries, etc.)
- ✅ Singleton instances that should be shared across the application
- ✅ Services with complex initialization that should happen once at startup

**When NOT to Use Instance Registration:**

- ❌ Services that need different instances per scope/request - use factory methods instead
- ❌ Services requiring runtime configuration - use `IOptions<T>` pattern or factory methods
- ❌ Services with mutable state that shouldn't be shared
- ❌ Non-singleton lifetimes - instance registration only supports Singleton

### Instance Registration Diagnostics

The generator provides compile-time validation:

**❌ ATCDIR007: Instance member not found**

```csharp
[Registration(As = typeof(ICache), Instance = "NonExistentMember")]
public class CacheService : ICache { }
// Error: Member 'NonExistentMember' not found. Must be a static field, property, or method.
```

**❌ ATCDIR008: Instance member must be static**

```csharp
[Registration(As = typeof(ICache), Instance = nameof(InstanceField))]
public class CacheService : ICache
{
    public readonly CacheService InstanceField = new();  // Not static!
}
// Error: Instance member 'InstanceField' must be static.
```

**❌ ATCDIR009: Instance and Factory are mutually exclusive**

```csharp
[Registration(
    As = typeof(IEmailSender),
    Factory = nameof(Create),
    Instance = nameof(DefaultInstance))]  // Cannot use both!
public class EmailSender : IEmailSender { }
// Error: Cannot use both Instance and Factory parameters on the same service.
```

**❌ ATCDIR010: Instance registration requires Singleton lifetime**

```csharp
[Registration(Lifetime.Scoped, As = typeof(ICache), Instance = nameof(Instance))]
public class CacheService : ICache
{
    public static CacheService Instance { get; } = new();
}
// Error: Instance registration can only be used with Singleton lifetime. Current lifetime is 'Scoped'.
```

### Instance vs Factory Method

**Use Instance when:**

- The instance is pre-created and immutable
- No runtime dependencies or configuration needed
- Singleton pattern with guaranteed single instance

**Use Factory Method when:**

- Service requires `IServiceProvider` to resolve dependencies
- Runtime configuration is needed (from `IConfiguration`, environment, etc.)
- Conditional initialization based on runtime state

**Example comparison:**

```csharp
// Instance: Pre-created, immutable configuration
[Registration(As = typeof(IAppSettings), Instance = nameof(Default))]
public class AppSettings : IAppSettings
{
    public static readonly AppSettings Default = new() { Timeout = 30 };
}

// Factory: Runtime configuration from IConfiguration
[Registration(Lifetime.Singleton, As = typeof(IEmailClient), Factory = nameof(Create))]
public class EmailClient : IEmailClient
{
    public static IEmailClient Create(IServiceProvider sp)
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var apiKey = config["Email:ApiKey"];
        return new EmailClient(apiKey);
    }
}
```

---

## 🔄 TryAdd* Registration

TryAdd* registration enables conditional service registration that only adds services if they're not already registered. This is particularly useful for library authors who want to provide default implementations that can be easily overridden by application code.

### Basic TryAdd Registration

```csharp
[Registration(As = typeof(ILogger), TryAdd = true)]
public class DefaultLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[DefaultLogger] {message}");
    }
}
```

**Generated Code:**

```csharp
services.TryAddSingleton<ILogger, DefaultLogger>();
```

### How TryAdd Works

When `TryAdd = true`, the generator uses `TryAdd{Lifetime}()` methods instead of `Add{Lifetime}()`:

- `TryAddSingleton<T>()` - Only registers if no `T` is already registered
- `TryAddScoped<T>()` - Only registers if no `T` is already registered
- `TryAddTransient<T>()` - Only registers if no `T` is already registered

This allows consumers to override default implementations:

```csharp
// Application code (runs BEFORE library registration)
services.AddSingleton<ILogger, CustomLogger>();  // This takes precedence

// Library registration (uses TryAdd)
services.AddDependencyRegistrationsFromLibrary();
// DefaultLogger will NOT be registered because CustomLogger is already registered
```

### Library Author Pattern

TryAdd is perfect for libraries that want to provide sensible defaults:

```csharp
// Library code: PetStore.Domain
namespace PetStore.Domain.Services;

[Registration(Lifetime.Singleton, As = typeof(IHealthCheck), TryAdd = true)]
public class DefaultHealthCheck : IHealthCheck
{
    public Task<bool> CheckHealthAsync()
    {
        Console.WriteLine("DefaultHealthCheck: Performing basic health check (always healthy)");
        return Task.FromResult(true);
    }
}
```

**Consumer can override:**

```csharp
// Application code
services.AddSingleton<IHealthCheck, AdvancedHealthCheck>();  // Custom implementation
services.AddDependencyRegistrationsFromDomain();  // DefaultHealthCheck won't be added
```

**Or consumer can use default:**

```csharp
// Application code
services.AddDependencyRegistrationsFromDomain();  // DefaultHealthCheck is added
```

### TryAdd with Different Lifetimes

```csharp
// Scoped with TryAdd
[Registration(Lifetime.Scoped, As = typeof(ICache), TryAdd = true)]
public class DefaultCache : ICache
{
    public string Get(string key) => /* implementation */;
}

// Transient with TryAdd
[Registration(Lifetime.Transient, As = typeof(IMessageFormatter), TryAdd = true)]
public class DefaultMessageFormatter : IMessageFormatter
{
    public string Format(string message) => /* implementation */;
}
```

**Generated Code:**

```csharp
services.TryAddScoped<ICache, DefaultCache>();
services.TryAddTransient<IMessageFormatter, DefaultMessageFormatter>();
```

### TryAdd with Factory Methods

TryAdd works seamlessly with factory methods:

```csharp
[Registration(Lifetime.Singleton, As = typeof(IEmailSender), TryAdd = true, Factory = nameof(CreateEmailSender))]
public class DefaultEmailSender : IEmailSender
{
    private readonly string smtpHost;

    private DefaultEmailSender(string smtpHost)
    {
        this.smtpHost = smtpHost;
    }

    public static IEmailSender CreateEmailSender(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var host = config["Email:SmtpHost"] ?? "localhost";
        return new DefaultEmailSender(host);
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Implementation...
    }
}
```

**Generated Code:**

```csharp
services.TryAddSingleton<IEmailSender>(sp => DefaultEmailSender.CreateEmailSender(sp));
```

### TryAdd with Generic Types

TryAdd supports generic interface registration:

```csharp
[Registration(Lifetime.Scoped, TryAdd = true)]
public class DefaultRepository<T> : IRepository<T> where T : class
{
    public T? GetById(int id) => /* implementation */;
    public IEnumerable<T> GetAll() => /* implementation */;
}
```

**Generated Code:**

```csharp
services.TryAddScoped(typeof(IRepository<>), typeof(DefaultRepository<>));
```

### TryAdd with Multiple Interfaces

When a service implements multiple interfaces, TryAdd is applied to each registration:

```csharp
[Registration(TryAdd = true)]
public class DefaultNotificationService : IEmailNotificationService, ISmsNotificationService
{
    public Task SendEmailAsync(string email, string message) => /* implementation */;
    public Task SendSmsAsync(string phoneNumber, string message) => /* implementation */;
}
```

**Generated Code:**

```csharp
services.TryAddSingleton<IEmailNotificationService, DefaultNotificationService>();
services.TryAddSingleton<ISmsNotificationService, DefaultNotificationService>();
```

### TryAdd Best Practices

**When to Use TryAdd:**

- ✅ Library projects providing default implementations
- ✅ Fallback services that applications may want to customize
- ✅ Services with sensible defaults but customizable behavior
- ✅ Avoiding registration conflicts in modular applications

**When NOT to Use TryAdd:**

- ❌ Core application services that should always be registered
- ❌ Services where registration order matters for business logic
- ❌ When you need to explicitly override existing registrations (use regular registration)

### Important Notes

**Keyed Services:**
TryAdd is **not supported** with keyed services. When both `Key` and `TryAdd` are specified, the generator will prioritize keyed registration and ignore `TryAdd`:

```csharp
// ⚠️ TryAdd is ignored when Key is specified
[Registration(Key = "Primary", TryAdd = true)]
public class PrimaryCache : ICache { }

// Generated (keyed registration, no TryAdd):
services.AddKeyedSingleton<ICache, PrimaryCache>("Primary");
```

**Registration Order:**
For TryAdd to work correctly, ensure library registrations happen **after** application-specific registrations:

```csharp
// ✅ Correct order
services.AddSingleton<ILogger, CustomLogger>();  // Application override
services.AddDependencyRegistrationsFromLibrary();  // Library defaults (TryAdd)

// ❌ Wrong order
services.AddDependencyRegistrationsFromLibrary();  // Library defaults register first
services.AddSingleton<ILogger, CustomLogger>();  // This creates a duplicate registration!
```

---

## 🚫 Assembly Scanning Filters

Assembly Scanning Filters allow you to exclude specific types, namespaces, or patterns from automatic registration. This is particularly useful for:

- Excluding internal/test services from production builds
- Preventing mock/stub services from being registered
- Filtering out utilities that shouldn't be in the DI container

### Basic Filter Usage

Filters are applied using the `[RegistrationFilter]` attribute at the assembly level:

```csharp
using Atc.DependencyInjection;

// Exclude internal services
[assembly: RegistrationFilter(ExcludeNamespaces = new[] { "MyApp.Internal" })]

// Your services
namespace MyApp.Services
{
    [Registration]
    public class ProductionService : IProductionService { } // ✅ Will be registered
}

namespace MyApp.Internal
{
    [Registration]
    public class InternalService : IInternalService { } // ❌ Excluded by filter
}
```

### Namespace Exclusion

Exclude types in specific namespaces. Sub-namespaces are also excluded:

```csharp
[assembly: RegistrationFilter(ExcludeNamespaces = new[] {
    "MyApp.Internal",
    "MyApp.Testing",
    "MyApp.Utilities"
})]

namespace MyApp.Services
{
    [Registration]
    public class UserService : IUserService { } // ✅ Registered
}

namespace MyApp.Internal
{
    [Registration]
    public class InternalCache : ICache { } // ❌ Excluded
}

namespace MyApp.Internal.Deep.Nested
{
    [Registration]
    public class DeepService : IDeepService { } // ❌ Also excluded (sub-namespace)
}
```

**How Namespace Filtering Works:**

- Exact match: `"MyApp.Internal"` excludes types in that namespace
- Sub-namespace match: Also excludes `"MyApp.Internal.Something"`, `"MyApp.Internal.Deep.Nested"`, etc.

### Pattern Exclusion

Exclude types whose names match wildcard patterns. Supports `*` (any characters) and `?` (single character):

```csharp
[assembly: RegistrationFilter(ExcludePatterns = new[] {
    "*Mock*",     // Excludes MockEmailService, EmailMockService, etc.
    "*Test*",     // Excludes TestHelper, UserTestService, etc.
    "Temp*",      // Excludes TempService, TempCache, etc.
    "Old?Data"    // Excludes OldAData, OldBData, but NOT OldAbcData
})]

namespace MyApp.Services
{
    [Registration]
    public class ProductionEmailService : IEmailService { } // ✅ Registered

    [Registration]
    public class MockEmailService : IEmailService { } // ❌ Excluded (*Mock*)

    [Registration]
    public class UserTestHelper : ITestHelper { } // ❌ Excluded (*Test*)

    [Registration]
    public class TempCache : ICache { } // ❌ Excluded (Temp*)
}
```

**Pattern Matching Rules:**

- `*` matches zero or more characters
- `?` matches exactly one character
- Matching is case-insensitive
- Patterns match against the type name (not the full namespace)

### Interface Exclusion

Exclude types that implement specific interfaces:

```csharp
[assembly: RegistrationFilter(ExcludeImplementing = new[] {
    typeof(ITestUtility),
    typeof(IInternalTool)
})]

namespace MyApp.Services
{
    public interface ITestUtility { }
    public interface IProductionService { }

    [Registration]
    public class ProductionService : IProductionService { } // ✅ Registered

    [Registration]
    public class TestHelper : ITestUtility { } // ❌ Excluded (implements ITestUtility)

    [Registration]
    public class MockDatabase : IDatabase, ITestUtility { } // ❌ Excluded (implements ITestUtility)
}
```

**How Interface Filtering Works:**

- Checks all interfaces implemented by the type
- Uses proper generic type comparison (`SymbolEqualityComparer`)
- Works with generic interfaces like `IRepository<T>`

### Combining Multiple Filters

You can combine multiple filter types in a single attribute:

```csharp
[assembly: RegistrationFilter(
    ExcludeNamespaces = new[] { "MyApp.Internal", "MyApp.Testing" },
    ExcludePatterns = new[] { "*Mock*", "*Test*", "*Fake*" },
    ExcludeImplementing = new[] { typeof(ITestUtility) })]

// All filter rules are applied
// A type is excluded if it matches ANY of the rules
```

### Multiple Filter Attributes

You can also apply multiple `[RegistrationFilter]` attributes:

```csharp
// Filter 1: Exclude internal namespaces
[assembly: RegistrationFilter(ExcludeNamespaces = new[] {
    "MyApp.Internal"
})]

// Filter 2: Exclude test patterns
[assembly: RegistrationFilter(ExcludePatterns = new[] {
    "*Mock*",
    "*Test*"
})]

// Filter 3: Exclude utility interfaces
[assembly: RegistrationFilter(ExcludeImplementing = new[] {
    typeof(ITestUtility)
})]

// All filters are combined
```

### Real-World Example

Here's a complete example showing filters in action:

```csharp
// AssemblyInfo.cs
using Atc.DependencyInjection;

[assembly: RegistrationFilter(
    ExcludeNamespaces = new[] {
        "MyApp.Internal",
        "MyApp.Development"
    },
    ExcludePatterns = new[] {
        "*Mock*",
        "*Test*",
        "*Fake*",
        "Temp*"
    })]
```

```csharp
// Production service - WILL be registered
namespace MyApp.Services
{
    [Registration]
    public class EmailService : IEmailService
    {
        public void SendEmail(string to, string message) { }
    }
}

// Internal service - EXCLUDED by namespace
namespace MyApp.Internal
{
    [Registration]
    public class InternalCache : ICache { } // ❌ Excluded
}

// Mock service - EXCLUDED by pattern
namespace MyApp.Services
{
    [Registration]
    public class MockEmailService : IEmailService { } // ❌ Excluded
}

// Test helper - EXCLUDED by pattern
namespace MyApp.Testing
{
    [Registration]
    public class TestDataBuilder : ITestHelper { } // ❌ Excluded
}
```

### Filter Priority and Behavior

**Important Notes:**

1. **Filters are applied first**: Types are filtered OUT before any registration happens
2. **ANY match excludes**: If a type matches ANY filter rule, it's excluded
3. **Applies to all registrations**: Filters affect both current assembly and referenced assemblies
4. **No diagnostics for filtered types**: Filtered types are silently skipped (this is intentional)

### Verification

You can verify filters are working by trying to resolve filtered services:

```csharp
var services = new ServiceCollection();
services.AddDependencyRegistrationsFromMyApp();

var provider = services.BuildServiceProvider();

// This will return null (service was filtered out)
var mockService = provider.GetService<IMockEmailService>();
Console.WriteLine($"MockEmailService registered: {mockService != null}"); // False

// This will succeed (service was not filtered)
var emailService = provider.GetRequiredService<IEmailService>();
Console.WriteLine($"EmailService registered: {emailService != null}"); // True
```

### Best Practices

**When to Use Filters:**

- ✅ Excluding internal implementation details from DI
- ✅ Preventing test/mock services from production builds
- ✅ Filtering development-only utilities
- ✅ Clean separation between production and development code

**When NOT to Use Filters:**

- ❌ Don't use filters as the primary way to control registration (use conditional compilation instead)
- ❌ Don't create overly complex filter patterns that are hard to understand
- ❌ Don't filter services that SHOULD be in DI but you forgot to configure properly

**Recommended Patterns:**

```csharp
// ✅ Good: Clear, specific exclusions
[assembly: RegistrationFilter(
    ExcludeNamespaces = new[] { "MyApp.Internal" },
    ExcludePatterns = new[] { "*Mock*", "*Test*" })]

// ❌ Avoid: Overly broad patterns
[assembly: RegistrationFilter(ExcludePatterns = new[] { "*" })] // Excludes everything!

// ❌ Avoid: Filters as primary registration control
// Instead of filtering, just don't add [Registration] attribute
```

---

## 🎯 Runtime Filtering

Runtime filtering allows you to exclude specific services **when calling** the registration methods, rather than at compile time. This is extremely useful when:

- Different applications need different subsets of services from a shared library
- You want to exclude services conditionally based on runtime configuration
- Testing scenarios require specific services to be excluded
- Multiple applications share the same domain library but have different infrastructure needs

### Basic Usage

All generated `AddDependencyRegistrationsFrom*()` methods support three optional filter parameters:

```csharp
services.AddDependencyRegistrationsFromDomain(
    excludedNamespaces: new[] { "MyApp.Domain.Internal" },
    excludedPatterns: new[] { "*Test*", "*Mock*" },
    excludedTypes: new[] { typeof(EmailService), typeof(SmsService) });
```

### 🔹 Filter by Type

Exclude specific types explicitly:

```csharp
// Exclude specific services by type
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[] { typeof(EmailService), typeof(NotificationService) });
```

**Example Scenario**: PetStore.Api uses email services, but PetStore.WpfApp doesn't need them:

```csharp
// PetStore.Api - includes all services
services.AddDependencyRegistrationsFromDomain();

// PetStore.WpfApp - excludes email/notification services
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[] { typeof(EmailService), typeof(INotificationService) });
```

### 🔹 Filter by Namespace

Exclude entire namespaces (including sub-namespaces):

```csharp
// Exclude all services in the Internal namespace
services.AddDependencyRegistrationsFromDomain(
    excludedNamespaces: new[] { "MyApp.Domain.Internal" });

// Also excludes MyApp.Domain.Internal.Utils, MyApp.Domain.Internal.Deep, etc.
```

**Example Scenario**: Different deployment environments need different services:

```csharp
#if PRODUCTION
    // Production: exclude development/debug services
    services.AddDependencyRegistrationsFromDomain(
        excludedNamespaces: new[] { "MyApp.Domain.Development", "MyApp.Domain.Debug" });
#else
    // Development: include all services
    services.AddDependencyRegistrationsFromDomain();
#endif
```

### 🔹 Filter by Pattern

Exclude services using wildcard patterns (`*` = any characters, `?` = single character):

```csharp
// Exclude all mock, test, and fake services
services.AddDependencyRegistrationsFromDomain(
    excludedPatterns: new[] { "*Mock*", "*Test*", "*Fake*", "*Stub*" });

// Exclude all services ending with "Helper" or "Utility"
services.AddDependencyRegistrationsFromDomain(
    excludedPatterns: new[] { "*Helper", "*Utility" });
```

**Example Scenario**: Exclude all logging services in a minimal deployment:

```csharp
// Minimal deployment - no logging services
services.AddDependencyRegistrationsFromDomain(
    excludedPatterns: new[] { "*Logger*", "*Logging*", "*Log*" });
```

### 🔹 Combining Filters

All three filter types can be used together:

```csharp
services.AddDependencyRegistrationsFromDomain(
    excludedNamespaces: new[] { "MyApp.Domain.Internal" },
    excludedPatterns: new[] { "*Test*", "*Development*" },
    excludedTypes: new[] { typeof(LegacyService), typeof(DeprecatedFeature) });
```

### 🔹 Filters with Transitive Registration

Runtime filters are automatically propagated to referenced assemblies:

```csharp
// Filters apply to Domain AND all referenced assemblies (DataAccess, Infrastructure, etc.)
services.AddDependencyRegistrationsFromDomain(
    includeReferencedAssemblies: true,
    excludedNamespaces: new[] { "*.Internal" },
    excludedPatterns: new[] { "*Test*" },
    excludedTypes: new[] { typeof(EmailService) });
```

All referenced assemblies will also exclude:

- Any namespace ending with `.Internal`
- Any type matching `*Test*` pattern
- The `EmailService` type

### Runtime vs. Compile-Time Filtering

| Feature | Compile-Time (Assembly) | Runtime (Method Parameters) |
|---------|------------------------|----------------------------|
| **Applied When** | During source generation | During service registration |
| **Scope** | All registrations from assembly | Specific registration call |
| **Use Case** | Global exclusions (test/mock services) | Application-specific exclusions |
| **Configured In** | `AssemblyInfo.cs` with `[RegistrationFilter]` | Method call parameters |
| **Flexibility** | Fixed at compile time | Can vary per application/scenario |

### Complete Example: Multi-Application Scenario

**Shared Domain Library** (PetStore.Domain):

```csharp
namespace PetStore.Domain.Services;

[Registration]
public class PetService : IPetService { } // Core service - needed by all apps

[Registration]
public class EmailService : IEmailService { } // Email - only needed by API

[Registration]
public class ReportService : IReportService { } // Reports - only needed by Admin

[Registration]
public class NotificationService : INotificationService { } // Notifications - only API
```

**PetStore.Api** (Web API - needs email + notifications):

```csharp
// Include all services
services.AddDependencyRegistrationsFromDomain();
```

**PetStore.WpfApp** (Desktop app - needs reports, not email):

```csharp
// Exclude email and notification services
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[] { typeof(EmailService), typeof(NotificationService) });
```

**PetStore.AdminPortal** (Admin - needs reports, not notifications):

```csharp
// Exclude notification services
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[] { typeof(NotificationService) });
```

**PetStore.MobileApp** (Minimal deployment):

```csharp
// Exclude email, notifications, and reports
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[]
    {
        typeof(EmailService),
        typeof(NotificationService),
        typeof(ReportService)
    });
```

### Best Practices

✅ **Do:**

- Use runtime filtering when different applications need different service subsets
- Use type exclusion for specific services you know by name
- Use pattern exclusion for groups of services (e.g., all `*Mock*` services)
- Use namespace exclusion for entire feature areas
- Combine with compile-time filters for maximum control

```csharp
// Good: Application-specific exclusions
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[] { typeof(EmailService) }); // This app doesn't send emails
```

❌ **Avoid:**

- Using overly broad patterns that might accidentally exclude needed services
- Runtime filtering as a replacement for proper service design
- Filtering when you should just not add `[Registration]` attribute

```csharp
// Bad: Overly broad pattern
services.AddDependencyRegistrationsFromDomain(
    excludedPatterns: new[] { "*Service*" }); // Excludes almost everything!

// Bad: Should use compile-time filtering instead
services.AddDependencyRegistrationsFromDomain(
    excludedPatterns: new[] { "*Test*" }); // Tests should be excluded at compile time
```

### Verification

You can verify which services are excluded by inspecting the service collection:

```csharp
services.AddDependencyRegistrationsFromDomain(
    excludedTypes: new[] { typeof(EmailService) });

// Verify EmailService is not registered
var emailService = serviceProvider.GetService<IEmailService>();
Console.WriteLine($"EmailService registered: {emailService != null}"); // False
```

---

## 🎨 Decorator Pattern

The decorator pattern allows you to wrap existing services with additional functionality (logging, caching, validation, etc.) without modifying the original implementation. This is perfect for implementing cross-cutting concerns in a clean, maintainable way.

### ✨ How It Works

1. **Register the base service** normally with `[Registration]`
2. **Create a decorator class** that implements the same interface and wraps the base service
3. **Mark the decorator** with `[Registration(Decorator = true)]`
4. The generator automatically:
   - Registers base services first
   - Then registers decorators that wrap them
   - Preserves the service lifetime

### 📝 Basic Example

```csharp
// Interface
public interface IOrderService
{
    Task PlaceOrderAsync(string orderId);
}

// Base service - registered first
[Registration(Lifetime.Scoped, As = typeof(IOrderService))]
public class OrderService : IOrderService
{
    public Task PlaceOrderAsync(string orderId)
    {
        Console.WriteLine($"[OrderService] Processing order {orderId}");
        return Task.CompletedTask;
    }
}

// Decorator - wraps the base service
[Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
public class LoggingOrderServiceDecorator : IOrderService
{
    private readonly IOrderService inner;
    private readonly ILogger<LoggingOrderServiceDecorator> logger;

    // First parameter MUST be the interface being decorated
    public LoggingOrderServiceDecorator(
        IOrderService inner,
        ILogger<LoggingOrderServiceDecorator> logger)
    {
        this.inner = inner;
        this.logger = logger;
    }

    public async Task PlaceOrderAsync(string orderId)
    {
        logger.LogInformation("Before placing order {OrderId}", orderId);
        await inner.PlaceOrderAsync(orderId);
        logger.LogInformation("After placing order {OrderId}", orderId);
    }
}
```

**Usage:**

```csharp
services.AddDependencyRegistrationsFromDomain();

var orderService = serviceProvider.GetRequiredService<IOrderService>();
await orderService.PlaceOrderAsync("ORDER-123");

// Output:
// [LoggingDecorator] Before placing order ORDER-123
// [OrderService] Processing order ORDER-123
// [LoggingDecorator] After placing order ORDER-123
```

### Generated Code

The generator creates special `Decorate` extension methods that:

1. Find the existing service registration
2. Remove it from the service collection
3. Create a new registration that resolves the original and wraps it

```csharp
// Generated registration code
services.AddScoped<IOrderService, OrderService>();  // Base service
services.Decorate<IOrderService>((provider, inner) =>  // Decorator
{
    return ActivatorUtilities.CreateInstance<LoggingOrderServiceDecorator>(provider, inner);
});
```

### 🔄 Multiple Decorators

You can stack multiple decorators - they are applied in the order they are discovered:

```csharp
[Registration(Lifetime.Scoped, As = typeof(IOrderService))]
public class OrderService : IOrderService { }

[Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
public class LoggingDecorator : IOrderService { }

[Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
public class ValidationDecorator : IOrderService { }

[Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
public class CachingDecorator : IOrderService { }
```

**Result:** `CachingDecorator → ValidationDecorator → LoggingDecorator → OrderService`

### 🎯 Common Use Cases

#### 1. Logging/Auditing

```csharp
[Registration(Decorator = true)]
public class AuditingDecorator : IPetService
{
    private readonly IPetService inner;
    private readonly IAuditLog auditLog;

    public Pet CreatePet(CreatePetRequest request)
    {
        var result = inner.CreatePet(request);
        auditLog.Log($"Created pet {result.Id} by {currentUser}");
        return result;
    }
}
```

#### 2. Caching

```csharp
[Registration(Decorator = true)]
public class CachingPetServiceDecorator : IPetService
{
    private readonly IPetService inner;
    private readonly IMemoryCache cache;

    public Pet? GetById(Guid id)
    {
        return cache.GetOrCreate($"pet:{id}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return inner.GetById(id);
        });
    }
}
```

#### 3. Validation

```csharp
[Registration(Decorator = true)]
public class ValidationDecorator : IPetService
{
    private readonly IPetService inner;
    private readonly IValidator<CreatePetRequest> validator;

    public Pet CreatePet(CreatePetRequest request)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        return inner.CreatePet(request);
    }
}
```

#### 4. Retry Logic

```csharp
[Registration(Decorator = true)]
public class RetryDecorator : IExternalApiService
{
    private readonly IExternalApiService inner;

    public async Task<Result> CallApiAsync()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                return await inner.CallApiAsync();
            }
            catch (HttpRequestException) when (i < 2)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
            }
        }
    }
}
```

### ⚠️ Important Notes

1. **Explicit `As` Required**: Decorators MUST specify the `As` parameter to indicate which interface they decorate

   ```csharp
   // ❌ Won't work - missing As parameter
   [Registration(Decorator = true)]
   public class MyDecorator : IService { }

   // ✅ Correct
   [Registration(As = typeof(IService), Decorator = true)]
   public class MyDecorator : IService { }
   ```

2. **Constructor First Parameter**: The decorator's constructor must accept the interface as the first parameter

   ```csharp
   // ✅ Correct - interface is first parameter
   public MyDecorator(IService inner, ILogger logger) { }

   // ✅ Also correct - only parameter
   public MyDecorator(IService inner) { }
   ```

3. **Matching Lifetime**: Decorators inherit the lifetime of the base service registration

4. **Registration Order**: Base services are always registered before decorators, regardless of file order

### 🔍 Complete Example

See the **PetStore.Domain** sample for a complete working example:

- Base service: [PetService.cs](../../sample/PetStore.Domain/Services/PetService.cs)
- Decorator: [LoggingPetServiceDecorator.cs](../../sample/PetStore.Domain/Services/LoggingPetServiceDecorator.cs)

---

## 🎛️ Conditional Registration

Conditional Registration allows you to register services based on configuration values at runtime. This is perfect for feature flags, environment-specific services, A/B testing, and gradual rollouts.

### ✨ How It Works

Services with a `Condition` parameter are only registered if the configuration value at the specified key path evaluates to `true`. The condition is checked at runtime when the registration methods are called.

When an assembly contains services with conditional registration:
- An `IConfiguration` parameter is **automatically added** to all generated extension method signatures
- The configuration value is checked using `configuration.GetValue<bool>("key")`
- Services are registered inside `if` blocks based on the condition

### 📝 Basic Example

```csharp
// Register RedisCache only when Features:UseRedisCache is true
[Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
public class RedisCache : ICache
{
    public string Get(string key) => /* Redis implementation */;
    public void Set(string key, string value) => /* Redis implementation */;
}

// Register MemoryCache only when Features:UseRedisCache is false
[Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
public class MemoryCache : ICache
{
    public string Get(string key) => /* In-memory implementation */;
    public void Set(string key, string value) => /* In-memory implementation */;
}
```

**Configuration (appsettings.json):**

```json
{
  "Features": {
    "UseRedisCache": true
  }
}
```

**Usage:**

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// IConfiguration is required when conditional services exist
services.AddDependencyRegistrationsFromDomain(configuration);

// Resolves to RedisCache (because Features:UseRedisCache = true)
var cache = serviceProvider.GetRequiredService<ICache>();
```

### Generated Code

```csharp
public static IServiceCollection AddDependencyRegistrationsFromDomain(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Conditional registration with positive check
    if (configuration.GetValue<bool>("Features:UseRedisCache"))
    {
        services.AddSingleton<ICache, RedisCache>();
    }

    // Conditional registration with negation
    if (!configuration.GetValue<bool>("Features:UseRedisCache"))
    {
        services.AddSingleton<ICache, MemoryCache>();
    }

    return services;
}
```

### 🔄 Negation Support

Prefix the condition with `!` to negate it (register when the value is `false`):

```csharp
// Register when Features:UseRedisCache is FALSE
[Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
public class MemoryCache : ICache { }
```

**Generated:**

```csharp
if (!configuration.GetValue<bool>("Features:UseRedisCache"))
{
    services.AddSingleton<ICache, MemoryCache>();
}
```

### 🎯 Common Use Cases

#### 1. Feature Flags

Enable/disable features without code changes:

```csharp
// Premium features only when enabled
[Registration(Lifetime.Scoped, As = typeof(IPremiumService), Condition = "Features:EnablePremium")]
public class PremiumService : IPremiumService
{
    public void ExecutePremiumFeature() { /* Premium logic */ }
}
```

**Configuration:**

```json
{
  "Features": {
    "EnablePremium": true
  }
}
```

#### 2. Environment-Specific Services

Different implementations for different environments:

```csharp
// Production email service
[Registration(As = typeof(IEmailService), Condition = "Environment:IsProduction")]
public class SendGridEmailService : IEmailService { }

// Development email service (logs instead of sending)
[Registration(As = typeof(IEmailService), Condition = "!Environment:IsProduction")]
public class LoggingEmailService : IEmailService { }
```

#### 3. A/B Testing

Register different implementations based on experiment configuration:

```csharp
[Registration(As = typeof(IRecommendationEngine), Condition = "Experiments:UseNewAlgorithm")]
public class NewRecommendationEngine : IRecommendationEngine { }

[Registration(As = typeof(IRecommendationEngine), Condition = "!Experiments:UseNewAlgorithm")]
public class LegacyRecommendationEngine : IRecommendationEngine { }
```

#### 4. Cost Optimization

Disable expensive services when not needed:

```csharp
// AI service only when enabled (cost-saving)
[Registration(As = typeof(IAIService), Condition = "Services:EnableAI")]
public class OpenAIService : IAIService { }

// Fallback simple service
[Registration(As = typeof(IAIService), Condition = "!Services:EnableAI")]
public class BasicTextService : IAIService { }
```

### 🎨 Advanced Scenarios

#### Multiple Conditional Services

```csharp
[Registration(As = typeof(IStorage), Condition = "Storage:UseAzure")]
public class AzureBlobStorage : IStorage { }

[Registration(As = typeof(IStorage), Condition = "Storage:UseAWS")]
public class S3Storage : IStorage { }

[Registration(As = typeof(IStorage), Condition = "Storage:UseLocal")]
public class LocalFileStorage : IStorage { }
```

**Configuration:**

```json
{
  "Storage": {
    "UseAzure": false,
    "UseAWS": true,
    "UseLocal": false
  }
}
```

#### Combining with Different Lifetimes

```csharp
// Scoped Redis cache (production)
[Registration(Lifetime.Scoped, As = typeof(ICache), Condition = "Cache:UseRedis")]
public class RedisCache : ICache { }

// Singleton memory cache (development)
[Registration(Lifetime.Singleton, As = typeof(ICache), Condition = "!Cache:UseRedis")]
public class MemoryCache : ICache { }
```

#### Mixing Conditional and Unconditional

```csharp
// Always registered (core service)
[Registration(Lifetime.Scoped, As = typeof(IUserService))]
public class UserService : IUserService { }

// Conditionally registered (optional feature)
[Registration(Lifetime.Scoped, As = typeof(IAnalyticsService), Condition = "Features:EnableAnalytics")]
public class AnalyticsService : IAnalyticsService { }
```

### ⚙️ Configuration Best Practices

**1. Use Hierarchical Configuration Keys:**

```csharp
// Good: Organized hierarchy
[Registration(Condition = "Features:Cache:UseRedis")]
[Registration(Condition = "Features:Email:UseSendGrid")]
[Registration(Condition = "Experiments:NewUI:Enabled")]
```

**2. Boolean Values:**

Conditions always use `GetValue<bool>()`, so ensure configuration values are boolean:

```json
{
  "Features": {
    "UseRedisCache": true,          // ✅ Correct
    "EnablePremium": "true",        // ⚠️ Works but not ideal
    "UseNewAlgorithm": 1            // ❌ Won't work as expected
  }
}
```

**3. Default Values:**

If a configuration key is missing, `GetValue<bool>()` returns `false` by default:

```csharp
// If "Features:UseRedisCache" doesn't exist in config, MemoryCache is used
[Registration(Condition = "Features:UseRedisCache")]
public class RedisCache : ICache { }

[Registration(Condition = "!Features:UseRedisCache")]
public class MemoryCache : ICache { }  // ← This one is registered (default false)
```

### 🔍 IConfiguration Parameter Behavior

**Without Conditional Services:**

```csharp
// No conditional services → no IConfiguration parameter
services.AddDependencyRegistrationsFromDomain();
```

**With Conditional Services:**

```csharp
// Has conditional services → IConfiguration parameter required
services.AddDependencyRegistrationsFromDomain(configuration);
```

**All Overloads Updated:**

When conditional services exist, ALL generated overloads include the `IConfiguration` parameter:

```csharp
// Overload 1: Basic registration
services.AddDependencyRegistrationsFromDomain(configuration);

// Overload 2: With transitive registration
services.AddDependencyRegistrationsFromDomain(configuration, includeReferencedAssemblies: true);

// Overload 3: With specific assembly
services.AddDependencyRegistrationsFromDomain(configuration, "DataAccess");

// Overload 4: With multiple assemblies
services.AddDependencyRegistrationsFromDomain(configuration, "DataAccess", "Infrastructure");
```

### ⚠️ Important Notes

**1. Configuration Not Passed Transitively:**

Configuration is NOT passed to referenced assemblies automatically. Each assembly manages its own conditional services:

```csharp
// Domain has conditional services → needs configuration
services.AddDependencyRegistrationsFromDomain(configuration);

// DataAccess also has conditional services → call it directly with configuration
services.AddDependencyRegistrationsFromDataAccess(configuration);

// Or use transitive registration (but configuration isn't passed through)
services.AddDependencyRegistrationsFromDomain(configuration, includeReferencedAssemblies: true);
// ↑ This registers DataAccess services, but they won't have conditional logic applied
```

**2. Thread-Safe:**

Configuration reading is thread-safe and can be used in concurrent scenarios.

**3. Native AOT Compatible:**

Conditional registration is fully compatible with Native AOT since all checks are simple boolean reads from configuration.

**4. No Circular Dependencies:**

Be careful not to create circular dependencies between conditional services.

### ✅ Benefits

- **🎯 Feature Flags**: Enable/disable features dynamically without redeployment
- **🌍 Environment-Specific**: Different implementations for dev/staging/prod
- **🧪 A/B Testing**: Easy experimentation with different implementations
- **💰 Cost Optimization**: Disable expensive services when not needed
- **🚀 Gradual Rollout**: Safely test new implementations before full deployment
- **🎨 Clean Code**: No `#if` preprocessor directives needed
- **⚡ Runtime Flexibility**: Change service implementations via configuration
- **🔒 Type-Safe**: All registrations validated at compile time

### 📝 Complete Example

See the **Atc.SourceGenerators.DependencyRegistration** sample for a complete working example:

- **appsettings.json**: Feature flag configuration
- **RedisCache.cs**: Conditional service (Features:UseRedisCache = true)
- **MemoryCache.cs**: Conditional service (Features:UseRedisCache = false)
- **PremiumFeatureService.cs**: Conditional premium features
- **Program.cs**: Usage demonstration

---

## 📚 Additional Examples

See the [sample projects](../sample) for complete working examples:

- **Simple Sample**: [Atc.SourceGenerators.DependencyRegistration](../sample/Atc.SourceGenerators.DependencyRegistration)
- **Domain Layer**: [Atc.SourceGenerators.DependencyRegistration.Domain](../sample/Atc.SourceGenerators.DependencyRegistration.Domain)
