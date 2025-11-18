# 🔄 Enum Mapping Sample

This sample demonstrates the **EnumMappingGenerator** in action with realistic enum mapping scenarios including special case handling, bidirectional mappings, and case-insensitive matching.

## 📂 Project Location

```text
sample/Atc.SourceGenerators.EnumMapping/
```

## 🚀 Running the Sample

```bash
cd sample/Atc.SourceGenerators.EnumMapping
dotnet run
```

Expected output:

```text
=== Atc.SourceGenerators - Enum Mapping Sample ===

1. Testing PetStatusEntity → PetStatusDto mapping:
   - Special case: None → Unknown
   - Bidirectional: true

   None → Unknown
   Available → Available
   Pending → Pending
   Adopted → Adopted

2. Testing PetStatusDto → PetStatusEntity (reverse mapping):
   Unknown → None

3. Testing FeatureState → FeatureFlag mapping:
   - Exact name matching
   - Bidirectional: false

   Active → Active
   Inactive → Inactive
   Testing → Testing

4. Testing case-insensitive matching:
   All enum values match regardless of casing

5. Performance characteristics:
   ✓ Zero runtime cost - pure switch expressions
   ✓ Compile-time safety with exhaustive checking
   ✓ ArgumentOutOfRangeException for unmapped values

=== All tests completed successfully! ===
```

## 🎯 What This Sample Demonstrates

### 1. Special Case Mapping: None → Unknown

**PetStatusEntity.cs**:

```csharp
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatusEntity
{
    None,       // Special case: maps to PetStatusDto.Unknown
    Pending,
    Available,
    Adopted,
}
```

**PetStatusDto.cs**:

```csharp
public enum PetStatusDto
{
    Unknown,    // Special case: maps from PetStatusEntity.None
    Available,
    Pending,
    Adopted,
}
```

**Key Points**:

- `None` automatically maps to `Unknown` (common pattern)
- `Bidirectional = true` generates both forward and reverse mappings
- All other values map by exact name match

### 2. Exact Name Matching

**FeatureState.cs**:

```csharp
[MapTo(typeof(FeatureFlag))]
public enum FeatureState
{
    Active,      // Exact match to FeatureFlag.Active
    Inactive,    // Exact match to FeatureFlag.Inactive
    Testing,     // Exact match to FeatureFlag.Testing
}
```

**FeatureFlag.cs**:

```csharp
public enum FeatureFlag
{
    Active,      // Exact match from FeatureState.Active
    Inactive,    // Exact match from FeatureState.Inactive
    Testing,     // Exact match from FeatureState.Testing
}
```

**Key Points**:

- All values match by exact name
- Unidirectional mapping (Bidirectional = false)

## 📁 Project Structure

```text
sample/Atc.SourceGenerators.EnumMapping/
├── Atc.SourceGenerators.EnumMapping.csproj
├── GlobalUsings.cs
├── Program.cs
└── Enums/
    ├── PetStatusEntity.cs      (Database layer enum)
    ├── PetStatusDto.cs         (API layer enum)
    ├── FeatureState.cs         (Domain layer enum)
    └── FeatureFlag.cs          (Configuration layer enum)
```

## 🔍 Generated Code

The source generator creates extension methods in the `Atc.Mapping` namespace:

**EnumMappingExtensions.g.cs** (simplified):

```csharp
namespace Atc.Mapping;

public static class EnumMappingExtensions
{
    // Forward mapping: PetStatusEntity → PetStatusDto
    public static PetStatusDto MapToPetStatusDto(this PetStatusEntity source)
    {
        return source switch
        {
            PetStatusEntity.None => PetStatusDto.Unknown,        // Special case
            PetStatusEntity.Pending => PetStatusDto.Pending,
            PetStatusEntity.Available => PetStatusDto.Available,
            PetStatusEntity.Adopted => PetStatusDto.Adopted,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unmapped enum value"),
        };
    }

    // Reverse mapping: PetStatusDto → PetStatusEntity (Bidirectional = true)
    public static PetStatusEntity MapToPetStatusEntity(this PetStatusDto source)
    {
        return source switch
        {
            PetStatusDto.Unknown => PetStatusEntity.None,        // Special case
            PetStatusDto.Available => PetStatusEntity.Available,
            PetStatusDto.Pending => PetStatusEntity.Pending,
            PetStatusDto.Adopted => PetStatusEntity.Adopted,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unmapped enum value"),
        };
    }

    // Forward mapping: FeatureState → FeatureFlag
    public static FeatureFlag MapToFeatureFlag(this FeatureState source)
    {
        return source switch
        {
            FeatureState.Active => FeatureFlag.Active,
            FeatureState.Inactive => FeatureFlag.Inactive,
            FeatureState.Testing => FeatureFlag.Testing,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unmapped enum value"),
        };
    }
}
```

## 🎨 Key Features Demonstrated

### ✅ Special Case Detection

The generator automatically recognizes common enum naming patterns:

| Pattern | Equivalent Values | Notes |
|---------|------------------|-------|
| **Zero/Null States** | None ↔ Unknown, Default, NotSet | Common default state mapping |
| **Active States** | Active ↔ Enabled, On, Running | Service/feature activation |
| **Inactive States** | Inactive ↔ Disabled, Off, Stopped | Service/feature deactivation |
| **Deletion States** | Deleted ↔ Removed, Archived | Soft delete patterns |
| **Pending States** | Pending ↔ InProgress, Processing | Async operation states |
| **Completion States** | Completed ↔ Done, Finished | Task completion states |

**Example:**

```csharp
// Database enum
public enum ServiceStatusEntity
{
    None,      // Maps to Unknown in API
    Active,    // Maps to Enabled in API
    Inactive   // Maps to Disabled in API
}

// API enum
public enum ServiceStatus
{
    Unknown,   // Maps from None in database
    Enabled,   // Maps from Active in database
    Disabled   // Maps from Inactive in database
}
```

**Smart matching**: The generator uses exact name matching first, then falls back to case-insensitive matching, and finally checks for special case patterns. This ensures predictable behavior while supporting common enum naming variations.

### ✅ Bidirectional Mapping

With `Bidirectional = true`:

```csharp
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatusEntity { ... }
```

You get **two methods**:

- `PetStatusEntity.MapToPetStatusDto()` (forward)
- `PetStatusDto.MapToPetStatusEntity()` (reverse)

### ✅ Case-Insensitive Matching

Enum values match regardless of casing:

```csharp
// These all match:
SourceEnum.ACTIVE    → TargetEnum.Active
SourceEnum.active    → TargetEnum.Active
SourceEnum.Active    → TargetEnum.Active
SourceEnum.AcTiVe    → TargetEnum.Active
```

### ✅ Compile-Time Safety

Unmapped values generate warnings:

```text
Warning ATCENUM002: Enum value 'SourceStatus.Deleted' has no matching value in target enum 'TargetStatus'
```

### ✅ Runtime Safety

Unmapped values throw at runtime:

```csharp
var status = SourceStatus.Deleted;  // Unmapped value
var dto = status.MapToTargetDto();  // Throws ArgumentOutOfRangeException
```

## 💡 Usage Patterns

### Pattern 1: Database → Domain → API

**Use Case**: Multi-layer architecture with enum separation

```csharp
// Database layer
[MapTo(typeof(Domain.Status))]
public enum StatusEntity
{
    None,
    Active,
    Inactive,
}

// Domain layer
[MapTo(typeof(Api.StatusDto))]
public enum Status
{
    Unknown,
    Active,
    Inactive,
}

// API layer
public enum StatusDto
{
    Unknown,
    Active,
    Inactive,
}

// Complete chain
var entity = database.GetStatus();           // StatusEntity.None
var domain = entity.MapToStatus();          // Status.Unknown
var dto = domain.MapToStatusDto();          // StatusDto.Unknown
```

### Pattern 2: Configuration → Domain

**Use Case**: Feature flags with consistent naming

```csharp
// Configuration (appsettings.json representation)
[MapTo(typeof(FeatureState))]
public enum FeatureFlag
{
    Active,
    Inactive,
}

// Domain (business logic representation)
public enum FeatureState
{
    Active,      // ← FeatureFlag.Active (exact match)
    Inactive,    // ← FeatureFlag.Inactive (exact match)
}

// Usage
var config = configuration.GetValue<FeatureFlag>("MyFeature");
var state = config.MapToFeatureState();

if (state == FeatureState.Active)
{
    // Feature is active
}
```

### Pattern 3: External API → Internal Domain

**Use Case**: Third-party API integration with case normalization

```csharp
// External API enum (from SDK)
[MapTo(typeof(InternalStatus))]
public enum ExternalStatus
{
    NONE,
    PENDING,
    ACTIVE,
}

// Internal domain enum (your naming)
public enum InternalStatus
{
    Unknown,     // ← ExternalStatus.NONE (special case: None → Unknown)
    Pending,     // ← ExternalStatus.PENDING (case-insensitive)
    Active,      // ← ExternalStatus.ACTIVE (case-insensitive)
}

// Usage
var external = apiClient.GetStatus();           // ExternalStatus.NONE
var internal = external.MapToInternalStatus();  // InternalStatus.Unknown
```

## 🏗️ Real-World Example: PetStore

This pattern is used in the [PetStore sample](PetStoreApi.md) to separate enum concerns across layers:

```text
PetStatusEntity (DataAccess)
    ↓ MapToPetStatus()
PetStatus (Domain)
    ↓ MapToPetStatus() [different types, same method name]
PetStatus (Api.Contract)
```

Each layer has its own enum definition, and mappings are generated automatically.

## 📝 Notes

- **No Reflection**: All mappings use pure switch expressions for maximum performance
- **AOT Compatible**: Works with Native AOT compilation
- **Type Safe**: Compiler errors if target types are incorrect
- **IntelliSense Support**: Generated methods appear in IDE autocomplete

## 📖 Related Samples

- [Object Mapping Sample](ObjectMappingGenerators-Samples.md) - For class-to-class mappings
- [PetStore Sample](PetStoreApi-Samples.md) - Complete application using all generators
- [Dependency Registration Sample](DependencyRegistrationGenerators-Samples.md) - DI registration
- [Options Binding Sample](OptionsBinding-Samples.md) - Configuration binding

---

**Need more examples?** Check the [EnumMapping Generator documentation](EnumMappingGenerators.md) for comprehensive guides and patterns.
