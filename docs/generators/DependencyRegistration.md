# 🎯 Dependency Registration Generator

Automatically register services in the dependency injection container using attributes instead of manual registration code. The generator creates type-safe registration code at compile time, eliminating boilerplate and reducing errors.

## 📑 Table of Contents

- [🚀 Get Started - Quick Guide](#-get-started---quick-guide)
  - [📂 Project Structure](#-project-structure)
  - [1️⃣ Setup Projects](#️-setup-projects)
  - [2️⃣ Data Access Layer](#️-data-access-layer-petstore-dataaccess-)
  - [3️⃣ Domain Layer](#️-domain-layer-petstore-domain-)
  - [4️⃣ API Layer](#️-api-layer-petstore-api-)
  - [5️⃣ Program.cs](#️-programcs-minimal-api-setup-)
  - [🎨 What Gets Generated](#-what-gets-generated)
  - [6️⃣ Testing the Application](#️-testing-the-application-)
  - [🔍 Viewing Generated Code](#-viewing-generated-code-optional)
  - [🎯 Key Takeaways](#-key-takeaways)
- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [💡 Basic Usage](#-basic-usage)
  - [1️⃣ Add Using Directives](#️-add-using-directives)
  - [2️⃣ Decorate Your Services](#️-decorate-your-services)
  - [3️⃣ Register in DI Container](#️-register-in-di-container)
- [🏗️ Multi-Project Setup](#️-multi-project-setup)
  - [📁 Example Structure](#-example-structure)
  - [⚡ Program.cs Registration](#-programcs-registration)
  - [🏷️ Method Naming Convention](#️-method-naming-convention)
  - [✨ Smart Naming](#-smart-naming)
- [🔍 Auto-Detection](#-auto-detection)
  - [1️⃣ Single Interface](#️-single-interface)
  - [🔢 Multiple Interfaces](#-multiple-interfaces)
  - [🧹 System Interfaces Filtered](#-system-interfaces-filtered)
  - [🎯 Explicit Override](#-explicit-override)
  - [🔀 Register As Both Interface and Concrete Type](#-register-as-both-interface-and-concrete-type)
- [⏱️ Service Lifetimes](#️-service-lifetimes)
  - [🔒 Singleton (Default)](#-singleton-default)
  - [🔄 Scoped](#-scoped)
  - [⚡ Transient](#-transient)
- [⚙️ RegistrationAttribute Parameters](#️-registrationattribute-parameters)
  - [📝 Examples](#-examples)
- [🛡️ Diagnostics](#️-diagnostics)
  - [❌ ATCDIR001: As Type Must Be Interface](#-ATCDIR001-as-type-must-be-interface)
  - [❌ ATCDIR002: Class Does Not Implement Interface](#-ATCDIR002-class-does-not-implement-interface)
  - [⚠️ ATCDIR003: Duplicate Registration with Different Lifetime](#️-ATCDIR003-duplicate-registration-with-different-lifetime)
  - [❌ ATCDIR004: Hosted Services Must Use Singleton Lifetime](#-ATCDIR004-hosted-services-must-use-singleton-lifetime)
- [🔷 Generic Interface Registration](#-generic-interface-registration)
- [🔑 Keyed Service Registration](#-keyed-service-registration)
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
- **Keyed Service Registration**: Multiple implementations of the same interface with different keys (.NET 8+) 🆕
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

// All parameters
[Registration(Lifetime.Scoped, As = typeof(IService), AsSelf = true)]
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

## 📚 Additional Examples

See the [sample projects](../sample) for complete working examples:

- **Simple Sample**: [Atc.SourceGenerators.DependencyRegistration](../sample/Atc.SourceGenerators.DependencyRegistration)
- **Domain Layer**: [Atc.SourceGenerators.DependencyRegistration.Domain](../sample/Atc.SourceGenerators.DependencyRegistration.Domain)
