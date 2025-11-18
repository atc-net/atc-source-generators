# 🗺️ Object Mapping Generator

Automatically generate type-safe object-to-object mapping code using attributes. The generator creates efficient mapping extension methods at compile time, eliminating manual mapping boilerplate and reducing errors.

**Key Benefits:**
- 🎯 **Zero boilerplate** - No manual property copying or constructor calls
- 🔗 **Automatic chaining** - Nested objects map automatically when mappings exist
- 🧩 **Constructor support** - Maps to classes with primary constructors or parameter-based constructors
- 🛡️ **Null-safe** - Generates proper null checks for nullable properties
- ⚡ **Native AOT ready** - Pure compile-time generation with zero reflection

**Quick Example:**
```csharp
// Input: Decorate your domain model
[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Generated: Extension method
public static UserDto MapToUserDto(this User source) =>
    new UserDto { Id = source.Id, Name = source.Name };
```

## 📑 Table of Contents

- [🚀 Get Started - Quick Guide](#-get-started---quick-guide)
  - [📂 Project Structure](#-project-structure)
  - [1️⃣ Setup Projects](#️-setup-projects)
  - [2️⃣ Data Access Layer](#️-data-access-layer-userapp-dataaccess-)
  - [3️⃣ Domain Layer](#️-domain-layer-userapp-domain-)
  - [4️⃣ API Layer](#️-api-layer-userapp-api-)
  - [5️⃣ Program.cs](#️-programcs-minimal-api-setup-)
  - [🎨 What Gets Generated](#-what-gets-generated)
  - [6️⃣ Testing the Application](#️-testing-the-application-)
  - [🔍 Viewing Generated Code](#-viewing-generated-code-optional)
  - [🎯 Key Takeaways](#-key-takeaways)
- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [💡 Basic Usage](#-basic-usage)
  - [1️⃣ Add Using Directives](#️-add-using-directives)
  - [2️⃣ Decorate Your Classes](#️-decorate-your-classes)
  - [3️⃣ Use Generated Mappings](#️-use-generated-mappings)
- [🏗️ Advanced Scenarios](#️-advanced-scenarios)
  - [🔄 Enum Conversion](#-enum-conversion)
  - [🪆 Nested Object Mapping](#-nested-object-mapping)
  - [📦 Collection Mapping](#-collection-mapping)
  - [🔁 Multi-Layer Mapping](#-multi-layer-mapping)
  - [🚫 Excluding Properties with `[MapIgnore]`](#-excluding-properties-with-mapignore)
  - [🏷️ Custom Property Name Mapping with `[MapProperty]`](#️-custom-property-name-mapping-with-mapproperty)
  - [🏗️ Constructor Mapping](#️-constructor-mapping)
- [⚙️ MapToAttribute Parameters](#️-maptoattribute-parameters)
- [🛡️ Diagnostics](#️-diagnostics)
  - [❌ ATCMAP001: Mapping Class Must Be Partial](#-atcmap001-mapping-class-must-be-partial)
  - [❌ ATCMAP002: Target Type Must Be Class or Struct](#-atcmap002-target-type-must-be-class-or-struct)
  - [❌ ATCMAP003: MapProperty Target Property Not Found](#-atcmap003-mapproperty-target-property-not-found)
- [🚀 Native AOT Compatibility](#-native-aot-compatibility)
- [📚 Additional Examples](#-additional-examples)

---

## 🚀 Get Started - Quick Guide

This guide demonstrates a realistic 3-layer architecture for a UserApp application using minimal APIs and automatic object mapping.

### 📂 Project Structure

```
UserApp.sln
├── UserApp.Api/              (Presentation layer - DTOs)
├── UserApp.Domain/           (Business logic layer - Domain models)
└── UserApp.DataAccess/       (Data access layer - Entities)
```

### 1️⃣ Setup Projects

**UserApp.DataAccess.csproj** (Base layer):
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />
    <PackageReference Include="Atc.SourceGenerators.Annotations" Version="1.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\UserApp.Domain\UserApp.Domain.csproj" />
  </ItemGroup>
</Project>
```

**UserApp.Domain.csproj** (Middle layer):
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />
    <PackageReference Include="Atc.SourceGenerators.Annotations" Version="1.0.0" />
  </ItemGroup>
</Project>
```

**UserApp.Api.csproj** (Top layer):
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\UserApp.Domain\UserApp.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 2️⃣ Data Access Layer (`UserApp.DataAccess`)

**Entities/UserEntity.cs** - Database entity with mapping attribute:
```csharp
using Atc.SourceGenerators.Annotations;
using UserApp.Domain;

namespace UserApp.DataAccess.Entities;

/// <summary>
/// Database entity for user (maps to Domain.User).
/// </summary>
[MapTo(typeof(User))]
public partial class UserEntity
{
    /// <summary>
    /// Gets or sets the database ID (auto-increment).
    /// </summary>
    public int DatabaseId { get; set; }

    /// <summary>
    /// Gets or sets the user's public unique identifier (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's status (stored as int in DB).
    /// </summary>
    public UserStatusEntity Status { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to address.
    /// </summary>
    public AddressEntity? Address { get; set; }

    /// <summary>
    /// Gets or sets when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the user was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets if the record is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
```

**Entities/AddressEntity.cs** - Nested entity with mapping:
```csharp
using Atc.SourceGenerators.Annotations;
using UserApp.Domain;

namespace UserApp.DataAccess.Entities;

/// <summary>
/// Database entity for address (maps to Domain.Address).
/// </summary>
[MapTo(typeof(Address))]
public partial class AddressEntity
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Entities/UserStatusEntity.cs** - Enum matching domain enum:
```csharp
namespace UserApp.DataAccess.Entities;

/// <summary>
/// Database representation of user status.
/// </summary>
public enum UserStatusEntity
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Deleted = 3,
}
```

### 3️⃣ Domain Layer (`UserApp.Domain`)

**User.cs** - Domain model with mapping to DTO:
```csharp
using Atc.SourceGenerators.Annotations;

namespace UserApp.Domain;

/// <summary>
/// Domain model for user (maps to UserDto for API responses).
/// </summary>
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
```

**Address.cs** - Nested domain model:
```csharp
using Atc.SourceGenerators.Annotations;

namespace UserApp.Domain;

[MapTo(typeof(AddressDto))]
public partial class Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}
```

**UserStatus.cs** - Domain enum:
```csharp
namespace UserApp.Domain;

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Deleted = 3,
}
```

**UserDto.cs** - API DTO:
```csharp
namespace UserApp.Domain;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserStatusDto Status { get; set; }
    public AddressDto? Address { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
```

**AddressDto.cs** - Nested DTO:
```csharp
namespace UserApp.Domain;

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
```

**UserStatusDto.cs** - DTO enum:
```csharp
namespace UserApp.Domain;

public enum UserStatusDto
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Deleted = 3,
}
```

### 4️⃣ API Layer (`UserApp.Api`)

**Program.cs** - Using generated mappings:
```csharp
using Atc.Mapping;
using UserApp.Domain;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory user repository for demo purposes
var users = new Dictionary<Guid, User>
{
    {
        Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
        new User
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = UserStatus.Active,
            Address = new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                PostalCode = "62701",
                Country = "USA",
            },
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-30),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        }
    },
};

// GET /users/{id} - Get user by ID
app.MapGet("/users/{id:guid}", (Guid id) =>
{
    if (!users.TryGetValue(id, out var user))
    {
        return Results.NotFound(new { message = $"User with ID {id} not found" });
    }

    // ✨ Use generated mapping extension method
    var dto = user.MapToUserDto();
    return Results.Ok(dto);
})
.WithName("GetUserById")
.Produces<UserDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// GET /users - Get all users
app.MapGet("/users", () =>
{
    // ✨ Use generated mapping extension method
    var dtos = users.Values.Select(u => u.MapToUserDto()).ToList();
    return Results.Ok(dtos);
})
.WithName("GetAllUsers")
.Produces<List<UserDto>>(StatusCodes.Status200OK);

app.Run();
```

### 🎨 What Gets Generated

The generator automatically creates extension methods in the `Atc.Mapping` namespace:

**For Data Access → Domain:**
```csharp
namespace Atc.Mapping;

public static class ObjectMappingExtensions
{
    public static UserApp.Domain.User MapToUser(this UserApp.DataAccess.Entities.UserEntity source)
    {
        if (source is null)
        {
            return default!;
        }

        return new UserApp.Domain.User
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            // ✨ Automatic enum conversion
            Status = (UserApp.Domain.UserStatus)source.Status,
            // ✨ Automatic nested object mapping
            Address = source.Address?.MapToAddress()!,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public static UserApp.Domain.Address MapToAddress(this UserApp.DataAccess.Entities.AddressEntity source)
    {
        if (source is null)
        {
            return default!;
        }

        return new UserApp.Domain.Address
        {
            Street = source.Street,
            City = source.City,
            State = source.State,
            PostalCode = source.PostalCode,
            Country = source.Country
        };
    }
}
```

**For Domain → DTOs:**
```csharp
namespace Atc.Mapping;

public static class ObjectMappingExtensions
{
    public static UserApp.Domain.UserDto MapToUserDto(this UserApp.Domain.User source)
    {
        if (source is null)
        {
            return default!;
        }

        return new UserApp.Domain.UserDto
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            // ✨ Automatic enum conversion
            Status = (UserApp.Domain.UserStatusDto)source.Status,
            // ✨ Automatic nested object mapping
            Address = source.Address?.MapToAddressDto()!,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public static UserApp.Domain.AddressDto MapToAddressDto(this UserApp.Domain.Address source)
    {
        if (source is null)
        {
            return default!;
        }

        return new UserApp.Domain.AddressDto
        {
            Street = source.Street,
            City = source.City,
            State = source.State,
            PostalCode = source.PostalCode,
            Country = source.Country
        };
    }
}
```

### 6️⃣ Testing the Application

```bash
# Run the application
dotnet run --project UserApp.Api

# Test the endpoints
curl https://localhost:7000/users
curl https://localhost:7000/users/550e8400-e29b-41d4-a716-446655440000
```

**Example Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "status": 0,
  "address": {
    "street": "123 Main St",
    "city": "Springfield",
    "state": "IL",
    "postalCode": "62701",
    "country": "USA"
  },
  "createdAt": "2024-12-15T10:00:00Z",
  "updatedAt": "2025-01-13T15:30:00Z"
}
```

### 🔍 Viewing Generated Code (Optional)

To see the generated mapping code:

```bash
dotnet build -p:EmitCompilerGeneratedFiles=true -p:CompilerGeneratedFilesOutputPath=Generated
```

Then look in `obj/Debug/net10.0/Atc.SourceGenerators/Atc.SourceGenerators.ObjectMappingGenerator/ObjectMappingExtensions.g.cs`

### 🎯 Key Takeaways

✅ **3-Layer Architecture:**
- **Data Access Layer:** `UserEntity` (with database-specific fields)
- **Domain Layer:** `User` (clean domain model)
- **API Layer:** `UserDto` (API contract)

✅ **Automatic Mapping Chain:**
```
UserEntity → User → UserDto
```

✅ **Features Demonstrated:**
- Enum conversion
- Nested object mapping
- Null safety
- Multiple properties
- DateTimeOffset/DateTime handling

✅ **Benefits:**
- 🚀 No manual mapping code
- ✅ Compile-time type safety
- 🎯 Zero runtime overhead
- 🔧 Easy to maintain

---

## ✨ Features

🎯 **Attribute-Based Configuration**
- Declarative mapping using `[MapTo(typeof(TargetType))]`
- Clean and readable code

🔄 **Automatic Type Handling**
- Direct property mapping (same name and type, case-insensitive)
- **Constructor mapping** - Automatically detects and uses constructors for records and classes with primary constructors
- Mixed initialization support (constructor + object initializer for remaining properties)
- **Property exclusion** - Use `[MapIgnore]` to exclude sensitive or internal properties
- **Custom property names** - Use `[MapProperty]` to map properties with different names
- Automatic enum conversion
- Nested object mapping
- Collection mapping with LINQ
- Null safety built-in

⚡ **Compile-Time Generation**
- Zero runtime reflection
- Zero performance overhead
- Type-safe extension methods

🏗️ **Multi-Layer Support**
- Entity → Domain → DTO chains
- Automatic chaining of nested mappings

🛡️ **Comprehensive Diagnostics**
- Clear error messages
- Build-time validation
- Helpful suggestions

---

## 📦 Installation

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

## 💡 Basic Usage

### 1️⃣ Add Using Directives

```csharp
using Atc.SourceGenerators.Annotations;
using Atc.Mapping; // For using generated extension methods
```

### 2️⃣ Decorate Your Classes

Mark the **source** class with `[MapTo(typeof(TargetType))]`:

```csharp
using Atc.SourceGenerators.Annotations;

namespace MyApp.Domain;

// Simple mapping example
[MapTo(typeof(PersonDto))]
public partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

**Important:** The source class **must** be `partial`.

### 3️⃣ Use Generated Mappings

```csharp
using Atc.Mapping;

var person = new Person { Id = 1, Name = "John", Age = 30 };

// ✨ Use the generated extension method
var dto = person.MapToPersonDto();

Console.WriteLine($"{dto.Name} is {dto.Age} years old");
```

---

## 🏗️ Advanced Scenarios

### 🔄 Enum Conversion

The generator automatically converts between enums using one of two approaches:

#### 🎯 Safe Enum Mapping (Recommended)

When enums are decorated with `[MapTo]` attributes, the generator uses **EnumMappingGenerator** extension methods for type-safe conversion with special case handling:

```csharp
// Source enum with MapTo attribute
[MapTo(typeof(StatusDto), Bidirectional = true)]
public enum Status
{
    None,        // Maps to StatusDto.Unknown (special case)
    Active,
    Inactive
}

// Target enum
public enum StatusDto
{
    Unknown,     // Maps from Status.None (special case)
    Active,
    Inactive
}

[MapTo(typeof(UserDto))]
public partial class User
{
    public string Name { get; set; } = string.Empty;
    public Status Status { get; set; }
}

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public StatusDto Status { get; set; }
}
```

**Generated code:**
```csharp
public static UserDto MapToUserDto(this User source)
{
    if (source is null)
    {
        return default!;
    }

    return new UserDto
    {
        Name = source.Name,
        // ✨ Uses EnumMapping extension method (safe)
        Status = source.Status.MapToStatusDto()
    };
}
```

**Benefits:**
- ✅ Type-safe with `ArgumentOutOfRangeException` for unmapped values
- ✅ Special case handling (None → Unknown, etc.)
- ✅ Compile-time warnings for unmapped enum values
- ✅ No silent failures from incorrect casts

#### ⚠️ Enum Cast (Fallback)

For enums **without** `[MapTo]` attributes, the generator falls back to simple casts:

```csharp
// Enums without MapTo attribute
public enum Priority { Low = 1, Medium = 2, High = 3 }
public enum PriorityDto { Low = 1, Medium = 2, High = 3 }

[MapTo(typeof(TaskDto))]
public partial class Task
{
    public Priority Priority { get; set; }
}
```

**Generated code:**
```csharp
public static TaskDto MapToTaskDto(this Task source)
{
    // ...
    return new TaskDto
    {
        // ⚠️ Simple cast (less safe, no validation)
        Priority = (PriorityDto)source.Priority
    };
}
```

**Limitations:**
- ⚠️ No runtime validation
- ⚠️ No special case handling
- ⚠️ Silent failures if enum values don't match

**Recommendation:** Always use `[MapTo]` on enums to enable safe mapping. See the [EnumMapping Guide](EnumMapping.md) for details.

### 🪆 Nested Object Mapping

The generator automatically chains mappings for nested objects:

```csharp
[MapTo(typeof(AddressDto))]
public partial class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

[MapTo(typeof(PersonDto))]
public partial class Person
{
    public string Name { get; set; } = string.Empty;
    public Address? Address { get; set; }
}

public class PersonDto
{
    public string Name { get; set; } = string.Empty;
    public AddressDto? Address { get; set; }
}
```

**Generated code:**
```csharp
public static PersonDto MapToPersonDto(this Person source)
{
    if (source is null)
    {
        return default!;
    }

    return new PersonDto
    {
        Name = source.Name,
        // ✨ Automatic nested object mapping
        Address = source.Address?.MapToAddressDto()!
    };
}
```

### 📦 Collection Mapping

The generator automatically maps collections using LINQ `.Select()` and generates appropriate conversion methods for different collection types.

**Supported Collection Types:**
- `List<T>` / `IList<T>`
- `IEnumerable<T>`
- `ICollection<T>` / `IReadOnlyCollection<T>`
- `IReadOnlyList<T>`
- `T[]` (arrays)
- `Collection<T>` / `ReadOnlyCollection<T>`

```csharp
[MapTo(typeof(TagDto))]
public partial class Tag
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class TagDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

[MapTo(typeof(PostDto))]
public partial class Post
{
    public string Title { get; set; } = string.Empty;
    public IList<Tag> Tags { get; set; } = new List<Tag>();
}

public class PostDto
{
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<TagDto> Tags { get; set; } = Array.Empty<TagDto>();
}
```

**Generated code:**
```csharp
public static PostDto MapToPostDto(this Post source)
{
    if (source is null)
    {
        return default!;
    }

    return new PostDto
    {
        Title = source.Title,
        // ✨ Automatic collection mapping with element conversion
        Tags = source.Tags?.Select(x => x.MapToTagDto()).ToList()!
    };
}
```

**Collection Conversion Rules:**
- **`List<T>`, `IList<T>`, `IEnumerable<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`** → Uses `.ToList()`
- **`T[]` (arrays)** → Uses `.ToArray()`
- **`Collection<T>`** → Uses `new Collection<T>(source.Items?.Select(...).ToList()!)`
- **`ReadOnlyCollection<T>`** → Uses `new ReadOnlyCollection<T>(source.Items?.Select(...).ToList()!)`

**Multi-Layer Collection Example:**

See the PetStore.Api sample which demonstrates collection mapping across 3 layers:

```
PetEntity (DataAccess)    → ICollection<PetEntity> Children
    ↓ .MapToPet()
Pet (Domain)              → IList<Pet> Children
    ↓ .MapToPetResponse()
PetResponse (API)         → IReadOnlyList<PetResponse> Children
```

Each layer automatically converts collections while preserving the element mappings.

### 🔁 Multi-Layer Mapping

Build complex mapping chains across multiple layers:

```
Database Entity → Domain Model → API DTO
```

**Layer 1 (Data Access):**
```csharp
namespace DataAccess;

[MapTo(typeof(Domain.Product))]
public partial class ProductEntity
{
    public int DatabaseId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; }  // Database-specific
}
```

**Layer 2 (Domain):**
```csharp
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
```

**Usage:**
```csharp
using Atc.Mapping;

// Database to Domain
var entity = repository.GetById(1);
var domainModel = entity.MapToProduct();

// Domain to DTO
var dto = domainModel.MapToProductDto();

// Or directly in LINQ
var dtos = repository.GetAll()
    .Select(e => e.MapToProduct())
    .Select(p => p.MapToProductDto())
    .ToList();
```

### 🚫 Excluding Properties with `[MapIgnore]`

Use the `[MapIgnore]` attribute to exclude specific properties from mapping. This is useful for sensitive data, internal state, or audit fields that should not be mapped to DTOs.

```csharp
using Atc.SourceGenerators.Annotations;

[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Sensitive data - never map to DTOs
    [MapIgnore]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    // Internal audit fields - excluded from mapping
    [MapIgnore]
    public DateTimeOffset CreatedAt { get; set; }

    [MapIgnore]
    public string? ModifiedBy { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // PasswordHash, CreatedAt, and ModifiedBy are NOT mapped
}

// Generated: Only Id, Name, and Email are mapped
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
        Email = source.Email
    };
}
```

**Use Cases:**
- **Sensitive data** - Password hashes, API keys, tokens
- **Audit fields** - CreatedAt, UpdatedAt, ModifiedBy
- **Internal state** - Cache values, computed fields, temporary flags
- **Navigation properties** - Complex relationships managed separately

**Works with:**
- Simple properties
- Nested objects (ignored properties in nested objects are also excluded)
- Bidirectional mappings (properties can be ignored in either direction)
- Constructor mappings (ignored properties are excluded from constructor parameters)

### 🏷️ Custom Property Name Mapping with `[MapProperty]`

When integrating with external APIs, legacy systems, or when property names differ between layers, use `[MapProperty]` to specify custom mappings without renaming your domain models.

**Example:**

```csharp
using Atc.SourceGenerators.Annotations;

// Domain model
[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Maps PreferredName → DisplayName in UserDto
    [MapProperty("DisplayName")]
    public string PreferredName { get; set; } = string.Empty;

    // Maps YearsOld → Age in UserDto
    [MapProperty("Age")]
    public int YearsOld { get; set; }
}

// DTO with different property names
public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Generated mapping code
public static UserDto MapToUserDto(this User source)
{
    if (source is null)
    {
        return default!;
    }

    return new UserDto
    {
        Id = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        DisplayName = source.PreferredName,  // ✨ Custom mapping
        Age = source.YearsOld                // ✨ Custom mapping
    };
}
```

**Use Cases:**
- 🔌 **API Integration** - Match external API property names without modifying your domain models
- 🏛️ **Legacy Systems** - Adapt to existing database column names or legacy DTOs
- 🌍 **Naming Conventions** - Bridge different naming conventions between layers (e.g., `firstName` ↔ `FirstName`)
- 📦 **Domain Clarity** - Keep meaningful domain property names while exposing simplified DTO names

**Works with:**
- Simple properties (strings, numbers, dates, etc.)
- Nested objects (custom property names on nested object references)
- Bidirectional mappings (apply `[MapProperty]` on both sides for reverse mapping)
- Constructor mappings (custom names are resolved when matching constructor parameters)

**Validation:**
- ✅ Compile-time validation ensures target properties exist
- ❌ `ATCMAP003` diagnostic if target property name is not found

### 🏗️ Constructor Mapping

The generator automatically detects and uses constructors when mapping to records or classes with primary constructors (C# 12+). This provides a more natural mapping approach for immutable types.

#### Simple Record Mapping

```csharp
// Target: Record with constructor
public record OrderDto(Guid Id, string CustomerName, decimal Total);

// Source: Class with properties
[MapTo(typeof(OrderDto))]
public partial class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

// Generated: Constructor call instead of object initializer
public static OrderDto MapToOrderDto(this Order source)
{
    if (source is null)
    {
        return default!;
    }

    return new OrderDto(
        source.Id,
        source.CustomerName,
        source.Total);
}
```

#### Bidirectional Record Mapping

```csharp
// Both sides are records with constructors
public record UserDto(Guid Id, string Name);

[MapTo(typeof(UserDto), Bidirectional = true)]
public partial record User(Guid Id, string Name);

// Generated: Both directions use constructors
// Forward: User → UserDto
public static UserDto MapToUserDto(this User source) =>
    new UserDto(source.Id, source.Name);

// Reverse: UserDto → User
public static User MapToUser(this UserDto source) =>
    new User(source.Id, source.Name);
```

#### Mixed Constructor + Initializer

When the target has constructor parameters AND additional settable properties, the generator uses both:

```csharp
// Target: Constructor for required properties, settable for optional
public record ProductDto(Guid Id, string Name, decimal Price)
{
    public string Description { get; set; } = string.Empty;
    public bool InStock { get; set; }
}

// Source: All properties settable
[MapTo(typeof(ProductDto))]
public partial class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool InStock { get; set; }
}

// Generated: Constructor for primary properties, initializer for extras
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

#### Case-Insensitive Parameter Matching

The generator matches properties to constructor parameters case-insensitively:

```csharp
// Target: camelCase parameters (less common but supported)
public record ItemDto(int id, string name);

// Source: PascalCase properties (standard C# convention)
[MapTo(typeof(ItemDto))]
public partial class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Generated: Correctly matches despite casing difference
public static ItemDto MapToItemDto(this Item source) =>
    new ItemDto(source.Id, source.Name);
```

---

## ⚙️ MapToAttribute Parameters

The `MapToAttribute` accepts the following parameters:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `targetType` | `Type` | ✅ Yes | - | The type to map to |
| `Bidirectional` | `bool` | ❌ No | `false` | Generate bidirectional mappings (both Source → Target and Target → Source) |

**Example:**
```csharp
// Basic mapping (one-way)
[MapTo(typeof(PersonDto))]
public partial class Person { }
// Generates: Person.MapToPersonDto()

// Bidirectional mapping (two-way)
[MapTo(typeof(PersonDto), Bidirectional = true)]
public partial class Person { }
// Generates: Person.MapToPersonDto() AND PersonDto.MapToPerson()
```

---

## 🛡️ Diagnostics

The generator provides helpful diagnostics during compilation.

### ❌ ATCMAP001: Mapping Class Must Be Partial

**Error:** The class decorated with `[MapTo]` is not marked as `partial`.

**Example:**
```csharp
[MapTo(typeof(PersonDto))]
public class Person  // ❌ Missing 'partial' keyword
{
    public string Name { get; set; } = string.Empty;
}
```

**Fix:**
```csharp
[MapTo(typeof(PersonDto))]
public partial class Person  // ✅ Added 'partial'
{
    public string Name { get; set; } = string.Empty;
}
```

**Why:** The generator needs to add extension methods in a separate file, which requires the class to be `partial`.

---

### ❌ ATCMAP002: Target Type Must Be Class or Struct

**Error:** The target type specified in `[MapTo(typeof(...))]` is not a class or struct.

**Example:**
```csharp
[MapTo(typeof(IPerson))]  // ❌ Interface
public partial class Person { }
```

**Fix:**
```csharp
[MapTo(typeof(PersonDto))]  // ✅ Class
public partial class Person { }
```

**Why:** You can only map to concrete types (classes or structs), not interfaces or abstract classes.

---

### ❌ ATCMAP003: MapProperty Target Property Not Found

**Error:** The target property specified in `[MapProperty("PropertyName")]` does not exist on the target type.

**Example:**
```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }

    [MapProperty("NonExistentProperty")]  // ❌ UserDto doesn't have this property
    public string Name { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}
```

**Fix:**
```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }

    [MapProperty("FullName")]  // ✅ UserDto has this property
    public string Name { get; set; } = string.Empty;
}
```

**Why:** The generator validates at compile time that the target property exists to prevent runtime errors. This ensures type-safe mappings.

---

## 🚀 Native AOT Compatibility

The Object Mapping Generator is **fully compatible with Native AOT** compilation, producing code that meets all AOT requirements:

### ✅ AOT-Safe Features

- **Zero reflection** - All mappings use direct property access and constructor calls
- **Compile-time generation** - Mapping code is generated during build, not at runtime
- **Trimming-safe** - No dynamic type discovery or metadata dependencies
- **Constructor detection** - Analyzes types at compile time, not runtime
- **Static analysis friendly** - All code paths are visible to the AOT compiler

### 🏗️ How It Works

1. **Build-time analysis**: The generator scans classes with `[MapTo]` attributes during compilation
2. **Property matching**: Creates direct property-to-property assignments without reflection
3. **Constructor detection**: Analyzes target type constructors at compile time
4. **Extension method generation**: Produces static extension methods with concrete implementations
5. **AOT compilation**: The generated code compiles to native machine code with full optimizations

### 📋 Example Generated Code

```csharp
// Source: [MapTo(typeof(UserDto))] public partial class User { ... }

// Generated AOT-safe code:
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
        Email = source.Email
    };
}
```

**Why This Is AOT-Safe:**
- No `Activator.CreateInstance()` calls (reflection)
- No dynamic property access via `PropertyInfo`
- All property assignments are compile-time verified
- Null checks are explicit and traceable
- Constructor calls use `new` keyword, not reflection

### 🎯 Multi-Layer AOT Support

Even complex mapping chains remain fully AOT-compatible:

```csharp
// Entity → Domain → DTO chain
var dto = entity
    .MapToDomainModel()    // ✅ AOT-safe
    .MapToDto();           // ✅ AOT-safe
```

Each mapping method is independently generated with zero reflection, ensuring the entire chain compiles to efficient native code.

---

## 📚 Additional Examples

### Example 1: Simple POCO Mapping

```csharp
[MapTo(typeof(ProductDto))]
public partial class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Usage
var product = new Product { Id = 1, Name = "Widget", Price = 9.99m };
var dto = product.MapToProductDto();
```

### Example 2: Record Types

```csharp
[MapTo(typeof(PersonDto))]
public partial record Person(int Id, string Name, int Age);

public record PersonDto(int Id, string Name, int Age);

// Usage
var person = new Person(1, "Alice", 25);
var dto = person.MapToPersonDto();
```

### Example 3: Complex Nested Structure

```csharp
[MapTo(typeof(ContactInfoDto))]
public partial class ContactInfo
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class ContactInfoDto
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

[MapTo(typeof(CompanyDto))]
public partial class Company
{
    public string Name { get; set; } = string.Empty;
    public ContactInfo? Contact { get; set; }
}

public class CompanyDto
{
    public string Name { get; set; } = string.Empty;
    public ContactInfoDto? Contact { get; set; }
}

// Usage
var company = new Company
{
    Name = "Acme Corp",
    Contact = new ContactInfo { Email = "info@acme.com", Phone = "555-1234" }
};
var dto = company.MapToCompanyDto();
```

### Example 4: Working with Collections

```csharp
[MapTo(typeof(TagDto))]
public partial class Tag
{
    public string Name { get; set; } = string.Empty;
}

public class TagDto
{
    public string Name { get; set; } = string.Empty;
}

// Usage with LINQ
List<Tag> tags = GetTags();
List<TagDto> tagDtos = tags.Select(t => t.MapToTagDto()).ToList();
```

---

**Happy Mapping! 🗺️✨**

For more information and examples, visit the [Atc.SourceGenerators GitHub repository](https://github.com/atc-net/atc-source-generators).
