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
  - [🔄 Property Flattening](#-property-flattening)
  - [🔀 Built-in Type Conversion](#-built-in-type-conversion)
  - [✅ Required Property Validation](#-required-property-validation)
  - [🌳 Polymorphic / Derived Type Mapping](#-polymorphic--derived-type-mapping)
  - [🏗️ Constructor Mapping](#️-constructor-mapping)
  - [🪝 Before/After Mapping Hooks](#-beforeafter-mapping-hooks)
  - [🏭 Object Factories](#-object-factories)
  - [🔄 Update Existing Target Instance](#-update-existing-target-instance)
  - [📊 IQueryable Projections](#-iqueryable-projections)
- [⚙️ MapToAttribute Parameters](#️-maptoattribute-parameters)
- [🛡️ Diagnostics](#️-diagnostics)
  - [❌ ATCMAP001: Mapping Class Must Be Partial](#-atcmap001-mapping-class-must-be-partial)
  - [❌ ATCMAP002: Target Type Must Be Class or Struct](#-atcmap002-target-type-must-be-class-or-struct)
  - [❌ ATCMAP003: MapProperty Target Property Not Found](#-atcmap003-mapproperty-target-property-not-found)
  - [⚠️ ATCMAP004: Required Property Not Mapped](#️-atcmap004-required-property-not-mapped)
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

### 🔄 Property Flattening

When working with nested objects that need to be flattened into a simpler DTO structure, use `EnableFlattening = true` to automatically map nested properties using a naming convention.

**Example:**

```csharp
using Atc.SourceGenerators.Annotations;

// Nested object
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

// Source with nested object
[MapTo(typeof(UserFlatDto), EnableFlattening = true)]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address? Address { get; set; }  // Nested object
}

// Flattened target DTO
public class UserFlatDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Flattened properties using {PropertyName}{NestedPropertyName} convention
    public string? AddressStreet { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressPostalCode { get; set; }
}

// Generated: Automatic property flattening with null-safety
public static UserFlatDto MapToUserFlatDto(this User source)
{
    if (source is null)
    {
        return default!;
    }

    return new UserFlatDto
    {
        Id = source.Id,
        Name = source.Name,
        AddressStreet = source.Address?.Street!,   // Null-safe flattening
        AddressCity = source.Address?.City!,
        AddressPostalCode = source.Address?.PostalCode!
    };
}
```

**Naming Convention:**
- Pattern: `{PropertyName}{NestedPropertyName}`
- Examples:
  - `Address.City` → `AddressCity`
  - `Address.Street` → `AddressStreet`
  - `HomeAddress.City` → `HomeAddressCity`
  - `WorkAddress.City` → `WorkAddressCity`

**Multiple Nested Objects:**

```csharp
[MapTo(typeof(PersonDto), EnableFlattening = true)]
public partial class Person
{
    public int Id { get; set; }
    public Address HomeAddress { get; set; } = new();
    public Address WorkAddress { get; set; } = new();
}

public class PersonDto
{
    public int Id { get; set; }
    // Home address flattened
    public string HomeAddressCity { get; set; } = string.Empty;
    public string HomeAddressStreet { get; set; } = string.Empty;
    // Work address flattened
    public string WorkAddressCity { get; set; } = string.Empty;
    public string WorkAddressStreet { get; set; } = string.Empty;
}
```

**Null Safety:**
- Nullable nested objects automatically use null-conditional operator (`?.`)
- Non-nullable nested objects use direct property access
- Flattened properties are marked as nullable if the source nested object is nullable

**Works with:**
- One-level deep nesting (can be extended in future)
- Multiple nested objects of the same type
- Bidirectional mappings (both directions support flattening)
- Other mapping features (MapIgnore, MapProperty, etc.)

**Use Cases:**
- **API responses** - Simplify complex domain models for client consumption
- **Report generation** - Flatten hierarchical data for tabular export
- **Legacy integration** - Map to flat database schemas or external APIs
- **Performance optimization** - Reduce object graph complexity in data transfer

### 🔀 Built-in Type Conversion

The generator automatically converts between common types when property names match but types differ. This is particularly useful when mapping domain models with strongly-typed properties to DTOs that use string representations.

**Example:**

```csharp
using Atc.SourceGenerators.Annotations;
using System;

// Domain model with strongly-typed properties
[MapTo(typeof(UserEventDto))]
public partial class UserEvent
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int DurationSeconds { get; set; }
    public bool Success { get; set; }
}

// DTO with string-based properties
public class UserEventDto
{
    public string EventId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string DurationSeconds { get; set; } = string.Empty;
    public string Success { get; set; } = string.Empty;
}

// Generated: Automatic type conversion
public static UserEventDto MapToUserEventDto(this UserEvent source)
{
    if (source is null)
    {
        return default!;
    }

    return new UserEventDto
    {
        EventId = source.EventId.ToString(),  // Guid → string
        UserId = source.UserId.ToString(),
        Timestamp = source.Timestamp.ToString("O", global::System.Globalization.CultureInfo.InvariantCulture),  // DateTimeOffset → string (ISO 8601)
        DurationSeconds = source.DurationSeconds.ToString(global::System.Globalization.CultureInfo.InvariantCulture),  // int → string
        Success = source.Success.ToString()  // bool → string
    };
}
```

**Supported Conversions:**

| Source Type | Target Type | Conversion Method |
|------------|-------------|-------------------|
| `DateTime` | `string` | `.ToString("O", InvariantCulture)` (ISO 8601) |
| `string` | `DateTime` | `DateTime.Parse(value, InvariantCulture)` |
| `DateTimeOffset` | `string` | `.ToString("O", InvariantCulture)` (ISO 8601) |
| `string` | `DateTimeOffset` | `DateTimeOffset.Parse(value, InvariantCulture)` |
| `Guid` | `string` | `.ToString()` |
| `string` | `Guid` | `Guid.Parse(value)` |
| Numeric types* | `string` | `.ToString(InvariantCulture)` |
| `string` | Numeric types* | `{Type}.Parse(value, InvariantCulture)` |
| `bool` | `string` | `.ToString()` |
| `string` | `bool` | `bool.Parse(value)` |

*Numeric types: `int`, `long`, `short`, `byte`, `sbyte`, `uint`, `ulong`, `ushort`, `decimal`, `double`, `float`

**Reverse Conversion Example:**

```csharp
// Reverse mapping: string → strong types
[MapTo(typeof(UserEvent))]
public partial class UserEventDto
{
    public string EventId { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string DurationSeconds { get; set; } = string.Empty;
}

// Generated: Parse methods for string → strong types
EventId = global::System.Guid.Parse(source.EventId),
Timestamp = global::System.DateTimeOffset.Parse(source.Timestamp, global::System.Globalization.CultureInfo.InvariantCulture),
DurationSeconds = int.Parse(source.DurationSeconds, global::System.Globalization.CultureInfo.InvariantCulture)
```

**Culture and Format:**
- All numeric and DateTime conversions use `InvariantCulture` for consistency
- DateTime/DateTimeOffset use ISO 8601 format ("O") for string conversion
- This ensures the generated mappings are culture-independent and portable

**Works with:**
- Bidirectional mappings (automatic conversion in both directions)
- Nullable types (proper null handling for both source and target)
- Other mapping features (MapIgnore, MapProperty, constructor mapping, etc.)

**Use Cases:**
- **API boundaries** - Convert strongly-typed domain models to string-based JSON DTOs
- **Database mappings** - Map between typed entities and string-based legacy schemas
- **Configuration** - Convert configuration values between types
- **Export/Import** - Generate CSV or other text-based formats from typed data

### ✅ Required Property Validation

The generator validates at **compile time** that all `required` properties (C# 11+) on the target type have corresponding mappings from the source type. This catches missing property mappings during development instead of discovering issues at runtime.

#### Basic Example

```csharp
// ❌ This will generate ATCMAP004 warning at compile time
[MapTo(typeof(UserRegistrationDto))]
public partial class UserRegistration
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    // Missing: Email property (required in target)
}

public class UserRegistrationDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }     // ⚠️ Required but not mapped!
    public required string FullName { get; set; }  // ✅ Mapped
}

// Compiler output:
// Warning ATCMAP004: Required property 'Email' on target type 'UserRegistrationDto' has no mapping from source type 'UserRegistration'
```

#### Correct Implementation

```csharp
// ✅ All required properties have mappings - no warnings
[MapTo(typeof(UserRegistrationDto))]
public partial class UserRegistration
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;     // ✅ Maps to required property
    public string FullName { get; set; } = string.Empty;   // ✅ Maps to required property
    public string? PhoneNumber { get; set; }               // Optional property (can be omitted)
}

public class UserRegistrationDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }     // ✅ Mapped from source
    public required string FullName { get; set; }  // ✅ Mapped from source
    public string? PhoneNumber { get; set; }       // Not required (can be omitted from source)
}

// Generated mapping method:
public static UserRegistrationDto MapToUserRegistrationDto(this UserRegistration source)
{
    if (source is null)
    {
        return default!;
    }

    return new UserRegistrationDto
    {
        Id = source.Id,
        Email = source.Email,        // ✅ Required property mapped
        FullName = source.FullName,  // ✅ Required property mapped
        PhoneNumber = source.PhoneNumber,
    };
}
```

#### Validation Behavior

**When ATCMAP004 is Generated:**
- Target property has the `required` modifier (C# 11+)
- No corresponding property exists in the source type
- Property is not marked with `[MapIgnore]`

**When No Warning is Generated:**
- All required properties have mappings (by name or via `[MapProperty]`)
- Target property is NOT required (no `required` keyword)
- Target property is marked with `[MapIgnore]`

**Diagnostic Details:**
- **ID**: ATCMAP004
- **Severity**: Warning (can be elevated to Error in `.editorconfig`)
- **Message**: "Required property '{PropertyName}' on target type '{TargetType}' has no mapping from source type '{SourceType}'"

#### Elevating to Error

You can configure the diagnostic as an error to enforce strict mapping validation:

**.editorconfig:**
```ini
# Treat missing required property mappings as compilation errors
dotnet_diagnostic.ATCMAP004.severity = error
```

**Project file:**
```xml
<PropertyGroup>
  <WarningsAsErrors>$(WarningsAsErrors);ATCMAP004</WarningsAsErrors>
</PropertyGroup>
```

**Works With:**
- Type conversions (built-in and enum mappings)
- Nested object mappings
- Collection mappings
- Custom property name mapping via `[MapProperty]`
- Bidirectional mappings
- Constructor mappings

**Use Cases:**
- **API contracts** - Ensure all required fields in request/response DTOs are mapped
- **Data validation** - Catch missing required properties at compile time instead of runtime
- **Refactoring safety** - Adding `required` to a DTO property immediately flags all unmapped sources
- **Team standards** - Enforce property mapping completeness across large codebases

### 🌳 Polymorphic / Derived Type Mapping

The generator supports polymorphic type mapping for abstract base classes and interfaces with multiple derived types. This enables runtime type discrimination using C# switch expressions and type pattern matching.

#### Basic Example

```csharp
// Domain layer - abstract base class
[MapTo(typeof(Contract.AnimalDto))]
[MapDerivedType(typeof(Dog), typeof(Contract.DogDto))]
[MapDerivedType(typeof(Cat), typeof(Contract.CatDto))]
public abstract partial class Animal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Domain layer - derived classes
[MapTo(typeof(Contract.DogDto))]
public partial class Dog : Animal
{
    public string Breed { get; set; } = string.Empty;
}

[MapTo(typeof(Contract.CatDto))]
public partial class Cat : Animal
{
    public int Lives { get; set; }
}

// Contract layer - DTOs
public abstract class AnimalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DogDto : AnimalDto
{
    public string Breed { get; set; } = string.Empty;
}

public class CatDto : AnimalDto
{
    public int Lives { get; set; }
}

// Generated: Polymorphic mapping with switch expression
public static AnimalDto MapToAnimalDto(this Animal source)
{
    if (source is null)
    {
        return default!;
    }

    return source switch
    {
        Dog dog => dog.MapToDogDto(),
        Cat cat => cat.MapToCatDto(),
        _ => throw new global::System.ArgumentException($"Unknown derived type: {source.GetType().Name}")
    };
}
```

#### How It Works

1. **Base Class Attribute**: Apply `[MapDerivedType]` attributes to the abstract base class for each derived type mapping
2. **Derived Class Mappings**: Each derived class must have its own `[MapTo]` attribute mapping to the corresponding target derived type
3. **Switch Expression**: The generator creates a switch expression that performs type pattern matching
4. **Null Safety**: The generated code includes null checks for the source parameter
5. **Error Handling**: Unmapped derived types throw an `ArgumentException` with a descriptive message

#### Real-World Example - Notification System

```csharp
// Domain layer
[MapTo(typeof(NotificationDto))]
[MapDerivedType(typeof(EmailNotification), typeof(EmailNotificationDto))]
[MapDerivedType(typeof(SmsNotification), typeof(SmsNotificationDto))]
public abstract partial class Notification
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

[MapTo(typeof(EmailNotificationDto))]
public partial class EmailNotification : Notification
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

[MapTo(typeof(SmsNotificationDto))]
public partial class SmsNotification : Notification
{
    public string PhoneNumber { get; set; } = string.Empty;
}

// Usage in API endpoint
app.MapGet("/notifications", () =>
{
    var notifications = new List<Notification>
    {
        new EmailNotification
        {
            Id = Guid.NewGuid(),
            Message = "Welcome to our service!",
            CreatedAt = DateTimeOffset.UtcNow,
            To = "user@example.com",
            Subject = "Welcome",
        },
        new SmsNotification
        {
            Id = Guid.NewGuid(),
            Message = "Your code is 123456",
            CreatedAt = DateTimeOffset.UtcNow,
            PhoneNumber = "+1-555-0123",
        },
    };

    // ✨ Polymorphic mapping - automatically handles derived types
    var dtos = notifications
        .Select(n => n.MapToNotificationDto())
        .ToList();

    return Results.Ok(dtos);
});
```

#### Key Features

**Compile-Time Validation:**
- Verifies that each derived type mapping has a corresponding `MapTo` attribute
- Ensures the target types match the declared derived type mappings

**Type Safety:**
- All type checking happens at compile time
- No reflection or runtime type discovery
- Switch expressions provide exhaustive type coverage

**Performance:**
- Zero runtime overhead - pure switch expressions
- No dictionary lookups or type caching
- Native AOT compatible

**Null Safety:**
- Generated code includes proper null checks
- Follows nullable reference type annotations

**Extensibility:**
- Support for arbitrary numbers of derived types
- Works with deep inheritance hierarchies
- Can be combined with other mapping features (collections, nesting, etc.)

**Use Cases:**
- **Polymorphic API responses** - Return different DTO types based on domain object type
- **Notification systems** - Map different notification types (Email, SMS, Push) from domain to DTOs
- **Payment processing** - Handle different payment method types (CreditCard, PayPal, BankTransfer)
- **Document types** - Map different document formats (PDF, Word, Excel) to DTOs
- **Event sourcing** - Map different event types from domain events to event DTOs

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

### 🪝 Before/After Mapping Hooks

Execute custom logic before or after the mapping operation using hook methods. This feature allows you to add validation, logging, enrichment, or any other custom behavior to your mappings without writing wrapper methods.

#### Basic Usage

```csharp
using Atc.SourceGenerators.Annotations;

[MapTo(typeof(UserDto), BeforeMap = nameof(ValidateUser), AfterMap = nameof(EnrichDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // BeforeMap hook - called after null check, before mapping
    private static void ValidateUser(User source)
    {
        if (string.IsNullOrWhiteSpace(source.Name))
        {
            throw new ArgumentException("Name cannot be empty");
        }
    }

    // AfterMap hook - called after mapping, before return
    private static void EnrichDto(User source, UserDto target)
    {
        target.DisplayName = $"{source.Name} (ID: {source.Id})";
    }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
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

    // BeforeMap hook - called after null check, before mapping
    User.ValidateUser(source);

    var target = new UserDto
    {
        Id = source.Id,
        Name = source.Name
    };

    // AfterMap hook - called after mapping, before return
    User.EnrichDto(source, target);

    return target;
}
```

#### Hook Signatures

**BeforeMap Hook:**
- **Signature**: `static void MethodName(SourceType source)`
- **When called**: After null check, before object creation
- **Parameters**: Source object only
- **Purpose**: Validation, preprocessing, logging

**AfterMap Hook:**
- **Signature**: `static void MethodName(SourceType source, TargetType target)`
- **When called**: After object creation, before return
- **Parameters**: Both source and target objects
- **Purpose**: Post-processing, enrichment, computed properties

#### Execution Order

The mapping lifecycle follows this sequence:

1. **Null check** on source object
2. **BeforeMap hook** (if specified)
3. **Polymorphic type check** (if derived type mappings exist)
4. **Object creation** (constructor or object initializer)
5. **AfterMap hook** (if specified)
6. **Return** target object

#### Using Only BeforeMap

For validation-only scenarios, use just the BeforeMap hook:

```csharp
[MapTo(typeof(OrderDto), BeforeMap = nameof(ValidateOrder))]
public partial class Order
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    private static void ValidateOrder(Order source)
    {
        if (source.Total <= 0)
        {
            throw new ArgumentException("Order total must be positive");
        }

        if (source.Items.Count == 0)
        {
            throw new ArgumentException("Order must have at least one item");
        }
    }
}

// Generated: Only BeforeMap is called
public static OrderDto MapToOrderDto(this Order source)
{
    if (source is null)
    {
        return default!;
    }

    Order.ValidateOrder(source);  // ✅ Validation before mapping

    return new OrderDto
    {
        Id = source.Id,
        Total = source.Total,
        Items = source.Items?.Select(x => x.MapToOrderItemDto()).ToList()!
    };
}
```

#### Using Only AfterMap

For enrichment-only scenarios, use just the AfterMap hook:

```csharp
[MapTo(typeof(ProductDto), AfterMap = nameof(CalculateDiscountPrice))]
public partial class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }

    private static void CalculateDiscountPrice(Product source, ProductDto target)
    {
        target.DiscountedPrice = source.Price * (1 - source.DiscountPercentage / 100);
    }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }  // Computed in AfterMap
}

// Generated: Only AfterMap is called
public static ProductDto MapToProductDto(this Product source)
{
    if (source is null)
    {
        return default!;
    }

    var target = new ProductDto
    {
        Id = source.Id,
        Name = source.Name,
        Price = source.Price
    };

    Product.CalculateDiscountPrice(source, target);  // ✅ Enrichment after mapping

    return target;
}
```

#### Hooks with Constructor Mapping

Hooks work seamlessly with constructor-based mappings:

```csharp
public record PersonDto(Guid Id, string FullName)
{
    public string Initials { get; set; } = string.Empty;
}

[MapTo(typeof(PersonDto), AfterMap = nameof(SetInitials))]
public partial class Person
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;

    private static void SetInitials(Person source, PersonDto target)
    {
        var names = source.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        target.Initials = string.Join("", names.Select(n => n[0]));
    }
}

// Generated: Constructor + AfterMap hook
public static PersonDto MapToPersonDto(this Person source)
{
    if (source is null)
    {
        return default!;
    }

    var target = new PersonDto(  // Constructor call
        source.Id,
        source.FullName);

    Person.SetInitials(source, target);  // AfterMap hook

    return target;
}
```

#### Use Cases

**Validation (BeforeMap):**
```csharp
private static void ValidateUser(User source)
{
    if (string.IsNullOrWhiteSpace(source.Email))
    {
        throw new ArgumentException("Email is required");
    }

    if (!source.Email.Contains('@'))
    {
        throw new ArgumentException("Invalid email format");
    }
}
```

**Logging (BeforeMap or AfterMap):**
```csharp
private static void LogMapping(User source, UserDto target)
{
    Console.WriteLine($"Mapped User {source.Id} to UserDto");
}
```

**Enrichment (AfterMap):**
```csharp
private static void EnrichUserDto(User source, UserDto target)
{
    target.FullName = $"{source.FirstName} {source.LastName}";
    target.Age = DateTime.UtcNow.Year - source.DateOfBirth.Year;
}
```

**Auditing (AfterMap):**
```csharp
private static void AuditMapping(Order source, OrderDto target)
{
    target.MappedAt = DateTime.UtcNow;
    target.MappedBy = "ObjectMappingGenerator";
}
```

**Side Effects (AfterMap):**
```csharp
private static void UpdateCache(Product source, ProductDto target)
{
    // Update cache after successful mapping
    _cache.Set($"product:{source.Id}", target);
}
```

#### Important Notes

- ✅ Hook methods **must be static**
- ✅ Both hooks are **optional** - use one, both, or neither
- ✅ Hooks are specified by method name (use `nameof()` for type safety)
- ✅ Hooks work with all mapping features (collections, nested objects, polymorphic types, etc.)
- ✅ **Reverse mappings** (Bidirectional = true) do NOT inherit hooks from the forward mapping
- ✅ Hooks are called via fully qualified name (e.g., `User.ValidateUser(source)`)
- ✅ Full **Native AOT compatibility**

#### Hooks in Bidirectional Mappings

When using bidirectional mappings, each direction can have its own hooks:

```csharp
[MapTo(typeof(UserDto), Bidirectional = true, BeforeMap = nameof(ValidateUser), AfterMap = nameof(EnrichDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    private static void ValidateUser(User source) { /* Validation */ }
    private static void EnrichDto(User source, UserDto target) { /* Enrichment */ }
}

// Generated forward mapping: User → UserDto (includes hooks)
public static UserDto MapToUserDto(this User source)
{
    // ... includes ValidateUser and EnrichDto hooks
}

// Generated reverse mapping: UserDto → User (NO hooks)
public static User MapToUser(this UserDto source)
{
    // ... reverse mapping does NOT call ValidateUser or EnrichDto
}
```

If you need hooks in the reverse direction, define them on the target type:

```csharp
[MapTo(typeof(User), BeforeMap = nameof(ValidateDto))]
public partial class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    private static void ValidateDto(UserDto source) { /* Validation */ }
}
```

### 🏭 Object Factories

Use custom factory methods to create target instances during mapping, allowing you to initialize objects with default values, use object pooling, or apply other custom creation logic.

#### Basic Usage

```csharp
using Atc.SourceGenerators.Annotations;

[MapTo(typeof(UserDto), Factory = nameof(CreateUserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Factory method creates the target instance
    internal static UserDto CreateUserDto()
    {
        return new UserDto
        {
            CreatedAt = DateTimeOffset.UtcNow,  // Set default value
        };
    }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
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

    var target = User.CreateUserDto();  // Factory creates instance

    target.Id = source.Id;               // Property mappings applied
    target.Name = source.Name;

    return target;
}
```

#### Factory Method Signature

**Signature**: `static TargetType MethodName()`

- Must be static
- Must return the target type
- Takes no parameters
- Can be `internal`, `public`, or `private`

#### Execution Order

When a factory is specified, the mapping lifecycle follows this sequence:

1. **Null check** on source object
2. **BeforeMap hook** (if specified)
3. **Factory method** creates target instance
4. **Property mappings** applied to target
5. **AfterMap hook** (if specified)
6. **Return** target object

#### Factory with Hooks

Factories work seamlessly with BeforeMap and AfterMap hooks:

```csharp
[MapTo(typeof(OrderDto), Factory = nameof(CreateOrderDto), BeforeMap = nameof(ValidateOrder), AfterMap = nameof(EnrichOrder))]
public partial class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTimeOffset OrderDate { get; set; }

    internal static void ValidateOrder(Order source)
    {
        if (string.IsNullOrWhiteSpace(source.OrderNumber))
        {
            throw new ArgumentException("Order number is required");
        }
    }

    internal static OrderDto CreateOrderDto()
    {
        return new OrderDto
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Status = "Pending",  // Default status
        };
    }

    internal static void EnrichOrder(
        Order source,
        OrderDto target)
    {
        target.FormattedOrderNumber = $"ORD-{source.OrderNumber}";
    }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTimeOffset OrderDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string FormattedOrderNumber { get; set; } = string.Empty;
}
```

**Generated code:**

```csharp
public static OrderDto MapToOrderDto(this Order source)
{
    if (source is null)
    {
        return default!;
    }

    Order.ValidateOrder(source);         // BeforeMap hook

    var target = Order.CreateOrderDto(); // Factory creates instance

    target.Id = source.Id;                // Property mappings
    target.OrderNumber = source.OrderNumber;
    target.OrderDate = source.OrderDate;

    Order.EnrichOrder(source, target);   // AfterMap hook

    return target;
}
```

#### Use Cases

**Default Values:**
```csharp
internal static ProductDto CreateProductDto()
{
    return new ProductDto
    {
        CreatedAt = DateTimeOffset.UtcNow,
        IsActive = true,
        Version = 1,
    };
}
```

**Object Pooling:**
```csharp
private static readonly ObjectPool<UserDto> _userDtoPool = new();

internal static UserDto CreateUserDto()
{
    return _userDtoPool.Get();  // Reuse objects from pool
}
```

**Dependency Injection (Service Locator):**
```csharp
internal static NotificationDto CreateNotificationDto()
{
    var factory = ServiceLocator.GetService<INotificationDtoFactory>();
    return factory.Create();
}
```

**Complex Initialization:**
```csharp
internal static ReportDto CreateReportDto()
{
    var dto = new ReportDto();
    dto.Initialize();  // Custom initialization logic
    dto.RegisterEventHandlers();
    return dto;
}
```

#### Important Notes

- ✅ Factory method **must be static**
- ✅ Factory **replaces** `new TargetType()` for object creation
- ✅ Property mappings are **applied after** factory creates the instance
- ✅ Fully compatible with **BeforeMap/AfterMap hooks**
- ✅ Works with all mapping features (nested objects, collections, etc.)
- ✅ **Reverse mappings** (Bidirectional = true) do NOT inherit factory methods
- ✅ Full **Native AOT compatibility**
- ⚠️ **Limitation**: Factory pattern doesn't work with init-only properties (records with `init` setters)
  - For init-only properties, use constructor mapping or object initializers instead

#### Factories in Bidirectional Mappings

When using bidirectional mappings, each direction can have its own factory:

```csharp
[MapTo(typeof(UserDto), Bidirectional = true, Factory = nameof(CreateUserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    internal static UserDto CreateUserDto()
    {
        return new UserDto { CreatedAt = DateTimeOffset.UtcNow };
    }
}

// Generated forward mapping: User → UserDto (includes factory)
public static UserDto MapToUserDto(this User source)
{
    // ... uses CreateUserDto factory
}

// Generated reverse mapping: UserDto → User (NO factory)
public static User MapToUser(this UserDto source)
{
    // ... uses standard object initializer
}
```

If you need a factory in the reverse direction, define it on the target type:

```csharp
[MapTo(typeof(User), Factory = nameof(CreateUser))]
public partial class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    internal static User CreateUser()
    {
        return new User();  // Custom creation logic
    }
}
```

---

## Update Existing Target Instance

The `UpdateTarget` parameter allows you to generate an additional method overload that updates an existing target instance instead of creating a new one. This is particularly useful when working with **EF Core tracked entities**, **ViewModels**, or when you want to reduce object allocations.

### Basic Usage

```csharp
[MapTo(typeof(UserDto), UpdateTarget = true)]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

**Generated Code:**
```csharp
// Method 1: Standard method (creates new instance)
public static UserDto MapToUserDto(this User source)
{
    if (source is null) return default!;

    return new UserDto
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email
    };
}

// Method 2: Update method (updates existing instance)
public static void MapToUserDto(this User source, UserDto target)
{
    if (source is null) return;
    if (target is null) return;

    target.Id = source.Id;
    target.Name = source.Name;
    target.Email = source.Email;
}
```

### EF Core Tracked Entities

The primary use case for `UpdateTarget` is updating EF Core tracked entities:

```csharp
[MapTo(typeof(PetEntity), Bidirectional = true, UpdateTarget = true)]
public partial class Pet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
}

// Usage in a service:
public async Task UpdatePetAsync(Guid petId, Pet domainPet)
{
    // Fetch tracked entity from database
    var existingPet = await _dbContext.Pets.FindAsync(petId);
    if (existingPet is null)
    {
        throw new NotFoundException($"Pet with ID {petId} not found");
    }

    // Update it with new data (EF Core tracks changes)
    domainPet.MapToPetEntity(existingPet);

    // Save changes (only modified properties are updated in database)
    await _dbContext.SaveChangesAsync();
}
```

### Update with Hooks

The update method fully supports `BeforeMap` and `AfterMap` hooks:

```csharp
[MapTo(typeof(OrderDto), UpdateTarget = true, BeforeMap = nameof(ValidateOrder), AfterMap = nameof(EnrichOrder))]
public partial class Order
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }

    internal static void ValidateOrder(Order source)
    {
        if (source.Total < 0)
            throw new ArgumentException("Total cannot be negative");
    }

    internal static void EnrichOrder(Order source, OrderDto target)
    {
        // Custom enrichment logic after update
        target.LastModified = DateTimeOffset.UtcNow;
    }
}

// Usage:
var existingDto = GetOrderDto(orderId);
updatedOrder.MapToOrderDto(existingDto);  // Validates, updates, then enriches
```

**Execution Order for Update Method:**
1. Null check for source
2. Null check for target
3. Execute `BeforeMap(source)` hook (if specified)
4. Update all properties on target
5. Execute `AfterMap(source, target)` hook (if specified)

### Reduce Object Allocations

Reuse DTO instances to reduce allocations in hot paths:

```csharp
[MapTo(typeof(SettingsDto), UpdateTarget = true)]
public partial class Settings
{
    public string Theme { get; set; } = "Light";
    public bool EnableNotifications { get; set; } = true;
}

// Reuse the same DTO instance
var settingsDto = new SettingsDto();

settings1.MapToSettingsDto(settingsDto);
ProcessSettings(settingsDto);

settings2.MapToSettingsDto(settingsDto);  // Reuse same instance
ProcessSettings(settingsDto);
```

### Important Notes

- **Both methods are generated**: When `UpdateTarget = true`, you get both the standard method (creates new instance) and the update method (updates existing instance)
- **Null checks**: The update method checks both source and target for null
- **Void return**: The update method returns `void` (no return value)
- **No factory**: The update method does not use factory methods (factory is only for creating new instances)
- **Bidirectional support**: Works seamlessly with `Bidirectional = true` - both directions get update overloads
- **All properties updated**: All mapped properties are updated, including nullable properties

### When to Use UpdateTarget

✅ **Use when:**
- Updating EF Core tracked entities
- Reducing allocations for frequently mapped objects
- Updating existing ViewModels or DTOs
- You need to preserve object identity
- Working with object pools

❌ **Don't use when:**
- You always need new instances
- Working with immutable types (records with init-only properties)
- Factory method is needed (factory creates new instances)
- You want the update operation to return a value

### Comparison with Standard Mapping

| Feature | Standard Method | Update Method |
|---------|----------------|---------------|
| **Return Type** | `TargetType` | `void` |
| **Creates New Instance** | ✅ Yes | ❌ No |
| **Updates Existing Instance** | ❌ No | ✅ Yes |
| **Target Parameter** | ❌ No | ✅ Yes (`TargetType target`) |
| **EF Core Compatible** | ⚠️ Requires attach | ✅ Yes (change tracking) |
| **Null Checks** | Source only | Source and target |
| **BeforeMap Hook** | ✅ Yes | ✅ Yes |
| **AfterMap Hook** | ✅ Yes | ✅ Yes |
| **Factory Support** | ✅ Yes | ❌ No |

---

## 📊 IQueryable Projections

Generate `Expression<Func<TSource, TTarget>>` for use with EF Core `.Select()` queries to enable server-side projection. This feature optimizes database queries by selecting only the required columns instead of fetching entire entities.

### When to Use IQueryable Projections

✅ **Use projections when:**
- Fetching data for list/grid views where you need minimal fields
- Optimizing database query performance
- Reducing network traffic between application and database
- Working with large datasets where full entity hydration is expensive
- Need server-side filtering and sorting with minimal data transfer

❌ **Don't use projections when:**
- You need BeforeMap/AfterMap hooks (not supported in expressions)
- You need Factory methods (not supported in expressions)
- You have nested objects or collections (require method calls)
- You need complex type conversions (only simple casts work)
- The mapping is used for write operations (projections are read-only)

### Basic Example

```csharp
using Atc.SourceGenerators.Annotations;

// Define a lightweight DTO for list views
public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserStatusDto Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

// Enable projection with GenerateProjection = true
[MapTo(typeof(UserSummaryDto), GenerateProjection = true)]
public partial class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PreferredName { get; set; } = string.Empty;  // Not in DTO, excluded from projection
    public UserStatus Status { get; set; }
    public Address? Address { get; set; }  // Nested object, excluded from projection
    public DateTimeOffset CreatedAt { get; set; }
    public byte[] PasswordHash { get; set; } = [];  // Not in DTO, excluded from projection
}

// Generated projection method
public static Expression<Func<User, UserSummaryDto>> ProjectToUserSummaryDto()
{
    return source => new UserSummaryDto
    {
        Id = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        Email = source.Email,
        Status = (UserStatusDto)source.Status,  // Simple enum cast
        CreatedAt = source.CreatedAt
    };
}

// Usage with EF Core
var users = await dbContext.Users
    .Where(u => u.IsActive)
    .OrderBy(u => u.LastName)
    .Select(User.ProjectToUserSummaryDto())
    .ToListAsync();

// SQL generated (optimized - only selected columns):
// SELECT Id, FirstName, LastName, Email, Status, CreatedAt
// FROM Users
// WHERE IsActive = 1
// ORDER BY LastName
```

### Projection Limitations

IQueryable projections have important limitations because they generate Expression trees that EF Core translates to SQL:

| Feature | Standard Mapping | IQueryable Projection |
|---------|------------------|----------------------|
| **BeforeMap Hook** | ✅ Supported | ❌ Not supported (expressions can't call methods) |
| **AfterMap Hook** | ✅ Supported | ❌ Not supported (expressions can't call methods) |
| **Factory Method** | ✅ Supported | ❌ Not supported (must use object initializer) |
| **Nested Objects** | ✅ Supported | ❌ Not supported (would require `.MapToX()` calls) |
| **Collections** | ✅ Supported | ❌ Not supported (would require `.Select()` calls) |
| **Built-in Type Conversions** | ✅ Supported | ❌ Not supported (only simple casts work) |
| **Simple Properties** | ✅ Supported | ✅ Supported |
| **Enum Conversions** | ✅ Supported | ✅ Supported (via simple casts) |
| **UpdateTarget** | ✅ Supported | ❌ Not applicable (projections are read-only) |

### What Gets Included in Projections

The generator **automatically excludes** the following from projection expressions:

1. **Nested Objects** - Properties of class/struct types (other than primitives/enums)
2. **Collections** - `IEnumerable<T>`, `List<T>`, arrays, etc.
3. **Properties without matching target** - Source properties not found in target DTO
4. **Properties marked with `[MapIgnore]`** - Excluded from all mappings

**Only simple properties are included:**
- Primitive types (`int`, `string`, `Guid`, `DateTime`, `DateTimeOffset`, etc.)
- Enums (converted via simple casts)
- Value types (`decimal`, `bool`, etc.)

### Comparison: Standard Mapping vs. Projection

```csharp
// Standard mapping (loads entire entity, then maps in memory)
var users = await dbContext.Users
    .Where(u => u.IsActive)
    .ToListAsync();  // ⚠️ Fetches ALL columns for ALL users
var dtos = users.Select(u => u.MapToUserDto()).ToList();  // ✅ Maps in-memory

// SQL: SELECT * FROM Users WHERE IsActive = 1  (fetches all columns)

// ---

// Projection (maps on the database server)
var dtos = await dbContext.Users
    .Where(u => u.IsActive)
    .Select(User.ProjectToUserDto())  // ✅ Translates to SQL SELECT
    .ToListAsync();

// SQL: SELECT Id, Name, Email FROM Users WHERE IsActive = 1  (only required columns)
```

### Performance Benefits

**Database Query Optimization:**
- ✅ Reduced data transfer (only selected columns)
- ✅ Smaller result sets (fewer bytes over network)
- ✅ Faster queries (database processes less data)
- ✅ Better index usage (covering indexes possible)

**Memory Optimization:**
- ✅ Less memory allocated (no full entity objects)
- ✅ Fewer GC collections (smaller object graphs)
- ✅ Better cache locality (smaller DTO objects)

### Real-World Example: Pet Store List View

```csharp
using Atc.SourceGenerators.Annotations;

// Lightweight DTO for pet list/grid view
public class PetListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    public PetStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

// Domain model with projection enabled
[MapTo(typeof(PetListItemDto), GenerateProjection = true)]
public partial class Pet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    public PetStatus Status { get; set; }
    public Owner? Owner { get; set; }  // ❌ Excluded (nested object)
    public IList<Pet> Children { get; set; } = new List<Pet>();  // ❌ Excluded (collection)
    public DateTimeOffset CreatedAt { get; set; }
}

// API endpoint using projection
app.MapGet("/pets", async (PetDbContext db) =>
{
    var pets = await db.Pets
        .Where(p => p.Status == PetStatus.Available)
        .OrderBy(p => p.Name)
        .Select(Pet.ProjectToPetListItemDto())  // ✅ Server-side projection
        .Take(100)
        .ToListAsync();

    return Results.Ok(pets);
});

// SQL (optimized):
// SELECT TOP(100) Id, Name, Species, Breed, Age, Status, CreatedAt
// FROM Pets
// WHERE Status = 1
// ORDER BY Name
```

### Best Practices

**1. Create Dedicated DTOs for Projections**
```csharp
// ✅ Good: Lightweight DTO designed for projections
public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// ❌ Bad: Heavy DTO with nested objects
public class UserDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = null!;  // Won't work in projection
}
```

**2. Use Standard Mapping for Complex Scenarios**
```csharp
// For read-only lists: Use projection
var summary = await db.Users.Select(User.ProjectToUserSummaryDto()).ToListAsync();

// For create/update: Use standard mapping with hooks
var userDto = user.MapToUserDto();  // Supports hooks, factory, etc.
```

**3. Combine Projections with EF Core Features**
```csharp
// Filtering, sorting, paging - all on the server
var results = await db.Pets
    .Where(p => p.Age > 2)               // Server-side filter
    .OrderBy(p => p.Name)                // Server-side sort
    .Skip(pageIndex * pageSize)          // Server-side skip
    .Take(pageSize)                      // Server-side take
    .Select(Pet.ProjectToPetListItemDto())  // Server-side projection
    .ToListAsync();                      // Single optimized SQL query
```

### Troubleshooting

**Q: Why isn't my nested object included in the projection?**

A: Projections only support simple properties. Nested objects require method calls like `.MapToX()` which can't be translated to SQL.

**Solution:** Either flatten the nested properties using `EnableFlattening = true` in a separate mapping, or use standard mapping instead.

**Q: Why can't I use BeforeMap/AfterMap with projections?**

A: Expression trees (which projections use) can only contain expressions that EF Core can translate to SQL. Method calls like hooks aren't supported.

**Solution:** Use standard mapping (`MapToX()`) when you need hooks. Use projections only for read-only, simple scenarios.

**Q: The generator excluded all my properties from the projection!**

A: Check that:
1. Target DTO properties match source properties by name (case-insensitive)
2. Properties are simple types (not classes, collections, or complex types)
3. Properties aren't marked with `[MapIgnore]`
4. Source and target types are compatible (or enum-to-enum)

---

## ⚙️ MapToAttribute Parameters

The `MapToAttribute` accepts the following parameters:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `targetType` | `Type` | ✅ Yes | - | The type to map to |
| `Bidirectional` | `bool` | ❌ No | `false` | Generate bidirectional mappings (both Source → Target and Target → Source) |
| `EnableFlattening` | `bool` | ❌ No | `false` | Enable property flattening (nested properties are flattened using {PropertyName}{NestedPropertyName} convention) |
| `BeforeMap` | `string?` | ❌ No | `null` | Name of a static method to call before performing the mapping. Signature: `static void MethodName(SourceType source)` |
| `AfterMap` | `string?` | ❌ No | `null` | Name of a static method to call after performing the mapping. Signature: `static void MethodName(SourceType source, TargetType target)` |
| `Factory` | `string?` | ❌ No | `null` | Name of a static factory method to use for creating the target instance. Signature: `static TargetType MethodName()` |
| `UpdateTarget` | `bool` | ❌ No | `false` | Generate an additional method overload that updates an existing target instance instead of creating a new one. Generates both `MapToX()` and `MapToX(target)` methods |
| `GenerateProjection` | `bool` | ❌ No | `false` | Generate an Expression projection method for use with IQueryable (EF Core server-side projection). Generates `ProjectToX()` method that returns `Expression<Func<TSource, TTarget>>`. Only simple property mappings are supported (no hooks, factory, nested objects, or collections) |

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

### ⚠️ ATCMAP004: Required Property Not Mapped

**Warning:** A required property on the target type has no corresponding mapping from the source type.

**Example:**
```csharp
[MapTo(typeof(UserRegistrationDto))]
public partial class UserRegistration
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    // Missing: Email property
}

public class UserRegistrationDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }     // ⚠️ Required but not mapped!
    public required string FullName { get; set; }
}

// Warning ATCMAP004: Required property 'Email' on target type 'UserRegistrationDto' has no mapping from source type 'UserRegistration'
```

**Fix:**
```csharp
[MapTo(typeof(UserRegistrationDto))]
public partial class UserRegistration
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;     // ✅ Added to fix ATCMAP004
    public string FullName { get; set; } = string.Empty;
}
```

**Why:** The generator validates at compile time that all `required` properties (C# 11+) on the target type have mappings. This catches missing required properties during development instead of discovering issues at runtime or during object initialization.

**Elevating to Error:** You can configure this diagnostic as an error in `.editorconfig`:
```ini
dotnet_diagnostic.ATCMAP004.severity = error
```

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
