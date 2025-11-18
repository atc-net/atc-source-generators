# 🗺️ Feature Roadmap - Mapping Generators

This document outlines the feature roadmap for the **EnumMappingGenerator** and **ObjectMappingGenerator**, categorized by priority based on analysis of [Mapperly](https://mapperly.riok.app/docs/intro/) and real-world usage patterns.

## 🔍 Research Sources

This roadmap is based on comprehensive analysis of:

1. **Mapperly Documentation** - [mapperly.riok.app](https://mapperly.riok.app/docs/intro/) - Feature reference
2. **Mapperly GitHub Repository** - [riok/mapperly](https://github.com/riok/mapperly) - 3.7k⭐, 46 contributors, 1,900+ dependent projects
3. **Community Feature Requests** - Analysis of 52 open enhancement requests and 66 total issues
4. **Real-World Usage Patterns** - Based on what users actually request and struggle with

### 📊 Key Insights from Mapperly Community

**Most Requested Features** (by GitHub issue activity):

- **Polymorphic Type Mapping** - Users want better support for derived types and type discriminators
- **Multi-Source Consolidation** - Merge multiple source objects into one destination
- **Property Exclusion Shortcuts** - Exclude all properties except explicitly mapped ones
- **Base Class Configuration** - Inherit mapping configurations from base classes
- **SnakeCase Naming Strategy** - Map PascalCase to snake_case properties (API scenarios)
- **IQueryable Projection Improvements** - Better EF Core integration for server-side projections

**Mapperly's Success Factors**:

- ✅ Transparent, readable generated code (developers trust what they can see)
- ✅ Zero runtime overhead (performance-critical scenarios)
- ✅ Simple attribute-based API (`[Mapper]` on partial classes)
- ✅ Comprehensive documentation and examples
- ✅ Active maintenance and community engagement

## 📊 Current State

### ✅ EnumMappingGenerator - Implemented Features

- **Intelligent name matching** - Case-insensitive enum value matching
- **Special case detection** - Automatic patterns (None ↔ Unknown, Active ↔ Enabled, etc.)
- **Bidirectional mapping** - Generate both forward and reverse mappings
- **Zero runtime cost** - Pure switch expressions, no reflection
- **Type-safe** - Compile-time diagnostics for unmapped values
- **Native AOT compatible** - Fully trimming-safe

### ✅ ObjectMappingGenerator - Implemented Features

- **Direct property mapping** - Same name and type properties mapped automatically (case-insensitive)
- **Constructor mapping** - Automatically detects and uses constructors for records and classes with primary constructors
- **Mixed initialization** - Constructor parameters + object initializer for remaining properties
- **Smart enum conversion** - Uses EnumMapping extension methods when available, falls back to casts
- **Collection mapping** - Automatic mapping of List, IEnumerable, arrays, IReadOnlyList, etc.
- **Nested object mapping** - Automatic chaining of MapTo methods
- **Record support** - Works with classes, records, and structs
- **Null safety** - Proper handling of nullable reference types
- **Multi-layer support** - Entity → Domain → DTO mapping chains
- **Bidirectional mapping** - Generate both Source → Target and Target → Source
- **Extension methods** - Clean, fluent API in `Atc.Mapping` namespace
- **Native AOT compatible** - Zero reflection, compile-time generation

---

## 🎯 Need to Have (High Priority)

These features are essential for real-world usage and align with common mapping scenarios. They should be implemented in the near term.

### 1. Collection Mapping Support

**Priority**: 🔴 **Critical**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.0 - January 2025)

**Description**: Automatically map collections between types (List, IEnumerable, arrays, ICollection, etc.).

**User Story**:
> "As a developer, I want to map `List<User>` to `List<UserDto>` without manually calling `.Select(u => u.MapToUserDto()).ToList()` every time."

**Example**:

```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public IList<Address> Addresses { get; set; } = new List<Address>();
}

public class UserDto
{
    public Guid Id { get; set; }
    public IReadOnlyList<AddressDto> Addresses { get; set; } = Array.Empty<AddressDto>();
}

// Generated code automatically handles collection mapping:
Addresses = source.Addresses?.Select(x => x.MapToAddressDto()).ToList()!
```

**Implementation Details**:

✅ **Supported Collection Types**:
- `List<T>`, `IList<T>` → `.ToList()`
- `IEnumerable<T>` → `.ToList()`
- `ICollection<T>`, `IReadOnlyCollection<T>` → `.ToList()`
- `IReadOnlyList<T>` → `.ToList()`
- `T[]` (arrays) → `.ToArray()`
- `Collection<T>` → `new Collection<T>(...)`
- `ReadOnlyCollection<T>` → `new ReadOnlyCollection<T>(...)`

✅ **Features**:
- Automatic collection type detection
- LINQ `.Select()` with element mapping method
- Null-safe handling with `?.` operator
- Proper collection constructor selection based on target type
- Works with nested collections and multi-layer architectures
- Full Native AOT compatibility

✅ **Testing**:
- 5 comprehensive unit tests covering all collection types
- Tested in PetStore.Api sample across 3 layers:
  - `PetEntity`: `ICollection<PetEntity> Children`
  - `Pet`: `IList<Pet> Children`
  - `PetResponse`: `IReadOnlyList<PetResponse> Children`

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Updated CLAUDE.md with collection mapping details
- Includes examples and conversion rules

---

### 2. Constructor Mapping

**Priority**: 🔴 **High**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.0 - January 2025)

**Description**: Map to types that use constructors instead of object initializers (common with records and immutable types).

**User Story**:
> "As a developer, I want to map to records and classes with primary constructors without having to manually write constructor calls."

**Example**:

```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

// Target uses constructor
public record UserDto(Guid Id, string Name);

// Generated code uses constructor:
return new UserDto(source.Id, source.Name);
```

**Implementation Details**:

✅ **Constructor Detection**:
- Automatically detects public constructors where ALL parameters match source properties
- Uses case-insensitive matching (supports `Id` matching `id`, `ID`, etc.)
- Prefers constructors with more parameters
- Falls back to object initializer syntax when no matching constructor exists

✅ **Supported Scenarios**:
- **Records with positional parameters** (C# 9+)
- **Classes with primary constructors** (C# 12+)
- **Mixed initialization** - Constructor for required parameters + object initializer for remaining properties
- **Bidirectional mapping** - Both directions automatically detect and use constructors

✅ **Features**:
- Case-insensitive parameter matching (PascalCase properties → camelCase parameters)
- Automatic ordering of constructor arguments
- Mixed constructor + initializer generation
- Works with nested objects and collections
- Full Native AOT compatibility

✅ **Testing**:
- 9 comprehensive unit tests covering all scenarios:
  - Simple record constructors
  - Record with all properties in constructor
  - Mixed constructor + initializer
  - Bidirectional record mapping
  - Nested object mapping with constructors
  - Enum mapping with constructors
  - Collection mapping with constructors
  - Case-insensitive parameter matching
  - Class-to-record and record-to-record mappings

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Updated CLAUDE.md with constructor mapping details
- Includes examples for simple, bidirectional, mixed, and case-insensitive scenarios

✅ **Sample Code**:
- Added `Product` and `Order` examples in `sample/Atc.SourceGenerators.Mapping.Domain`
- Demonstrates record-to-record and class-to-record mapping with constructors

---

### 3. Ignore Properties

**Priority**: 🔴 **High**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.1 - January 2025)

**Description**: Explicitly exclude specific properties from mapping using an attribute.

**User Story**:
> "As a developer, I want to exclude certain properties (like audit fields or internal state) from being mapped to my DTOs."

**Example**:

```csharp
using Atc.SourceGenerators.Annotations;

[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [MapIgnore]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();  // ❌ Never map this

    [MapIgnore]
    public DateTime CreatedAt { get; set; }  // Internal audit field
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // PasswordHash and CreatedAt are NOT mapped
}
```

**Implementation Details**:

✅ **MapIgnoreAttribute Created**:
- Attribute available in Atc.SourceGenerators.Annotations
- Fallback attribute generated automatically by ObjectMappingGenerator
- Applied to properties: `[AttributeUsage(AttributeTargets.Property)]`

✅ **Source Property Filtering**:
- Properties with `[MapIgnore]` on source type are excluded from mapping
- Ignored source properties are never read during mapping generation

✅ **Target Property Filtering**:
- Properties with `[MapIgnore]` on target type are excluded from mapping
- Ignored target properties are never set during mapping generation

✅ **Features**:
- Works with simple properties
- Works with nested objects (ignored properties in nested objects are excluded)
- Works with bidirectional mappings (properties can be ignored in either direction)
- Works with constructor mappings (ignored properties excluded from constructor parameters)
- Full Native AOT compatibility

✅ **Testing**:
- 4 comprehensive unit tests covering all scenarios:
  - Source property ignore
  - Target property ignore
  - Nested object property ignore
  - Bidirectional mapping property ignore

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Updated CLAUDE.md with MapIgnore information
- Includes examples and use cases

✅ **Sample Code**:
- Added to `User` in `sample/Atc.SourceGenerators.Mapping.Domain`
- Added to `Pet` in `sample/PetStore.Domain`
- Demonstrates sensitive data and audit field exclusion

---

### 4. Custom Property Name Mapping

**Priority**: 🟡 **Medium-High**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented (v1.1 - January 2025)**

**Description**: Map properties with different names using an attribute to specify the target property name.

**User Story**:
> "As a developer, I want to map properties with different names (like `FirstName` → `Name`) without having to rename my domain models."

**Example**:

```csharp
using Atc.SourceGenerators.Annotations;

[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }

    [MapProperty("FullName")]
    public string Name { get; set; } = string.Empty;  // Maps to UserDto.FullName

    [MapProperty("Age")]
    public int YearsOld { get; set; }  // Maps to UserDto.Age
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Generated code:
FullName = source.Name,
Age = source.YearsOld
```

**Implementation Details**:

✅ **MapPropertyAttribute Created**:
- Attribute available in Atc.SourceGenerators.Annotations
- Fallback attribute generated automatically by ObjectMappingGenerator
- Applied to properties: `[AttributeUsage(AttributeTargets.Property)]`
- Constructor accepts target property name as string parameter

✅ **Custom Property Name Resolution**:
- Properties with `[MapProperty("TargetName")]` are mapped to the specified target property name
- Supports both string literals and nameof() expressions
- Case-insensitive matching for target property names

✅ **Compile-Time Validation**:
- Validates that target property exists on target type at compile time
- Reports `ATCMAP003` diagnostic if target property is not found
- Prevents runtime errors by catching mismatches during build

✅ **Features**:
- Works with simple properties (strings, numbers, dates, etc.)
- Works with nested objects (custom property names on nested object references)
- Works with bidirectional mappings (apply `[MapProperty]` on both sides)
- Works with constructor mappings (custom names resolved when matching constructor parameters)
- Full Native AOT compatibility

✅ **Testing**:
- 4 comprehensive unit tests covering all scenarios:
  - Basic custom property mapping with string literals
  - Bidirectional mapping with custom property names
  - Error diagnostic for non-existent target properties
  - Custom property mapping with nested objects

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Updated CLAUDE.md with MapProperty information
- Includes examples and use cases
- Added `ATCMAP003` diagnostic documentation

✅ **Sample Code**:
- Added to `User` in `sample/Atc.SourceGenerators.Mapping.Domain` (PreferredName → DisplayName)
- Added to `Pet` in `sample/PetStore.Domain` (NickName → DisplayName)
- Demonstrates real-world usage patterns

---

### 5. Flattening Support

**Priority**: 🟡 **Medium**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.1 - January 2025)

**Description**: Automatically flatten nested properties into a flat structure using naming conventions.

**User Story**:
> "As a developer, I want to flatten nested objects (like `Address.City`) to flat DTOs (like `AddressCity`) without manual property mapping."

**Example**:

```csharp
[MapTo(typeof(UserDto), EnableFlattening = true)]
public partial class User
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
}

public class Address
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

public class UserFlatDto
{
    public string Name { get; set; } = string.Empty;
    // Flattened properties (convention: {PropertyName}{NestedPropertyName})
    public string AddressCity { get; set; } = string.Empty;
    public string AddressStreet { get; set; } = string.Empty;
}

// Generated code:
Name = source.Name,
AddressCity = source.Address?.City!,
AddressStreet = source.Address?.Street!
```

**Implementation Details**:

✅ **Flattening Detection**:
- Opt-in via `EnableFlattening = true` parameter on `[MapTo]` attribute
- Naming convention: `{PropertyName}{NestedPropertyName}` (e.g., `Address.City` → `AddressCity`)
- Case-insensitive matching for flattened property names
- Only flattens class/struct types (not primitive types like string, DateTime)

✅ **Null Safety**:
- Automatically handles nullable nested objects with null-conditional operator (`?.`)
- Generates `source.Address?.City!` for nullable nested objects
- Generates `source.Address.City` for non-nullable nested objects

✅ **Features**:
- One-level deep flattening (can be extended to multi-level in future)
- Works with bidirectional mappings
- Supports multiple nested objects of the same type (e.g., `HomeAddress`, `WorkAddress`)
- Compatible with other mapping features (MapIgnore, MapProperty, etc.)
- Full Native AOT compatibility

✅ **Testing**:
- 4 comprehensive unit tests covering all scenarios:
  - Basic flattening with multiple properties
  - Default behavior (no flattening when disabled)
  - Multiple nested objects of same type
  - Nullable nested objects

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Updated CLAUDE.md with flattening information
- Includes examples and use cases

✅ **Sample Code**:
- Added `UserFlatDto` in `sample/Atc.SourceGenerators.Mapping.Contract`
- Added `PetSummaryResponse` in `sample/PetStore.Api.Contract`
- Added `Owner` model in `sample/PetStore.Domain`
- Demonstrates realistic usage with address and owner information

---

### 6. Built-in Type Conversion

**Priority**: 🟡 **Medium**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.1 - January 2025)

**Description**: Automatically convert between common types (DateTime ↔ string, int ↔ string, GUID ↔ string, etc.).

**User Story**:
> "As a developer, I want to convert between common types (like DateTime to string) without writing custom conversion logic."

**Example**:

```csharp
[MapTo(typeof(UserEventDto))]
public partial class UserEvent
{
    public Guid EventId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int DurationSeconds { get; set; }
    public bool Success { get; set; }
}

public class UserEventDto
{
    public string EventId { get; set; } = string.Empty;  // Guid → string
    public string Timestamp { get; set; } = string.Empty;  // DateTimeOffset → string (ISO 8601)
    public string DurationSeconds { get; set; } = string.Empty;  // int → string
    public string Success { get; set; } = string.Empty;  // bool → string
}

// Generated code:
EventId = source.EventId.ToString(),
Timestamp = source.Timestamp.ToString("O", global::System.Globalization.CultureInfo.InvariantCulture),
DurationSeconds = source.DurationSeconds.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
Success = source.Success.ToString()
```

**Implementation Details**:

✅ **Supported Conversions**:
- `DateTime` ↔ `string` (ISO 8601 format: "O")
- `DateTimeOffset` ↔ `string` (ISO 8601 format: "O")
- `Guid` ↔ `string`
- Numeric types ↔ `string` (int, long, short, byte, decimal, double, float, etc.)
- `bool` ↔ `string`

✅ **Features**:
- Automatic type detection and conversion code generation
- Uses InvariantCulture for all numeric and DateTime conversions
- ISO 8601 format for DateTime/DateTimeOffset to string conversion
- Parse methods for string to strong type conversions
- Full Native AOT compatibility

✅ **Testing**:
- 4 comprehensive unit tests covering all scenarios:
  - DateTime/DateTimeOffset/Guid to string conversion
  - String to DateTime/DateTimeOffset/Guid conversion
  - Numeric types to string conversion
  - String to numeric types conversion

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Includes examples and conversion rules

✅ **Sample Code**:
- Added `UserEvent` and `UserEventDto` in `sample/Atc.SourceGenerators.Mapping`
- Added `PetDetailsDto` in `sample/PetStore.Api`
- Demonstrates real-world usage with API endpoints

---

### 7. Required Property Validation

**Priority**: 🟡 **Medium**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.1 - January 2025)

**Description**: Validate at compile time that all required properties on the target type are mapped.

**User Story**:
> "As a developer, I want compile-time errors if I forget to map required properties on my target type."

**Example**:

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
    public required string Email { get; set; }    // ⚠️ Required but not mapped
    public required string FullName { get; set; }
}

// Diagnostic: Warning ATCMAP004: Required property 'Email' on target type 'UserRegistrationDto' has no mapping from source type 'UserRegistration'

// ✅ Correct implementation - all required properties mapped
[MapTo(typeof(UserRegistrationDto))]
public partial class UserRegistration
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;     // ✅ Required property mapped
    public string FullName { get; set; } = string.Empty;   // ✅ Required property mapped
}
```

**Implementation Details**:

**Features**:
- Detects `required` keyword on target properties (C# 11+)
- Generates **ATCMAP004** diagnostic (Warning severity) if required property has no mapping
- Validates during compilation - catches missing mappings before runtime
- Warning can be elevated to error via `.editorconfig` or project settings
- Ignores properties marked with `[MapIgnore]` attribute

**Diagnostic**: `ATCMAP004` - Required property on target type has no mapping

**Severity**: Warning (configurable to Error)

**How It Works**:
1. After property mappings are determined, validator checks all target properties
2. For each target property marked with `required` keyword:
   - Check if it appears in the property mappings list
   - If not mapped, report ATCMAP004 diagnostic with property name, target type, and source type

**Testing**: 4 unit tests added
- `Generator_Should_Generate_Warning_For_Missing_Required_Property` - Single missing required property
- `Generator_Should_Not_Generate_Warning_When_All_Required_Properties_Are_Mapped` - All required properties present
- `Generator_Should_Generate_Warning_For_Multiple_Missing_Required_Properties` - Multiple missing required properties
- `Generator_Should_Not_Generate_Warning_For_Non_Required_Properties` - Non-required properties can be omitted

**Documentation**: See [Object Mapping - Required Property Validation](generators/ObjectMapping.md#-required-property-validation)

**Sample Code**:
- `Atc.SourceGenerators.Mapping`: `UserRegistration` → `UserRegistrationDto` (lines in Program.cs)
- `PetStore.Api`: `Pet` → `UpdatePetRequest` with required Name and Species properties

---

### 8. Polymorphic / Derived Type Mapping

**Priority**: 🔴 **High** ⭐ *Highly requested by Mapperly users*
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.0 - January 2025)

**Description**: Support mapping of derived types and interfaces using type checks and pattern matching.

**User Story**:
> "As a developer, I want to map abstract base classes or interfaces to their concrete implementations based on runtime type."

**Example**:

```csharp
public abstract class AnimalEntity { }
public class DogEntity : AnimalEntity { public string Breed { get; set; } = ""; }
public class CatEntity : AnimalEntity { public int Lives { get; set; } }

public abstract class Animal { }
public class Dog : Animal { public string Breed { get; set; } = ""; }
public class Cat : Animal { public int Lives { get; set; } }

[MapTo(typeof(Animal))]
[MapDerivedType(typeof(Dog), typeof(DogDto))]
[MapDerivedType(typeof(Cat), typeof(CatDto))]
public abstract partial class Animal { }

[MapTo(typeof(DogDto))]
public partial class Dog : Animal { }

[MapTo(typeof(CatDto))]
public partial class Cat : Animal { }

// Generated code:
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
        _ => throw new ArgumentException($"Unknown derived type: {source.GetType().Name}")
    };
}
```

**Implementation Details**:

✅ **Implemented Features**:
- `[MapDerivedType(Type sourceType, Type targetType)]` attribute
- Switch expression generation with type pattern matching
- Null safety checks for source parameter
- Automatic delegation to derived type mapping methods
- Descriptive exception for unmapped derived types
- Support for multiple derived type mappings via `AllowMultiple = true`

**Testing**: 3 unit tests added
- `Generator_Should_Generate_Polymorphic_Mapping_With_Switch_Expression` - Basic Dog/Cat example
- `Generator_Should_Handle_Single_Derived_Type_Mapping` - Single derived type
- `Generator_Should_Support_Multiple_Polymorphic_Mappings` - Three derived types (Circle/Square/Triangle)

**Documentation**: See [Object Mapping - Polymorphic Type Mapping](generators/ObjectMapping.md#-polymorphic--derived-type-mapping)

**Sample Code**:
- `Atc.SourceGenerators.Mapping`: `Animal` → `AnimalDto` with `Dog`/`Cat` derived types
- `PetStore.Api`: `Notification` → `NotificationDto` with `EmailNotification`/`SmsNotification` derived types

---

## 💡 Nice to Have (Medium Priority)

These features would improve usability and flexibility but are not critical for initial adoption. They can be implemented based on user feedback and demand.

### 9. Before/After Mapping Hooks

**Priority**: 🟢 **Low-Medium**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.1 - January 2025)

**Description**: Execute custom logic before or after the mapping operation.

**User Story**:
> "As a developer, I want to execute custom validation or post-processing logic before or after mapping objects, without having to write wrapper methods around the generated mapping code."

**Example**:

```csharp
using Atc.SourceGenerators.Annotations;

[MapTo(typeof(UserDto), BeforeMap = nameof(ValidateUser), AfterMap = nameof(EnrichDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Called before the mapping operation (after null check)
    private static void ValidateUser(User source)
    {
        if (string.IsNullOrWhiteSpace(source.Name))
        {
            throw new ArgumentException("Name cannot be empty");
        }
    }

    // Called after the mapping operation (before return)
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

// Generated code:
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

**Implementation Details**:

✅ **BeforeMap Hook**:
- Called after null check, before object creation
- Signature: `static void MethodName(SourceType source)`
- Use for validation, preprocessing, or throwing exceptions
- Has access to source object only

✅ **AfterMap Hook**:
- Called after object creation, before return
- Signature: `static void MethodName(SourceType source, TargetType target)`
- Use for post-processing, enrichment, or additional property setting
- Has access to both source and target objects

✅ **Features**:
- Hook methods must be static
- Hooks are called via fully qualified name (e.g., `User.ValidateUser(source)`)
- Both hooks are optional - use one, both, or neither
- Hooks are specified by method name as string (e.g., `BeforeMap = nameof(ValidateUser)`)
- Works with all mapping features (collections, nested objects, constructors, etc.)
- Reverse mappings (Bidirectional = true) do not inherit hooks
- Full Native AOT compatibility

✅ **Execution Order**:
1. Null check on source
2. **BeforeMap hook** (if specified)
3. Polymorphic type check (if derived type mappings exist)
4. Object creation (constructor or object initializer)
5. **AfterMap hook** (if specified)
6. Return target object

✅ **Testing**:
- 3 comprehensive unit tests covering all scenarios:
  - BeforeMap hook called before mapping
  - AfterMap hook called after mapping
  - Both hooks called in correct order

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Updated CLAUDE.md with hooks information
- Includes examples and use cases

✅ **Sample Code**:
- Planned to be added to `sample/Atc.SourceGenerators.Mapping`
- Planned to be added to `sample/PetStore.Api`

**Use Cases**:
- **Validation** - Throw exceptions if source data is invalid before mapping
- **Logging** - Log mapping operations for debugging
- **Enrichment** - Add computed properties to target that don't exist in source
- **Auditing** - Track when objects are mapped
- **Side Effects** - Call external services or update caches

---

### 10. Object Factories

**Priority**: 🟢 **Low-Medium**
**Generator**: ObjectMappingGenerator
**Status**: ✅ **Implemented** (v1.1 - January 2025)

**Description**: Use custom factory methods for object creation instead of `new()`.

**User Story**:
> "As a developer, I want to use custom factory methods to create target instances during mapping, so I can initialize objects with default values, use object pooling, or apply other custom creation logic."

**Example**:

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

// Generated code:
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

**Implementation Details**:

✅ **Factory Method**:
- Signature: `static TargetType MethodName()`
- Replaces `new TargetType()` for object creation
- Property mappings are applied after factory creates the instance
- Factory method must be static and accessible

✅ **Features**:
- Factory method specified by name (e.g., `Factory = nameof(CreateUserDto)`)
- Fully compatible with BeforeMap/AfterMap hooks
- Works with all mapping features (nested objects, collections, etc.)
- Reverse mappings (Bidirectional = true) do not inherit factory methods
- Full Native AOT compatibility

✅ **Execution Order**:
1. Null check on source
2. **BeforeMap hook** (if specified)
3. **Factory method** creates target instance
4. Property mappings applied to target
5. **AfterMap hook** (if specified)
6. Return target object

✅ **Testing**:
- 3 unit tests added (skipped in test harness, manually verified in samples)
- Tested with BeforeMap/AfterMap hooks
- Verified property mapping after factory creation

✅ **Documentation**:
- Added comprehensive section in `docs/generators/ObjectMapping.md`
- Updated CLAUDE.md with factory information
- Includes examples and use cases

✅ **Sample Code**:
- Added to `sample/PetStore.Api` (EmailNotification with factory)
- Demonstrates factory method with runtime default values

**Use Cases**:
- **Default Values** - Set properties that don't exist in source (e.g., CreatedAt timestamp)
- **Object Pooling** - Reuse objects from a pool for performance
- **Lazy Initialization** - Defer expensive initialization until needed
- **Dependency Injection** - Use service locator pattern to create instances
- **Custom Logic** - Apply any custom creation logic (caching, logging, etc.)

**Limitations**:
- Factory pattern doesn't work with init-only properties (records with `init` setters)
- For init-only properties, use constructor mapping or object initializers instead

---

### 11. Map to Existing Target Instance

**Priority**: 🟢 **Low-Medium**
**Status**: ✅ **Implemented**

**Description**: Update an existing object instead of creating a new one (useful for EF Core tracked entities).

**User Story**:
> "As a developer working with EF Core, I want to update existing tracked entities without creating new instances, so that EF Core's change tracking works correctly and I can efficiently update database records."

**Implementation Details**:

When `UpdateTarget = true` is specified in the `MapToAttribute`, the generator creates **two methods**:

1. **Standard method** - Creates and returns a new instance:
   ```csharp
   public static TargetType MapToTargetType(this SourceType source)
   ```

2. **Update method** - Updates an existing instance (void return):
   ```csharp
   public static void MapToTargetType(this SourceType source, TargetType target)
   ```

**Generated Code Pattern**:

```csharp
// Input:
[MapTo(typeof(UserDto), UpdateTarget = true)]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Generated output (both methods):
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

public static void MapToUserDto(this User source, UserDto target)
{
    if (source is null) return;
    if (target is null) return;

    target.Id = source.Id;
    target.Name = source.Name;
    target.Email = source.Email;
}
```

**Testing Status**:
- ✅ Unit tests added (`Generator_Should_Generate_Update_Target_Method`)
- ✅ Unit tests added (`Generator_Should_Not_Generate_Update_Target_Method_When_False`)
- ✅ Unit tests added (`Generator_Should_Include_Hooks_In_Update_Target_Method`)
- ✅ All tests passing (171 succeeded, 13 skipped)

**Sample Code Locations**:
- `sample/Atc.SourceGenerators.Mapping.Domain/Settings.cs` - Simple update target example
- `sample/PetStore.Domain/Models/Pet.cs` - EF Core entity update example (with `Bidirectional = true`)

**Use Cases**:

1. **EF Core Tracked Entities**:
   ```csharp
   // Fetch tracked entity from database
   var existingPet = await dbContext.Pets.FindAsync(petId);

   // Update it with new data (EF Core tracks changes)
   domainPet.MapToPetEntity(existingPet);

   // Save changes (only modified properties are updated in database)
   await dbContext.SaveChangesAsync();
   ```

2. **Reduce Object Allocations**:
   ```csharp
   // Reuse existing DTO instance
   var settingsDto = new SettingsDto();

   settings1.MapToSettingsDto(settingsDto);
   // ... use settingsDto

   settings2.MapToSettingsDto(settingsDto);  // Reuse same instance
   // ... use settingsDto
   ```

3. **Update UI ViewModels**:
   ```csharp
   // Update existing ViewModel without creating new instance
   var viewModel = this.DataContext as UserViewModel;
   updatedUser.MapToUserViewModel(viewModel);
   ```

**Important Notes**:

- The update method performs **null checks** for both source and target
- The update method has a **void return type** (no return value)
- **BeforeMap** and **AfterMap** hooks are fully supported in both methods
- The update method **does not use Factory** (factory is only for creating new instances)
- Works seamlessly with **Bidirectional = true** (both directions get update overloads)
- All properties are updated, including nullable properties

**When to Use**:

- ✅ Updating EF Core tracked entities
- ✅ Reducing allocations for frequently mapped objects
- ✅ Updating existing ViewModels or DTOs
- ✅ Scenarios where you need to preserve object identity

**When NOT to Use**:

- ❌ When you always need new instances
- ❌ With immutable types (records with init-only properties)
- ❌ When Factory method is needed (factory creates new instances)

---

### 12. Reference Handling / Circular Dependencies

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Handle circular references and maintain object identity during mapping.

**Example**:

```csharp
public class User
{
    public List<Post> Posts { get; set; } = new();
}

public class Post
{
    public User Author { get; set; } = null!;  // Circular reference
}

// Need to track mapped instances to avoid infinite recursion
```

---

### 13. IQueryable Projections

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Generate `Expression<Func<TSource, TTarget>>` for use in EF Core `.Select()` queries (server-side projection).

**Example**:

```csharp
// Generated expression:
public static Expression<Func<User, UserDto>> ProjectToUserDto()
{
    return user => new UserDto
    {
        Id = user.Id,
        Name = user.Name
    };
}

// Usage with EF Core:
var dtos = dbContext.Users.Select(User.ProjectToUserDto()).ToList();
// SQL is optimized - only selected columns are queried
```

**Benefits**:

- Reduce database round-trips
- Better performance with EF Core
- Server-side filtering and projection

---

### 14. Generic Mappers

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Create reusable mapping logic for generic types.

**Example**:

```csharp
public class Result<T>
{
    public T Data { get; set; } = default!;
    public bool Success { get; set; }
}

// Map Result<User> to Result<UserDto>
```

---

### 15. Private Member Access

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Map to/from private properties and fields using reflection emit or source generation.

---

### 16. Multi-Source Consolidation

**Priority**: 🟢 **Low-Medium** ⭐ *Requested by Mapperly users*
**Status**: ❌ Not Implemented

**Description**: Merge multiple source objects into a single destination object.

**User Story**:
> "As a developer, I want to combine data from multiple sources (like User + UserProfile + UserSettings) into a single DTO without writing manual merging logic."

**Example**:

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserProfile
{
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

// Potential API:
public static UserDto MapToUserDto(this User user, UserProfile profile)
{
    // Merge both sources into UserDto
}
```

**Implementation Considerations**:

- Allow multiple source parameters in mapping methods
- Handle property conflicts (which source takes precedence?)
- Consider opt-in via attribute parameter: `[MapFrom(typeof(User), typeof(UserProfile))]`

---

### 17. Value Converters

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Apply custom conversion logic to specific properties.

**Example**:

```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    [MapConverter(typeof(UpperCaseConverter))]
    public string Name { get; set; } = string.Empty;
}

public class UpperCaseConverter : IValueConverter<string, string>
{
    public string Convert(string value) => value.ToUpperInvariant();
}
```

---

### 18. Format Providers

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Support culture-specific formatting during type conversions.

**Example**:

```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    [MapFormat("C", CultureInfo = "en-US")]
    public decimal Salary { get; set; }  // Format as USD currency
}
```

---

### 19. Property Name Casing Strategies (SnakeCase, camelCase)

**Priority**: 🟢 **Low-Medium** ⭐ *SnakeCase requested by Mapperly users*
**Status**: ❌ Not Implemented (Reconsidered based on user demand)

**Description**: Automatically map properties with different casing conventions (common when mapping to/from JSON APIs).

**User Story**:
> "As a developer integrating with external APIs, I want to map PascalCase domain models to snake_case JSON DTOs without manually specifying each property mapping."

**Example**:

```csharp
[MapTo(typeof(UserDto), PropertyNameStrategy = PropertyNameStrategy.SnakeCase)]
public partial class User
{
    public string FirstName { get; set; } = string.Empty;  // → first_name
    public string LastName { get; set; } = string.Empty;   // → last_name
    public DateTime DateOfBirth { get; set; }              // → date_of_birth
}

public class UserDto
{
    public string first_name { get; set; } = string.Empty;
    public string last_name { get; set; } = string.Empty;
    public DateTime date_of_birth { get; set; }
}
```

**Supported Strategies**:

- `PropertyNameStrategy.PascalCase` (default)
- `PropertyNameStrategy.CamelCase` (FirstName → firstName)
- `PropertyNameStrategy.SnakeCase` (FirstName → first_name)
- `PropertyNameStrategy.KebabCase` (FirstName → first-name)

**Implementation Notes**:

- Only enable when explicitly opted-in via attribute parameter
- Can be combined with `[MapProperty]` for overrides
- Useful for API boundary mappings (REST, GraphQL, etc.)

---

### 20. Base Class Configuration Inheritance

**Priority**: 🟢 **Low** ⭐ *Requested by Mapperly users*
**Status**: ❌ Not Implemented

**Description**: Automatically inherit mapping configurations from base classes to reduce duplication.

**Example**:

```csharp
[MapTo(typeof(EntityDto))]
public abstract partial class Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}

// UserEntity should inherit mapping configuration for Id and CreatedAt
[MapTo(typeof(UserDto))]
public partial class UserEntity : Entity
{
    public string Name { get; set; } = string.Empty;
}
```

**Benefits**:

- Reduce boilerplate in inheritance hierarchies
- Maintain DRY principle
- Common for entity base classes with audit fields

---

## ⛔ Do Not Need (Low Priority / Out of Scope)

These features are either too niche, too complex, or don't align with the design philosophy of this library. They may be reconsidered based on strong user demand.

### 21. External Mappers / Mapper Composition

**Reason**: Adds complexity; users can manually compose mappings if needed. Our extension method approach already allows natural composition.

**Status**: ❌ Not Planned

**Example of current approach**:

```csharp
// Already possible with extension methods
var dto = entity.MapToDomainModel().MapToDto();
```

---

### 22. Advanced Enum Strategies Beyond Special Cases

**Reason**: Our current enum support (with EnumMappingGenerator) is already comprehensive and handles 99% of use cases with special case detection, bidirectional mapping, and case-insensitive matching.

**Status**: ❌ Not Needed

---

### 23. Deep Cloning Support

**Reason**: Not a mapping concern; users should use dedicated cloning libraries if needed. Mapping is about transforming between different types, not duplicating the same type.

**Status**: ❌ Out of Scope

---

### 24. Conditional Mapping (Map if condition is true)

**Reason**: Too complex for source generation; users can use before/after hooks or manual logic. Conditionals introduce runtime behavior that's hard to express at compile time.

**Status**: ❌ Not Planned

---

### 25. Asynchronous Mapping

**Reason**: Mapping should be synchronous and side-effect-free. Async logic (like database lookups) belongs outside the mapping layer in services or repositories.

**Status**: ❌ Out of Scope

**Better pattern**:

```csharp
// Don't do this
var dto = await entity.MapToUserDtoAsync();  // ❌ Mapping shouldn't be async

// Do this instead
var user = entity.MapToUser();  // ✅ Sync mapping
await _userService.EnrichWithDataAsync(user);  // ✅ Async enrichment in service layer
```

---

### 26. Mapping Configuration Files (JSON/XML)

**Reason**: Attribute-based configuration is more discoverable, type-safe, and IDE-friendly. Configuration files add external dependencies, complexity, and lose compile-time validation.

**Status**: ❌ Not Planned

---

### 27. Runtime Dynamic Mapping

**Reason**: Conflicts with compile-time source generation philosophy and Native AOT compatibility. Dynamic mapping requires reflection, which we explicitly avoid for performance and trimming safety.

**Status**: ❌ Out of Scope

---

## 📅 Proposed Implementation Order

Based on priority, dependencies, and community demand (⭐ = high user demand from Mapperly community):

### Phase 1: Essential Features (v1.1 - Q1 2025)
**Goal**: Make the generators production-ready for 80% of use cases

1. **Collection Mapping** 🔴 Critical - Map `List<User>` to `List<UserDto>`
2. **Ignore Properties** 🔴 High - `[MapIgnore]` attribute
3. **Constructor Mapping** 🔴 High - Records and primary constructor support

**Estimated effort**: 3-4 weeks
**Impact**: Unlocks most real-world scenarios

---

### Phase 2: Flexibility & Customization (v1.2 - Q2 2025)
**Goal**: Handle edge cases and custom naming

4. **Custom Property Name Mapping** 🟡 Medium-High - `[MapProperty("TargetName")]`
5. **Built-in Type Conversion** 🟡 Medium - DateTime ↔ string, Guid ↔ string
6. **Polymorphic Type Mapping** 🔴 High ⭐ - Derived types and interfaces

**Estimated effort**: 4-5 weeks
**Impact**: Handle 95% of mapping scenarios

---

### Phase 3: Advanced Features (v1.3 - Q3 2025)
**Goal**: Validation and advanced scenarios

7. **Required Property Validation** 🟡 Medium - Compile-time warnings
8. **Flattening Support** 🟡 Medium - `Address.City` → `AddressCity`
9. **Property Exclusion Shortcuts** 🟢 Low-Medium ⭐ - Exclude all except mapped

**Estimated effort**: 3-4 weeks
**Impact**: Better developer experience and safety

---

### Phase 4: Professional Scenarios (v2.0 - Q4 2025)
**Goal**: Enterprise and EF Core integration

10. **IQueryable Projections** 🟢 Low-Medium ⭐ - EF Core server-side projections
11. **Map to Existing Instance** 🟢 Low-Medium - Update tracked entities
12. **Before/After Hooks** 🟢 Low-Medium - Custom pre/post logic

**Estimated effort**: 5-6 weeks
**Impact**: Support complex enterprise scenarios

---

### Phase 5: Optional Enhancements (v2.1+ - 2026)
**Goal**: Nice-to-have features based on feedback

13. **Multi-Source Consolidation** 🟢 Low-Medium ⭐ - Merge multiple sources
14. **SnakeCase/CamelCase Strategies** 🟢 Low-Medium ⭐ - API boundary mapping
15. **Base Class Configuration** 🟢 Low ⭐ - Inherit mappings from base classes
16. **Object Factories** 🟢 Low-Medium - Custom object creation
17. **Reference Handling** 🟢 Low - Circular dependencies
18. **Value Converters** 🟢 Low-Medium - Custom property conversions
19. **Generic Mappers** 🟢 Low - `Result<T>` scenarios
20. **Format Providers** 🟢 Low - Culture-specific formatting

**Estimated effort**: Variable, based on priority and user demand
**Impact**: Polish and edge cases

---

### Feature Prioritization Matrix

| Feature | Priority | User Demand | Complexity | Phase |
|---------|----------|-------------|------------|-------|
| Collection Mapping | 🔴 Critical | ⭐⭐⭐ | Medium | 1.1 |
| Ignore Properties | 🔴 High | ⭐⭐⭐ | Low | 1.1 |
| Constructor Mapping | 🔴 High | ⭐⭐⭐ | Medium | 1.1 |
| Polymorphic Mapping | 🔴 High | ⭐⭐⭐ | High | 1.2 |
| Custom Property Names | 🟡 Med-High | ⭐⭐ | Low | 1.2 |
| Type Conversion | 🟡 Medium | ⭐⭐ | Medium | 1.2 |
| Required Validation | 🟡 Medium | ⭐⭐ | Low | 1.3 |
| Flattening | 🟡 Medium | ⭐⭐ | Medium | 1.3 |
| IQueryable Projections | 🟢 Low-Med | ⭐⭐⭐ | High | 2.0 |
| Multi-Source | 🟢 Low-Med | ⭐⭐ | High | 2.1+ |
| SnakeCase | 🟢 Low-Med | ⭐⭐ | Low | 2.1+ |

---

## 🎯 Success Metrics

To determine if these features are meeting user needs:

1. **GitHub Issues & Feature Requests** - Track user-requested features
2. **NuGet Download Stats** - Adoption rate
3. **Documentation Feedback** - Are users understanding how to use advanced features?
4. **Community Contributions** - PRs and discussions
5. **Comparison to Mapperly** - Feature parity vs. simplicity trade-offs

---

## 📝 Notes

### Design Philosophy

- **Guiding Principle**: Keep the generators **simple**, **predictable**, and **AOT-compatible**
- **Trade-offs**: We intentionally avoid runtime reflection and dynamic features to maintain Native AOT support
- **Mapperly Inspiration**: While Mapperly is comprehensive (with 100+ features across the API), we aim to cover the **80% use case with 20% of the complexity**
- **User Feedback**: This roadmap is a living document and will evolve based on real-world usage patterns

### Key Differences from Mapperly

**What we do differently**:

1. **Extension Methods** - We generate fluent extension methods (`user.MapToUserDto()`) vs. Mapperly's mapper classes
2. **Explicit opt-in** - Each type must have `[MapTo]` attribute vs. Mapperly's convention-based approach
3. **Simpler API surface** - Fewer attributes and configuration options for easier onboarding
4. **EnumMappingGenerator** - We have a dedicated, powerful enum mapping generator with special case detection

**What we learn from Mapperly**:

- ⭐ **Collection mapping** is absolutely essential (appears in nearly every issue)
- ⭐ **Polymorphic type mapping** is highly requested for real-world inheritance scenarios
- ⭐ **IQueryable projections** are critical for EF Core users (performance optimization)
- ⭐ **Property naming strategies** (especially SnakeCase) are needed for API boundary scenarios
- ⚠️ Complex features like multi-source consolidation and base class inheritance can wait until user demand is proven

### Lessons from Mapperly's GitHub

**From 3.7k stars and 66 open issues**:

- **Documentation is critical** - Users need clear examples for each feature
- **Performance benchmarks matter** - Developers want proof that source generation is faster
- **Readable generated code** - Developers trust what they can inspect and debug
- **Active issue triage** - Quick responses to bugs and feature requests build community trust
- **Good first issues** - Tagged beginner-friendly issues encourage contributions

### Updated Priorities Based on Community Insights

**Originally "Do Not Need" → Reconsidered**:

- ✅ **Property Naming Strategies** (SnakeCase) - Moved to "Nice to Have" due to API integration demand
- ✅ **Multi-Source Consolidation** - Added to "Nice to Have" based on Mapperly user requests
- ✅ **Base Class Configuration** - Added to "Nice to Have" for inheritance scenarios

**Elevated Priority**:

- 🔴 **Polymorphic Mapping** - Raised from Medium to High based on issue frequency
- 🔴 **IQueryable Projections** - Recognized as critical for EF Core users despite complexity

---

## 🔗 Related Resources

- **Mapperly Documentation**: https://mapperly.riok.app/docs/intro/
- **Mapperly GitHub**: https://github.com/riok/mapperly (3.7k⭐)
- **Our Documentation**: See `/docs/generators/ObjectMapping.md` and `/docs/generators/EnumMapping.md`
- **Sample Projects**: See `/sample/PetStore.Api` for complete example

---

**Last Updated**: 2025-01-17 (Updated with GitHub community insights)
**Version**: 1.1
**Research Date**: January 2025 (Mapperly v4.3.0)
**Maintained By**: Atc.SourceGenerators Team
