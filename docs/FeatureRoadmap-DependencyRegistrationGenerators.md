# 🎯 Feature Roadmap - Dependency Registration Generator

This document outlines the feature roadmap for the **DependencyRegistrationGenerator**, based on analysis of popular DI registration libraries and real-world usage patterns.

## 🔍 Research Sources

This roadmap is based on comprehensive analysis of:

1. **Scrutor** - [khellang/Scrutor](https://github.com/khellang/Scrutor) - 4.2k⭐, 273 forks, 11.8k dependent projects
   - Runtime assembly scanning and decoration
   - Convention-based registration
   - Mature ecosystem (MIT license)

2. **AutoRegisterInject** - [patrickklaeren/AutoRegisterInject](https://github.com/patrickklaeren/AutoRegisterInject) - 119⭐
   - Source generator approach with attributes
   - Per-type registration with `[RegisterScoped]`, `[RegisterSingleton]`, etc.
   - Multi-assembly support

3. **Jab** - [pakrym/jab](https://github.com/pakrym/jab) - Compile-time DI container
   - 200x faster startup than Microsoft.Extensions.DependencyInjection
   - 7x faster resolution
   - AOT-friendly, zero reflection

4. **Microsoft.Extensions.DependencyInjection** - Standard .NET DI abstractions
   - Keyed services (.NET 8+)
   - `IHostedService` and `BackgroundService` registration
   - Factory methods and implementation instances

### 📊 Key Insights from Community

**What Users Care About** (from Scrutor's 11.8k dependents):

- **Convention-based registration** - Reduce boilerplate for large projects
- **Generic interface support** - Handle `IRepository<T>`, `IHandler<TRequest, TResponse>`
- **Decorator pattern** - Wrap existing registrations without modifying original code
- **Assembly scanning** - Auto-discover services from referenced assemblies
- **Filtering capabilities** - Exclude specific namespaces, types, or patterns
- **Lifetime flexibility** - Different services need different lifetimes

**Jab's Performance Claims**:

- Compile-time generation eliminates startup overhead
- Clean stack traces (no reflection noise)
- Registration validation at compile time

**AutoRegisterInject's Approach**:

- Decentralized registration (attributes on types, not central config)
- Reduces merge conflicts in team environments
- Assembly-specific extension methods for modular registration

---

## 📊 Current State

### ✅ DependencyRegistrationGenerator - Implemented Features

- **Auto-interface detection** - Automatically registers all implemented interfaces (excluding System.*)
- **Explicit interface override** - Use `As` parameter to specify exact interface
- **Register as self** - Use `As = typeof(void)` to register concrete type only
- **Smart naming** - Generate unique extension method names (`AddDependencyRegistrationsFromDomain()`)
- **Transitive registration** - 4 overloads support automatic or selective assembly registration
- **Hosted service detection** - Automatically uses `AddHostedService<T>()` for `BackgroundService` and `IHostedService`
- **Generic interface registration** - Support open generic types like `IRepository<T>`, `IHandler<TRequest, TResponse>`
- **Keyed service registration** - Multiple implementations with keys (.NET 8+)
- **Factory method registration** - Custom initialization logic via static factory methods
- **Instance registration** - Register pre-created singleton instances via static fields, properties, or methods
- **TryAdd* registration** - Conditional registration for default implementations (library pattern)
- **Decorator pattern support** - Wrap services with cross-cutting concerns (logging, caching, validation)
- **Assembly scanning filters** - Exclude types by namespace, pattern, or interface (supports wildcards)
- **Lifetime support** - Singleton (default), Scoped, Transient
- **Multi-project support** - Assembly-specific extension methods
- **Compile-time validation** - Diagnostics for invalid configurations (ATCDIR001-010)
- **Native AOT compatible** - Zero reflection, compile-time generation

---

## 📋 Feature Status Overview

| Status | Feature | Priority | Version |
|:------:|---------|----------|---------|
| ✅ | [Generic Interface Registration](#1-generic-interface-registration) | 🔴 Critical | v1.1 |
| ✅ | [Keyed Service Registration](#2-keyed-service-registration) | 🔴 High | v1.1 |
| ✅ | [Factory Method Registration](#3-factory-method-registration) | 🔴 High | v1.1 |
| ✅ | [TryAdd* Registration](#4-tryadd-registration) | 🟡 Medium | v1.2 |
| ✅ | [Assembly Scanning Filters](#5-assembly-scanning-filters) | 🟡 Medium | v1.2 |
| ✅ | [Decorator Pattern Support](#6-decorator-pattern-support) | 🟢 Low-Medium | v1.3 |
| ✅ | [Implementation Instance Registration](#7-implementation-instance-registration) | 🟢 Low-Medium | v1.4 |
| ✅ | [Conditional Registration](#8-conditional-registration) | 🟢 Low-Medium | v1.5 |
| ❌ | [Auto-Discovery by Convention](#9-auto-discovery-by-convention) | 🟢 Low | - |
| ❌ | [Registration Validation Diagnostics](#10-registration-validation-diagnostics) | 🟢 Low | - |
| ⚠️ | [Multi-Interface Registration](#11-multi-interface-registration-enhanced) | 🟢 Low | Partial |
| 🚫 | [Runtime Assembly Scanning](#12-runtime-assembly-scanning) | - | Out of Scope |
| 🚫 | [Property/Field Injection](#13-propertyfield-injection) | - | Not Planned |
| 🚫 | [Auto-Wiring Based on Reflection](#14-auto-wiring-based-on-reflection) | - | Out of Scope |
| 🚫 | [Service Replacement/Override at Runtime](#15-service-replacementoverride-at-runtime) | - | Not Planned |

**Legend:**
- ✅ **Implemented** - Feature is complete and available
- ⚠️ **Partially Implemented** - Some aspects are available, others are in progress
- ❌ **Not Implemented** - Feature is planned but not yet developed
- 🚫 **Not Planned** - Feature is out of scope or not aligned with project goals

---

## 🎯 Need to Have (High Priority)

These features are essential based on Scrutor's popularity and real-world DI patterns.

### 1. Generic Interface Registration

**Priority**: 🔴 **Critical**
**Status**: ✅ **Implemented** (v1.1)
**Inspiration**: Scrutor's generic type support

**Description**: Support registering services that implement open generic interfaces like `IRepository<T>`, `IHandler<TRequest, TResponse>`.

**User Story**:
> "As a developer using the repository pattern, I want to register `IRepository<T>` implementations without manually registering each entity type."

**Example**:

```csharp
// Generic interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task SaveAsync(T entity);
}

// Generic implementation
[Registration(Lifetime = Lifetime.Scoped)]
public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;

    public Repository(DbContext context) => _context = context;

    public Task<T?> GetByIdAsync(int id) => _context.Set<T>().FindAsync(id).AsTask();
    public Task SaveAsync(T entity) { /* ... */ }
}

// Generated code should register open generic:
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

**Implementation Notes**:

- ✅ Detects when service implements open generic interface
- ✅ Generates `typeof(IInterface<>)` and `typeof(Implementation<>)` syntax
- ✅ Validates generic constraints match between interface and implementation
- ✅ Supports multiple generic parameters (`IHandler<TRequest, TResponse>`)
- ✅ Works with explicit `As` parameter and auto-detection
- ✅ Supports constraints (where T : class, IEntity, new())

---

### 2. Keyed Service Registration

**Priority**: 🔴 **High**
**Status**: ✅ **Implemented** (v1.1)
**Inspiration**: .NET 8+ keyed services, Scrutor's named registrations

**Description**: Support keyed service registration for multiple implementations of the same interface.

**User Story**:
> "As a developer, I want to register multiple implementations of `IPaymentProcessor` (Stripe, PayPal, Square) and resolve them by key."

**Example**:

```csharp
[Registration(As = typeof(IPaymentProcessor), Key = "Stripe")]
public class StripePaymentProcessor : IPaymentProcessor
{
    public Task ProcessPaymentAsync(decimal amount) { /* ... */ }
}

[Registration(As = typeof(IPaymentProcessor), Key = "PayPal")]
public class PayPalPaymentProcessor : IPaymentProcessor
{
    public Task ProcessPaymentAsync(decimal amount) { /* ... */ }
}

// Generated code:
services.AddKeyedScoped<IPaymentProcessor, StripePaymentProcessor>("Stripe");
services.AddKeyedScoped<IPaymentProcessor, PayPalPaymentProcessor>("PayPal");

// Usage:
public class CheckoutService
{
    public CheckoutService([FromKeyedServices("Stripe")] IPaymentProcessor processor)
    {
        // ...
    }
}
```

**Implementation Notes**:

- ✅ Added `Key` parameter to `[Registration]` attribute
- ✅ Generates `AddKeyed{Lifetime}()` calls
- ✅ Supports both string and type keys
- ✅ Works with generic types (AddKeyedScoped(typeof(IRepository<>), "Key", typeof(Repository<>)))

---

### 3. Factory Method Registration

**Priority**: 🟡 **Medium-High**
**Status**: ✅ **Implemented** (v1.1)
**Inspiration**: Microsoft.Extensions.DependencyInjection factories, Jab's custom instantiation

**Description**: Support registering services via factory methods for complex initialization logic.

**User Story**:
> "As a developer, I want to register services that require custom initialization logic (like reading configuration, conditional setup, etc.) without creating intermediate builder classes."

**Example**:

```csharp
[Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
public class EmailSender : IEmailSender
{
    private readonly string apiKey;

    private EmailSender(string apiKey) => this.apiKey = apiKey;

    // Factory method signature: static T Create(IServiceProvider provider)
    public static IEmailSender CreateEmailSender(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var apiKey = config["EmailSettings:ApiKey"] ?? throw new InvalidOperationException();
        return new EmailSender(apiKey);
    }

    public Task SendAsync(string to, string subject, string body) { /* ... */ }
}

// Generated code:
services.AddScoped<IEmailSender>(sp => EmailSender.CreateEmailSender(sp));
```

**Implementation Notes**:

- ✅ Added `Factory` property to `[Registration]` attribute
- ✅ Factory method must be `static` and return the service type (interface or class)
- ✅ Factory method must accept `IServiceProvider` as single parameter
- ✅ Validates factory method signature at compile time
- ✅ Generates factory delegate registration: `services.Add{Lifetime}<T>(sp => Class.Factory(sp))`
- ✅ Works with all lifetimes (Singleton, Scoped, Transient)
- ✅ Supports registering as interface, as self, or multiple interfaces
- ✅ Diagnostics: ATCDIR005 (factory method not found), ATCDIR006 (invalid signature)

---

### 4. TryAdd* Registration

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented** (v1.2)
**Inspiration**: Scrutor's TryAdd support, AutoRegisterInject's "Try" variants

**Description**: Support conditional registration that only adds services if not already registered.

**User Story**:
> "As a library author, I want to register default implementations that can be overridden by application code."

**Example**:

```csharp
[Registration(As = typeof(ILogger), TryAdd = true)]
public class DefaultLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
}

// Generated code:
services.TryAddScoped<ILogger, DefaultLogger>();

// User can override:
services.AddScoped<ILogger, CustomLogger>();  // This wins
```

**Implementation Notes**:

- ✅ Added `TryAdd` boolean parameter to `[Registration]` attribute
- ✅ Generates `TryAdd{Lifetime}()` calls instead of `Add{Lifetime}()`
- ✅ Works with factory methods: `services.TryAddScoped<T>(sp => Factory(sp))`
- ✅ Supports all lifetimes: TryAddSingleton, TryAddScoped, TryAddTransient
- ✅ Works with generic types: `services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>))`
- ✅ Compatible with AsSelf and multiple interface registrations
- ⚠️ Note: Keyed services take precedence (no TryAdd support for keyed registrations)
- ✅ Requires `using Microsoft.Extensions.DependencyInjection.Extensions;` (automatically added to generated code)

---

### 5. Assembly Scanning Filters

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented** (v1.2)
**Inspiration**: Scrutor's filtering capabilities

**Description**: Provide filtering options to exclude specific types, namespaces, or patterns from transitive registration.

**User Story**:
> "As a developer, I want to exclude internal services or test utilities from automatic registration when using `includeReferencedAssemblies: true`."

**Example**:

```csharp
// Option A: Exclude specific namespace (supports arrays)
[assembly: RegistrationFilter(ExcludeNamespaces = new[] { "MyApp.Internal", "MyApp.Tests" })]

// Option B: Exclude by naming pattern (supports wildcards)
[assembly: RegistrationFilter(ExcludePatterns = new[] { "*Test*", "*Mock*" })]

// Option C: Exclude types implementing specific interface
[assembly: RegistrationFilter(ExcludeImplementing = new[] { typeof(ITestUtility) })]

// Option D: Multiple filters in one attribute
[assembly: RegistrationFilter(
    ExcludeNamespaces = new[] { "MyApp.Internal" },
    ExcludePatterns = new[] { "*Test*", "*Mock*" })]

// Generated code only includes non-excluded types
```

**Implementation Notes**:

- ✅ Assembly-level `RegistrationFilterAttribute` with `AllowMultiple = true`
- ✅ Support namespace exclusion (exact match + sub-namespaces)
- ✅ Support wildcard patterns: `*` (any characters), `?` (single character)
- ✅ Support interface exclusion with proper generic type comparison
- ✅ Multiple filter attributes can be applied
- ✅ Filters applied to both current assembly and referenced assemblies
- ✅ All properties accept arrays for multiple values
- ✅ Pattern matching is case-insensitive
- ✅ Sub-namespace matching: "MyApp.Internal" excludes "MyApp.Internal.Deep"

---

### 6. Decorator Pattern Support

**Priority**: 🟢 **Low-Medium** ⭐ *Highly valued by Scrutor users*
**Status**: ✅ **Implemented** (v1.3 - January 2025)
**Inspiration**: Scrutor's `Decorate()` method

**Description**: Support decorating already-registered services with additional functionality (logging, caching, validation, etc.).

**User Story**:
> "As a developer, I want to add cross-cutting concerns (logging, caching, retry logic) to services without modifying the original implementation."

**Example**:

```csharp
// Original service
[Registration(As = typeof(IOrderService))]
public class OrderService : IOrderService
{
    public Task PlaceOrderAsync(Order order) { /* ... */ }
}

// Decorator (wraps original)
[Registration(As = typeof(IOrderService), Decorator = true)]
public class LoggingOrderServiceDecorator : IOrderService
{
    private readonly IOrderService _inner;
    private readonly ILogger<IOrderService> _logger;

    public LoggingOrderServiceDecorator(IOrderService inner, ILogger<IOrderService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task PlaceOrderAsync(Order order)
    {
        _logger.LogInformation("Placing order {OrderId}", order.Id);
        await _inner.PlaceOrderAsync(order);
        _logger.LogInformation("Order {OrderId} placed successfully", order.Id);
    }
}

// Generated code:
services.AddScoped<IOrderService, OrderService>();
services.Decorate<IOrderService, LoggingOrderServiceDecorator>();  // Wraps existing registration
```

**Implementation Notes**:

- ✅ Added `Decorator` boolean parameter to `[Registration]` attribute
- ✅ Decorators must specify explicit `As` parameter (interface being decorated)
- ✅ Decorator registration automatically comes after base service registration
- ✅ Decorator constructor must accept the interface as first parameter
- ✅ Supports multiple decorators (chaining) - applied in discovery order
- ✅ Generates `Decorate<T>()` extension methods (both generic and non-generic for open generics)
- ✅ Uses `ActivatorUtilities.CreateInstance()` to properly inject inner service
- ✅ Preserves service lifetime from original registration
- ✅ Works with Singleton, Scoped, and Transient lifetimes
- ✅ Complete test coverage with 7 unit tests
- ✅ Documented in comprehensive decorator pattern section of docs

---

### 7. Implementation Instance Registration

**Priority**: 🟢 **Low**
**Status**: ✅ **Implemented** (v1.4 - January 2025)

**Description**: Register pre-created singleton instances via static fields, properties, or methods.

**User Story**:
> "As a developer, I want to register pre-configured singleton instances (like immutable configuration objects) without requiring factory methods or runtime initialization."

**Example**:

```csharp
[Registration(As = typeof(IConfiguration), Instance = nameof(DefaultInstance))]
public class AppConfiguration : IConfiguration
{
    // Static field providing pre-created instance
    public static readonly AppConfiguration DefaultInstance = new()
    {
        Setting1 = "default",
        Setting2 = 42
    };

    private AppConfiguration() { }  // Private constructor enforces singleton

    public string Setting1 { get; init; } = string.Empty;
    public int Setting2 { get; init; }
}

// Generated code:
services.AddSingleton<IConfiguration>(AppConfiguration.DefaultInstance);
```

**Alternative patterns supported**:

```csharp
// Static property
[Registration(As = typeof(ICache), Instance = nameof(Instance))]
public class MemoryCache : ICache
{
    public static MemoryCache Instance { get; } = new();
}

// Static method
[Registration(As = typeof(ILogger), Instance = nameof(GetDefault))]
public class DefaultLogger : ILogger
{
    public static DefaultLogger GetDefault() => new();
}
```

**Implementation Notes**:

- ✅ Added `Instance` property to `[Registration]` attribute
- ✅ Supports static fields, properties, and parameterless methods
- ✅ Generates `services.AddSingleton<T>(ClassName.MemberName)` or `services.AddSingleton<T>(ClassName.Method())`
- ✅ **Constraint**: Instance registration requires Singleton lifetime (enforced at compile-time)
- ✅ **Constraint**: Instance and Factory parameters are mutually exclusive
- ✅ Validates member exists and is static at compile-time
- ✅ Diagnostics: ATCDIR007 (member not found), ATCDIR008 (not static), ATCDIR009 (mutually exclusive), ATCDIR010 (requires Singleton)
- ✅ Works with TryAdd: `services.TryAddSingleton<T>(ClassName.Instance)`
- ✅ Complete test coverage with 8 unit tests
- ✅ Demonstrated in both DependencyRegistration and PetStore samples

---

## 💡 Nice to Have (Medium Priority)

These features would improve usability but are not critical for initial adoption.

### 8. Conditional Registration

**Priority**: 🟢 **Low-Medium**
**Status**: ✅ **Implemented** (v1.5 - January 2025)

**Description**: Register services based on configuration values at runtime (feature flags, environment-specific services).

**User Story**:
> "As a developer, I want to register different service implementations based on configuration values (feature flags) without code changes or redeployment."

**Example**:

```csharp
// appsettings.json
{
  "Features": {
    "UseRedisCache": true
  }
}

[Registration(As = typeof(ICache), Condition = "Features:UseRedisCache")]
public class RedisCache : ICache { }

[Registration(As = typeof(ICache), Condition = "!Features:UseRedisCache")]
public class MemoryCache : ICache { }

// Generated code checks configuration at runtime
public static IServiceCollection AddDependencyRegistrationsFromDomain(
    this IServiceCollection services,
    IConfiguration configuration)  // ← IConfiguration parameter added automatically
{
    if (configuration.GetValue<bool>("Features:UseRedisCache"))
    {
        services.AddSingleton<ICache, RedisCache>();
    }

    if (!configuration.GetValue<bool>("Features:UseRedisCache"))
    {
        services.AddSingleton<ICache, MemoryCache>();
    }

    return services;
}

// Usage
services.AddDependencyRegistrationsFromDomain(configuration);
```

**Implementation Notes**:

- ✅ Added `Condition` property to `[Registration]` attribute
- ✅ Supports negation with `!` prefix
- ✅ IConfiguration parameter automatically added to all method overloads when conditional services exist
- ✅ Generates `if (configuration.GetValue<bool>("key"))` checks wrapping registration calls
- ✅ Configuration is NOT passed transitively to referenced assemblies (each assembly manages its own)
- ✅ Works with all lifetimes (Singleton, Scoped, Transient)
- ✅ Fully Native AOT compatible (simple boolean reads from configuration)
- ✅ Thread-safe configuration reading
- ✅ Complete test coverage with 6 unit tests
- ✅ Demonstrated in both DependencyRegistration and PetStore samples
- ✅ Comprehensive documentation in Conditional Registration section

**Benefits**:
- 🎯 Feature Flags - Enable/disable features without redeployment
- 🌍 Environment-Specific - Different implementations for dev/staging/prod
- 🧪 A/B Testing - Easy experimentation with different implementations
- 💰 Cost Optimization - Disable expensive services when not needed
- 🚀 Gradual Rollout - Safely test new implementations before full deployment

---

### 9. Auto-Discovery by Convention

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented
**Inspiration**: Scrutor's convention-based scanning

**Description**: Automatically register types based on naming conventions without requiring attributes.

**Example**:

```csharp
// Convention: Classes ending with "Service" implement I{ClassName}
public class UserService : IUserService { }  // Auto-registered
public class OrderService : IOrderService { }  // Auto-registered

// Generated code discovers these by convention
```

**Considerations**:

- Conflicts with our explicit opt-in philosophy
- May lead to unexpected registrations
- Consider as opt-in feature with assembly-level attribute

---

### 10. Registration Validation Diagnostics

**Priority**: 🟡 **Medium**
**Status**: ❌ Not Implemented

**Description**: Provide compile-time diagnostics for common DI mistakes.

**Examples**:

- Warning if service has no public constructor
- Warning if constructor parameters cannot be resolved (missing registrations)
- Warning if circular dependencies detected
- Error if hosted service is not registered as Singleton

**Implementation**:

- Analyze constructor parameters
- Build dependency graph
- Detect cycles and missing dependencies

---

### 11. Multi-Interface Registration (Enhanced)

**Priority**: 🟢 **Low**
**Status**: ⚠️ Partially Implemented (auto-detects all interfaces)

**Description**: Allow explicit control over which interfaces to register when a class implements multiple interfaces.

**Current behavior**: Registers ALL implemented interfaces (excluding System.*)

**Enhancement**: Allow selective registration of specific interfaces

**Example**:

```csharp
// Register only specific interfaces
[Registration(As = new[] { typeof(IUserService), typeof(IEmailService) })]
public class UserService : IUserService, IEmailService, IAuditLogger
{
    // Only IUserService and IEmailService are registered, not IAuditLogger
}
```

---

## ⛔ Do Not Need (Low Priority / Out of Scope)

These features either conflict with design principles or are too complex.

### 12. Runtime Assembly Scanning

**Reason**: Conflicts with compile-time source generation philosophy. Scrutor already handles this well for runtime scenarios.

**Status**: ❌ Out of Scope

---

### 13. Property/Field Injection

**Reason**: Constructor injection is the recommended pattern. Property injection is an anti-pattern that hides dependencies.

**Status**: ❌ Not Planned

---

### 14. Auto-Wiring Based on Reflection

**Reason**: Breaks AOT compatibility. Defeats the purpose of compile-time generation.

**Status**: ❌ Out of Scope

---

### 15. Service Replacement/Override at Runtime

**Reason**: DI container should be immutable after configuration. Runtime replacement is fragile.

**Status**: ❌ Not Planned

---

## 📅 Proposed Implementation Order

Based on priority, user demand, and implementation complexity:

### Phase 1: Essential Features (v1.1 - Q1 2025) ✅ COMPLETED

**Goal**: Support advanced DI patterns (generics, keyed services, factory methods)

1. ✅ **Generic Interface Registration** 🔴 Critical - `IRepository<T>`, `IHandler<TRequest, TResponse>`
2. ✅ **Keyed Service Registration** 🔴 High - Multiple implementations with keys (.NET 8+)
3. ✅ **Factory Method Registration** 🟡 Medium-High - Custom initialization logic

**Status**: ✅ COMPLETED (January 2025)
**Impact**: Unlock repository pattern, multi-tenant scenarios, plugin architectures, complex initialization

---

### Phase 2: Flexibility & Control (v1.2 - Q1 2025) ✅ COMPLETED

**Goal**: Conditional registration and filtering

4. ✅ **TryAdd* Registration** 🟡 Medium - Conditional registration for library scenarios
5. ✅ **Assembly Scanning Filters** 🟡 Medium - Exclude namespaces/patterns from transitive registration

**Status**: ✅ COMPLETED (January 2025)
**Impact**: Better control over transitive registration, library author support

---

### Phase 2.5: Advanced Patterns (v1.3 - Q1 2025) ✅ COMPLETED

**Goal**: Decorator pattern for cross-cutting concerns

6. ✅ **Decorator Pattern Support** 🟢 Low-Medium ⭐ - Wrap services with logging, caching, validation

**Status**: ✅ COMPLETED (January 2025)
**Impact**: Enterprise-grade cross-cutting concerns without code modification

---

### Phase 3: Advanced Scenarios (v1.4 - Q1 2025) ✅ COMPLETED

**Goal**: Instance registration for pre-created singletons

7. ✅ **Implementation Instance Registration** 🟢 Low - Pre-created singleton instances

**Status**: ✅ COMPLETED (January 2025)
**Impact**: Support immutable configuration objects and pre-initialized singletons

---

### Phase 4: Advanced Scenarios (v1.5 - Q2 2025)

**Goal**: Validation and diagnostics

8. **Multi-Interface Registration** 🟢 Low - Selective interface registration
9. **Registration Validation Diagnostics** 🟡 Medium - Compile-time warnings for missing dependencies
10. **Conditional Registration** 🟢 Low-Medium - Feature flag-based registration

**Estimated effort**: 3-4 weeks
**Impact**: Catch DI mistakes at compile time, support feature toggles

---

### Phase 5: Enterprise Features (v2.0 - Q3 2025)

**Goal**: Convention-based patterns

11. **Auto-Discovery by Convention** 🟢 Low-Medium - Optional convention-based registration

**Estimated effort**: 2-3 weeks
**Impact**: Reduce boilerplate further with conventions

---

### Feature Prioritization Matrix

| Feature | Priority | User Demand | Complexity | Phase | Status |
|---------|----------|-------------|------------|-------|--------|
| Generic Interface Registration | 🔴 Critical | ⭐⭐⭐ | High | 1.1 | ✅ Done |
| Keyed Service Registration | 🔴 High | ⭐⭐⭐ | Medium | 1.1 | ✅ Done |
| Factory Method Registration | 🟡 Med-High | ⭐⭐ | Medium | 1.1 | ✅ Done |
| TryAdd* Registration | 🟡 Medium | ⭐⭐ | Low | 1.2 | ✅ Done |
| Assembly Scanning Filters | 🟡 Medium | ⭐⭐ | Medium | 1.2 | ✅ Done |
| Decorator Pattern | 🟢 Low-Med | ⭐⭐⭐ | Very High | 1.3 | ✅ Done |
| Implementation Instance Registration | 🟢 Low | ⭐ | Medium | 1.4 | ✅ Done |
| Multi-Interface Registration | 🟢 Low | ⭐ | Low | 1.5 | 📋 Planned |
| Registration Validation | 🟡 Medium | ⭐⭐ | High | 1.5 | 📋 Planned |
| Conditional Registration | 🟢 Low-Med | ⭐ | Medium | 1.5 | 📋 Planned |
| Convention-Based Discovery | 🟢 Low-Med | ⭐⭐ | Medium | 2.0 | 📋 Planned |

---

## 🎯 Success Metrics

To determine if these features are meeting user needs:

1. **Adoption Rate** - NuGet download statistics
2. **GitHub Issues** - Track feature requests and pain points
3. **Performance Benchmarks** - Compare startup time vs. Scrutor/runtime registration
4. **Community Feedback** - Surveys, blog posts, conference talks
5. **Real-World Usage** - Case studies from production applications

---

## 📝 Notes

### Design Philosophy

- **Guiding Principle**: **Explicit opt-in**, **compile-time safety**, **AOT-compatible**
- **Trade-offs**: We prefer attribute-based registration over convention-based to maintain predictability
- **Scrutor vs. Our Approach**: Scrutor is runtime-based (assembly scanning), we are compile-time (source generation). Both have their place.
- **Performance Focus**: Like Jab, we eliminate reflection overhead by generating code at compile time

### Key Differences from Scrutor

**What we do differently**:

1. **Compile-time generation** - Zero startup overhead vs. Scrutor's runtime scanning
2. **Per-type attributes** - Explicit `[Registration]` on each service vs. assembly-wide conventions
3. **Transitive registration** - Our 4-overload approach vs. Scrutor's fluent API
4. **AOT-friendly** - Native AOT compatible out of the box

**What we learn from Scrutor**:

- ⭐ **Generic interface support** is critical for repository/handler patterns
- ⭐ **Decorator pattern** is highly valued (cross-cutting concerns)
- ⭐ **Filtering capabilities** prevent unintended registrations in large codebases
- ⚠️ Runtime flexibility (Scrutor's strength) is less important for our compile-time approach

### Lessons from AutoRegisterInject

**From 119 stars and attribute-based approach**:

- **Decentralized registration** reduces merge conflicts in teams
- **Assembly-specific extension methods** enable modular registration
- **TryAdd variants** are important for library authors
- **Keyed services** support multi-tenant scenarios (.NET 8+)

### Lessons from Jab

**From performance-focused DI container**:

- **Compile-time validation** catches errors before runtime
- **Readable generated code** builds developer trust
- **Zero reflection** is essential for AOT and startup performance
- **Clean stack traces** improve debugging experience

---

## 🔗 Related Resources

- **Scrutor**: <https://github.com/khellang/Scrutor> (4.2k⭐, runtime scanning)
- **AutoRegisterInject**: <https://github.com/patrickklaeren/AutoRegisterInject> (119⭐, attribute-based)
- **Jab**: <https://github.com/pakrym/jab> (compile-time DI container)
- **Microsoft.Extensions.DependencyInjection**: <https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection>
- **Our Documentation**: See `/docs/generators/DependencyRegistration.md`
- **Sample Projects**: See `/sample/PetStore.Api` for complete example

---

**Last Updated**: 2025-01-17 (Implementation Instance Registration completed)
**Version**: 1.4
**Research Date**: January 2025 (Scrutor v6.1.0)
**Maintained By**: Atc.SourceGenerators Team
