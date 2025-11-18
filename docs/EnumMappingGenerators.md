# 🔄 Enum Mapping Generator

Automatically generate type-safe enum-to-enum mapping code using attributes. The generator creates efficient switch expression mappings at compile time with intelligent name matching and special case handling, eliminating manual enum conversions and reducing errors.

**Key Benefits:**

- 🎯 **Zero runtime cost** - Pure switch expressions generated at compile time
- 🧠 **Intelligent matching** - Automatic special case detection (None → Unknown, Active → Enabled, etc.)
- 🔄 **Bidirectional support** - Generate forward and reverse mappings with one attribute
- 🛡️ **Type-safe** - Compile-time diagnostics for unmapped values
- ⚡ **Native AOT ready** - No reflection, fully trimming-safe

**Quick Example:**

```csharp
// Input: Decorate your enum
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatus { None, Available, Adopted }

// Generated: Efficient switch expression
public static PetStatusDto MapToPetStatusDto(this PetStatus source) =>
    source switch {
        PetStatus.None => PetStatusDto.Unknown,  // Special case auto-detected
        PetStatus.Available => PetStatusDto.Available,
        PetStatus.Adopted => PetStatusDto.Adopted,
        _ => throw new ArgumentOutOfRangeException(nameof(source))
    };
```

## 📖 Documentation Navigation

- **[🎯 Sample Projects](EnumMappingGenerators-Samples.md)** - Working code examples with architecture diagrams

## 📑 Table of Contents

- [� Enum Mapping Generator](#-enum-mapping-generator)
  - [📑 Table of Contents](#-table-of-contents)
  - [🚀 Get Started - Quick Guide](#-get-started---quick-guide)
    - [📂 Project Structure](#-project-structure)
    - [1️⃣ Setup Project](#1️⃣-setup-project)
    - [2️⃣ Define Enums](#2️⃣-define-enums)
    - [3️⃣ Use Generated Mappings](#3️⃣-use-generated-mappings)
    - [🎨 What Gets Generated](#-what-gets-generated)
    - [🎯 Key Takeaways](#-key-takeaways)
  - [✨ Features](#-features)
  - [📦 Installation](#-installation)
  - [💡 Basic Usage](#-basic-usage)
    - [1️⃣ Add Using Directives](#1️⃣-add-using-directives)
    - [2️⃣ Decorate Your Enums](#2️⃣-decorate-your-enums)
    - [3️⃣ Use Generated Mappings](#3️⃣-use-generated-mappings-1)
  - [🏗️ Advanced Scenarios](#️-advanced-scenarios)
    - [🔀 Special Case Mappings](#-special-case-mappings)
    - [🔁 Bidirectional Mapping](#-bidirectional-mapping)
    - [🔤 Case-Insensitive Matching](#-case-insensitive-matching)
    - [🏛️ Multi-Layer Architecture](#️-multi-layer-architecture)
  - [⚙️ MapToAttribute Parameters](#️-maptoattribute-parameters)
  - [🛡️ Diagnostics](#️-diagnostics)
    - [❌ ATCENUM001: Target Type Must Be Enum](#-atcenum001-target-type-must-be-enum)
    - [⚠️ ATCENUM002: Unmapped Enum Value](#️-atcenum002-unmapped-enum-value)
  - [🚀 Native AOT Compatibility](#-native-aot-compatibility)
    - [✅ AOT-Safe Features](#-aot-safe-features)
    - [🏗️ How It Works](#️-how-it-works)
    - [📋 Example Generated Code](#-example-generated-code)
  - [📚 Additional Examples](#-additional-examples)
    - [Example 1: Order Status with None/Unknown](#example-1-order-status-with-noneunknown)
    - [Example 2: Bidirectional Mapping](#example-2-bidirectional-mapping)
    - [Example 3: Case-Insensitive Matching](#example-3-case-insensitive-matching)
  - [🔧 Best Practices](#-best-practices)
  - [📖 Related Documentation](#-related-documentation)

---

## 🚀 Get Started - Quick Guide

This guide demonstrates using enum mapping in a realistic application scenario with database entities and API DTOs.

### 📂 Project Structure

```
PetStore.sln
├── PetStore.Api/              (Presentation layer - DTOs)
├── PetStore.Domain/           (Business logic layer - Domain enums)
└── PetStore.DataAccess/       (Data access layer - Entity enums)
```

### 1️⃣ Setup Project

**PetStore.DataAccess.csproj**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />
    <PackageReference Include="Atc.SourceGenerators.Annotations" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### 2️⃣ Define Enums

**PetStore.DataAccess/Entities/PetStatusEntity.cs**:

```csharp
using Atc.SourceGenerators.Annotations;

namespace PetStore.DataAccess.Entities;

/// <summary>
/// Pet status in the database layer.
/// </summary>
[MapTo(typeof(Domain.Models.PetStatus), Bidirectional = true)]
public enum PetStatusEntity
{
    None,       // Will map to PetStatus.Unknown (special case)
    Pending,
    Available,
    Adopted,
}
```

**PetStore.Domain/Models/PetStatus.cs**:

```csharp
namespace PetStore.Domain.Models;

/// <summary>
/// Pet status in the domain layer.
/// </summary>
public enum PetStatus
{
    Unknown,    // Maps from PetStatusEntity.None
    Available,
    Pending,
    Adopted,
}
```

### 3️⃣ Use Generated Mappings

**PetStore.DataAccess/Repositories/PetRepository.cs**:

```csharp
using Atc.Mapping;  // Generated extension methods live here
using PetStore.DataAccess.Entities;
using PetStore.Domain.Models;

public class PetRepository
{
    public IEnumerable<Pet> GetByStatus(PetStatus status)
    {
        // Use generated mapping to convert domain enum to entity enum
        var entityStatus = status.MapToPetStatusEntity();

        var pets = database
            .Pets
            .Where(p => p.Status == entityStatus)
            .ToList();

        // Use generated mapping to convert entities back to domain
        return pets.Select(e => new Pet
        {
            Id = e.Id,
            Name = e.Name,
            Status = e.Status.MapToPetStatus(),  // ✨ Generated mapping
        });
    }
}
```

### 🎨 What Gets Generated

The generator creates extension methods with switch expressions in the `Atc.Mapping` namespace:

**Generated Code**:

```csharp
// <auto-generated/>
#nullable enable

namespace Atc.Mapping;

/// <summary>
/// Extension methods for enum mapping.
/// </summary>
[global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.EnumMapping", "1.0.0")]
public static class EnumMappingExtensions
{
    /// <summary>
    /// Maps <see cref="PetStore.DataAccess.Entities.PetStatusEntity"/>
    /// to <see cref="PetStore.Domain.Models.PetStatus"/>.
    /// </summary>
    public static PetStore.Domain.Models.PetStatus MapToPetStatus(
        this PetStore.DataAccess.Entities.PetStatusEntity source)
    {
        return source switch
        {
            PetStatusEntity.None => PetStatus.Unknown,        // Special case mapping
            PetStatusEntity.Pending => PetStatus.Pending,
            PetStatusEntity.Available => PetStatus.Available,
            PetStatusEntity.Adopted => PetStatus.Adopted,
            _ => throw new global::System.ArgumentOutOfRangeException(
                nameof(source), source, "Unmapped enum value"),
        };
    }

    /// <summary>
    /// Maps <see cref="PetStore.Domain.Models.PetStatus"/>
    /// to <see cref="PetStore.DataAccess.Entities.PetStatusEntity"/>.
    /// </summary>
    public static PetStore.DataAccess.Entities.PetStatusEntity MapToPetStatusEntity(
        this PetStore.Domain.Models.PetStatus source)
    {
        return source switch
        {
            PetStatus.Unknown => PetStatusEntity.None,        // Special case mapping
            PetStatus.Available => PetStatusEntity.Available,
            PetStatus.Pending => PetStatusEntity.Pending,
            PetStatus.Adopted => PetStatusEntity.Adopted,
            _ => throw new global::System.ArgumentOutOfRangeException(
                nameof(source), source, "Unmapped enum value"),
        };
    }
}
```

### 🎯 Key Takeaways

✅ **Zero Runtime Cost** - Pure switch expressions, no reflection
✅ **Compile-Time Safety** - Catch errors before runtime
✅ **Intelligent Matching** - Automatic special case detection (None → Unknown → Default)
✅ **Case-Insensitive** - Matches enum values regardless of casing
✅ **Bidirectional** - Generate both forward and reverse mappings with one attribute
✅ **Runtime Safety** - ArgumentOutOfRangeException for unmapped values

---

## ✨ Features

- **🎯 Intelligent Name Matching** - Maps enum values by name with case-insensitive support
- **🔀 Special Case Detection** - Automatically handles "zero/empty/null" state equivalents:
  - `None` ↔ `Unknown`, `Default`
  - `Unknown` ↔ `None`, `Default`
  - `Default` ↔ `None`, `Unknown`
- **⚡ Zero Runtime Cost** - Pure switch expressions, no reflection or runtime code generation
- **🚀 Native AOT Compatible** - Fully trimming-safe, works with Native AOT
- **🛡️ Type-Safe** - Compile-time validation with diagnostics for unmapped values
- **🔁 Bidirectional Mapping** - Generate both forward and reverse mappings with `Bidirectional = true`
- **🔤 Case-Insensitive** - Matches enum values regardless of casing differences
- **⚠️ Runtime Safety** - `ArgumentOutOfRangeException` thrown for unmapped values

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
using Atc.SourceGenerators.Annotations;  // For [MapTo]
using Atc.Mapping;                        // For generated extension methods
```

### 2️⃣ Decorate Your Enums

```csharp
// Source enum
[MapTo(typeof(StatusDto))]
public enum StatusEntity
{
    None,
    Active,
    Inactive,
}

// Target enum
public enum StatusDto
{
    Unknown,
    Active,
    Inactive,
}
```

### 3️⃣ Use Generated Mappings

```csharp
var entity = StatusEntity.None;
var dto = entity.MapToStatusDto();  // StatusDto.Unknown

Console.WriteLine(dto);  // Output: Unknown
```

---

## 🏗️ Advanced Scenarios

### 🔀 Special Case Mappings

The generator automatically handles common enum naming patterns:

```csharp
// Entity layer
[MapTo(typeof(StatusDto))]
public enum StatusEntity
{
    None,        // Maps to StatusDto.Unknown
    Active,      // Maps to StatusDto.Active (exact match)
    Inactive,    // Maps to StatusDto.Inactive (exact match)
}

// DTO layer
public enum StatusDto
{
    Unknown,     // Maps from StatusEntity.None (special case)
    Active,      // Maps from StatusEntity.Active (exact match)
    Inactive,    // Maps from StatusEntity.Inactive (exact match)
}

// Usage
var entity = StatusEntity.None;
var dto = entity.MapToStatusDto();  // StatusDto.Unknown
```

**Supported Special Cases**:

- **"Zero/Empty/Null" State Equivalents**: `None` ↔ `Unknown` ↔ `Default`
- Limited to just these three values to avoid unexpected mappings
- Use exact name matching for all other enum values

### 🔁 Bidirectional Mapping

Generate both forward and reverse mappings with a single attribute:

```csharp
[MapTo(typeof(StatusDto), Bidirectional = true)]
public enum StatusEntity
{
    None,
    Active,
    Inactive,
}

// Generated methods:
// - StatusEntity.MapToStatusDto()
// - StatusDto.MapToStatusEntity()

var entity = StatusEntity.Active;
var dto = entity.MapToStatusDto();      // Forward
var back = dto.MapToStatusEntity();     // Reverse
```

### 🔤 Case-Insensitive Matching

Enum values match regardless of casing:

```csharp
[MapTo(typeof(TargetStatus))]
public enum SourceStatus
{
    ACTIVE,      // Matches TargetStatus.Active
    pending,     // Matches TargetStatus.Pending
    InActive,    // Matches TargetStatus.Inactive
}

public enum TargetStatus
{
    Active,
    Pending,
    Inactive,
}
```

### 🏛️ Multi-Layer Architecture

Perfect for 3-layer architectures with clean enum separation:

```
Database (Entity Enums) → Domain (Model Enums) → API (DTO Enums)
```

**PetStore.DataAccess/Entities/PetStatusEntity.cs**:

```csharp
[MapTo(typeof(Domain.Models.PetStatus))]
public enum PetStatusEntity
{
    None,        // Database: 0 = unknown/null state
    Pending,     // Database: 1
    Available,   // Database: 2
    Adopted,     // Database: 3
}
```

**PetStore.Domain/Models/PetStatus.cs**:

```csharp
[MapTo(typeof(Api.Contract.PetStatus))]
public enum PetStatus
{
    Unknown,     // Domain: maps from PetStatusEntity.None
    Available,   // Domain representation
    Pending,
    Adopted,
}
```

**PetStore.Api.Contract/PetStatus.cs**:

```csharp
public enum PetStatus
{
    Unknown,     // API: client-facing representation
    Available,
    Pending,
    Adopted,
}
```

**Complete Mapping Chain**:

```csharp
// Repository: Entity → Domain
var entity = database.Pets.First();
var domain = entity.Status.MapToPetStatus();  // PetStatusEntity → PetStatus

// Service: Domain → DTO
var dto = domain.MapToPetStatus();  // PetStatus (Domain) → PetStatus (API)
```

---

## ⚙️ MapToAttribute Parameters

| Parameter       | Type     | Default | Description |
|----------------|----------|---------|-------------|
| `targetType`   | `Type`   | *(required)* | The target enum type to map to. Must be an enum type. |
| `Bidirectional` | `bool`  | `false` | When `true`, generates both forward (Source → Target) and reverse (Target → Source) mappings. |

**Examples**:

```csharp
// Basic mapping (one direction)
[MapTo(typeof(StatusDto))]
public enum StatusEntity { ... }

// Bidirectional mapping (both directions)
[MapTo(typeof(StatusDto), Bidirectional = true)]
public enum StatusEntity { ... }
```

---

## 🛡️ Diagnostics

The generator reports diagnostics for potential issues at compile time.

### ❌ ATCENUM001: Target Type Must Be Enum

**Severity**: Error
**Category**: EnumMapping

**Cause**: The target type specified in `[MapTo(typeof(...))]` is not an enum.

**Example**:

```csharp
public class StatusDto { }  // ❌ Not an enum

[MapTo(typeof(StatusDto))]  // ❌ Error ATCENUM001
public enum StatusEntity
{
    Active,
    Inactive,
}
```

**Fix**: Ensure the target type is an enum.

```csharp
public enum StatusDto       // ✅ Enum type
{
    Active,
    Inactive,
}

[MapTo(typeof(StatusDto))]  // ✅ OK
public enum StatusEntity
{
    Active,
    Inactive,
}
```

### ⚠️ ATCENUM002: Unmapped Enum Value

**Severity**: Warning
**Category**: EnumMapping

**Cause**: A value in the source enum has no matching value in the target enum (including special cases).

**Example**:

```csharp
public enum TargetStatus
{
    Active,
    Inactive,
}

[MapTo(typeof(TargetStatus))]
public enum SourceStatus
{
    Active,
    Inactive,
    Deleted,    // ⚠️ Warning ATCENUM002: No match for 'Deleted'
    Archived,   // ⚠️ Warning ATCENUM002: No match for 'Archived'
}
```

**Generated Code** (unmapped values are excluded from switch):

```csharp
public static TargetStatus MapToTargetStatus(this SourceStatus source)
{
    return source switch
    {
        SourceStatus.Active => TargetStatus.Active,
        SourceStatus.Inactive => TargetStatus.Inactive,
        // Deleted and Archived are unmapped - will throw at runtime if used
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unmapped enum value"),
    };
}
```

**Fix Options**:

1. **Add missing values to target enum**:

```csharp
public enum TargetStatus
{
    Active,
    Inactive,
    Deleted,    // ✅ Added
    Archived,   // ✅ Added
}
```

2. **Use exact name matching or rename values**:

```csharp
public enum TargetStatus
{
    Active,
    Inactive,
    Deleted,    // ✅ Renamed to match source
    Archived,   // ✅ Renamed to match source
}
```

3. **Accept the warning** if those values should never be used in the mapping context.

---

## 🚀 Native AOT Compatibility

The Enum Mapping Generator is **fully compatible with Native AOT** compilation, producing code that meets all AOT requirements:

### ✅ AOT-Safe Features

- **Zero reflection** - All mappings use switch expressions, not reflection-based converters
- **Compile-time generation** - Mapping code is generated during build, not at runtime
- **Trimming-safe** - No dynamic type discovery or metadata dependencies
- **Value type optimization** - Enums remain stack-allocated value types
- **Static analysis friendly** - All code paths are visible to the AOT compiler

### 🏗️ How It Works

1. **Build-time analysis**: The generator scans enums with `[MapTo]` attributes during compilation
2. **Switch expression generation**: Creates pure C# switch expressions without any reflection
3. **Direct value mapping**: Each enum value maps to target value via simple assignment
4. **AOT compilation**: The generated code compiles to native machine code with full optimizations

### 📋 Example Generated Code

```csharp
// Source: [MapTo(typeof(Status))] public enum EntityStatus { Active, Inactive }

// Generated AOT-safe code:
public static Status MapToStatus(this EntityStatus source)
{
    return source switch
    {
        EntityStatus.Active => Status.Active,
        EntityStatus.Inactive => Status.Inactive,
        _ => throw new global::System.ArgumentOutOfRangeException(
            nameof(source), source, "Unmapped enum value")
    };
}
```

**Why This Is AOT-Safe:**

- No `Enum.Parse()` or `Enum.GetValues()` calls (reflection)
- No dynamic type conversion
- All branches known at compile time
- Exception paths are concrete and traceable
- Zero heap allocations for value type operations

---

## 📚 Additional Examples

### Example 1: Order Status with None/Unknown

```csharp
// Database layer
[MapTo(typeof(OrderStatusDto))]
public enum OrderStatusEntity
{
    None,           // → OrderStatusDto.Unknown (special case)
    Pending,
    Completed,
    Cancelled,
}

// API layer
public enum OrderStatusDto
{
    Unknown,        // ← OrderStatusEntity.None
    Pending,
    Completed,
    Cancelled,
}

// Usage
var entity = OrderStatusEntity.None;
var dto = entity.MapToOrderStatusDto();  // OrderStatusDto.Unknown
```

### Example 2: Bidirectional Mapping

```csharp
// Domain enum
[MapTo(typeof(StatusDto), Bidirectional = true)]
public enum StatusEntity
{
    None,        // → StatusDto.Unknown
    Active,
    Inactive,
}

// DTO enum
public enum StatusDto
{
    Unknown,     // ← StatusEntity.None
    Active,
    Inactive,
}

// Usage - both directions work
var entity = StatusEntity.None;
var dto = entity.MapToStatusDto();       // StatusDto.Unknown
var backToEntity = dto.MapToStatusEntity();  // StatusEntity.None (bidirectional)
```

### Example 3: Case-Insensitive Matching

```csharp
[MapTo(typeof(TargetPriority))]
public enum SourcePriority
{
    LOW,         // Matches TargetPriority.Low (case-insensitive)
    MEDIUM,      // Matches TargetPriority.Medium
    HIGH,        // Matches TargetPriority.High
}

public enum TargetPriority
{
    Low,
    Medium,
    High,
}
```

---

## 🔧 Best Practices

1. **Use Bidirectional When Appropriate**: If you need to convert in both directions, use `Bidirectional = true` to avoid duplicate attributes.

2. **Leverage Special Cases**: The generator knows "zero/empty/null" state equivalents. Use names like `None`/`Unknown`/`Default` to get automatic mappings for these common states.

3. **Handle Warnings**: `ATCENUM002` warnings indicate potential runtime errors. Address them by adding missing values or accepting the risk.

4. **Layer Separation**: Keep enum definitions separate per layer (Entity, Domain, DTO) to maintain clean architecture.

5. **Namespace Organization**: Generated extension methods live in `Atc.Mapping`. Add `using Atc.Mapping;` where you use the mappings.

---

## 📖 Related Documentation

- [Object Mapping Generator](ObjectMappingGenerators.md) - For class-to-class mappings
- [Dependency Registration Generator](DependencyRegistrationGenerators.md) - For automatic DI registration
- [Options Binding Generator](OptionsBindingGenerators.md) - For configuration binding
- [Sample Projects](EnumMappingGenerators-Samples.md) - Working code examples

---

**Need Help?** Check out the [sample project](EnumMappingGenerators-Samples.md) for a complete working example.
