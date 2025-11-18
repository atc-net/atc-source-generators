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
**Status**: ❌ Not Implemented

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

**Implementation Notes**:

- Create `[MapIgnore]` attribute in Atc.SourceGenerators.Annotations
- Skip properties decorated with this attribute during mapping generation
- Consider allowing ignore on target properties as well (different use case)

---

### 4. Custom Property Name Mapping

**Priority**: 🟡 **Medium-High**
**Generator**: ObjectMappingGenerator
**Status**: ❌ Not Implemented

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

    [MapProperty(nameof(UserDto.FullName))]
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

**Implementation Notes**:

- Create `[MapProperty(string targetPropertyName)]` attribute
- Validate that target property exists at compile time
- Support both `nameof()` and string literals
- Report diagnostic if target property doesn't exist

---

### 5. Flattening Support

**Priority**: 🟡 **Medium**
**Generator**: ObjectMappingGenerator
**Status**: ❌ Not Implemented

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

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    // Flattened properties (convention: {PropertyName}{NestedPropertyName})
    public string AddressCity { get; set; } = string.Empty;
    public string AddressStreet { get; set; } = string.Empty;
}

// Generated code:
Name = source.Name,
AddressCity = source.Address.City,
AddressStreet = source.Address.Street
```

**Implementation Notes**:

- Opt-in via `EnableFlattening = true` parameter on `[MapTo]`
- Use naming convention: `{PropertyName}{NestedPropertyName}`
- Only flatten one level deep initially (can expand later)
- Handle null nested objects gracefully

---

### 6. Built-in Type Conversion

**Priority**: 🟡 **Medium**
**Generator**: ObjectMappingGenerator
**Status**: ❌ Not Implemented

**Description**: Automatically convert between common types (DateTime ↔ string, int ↔ string, GUID ↔ string, etc.).

**User Story**:
> "As a developer, I want to convert between common types (like DateTime to string) without writing custom conversion logic."

**Example**:

```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    public DateTime CreatedAt { get; set; }
    public Guid Id { get; set; }
    public int Age { get; set; }
}

public class UserDto
{
    public string CreatedAt { get; set; } = string.Empty;  // DateTime → string
    public string Id { get; set; } = string.Empty;          // Guid → string
    public string Age { get; set; } = string.Empty;         // int → string
}

// Generated code:
CreatedAt = source.CreatedAt.ToString("O"),  // ISO 8601 format
Id = source.Id.ToString(),
Age = source.Age.ToString()
```

**Implementation Notes**:

- Support common conversions:
  - `DateTime` ↔ `string` (use ISO 8601 format)
  - `DateTimeOffset` ↔ `string`
  - `Guid` ↔ `string`
  - Numeric types ↔ `string`
  - `bool` ↔ `string`
- Use invariant culture for string conversions
- Consider adding `[MapFormat("format")]` attribute for custom formats

---

### 7. Required Property Validation

**Priority**: 🟡 **Medium**
**Generator**: ObjectMappingGenerator
**Status**: ❌ Not Implemented

**Description**: Validate at compile time that all required properties on the target type are mapped.

**User Story**:
> "As a developer, I want compile-time errors if I forget to map required properties on my target type."

**Example**:

```csharp
[MapTo(typeof(UserDto))]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Missing: Email property
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // This property is required but not mapped - should generate warning/error
    public required string Email { get; set; }
}

// Diagnostic: Warning ATCMAP003: Required property 'Email' on target type 'UserDto' has no mapping
```

**Implementation Notes**:

- Detect `required` keyword on target properties (C# 11+)
- Generate diagnostic if no mapping exists for required property
- Severity: Warning (can be elevated to error by user)
- Consider all target properties as "recommended to map" with diagnostics

---

### 8. Polymorphic / Derived Type Mapping

**Priority**: 🔴 **High** ⭐ *Highly requested by Mapperly users*
**Generator**: ObjectMappingGenerator
**Status**: ❌ Not Implemented

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
[MapDerivedType(typeof(DogEntity), typeof(Dog))]
[MapDerivedType(typeof(CatEntity), typeof(Cat))]
public partial class AnimalEntity { }

// Generated code:
public static Animal MapToAnimal(this AnimalEntity source)
{
    return source switch
    {
        DogEntity dog => dog.MapToDog(),
        CatEntity cat => cat.MapToCat(),
        _ => throw new ArgumentException("Unknown type")
    };
}
```

**Implementation Notes**:

- Create `[MapDerivedType(Type sourceType, Type targetType)]` attribute
- Generate switch expression with type patterns
- Require mapping methods to exist for each derived type
- Consider inheritance hierarchies

---

## 💡 Nice to Have (Medium Priority)

These features would improve usability and flexibility but are not critical for initial adoption. They can be implemented based on user feedback and demand.

### 9. Before/After Mapping Hooks

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Execute custom logic before or after the mapping operation.

**Example**:

```csharp
[MapTo(typeof(UserDto), BeforeMap = nameof(BeforeMapUser), AfterMap = nameof(AfterMapUser))]
public partial class User
{
    public Guid Id { get; set; }

    private static void BeforeMapUser(User source)
    {
        // Custom validation or preprocessing
    }

    private static void AfterMapUser(User source, UserDto target)
    {
        // Custom post-processing
    }
}
```

---

### 10. Object Factories

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Use custom factory methods for object creation instead of `new()`.

**Example**:

```csharp
[MapTo(typeof(UserDto), Factory = nameof(CreateUserDto))]
public partial class User
{
    private static UserDto CreateUserDto()
    {
        return new UserDto { CreatedAt = DateTime.UtcNow };
    }
}
```

---

### 11. Map to Existing Target Instance

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Update an existing object instead of creating a new one (useful for EF Core tracked entities).

**Example**:

```csharp
// Generated method:
public static void MapToUserDto(this User source, UserDto target)
{
    target.Id = source.Id;
    target.Name = source.Name;
    // ... update existing instance
}

// Usage:
var existingDto = repository.GetDto(id);
user.MapToUserDto(existingDto);  // Update existing instance
```

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
