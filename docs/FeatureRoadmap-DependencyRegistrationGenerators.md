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
- **Lifetime support** - Singleton (default), Scoped, Transient
- **Multi-project support** - Assembly-specific extension methods
- **Compile-time validation** - Diagnostics for invalid configurations
- **Native AOT compatible** - Zero reflection, compile-time generation

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
**Status**: ❌ Not Implemented
**Inspiration**: Microsoft.Extensions.DependencyInjection factories, Jab's custom instantiation

**Description**: Support registering services via factory methods for complex initialization logic.

**User Story**:
> "As a developer, I want to register services that require custom initialization logic (like reading configuration, conditional setup, etc.) without creating intermediate builder classes."

**Example**:

```csharp
[Registration(As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
public partial class EmailSender : IEmailSender
{
    private readonly string _apiKey;

    private EmailSender(string apiKey) => _apiKey = apiKey;

    // Factory method signature: static T Create(IServiceProvider provider)
    private static EmailSender CreateEmailSender(IServiceProvider provider)
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

- Factory method must be `static` and return the service type
- Accept `IServiceProvider` parameter for dependency resolution
- Support both instance factories and delegate factories
- Validate factory method signature at compile time

---

### 4. TryAdd* Registration

**Priority**: 🟡 **Medium**
**Status**: ❌ Not Implemented
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

- Add `TryAdd` boolean parameter to `[Registration]`
- Generate `TryAdd{Lifetime}()` calls instead of `Add{Lifetime}()`
- Useful for default implementations and library code

---

### 5. Assembly Scanning Filters

**Priority**: 🟡 **Medium**
**Status**: ❌ Not Implemented
**Inspiration**: Scrutor's filtering capabilities

**Description**: Provide filtering options to exclude specific types, namespaces, or patterns from transitive registration.

**User Story**:
> "As a developer, I want to exclude internal services or test utilities from automatic registration when using `includeReferencedAssemblies: true`."

**Example**:

```csharp
// Option A: Exclude specific namespace
[assembly: RegistrationFilter(ExcludeNamespace = "MyApp.Internal")]

// Option B: Exclude by naming pattern
[assembly: RegistrationFilter(ExcludePattern = "*Test*")]

// Option C: Exclude types implementing specific interface
[assembly: RegistrationFilter(ExcludeImplementing = typeof(ITestUtility))]

// Generated code only includes non-excluded types
```

**Implementation Notes**:

- Assembly-level attribute for configuration
- Support namespace exclusion, type name patterns, interface exclusion
- Apply filters during transitive registration discovery

---

### 6. Decorator Pattern Support

**Priority**: 🟢 **Low-Medium** ⭐ *Highly valued by Scrutor users*
**Status**: ❌ Not Implemented
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

- Decorator registration must come after base registration
- Decorator constructor should accept the interface it decorates
- Support multiple decorators (chaining)
- Complex to implement with source generators (may require runtime helper)

---

### 7. Implementation Instance Registration

**Priority**: 🟢 **Low**
**Status**: ❌ Not Implemented

**Description**: Register pre-created instances as singletons.

**Example**:

```csharp
var myService = new MyService("config");
services.AddSingleton<IMyService>(myService);
```

**Note**: This is difficult to support with source generators since instances are created at runtime. May be out of scope.

---

## 💡 Nice to Have (Medium Priority)

These features would improve usability but are not critical for initial adoption.

### 8. Conditional Registration

**Priority**: 🟢 **Low-Medium**
**Status**: ❌ Not Implemented

**Description**: Register services only if certain conditions are met (e.g., feature flags, environment checks).

**Example**:

```csharp
[Registration(As = typeof(ICache), Condition = "Features:UseRedis")]
public class RedisCache : ICache { }

[Registration(As = typeof(ICache), Condition = "!Features:UseRedis")]
public class MemoryCache : ICache { }

// Generated code checks configuration at runtime
if (configuration.GetValue<bool>("Features:UseRedis"))
{
    services.AddScoped<ICache, RedisCache>();
}
else
{
    services.AddScoped<ICache, MemoryCache>();
}
```

**Implementation Considerations**:

- Requires runtime configuration access
- Adds complexity to generated code

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

### Phase 1: Essential Features (v1.1 - Q1 2025)

**Goal**: Support advanced DI patterns (generics, keyed services)

1. **Generic Interface Registration** 🔴 Critical - `IRepository<T>`, `IHandler<TRequest, TResponse>`
2. **Keyed Service Registration** 🔴 High - Multiple implementations with keys (.NET 8+)
3. **TryAdd* Registration** 🟡 Medium - Conditional registration for library scenarios

**Estimated effort**: 4-5 weeks
**Impact**: Unlock repository pattern, multi-tenant scenarios, plugin architectures

---

### Phase 2: Flexibility & Control (v1.2 - Q2 2025)

**Goal**: Factory methods and filtering

4. **Factory Method Registration** 🟡 Medium-High - Custom initialization logic
5. **Assembly Scanning Filters** 🟡 Medium - Exclude namespaces/patterns from transitive registration
6. **Multi-Interface Registration** 🟢 Low - Selective interface registration

**Estimated effort**: 3-4 weeks
**Impact**: Complex initialization scenarios, better control over transitive registration

---

### Phase 3: Advanced Scenarios (v1.3 - Q3 2025)

**Goal**: Validation and diagnostics

7. **Registration Validation Diagnostics** 🟡 Medium - Compile-time warnings for missing dependencies
8. **Conditional Registration** 🟢 Low-Medium - Feature flag-based registration

**Estimated effort**: 3-4 weeks
**Impact**: Catch DI mistakes at compile time, support feature toggles

---

### Phase 4: Enterprise Features (v2.0 - Q4 2025)

**Goal**: Advanced patterns (decorators, conventions)

9. **Decorator Pattern Support** 🟢 Low-Medium ⭐ - Cross-cutting concerns (logging, caching)
10. **Auto-Discovery by Convention** 🟢 Low-Medium - Optional convention-based registration

**Estimated effort**: 5-6 weeks
**Impact**: Complex enterprise patterns, reduce boilerplate further

---

### Feature Prioritization Matrix

| Feature | Priority | User Demand | Complexity | Phase |
|---------|----------|-------------|------------|-------|
| Generic Interface Registration | 🔴 Critical | ⭐⭐⭐ | High | 1.1 |
| Keyed Service Registration | 🔴 High | ⭐⭐⭐ | Medium | 1.1 |
| TryAdd* Registration | 🟡 Medium | ⭐⭐ | Low | 1.1 |
| Factory Method Registration | 🟡 Med-High | ⭐⭐ | Medium | 1.2 |
| Assembly Scanning Filters | 🟡 Medium | ⭐⭐ | Medium | 1.2 |
| Multi-Interface Registration | 🟢 Low | ⭐ | Low | 1.2 |
| Registration Validation | 🟡 Medium | ⭐⭐ | High | 1.3 |
| Conditional Registration | 🟢 Low-Med | ⭐ | Medium | 1.3 |
| Decorator Pattern | 🟢 Low-Med | ⭐⭐⭐ | Very High | 2.0 |
| Convention-Based Discovery | 🟢 Low-Med | ⭐⭐ | Medium | 2.0 |

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

**Last Updated**: 2025-01-17
**Version**: 1.0
**Research Date**: January 2025 (Scrutor v6.1.0)
**Maintained By**: Atc.SourceGenerators Team
