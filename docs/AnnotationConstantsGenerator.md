# 📝 Annotation Constants Source Generator

Automatically extract DataAnnotation metadata from class/record properties and generate compile-time accessible constants without reflection.

**Key Benefits:**

- 🎯 **Zero reflection** - Access annotation values at compile time
- ⚡ **Native AOT ready** - Works with trimming and ahead-of-time compilation
- 🛡️ **Type-safe constants** - Strongly-typed int, string, and bool values
- 🔍 **Discoverable** - Full IntelliSense support via nested class structure
- 🚀 **Zero configuration** - Works out of the box, no opt-in attribute needed

**Quick Example:**

```csharp
// Input: Class with DataAnnotation attributes
public class Product
{
    [Display(Name = "Product Name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Price")]
    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Price { get; set; }
}

// Generated: Compile-time constants
string label = AnnotationConstants.Product.Name.DisplayName;       // "Product Name"
int maxLength = AnnotationConstants.Product.Name.MaximumLength;    // 100
bool required = AnnotationConstants.Product.Name.IsRequired;       // true

string minPrice = AnnotationConstants.Product.Price.Minimum;       // "0.01"
string maxPrice = AnnotationConstants.Product.Price.Maximum;       // "999999.99"
```

## 📖 Documentation Navigation

- **[📋 Feature Roadmap](AnnotationConstantsGenerator-FeatureRoadmap.md)** - See all implemented and planned features
- **[🎯 Sample Projects](AnnotationConstantsGenerator-Samples.md)** - Working code examples

> **Note:** This generator supports both **Microsoft DataAnnotations** and **Atc attributes**. When the Atc package is referenced, additional validation attributes (IPAddress, Uri, String, KeyString, IsoCurrencySymbol) are also extracted.

## 📑 Table of Contents

- [📝 Annotation Constants Source Generator](#-annotation-constants-source-generator)
  - [📖 Documentation Navigation](#-documentation-navigation)
  - [📑 Table of Contents](#-table-of-contents)
  - [📖 Overview](#-overview)
    - [😫 Before (Reflection Approach)](#-before-reflection-approach)
    - [✨ After (With Source Generator)](#-after-with-source-generator)
  - [🚀 Quick Start](#-quick-start)
    - [1. Install the Package](#1-install-the-package)
    - [2. Add DataAnnotation Attributes](#2-add-dataannotation-attributes)
    - [3. Use Generated Constants](#3-use-generated-constants)
  - [✨ Features](#-features)
  - [📦 Supported Attributes](#-supported-attributes)
    - [Display Attributes](#display-attributes)
    - [Validation Attributes](#validation-attributes)
    - [Data Type Attributes](#data-type-attributes)
    - [Metadata Attributes](#metadata-attributes)
    - [Atc Attributes (from Atc Package)](#atc-attributes-from-atc-package)
  - [💡 Usage Examples](#-usage-examples)
    - [Blazor Form Labels](#blazor-form-labels)
    - [Client-Side Validation](#client-side-validation)
    - [API Documentation](#api-documentation)
  - [🔧 Configuration](#-configuration)
    - [Include Unannotated Properties](#include-unannotated-properties)
  - [🔧 How It Works](#-how-it-works)
    - [1. Class Scanning](#1-class-scanning)
    - [2. Attribute Extraction](#2-attribute-extraction)
    - [3. Code Generation](#3-code-generation)
  - [🛡️ Compile-Time Safety](#️-compile-time-safety)
  - [🎯 Use Cases](#-use-cases)

## 📖 Overview

### 😫 Before (Reflection Approach)

```csharp
// Using reflection to get annotation values - slow and not AOT-compatible 😫
var displayAttr = typeof(Product)
    .GetProperty(nameof(Product.Name))
    ?.GetCustomAttribute<DisplayAttribute>();

string? displayName = displayAttr?.Name;  // Runtime reflection call

var stringLengthAttr = typeof(Product)
    .GetProperty(nameof(Product.Name))
    ?.GetCustomAttribute<StringLengthAttribute>();

int? maxLength = stringLengthAttr?.MaximumLength;  // Another reflection call

// Problems:
// - Runtime overhead for every access
// - Not compatible with Native AOT
// - No compile-time validation
// - Verbose and repetitive code
```

### ✨ After (With Source Generator)

```csharp
// Using generated constants - zero reflection, AOT-compatible ✨
string displayName = AnnotationConstants.Product.Name.DisplayName;  // Compile-time constant
int maxLength = AnnotationConstants.Product.Name.MaximumLength;     // Compile-time constant

// Benefits:
// - Zero runtime overhead (compile-time constants)
// - Native AOT compatible
// - Full IntelliSense support
// - Type-safe (int for lengths, string for names)
```

## 🚀 Quick Start

### 1. Install the Package

```bash
dotnet add package Atc.SourceGenerators
```

Or in your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Atc.SourceGenerators" Version="1.0.0" />
</ItemGroup>
```

### 2. Add DataAnnotation Attributes

No special setup needed! Just use standard DataAnnotation attributes on your classes:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models;

public class Customer
{
    [Display(Name = "Customer Name", Description = "Full legal name")]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Age")]
    [Range(18, 120)]
    public int Age { get; set; }
}
```

### 3. Use Generated Constants

The generator automatically creates a nested static class structure:

```csharp
using MyApp.Models;

// Access constants without reflection
Console.WriteLine($"Field: {AnnotationConstants.Customer.Name.DisplayName}");         // "Customer Name"
Console.WriteLine($"Max: {AnnotationConstants.Customer.Name.MaximumLength}");          // 100
Console.WriteLine($"Min: {AnnotationConstants.Customer.Name.MinimumLength}");          // 2
Console.WriteLine($"Required: {AnnotationConstants.Customer.Name.IsRequired}");        // true

Console.WriteLine($"Email Required: {AnnotationConstants.Customer.Email.IsRequired}"); // true
Console.WriteLine($"Is Email: {AnnotationConstants.Customer.Email.IsEmailAddress}");   // true

Console.WriteLine($"Age Min: {AnnotationConstants.Customer.Age.Minimum}");             // "18"
Console.WriteLine($"Age Max: {AnnotationConstants.Customer.Age.Maximum}");             // "120"
```

## ✨ Features

- **🔍 Automatic Scanning** - No opt-in attribute needed; scans all classes with DataAnnotation attributes
- **📋 Display Attribute** - DisplayName, Description, ShortName, GroupName, Prompt, Order
- **✅ Validation Attributes** - Required, StringLength, Range, MinLength, MaxLength, RegularExpression
- **📧 Data Type Attributes** - EmailAddress, Phone, Url, CreditCard, DataType
- **🔑 Metadata Attributes** - Key, Editable, ScaffoldColumn, Timestamp, Compare
- **⚙️ Configurable** - Customize behavior via .editorconfig
- **📦 Multi-Assembly** - Works across project references
- **⚡ Zero Runtime Cost** - All constants generated at compile time
- **🚀 Native AOT Ready** - No reflection, fully trimming-safe

## 📦 Supported Attributes

### Display Attributes

| Attribute Property    | Generated Constant | Type     |
|-----------------------|--------------------|----------|
| `Display.Name`        | `DisplayName`      | `string` |
| `Display.Description` | `Description`      | `string` |
| `Display.ShortName`   | `ShortName`        | `string` |
| `Display.GroupName`   | `GroupName`        | `string` |
| `Display.Prompt`      | `Prompt`           | `string` |
| `Display.Order`       | `Order`            | `int`    |

### Validation Attributes

| Attribute | Generated Constants | Types |
|----------|-------------------|-------|
| `Required` | `IsRequired`, `AllowEmptyStrings`, `RequiredErrorMessage` | `bool`, `bool`, `string` |
| `StringLength` | `MinimumLength`, `MaximumLength`, `StringLengthErrorMessage` | `int`, `int`, `string` |
| `Range` | `Minimum`, `Maximum`, `OperandType`, `RangeErrorMessage` | `string`, `string`, `Type`, `string` |
| `MinLength` | `MinimumLength`, `MinLengthErrorMessage` | `int`, `string` |
| `MaxLength` | `MaximumLength`, `MaxLengthErrorMessage` | `int`, `string` |
| `RegularExpression` | `Pattern`, `RegularExpressionErrorMessage` | `string`, `string` |

### Data Type Attributes

| Attribute | Generated Constant | Type |
|----------|-------------------|------|
| `EmailAddress` | `IsEmailAddress` | `bool` |
| `Phone` | `IsPhone` | `bool` |
| `Url` | `IsUrl` | `bool` |
| `CreditCard` | `IsCreditCard` | `bool` |
| `DataType` | `DataType` | `int` (enum value) |

### Metadata Attributes

| Attribute | Generated Constant | Type |
|----------|-------------------|------|
| `Key` | `IsKey` | `bool` |
| `Editable` | `IsEditable` | `bool` |
| `ScaffoldColumn` | `IsScaffoldColumn` | `bool` |
| `Timestamp` | `IsTimestamp` | `bool` |
| `Compare` | `CompareProperty` | `string` |

### Atc Attributes (from Atc Package)

When your project references the [Atc](https://www.nuget.org/packages/Atc/) package, the generator also extracts constants from Atc-specific validation attributes:

| Attribute | Generated Constants | Types |
|----------|-------------------|-------|
| `IPAddressAttribute` | `IsIPAddress`, `IPAddressRequired` | `bool`, `bool` |
| `IsoCurrencySymbolAttribute` | `IsIsoCurrencySymbol`, `IsoCurrencySymbolRequired`, `AllowedIsoCurrencySymbols` | `bool`, `bool`, `string[]` |
| `StringAttribute` | `IsAtcString`, `AtcStringRequired`, `AtcStringMinLength`, `AtcStringMaxLength`, `AtcStringRegularExpression`, `AtcStringInvalidCharacters`, `AtcStringInvalidPrefixStrings` | `bool`, `bool`, `uint`, `uint`, `string`, `char[]`, `string[]` |
| `KeyStringAttribute` | `IsKeyString` (plus all StringAttribute constants) | `bool` |
| `UriAttribute` | `IsAtcUri`, `AtcUriRequired`, `AtcUriAllowHttp`, `AtcUriAllowHttps`, `AtcUriAllowFtp`, `AtcUriAllowFtps`, `AtcUriAllowFile`, `AtcUriAllowOpcTcp` | all `bool` |
| `IgnoreDisplayAttribute` | `IsIgnoreDisplay` | `bool` |
| `EnumGuidAttribute` | `EnumGuid` | `string` |
| `CasingStyleDescriptionAttribute` | `CasingStyleDefault`, `CasingStylePrefix` | `string`, `string` |

**Example with Atc attributes:**

```csharp
// Add Atc package reference
// <PackageReference Include="Atc" Version="2.*" />

public class NetworkConfig
{
    [Display(Name = "Server IP")]
    [IPAddress(Required = true)]
    public string ServerAddress { get; set; } = string.Empty;

    [Display(Name = "API Endpoint")]
    [UriAttribute(Required = true, AllowHttp = false, AllowHttps = true)]
    public string ApiEndpoint { get; set; } = string.Empty;

    [Display(Name = "Currency")]
    [IsoCurrencySymbol(IsoCurrencySymbols = new[] { "USD", "EUR", "GBP" })]
    public string Currency { get; set; } = "USD";
}

// Access Atc constants
bool isIPAddress = AnnotationConstants.NetworkConfig.ServerAddress.IsIPAddress;       // true
bool httpsOnly = AnnotationConstants.NetworkConfig.ApiEndpoint.AtcUriAllowHttps;       // true
string[] currencies = AnnotationConstants.NetworkConfig.Currency.AllowedIsoCurrencySymbols; // ["USD", "EUR", "GBP"]
```

## 💡 Usage Examples

### Blazor Form Labels

```razor
@* Use generated constants in Blazor forms *@
<EditForm Model="customer">
    <div class="form-group">
        <label>@AnnotationConstants.Customer.Name.DisplayName</label>
        <InputText @bind-Value="customer.Name"
                   maxlength="@AnnotationConstants.Customer.Name.MaximumLength" />
        <small>@AnnotationConstants.Customer.Name.Description</small>
    </div>
</EditForm>
```

### Client-Side Validation

```csharp
// Generate JavaScript validation rules from constants
var validationRules = new
{
    name = new
    {
        required = AnnotationConstants.Customer.Name.IsRequired,
        maxLength = AnnotationConstants.Customer.Name.MaximumLength,
        minLength = AnnotationConstants.Customer.Name.MinimumLength
    },
    email = new
    {
        required = AnnotationConstants.Customer.Email.IsRequired,
        isEmail = AnnotationConstants.Customer.Email.IsEmailAddress
    }
};

// Serialize and send to JavaScript
var json = JsonSerializer.Serialize(validationRules);
```

### API Documentation

```csharp
// Generate OpenAPI/Swagger documentation from constants
app.MapPost("/customers", (Customer customer) => { })
    .WithDescription($"""
        Creates a new customer.

        Name: {AnnotationConstants.Customer.Name.Description}
        - Required: {AnnotationConstants.Customer.Name.IsRequired}
        - Max Length: {AnnotationConstants.Customer.Name.MaximumLength}

        Email: {AnnotationConstants.Customer.Email.DisplayName}
        - Required: {AnnotationConstants.Customer.Email.IsRequired}
        """);
```

## 🔧 Configuration

The generator can be configured via `.editorconfig` or MSBuild properties.

### Include Unannotated Properties

By default, only properties with at least one DataAnnotation attribute are included. To include all public properties:

**Option 1: .editorconfig**

```ini
[*.cs]
atc_annotation_constants.include_unannotated_properties = true
```

**Option 2: MSBuild property**

```xml
<PropertyGroup>
  <AtcAnnotationConstantsIncludeUnannotatedProperties>true</AtcAnnotationConstantsIncludeUnannotatedProperties>
</PropertyGroup>
```

## 🔧 How It Works

### 1. Class Scanning

The generator scans all classes and records in your project that have properties with DataAnnotation attributes.

```csharp
// This class will be scanned (has [Display] on Name)
public class Product
{
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;  // Ignored by default
}

// This class will NOT be scanned (no DataAnnotation attributes)
public class InternalEntity
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
}
```

### 2. Attribute Extraction

For each property with DataAnnotation attributes, the generator extracts:

- Display metadata (name, description, etc.)
- Validation rules (required, length constraints, etc.)
- Data type hints (email, phone, etc.)
- Entity metadata (key, editable, etc.)

### 3. Code Generation

The generator creates a nested static partial class hierarchy:

```csharp
// Generated in: AnnotationConstants.MyNamespace.Product.g.cs

namespace MyNamespace;

[global::System.CodeDom.Compiler.GeneratedCode("Atc.SourceGenerators.AnnotationConstants", "1.0.0")]
[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
[global::System.Runtime.CompilerServices.CompilerGenerated]
[global::System.Diagnostics.DebuggerNonUserCode]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class AnnotationConstants
{
    public static partial class Product
    {
        public static partial class Name
        {
            public const string DisplayName = "Product Name";
            public const int MaximumLength = 100;
            public const bool IsRequired = true;
        }

        public static partial class Price
        {
            public const string DisplayName = "Price";
            public const string Minimum = "0.01";
            public const string Maximum = "999999.99";
            public static readonly System.Type OperandType = typeof(decimal);
        }
    }
}
```

## 🎯 Use Cases

1. **Blazor/MAUI Forms** - Generate form labels and validation rules without reflection
2. **API Documentation** - Extract constraint metadata for OpenAPI/Swagger
3. **Client-Side Validation** - Send validation rules to JavaScript/TypeScript
4. **Code Generation** - Use constants in T4 templates or other generators
5. **Testing** - Verify validation rules match expected values
6. **Native AOT Applications** - Access metadata without breaking trimming

---

**See Also:**

- [Feature Roadmap](AnnotationConstantsGenerator-FeatureRoadmap.md)
- [Sample Projects](AnnotationConstantsGenerator-Samples.md)
