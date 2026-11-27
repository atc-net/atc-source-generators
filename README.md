# 🎯 Atc Source Generators

A collection of Roslyn C# source generators for .NET that eliminate boilerplate code and improve developer productivity. The generators are designed with **Native AOT compatibility** in focus, enabling faster startup times, smaller deployment sizes, and optimal performance for modern cloud-native applications (see [Native AOT and trimming](#-native-aot-and-trimming) for the details per generator).

**Why Choose Atc Source Generators?**

- 🎯 **Zero boilerplate** - Attribute-based approach eliminates repetitive code
- ⚡ **Compile-time generation** - Catch errors during build, not at runtime
- 🚀 **Native AOT friendly** - Mapping, enum mapping, annotation constants and DI registration generate plain code without reflection
- 🧩 **Multi-project architecture** - Smart naming for clean layered applications
- 🛡️ **Type-safe** - Full IntelliSense and compile-time validation
- 📦 **Single package** - Install once, use all generators

## 🚀 Source Generators

| Generator | Attribute | What It Does |
|-----------|-----------|-------------|
| **⚡ DependencyRegistration** | `[Registration]` | Automatic DI service registration with auto-detection, keyed services, factories, decorators |
| **⚙️ OptionsBinding** | `[OptionsBinding]` | Configuration binding with validation, change callbacks, named options |
| **🗺️ ObjectMapping** | `[MapTo(typeof(T))]` | Object-to-object mapping with bidirectional, nested, and collection support |
| **🔄 EnumMapping** | `[MapTo(typeof(T))]` | Enum-to-enum mapping with special case detection (None ↔ Unknown) |
| **📋 AnnotationConstants** | *(automatic)* | Compile-time access to DataAnnotation metadata without reflection |

## ✨ See It In Action

All generators work together seamlessly in a typical 3-layer architecture:

```csharp
// 1️⃣ Domain Layer - Your business logic
[MapTo(typeof(PetStatusDto), Bidirectional = true)]
public enum PetStatus { Available, Adopted }

[MapTo(typeof(PetDto))]
public partial class Pet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetStatus Status { get; set; }
}

[Registration(Lifetime.Scoped)]
public class PetService : IPetService
{
    public async Task<Pet> GetPetAsync(Guid id) { /* ... */ }
}

[OptionsBinding("PetStore")]
public partial class PetStoreOptions
{
    [Required] public int MaxPetsPerPage { get; set; }
}

// 2️⃣ Program.cs - One line per concern
using Atc.DependencyInjection;
using Atc.Mapping;

builder.Services.AddDependencyRegistrationsFromDomain();
builder.Services.AddOptionsFromDomain(builder.Configuration);

// 3️⃣ Usage - Clean and type-safe
app.MapGet("/pets/{id}", async (Guid id, IPetService service) =>
{
    var pet = await service.GetPetAsync(id);
    return Results.Ok(pet.MapToPetDto());  // ✨ Generated mapping
});
```

**Result:** Zero boilerplate, full type safety, Native AOT ready! 🚀

## 📦 Installation

```bash
dotnet add package Atc.SourceGenerators
```

That's the only package you need: the attributes (`[Registration]`, `[OptionsBinding]`, `[MapTo]`, `[MappingConfiguration]`, …) are generated into your project by the package itself.

When several of your projects use the generators and reference each other, each project gets its own copy of the attributes. The compiler then reports warning CS0436, which is expected and can be suppressed:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);CS0436</NoWarn>
</PropertyGroup>
```

## 🔎 Strict mapping

Mapping generators report target properties that nothing maps to, so they don't silently keep their default value:
`ATCMAP014` for `[MapTo]` and `ATCMCF016` for `[MappingConfiguration]`. They are **Info** by default. To make
unmapped target properties a build error, raise their severity in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.ATCMAP014.severity = error
dotnet_diagnostic.ATCMCF016.severity = error
```

For `[MapTo]`, exclude a single target property with `[MapIgnore]`.

## 🚀 Native AOT and trimming

| Generator | Generated code | AOT / trimming |
|-----------|----------------|----------------|
| ObjectMapping, MappingConfiguration, EnumMapping | Plain assignments, constructor calls and switch expressions | ✅ No reflection |
| AnnotationConstants | Constants | ✅ No reflection |
| DependencyRegistration | Direct `services.AddXxx<TService, TImplementation>()` calls | ✅ No reflection, except decorators, which use `ActivatorUtilities.CreateInstance` |
| OptionsBinding | `AddOptions<T>().Bind(...)`, and `ValidateDataAnnotations()` when enabled | ⚠️ Uses the reflection-based configuration binder and DataAnnotations validation, so Native AOT and trimming report warnings for it |

For reflection-free options validation, let Microsoft's options validation source generator implement the validator and register it with `Validator`, instead of `ValidateDataAnnotations`:

```csharp
[OptionsValidator]
public partial class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
}

[OptionsBinding("Database", Validator = typeof(DatabaseOptionsValidator), ValidateOnStart = true)]
public partial class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
```

## ⚙️ Requirements

- **.NET 10 SDK** required at build time (Roslyn 5.0.0)
- Projects can still **target** .NET 9, .NET 8, or earlier
- This is a build-time requirement only, not a runtime requirement

## 📖 Documentation

Full documentation is available in the **[Wiki](https://github.com/atc-net/atc-source-generators/wiki)**:

| Page | Description |
|------|-------------|
| [Getting Started](https://github.com/atc-net/atc-source-generators/wiki/Getting-Started) | Installation, packages, and SDK requirements |
| [Dependency Registration](https://github.com/atc-net/atc-source-generators/wiki/Working-with-Dependency-Registration) | Automatic DI with `[Registration]` — auto-detection, keyed services, factories, decorators, transitive registration |
| [Options Binding](https://github.com/atc-net/atc-source-generators/wiki/Working-with-Options-Binding) | Configuration binding with `[OptionsBinding]` — validation, change callbacks, named options, child sections |
| [Object Mapping](https://github.com/atc-net/atc-source-generators/wiki/Working-with-Object-Mapping) | Object mapping with `[MapTo]` — bidirectional, nested objects, collections, projections, property strategies |
| [Enum Mapping](https://github.com/atc-net/atc-source-generators/wiki/Working-with-Enum-Mapping) | Enum mapping with `[MapTo]` — special case detection, bidirectional, case-insensitive |
| [Annotation Constants](https://github.com/atc-net/atc-source-generators/wiki/Working-with-Annotation-Constants) | Compile-time DataAnnotation metadata — zero reflection, Blazor-ready, Native AOT |
| [Sample Projects](https://github.com/atc-net/atc-source-generators/wiki/Sample-Projects) | Working code examples for each generator |
| [PetStore API Example](https://github.com/atc-net/atc-source-generators/wiki/PetStore-API-Example) | Full application using all generators with OpenAPI/Scalar |

## 🔨 Building

```bash
dotnet build Atc.SourceGenerators.slnx -c Release
```

## 🧪 Testing

```bash
dotnet test --solution Atc.SourceGenerators.slnx -c Release
```

## 🤝 Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for how to build, test and write commit messages.

## 📄 License

[MIT](LICENSE)
