# 📝 Feature Roadmap - Annotation Constants Generator

This document outlines the feature roadmap for the **AnnotationConstantsGenerator**, a source generator that extracts DataAnnotation metadata from class/record properties and generates compile-time accessible constants.

## 🔍 Research Sources

This roadmap is based on analysis of:

1. **System.ComponentModel.DataAnnotations** - [Official Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
   - Validation attributes (Required, StringLength, Range, etc.)
   - Display attributes (Display, DisplayFormat, DataType)
   - Metadata attributes (Key, Editable, ScaffoldColumn)

2. **Common Developer Pain Points**:
   - Runtime reflection overhead when accessing annotation metadata
   - Hard-to-discover attribute values (need to navigate to source files)
   - No compile-time constants for validation rules
   - Manual extraction of metadata for documentation or UI generation

3. **Use Cases**:
   - Client-side validation (send max length to JavaScript without reflection)
   - API documentation generation (extract display names and constraints)
   - Form generation (access validation rules at compile time)
   - Blazor/MAUI applications (avoid reflection for AOT compatibility)

### 📊 Key Insights

**What Developers Need**:

- **Zero-reflection access** - Get annotation values without runtime reflection
- **Compile-time constants** - Use values in switch expressions, attribute arguments
- **Type safety** - Strongly-typed constants (int for lengths, string for names)
- **Discoverability** - IntelliSense for annotation values
- **Native AOT compatibility** - Works with trimming and ahead-of-time compilation

**Common Scenarios**:

- Building form validation in Blazor/MAUI without reflection
- Generating API documentation from DataAnnotations
- Sending validation constraints to client-side JavaScript
- Creating compile-time validated configuration

---

## 📊 Current State

### ✅ AnnotationConstantsGenerator - Implemented Features

This generator is **fully implemented** and supports:

- **Automatic class scanning** - No opt-in attribute required, scans all classes with DataAnnotation attributes
- **Display attribute extraction** - DisplayName, Description, ShortName, GroupName, Prompt, Order
- **Validation attribute extraction** - Required, StringLength, Range, MinLength, MaxLength, RegularExpression
- **Data type attribute extraction** - EmailAddress, Phone, Url, CreditCard, DataType
- **Metadata attribute extraction** - Key, Editable, ScaffoldColumn, Timestamp, Compare
- **Atc attributes support** - IPAddress, Uri, String, KeyString, IsoCurrencySymbol, IgnoreDisplay, EnumGuid, CasingStyleDescription
- **Configuration via .editorconfig** - Include/exclude unannotated properties
- **Nested class structure** - AnnotationConstants.{TypeName}.{PropertyName}.{ConstantName}
- **Native AOT compatible** - Zero reflection, compile-time generation

---

## 📋 Feature Status Overview

| Status | Feature | Priority |
|:------:|---------|----------|
| ✅ | [Display Attribute Support](#1-display-attribute-support) | 🔴 High |
| ✅ | [Required Attribute Support](#2-required-attribute-support) | 🔴 High |
| ✅ | [StringLength Attribute Support](#3-stringlength-attribute-support) | 🔴 High |
| ✅ | [Range Attribute Support](#4-range-attribute-support) | 🔴 High |
| ✅ | [MinLength/MaxLength Attribute Support](#5-minlengthmaxlength-attribute-support) | 🟡 Medium |
| ✅ | [RegularExpression Attribute Support](#6-regularexpression-attribute-support) | 🟡 Medium |
| ✅ | [Data Type Attributes Support](#7-data-type-attributes-support) | 🟡 Medium |
| ✅ | [Metadata Attributes Support](#8-metadata-attributes-support) | 🟢 Low |
| ✅ | [Configuration via .editorconfig](#9-configuration-via-editorconfig) | 🟡 Medium |
| ✅ | [Atc Attributes Support](#10-atc-attributes-support) | 🟡 Medium |
| ❌ | [Multi-Assembly Support](#11-multi-assembly-support) | 🟢 Low |

**Legend:**

- ✅ **Implemented** - Feature is complete and available
- ❌ **Not Implemented** - Feature is planned but not yet developed
- 🚫 **Not Planned** - Feature is out of scope or not aligned with project goals

---

## 🎯 Need to Have (High Priority)

These features address the core functionality of extracting annotation metadata.

### 1. Display Attribute Support

**Priority**: 🔴 **High**
**Status**: ✅ **Implemented**
**Inspiration**: System.ComponentModel.DataAnnotations.DisplayAttribute

**Description**: Extract Display attribute properties and generate constants.

**User Story**:
> "As a developer, I want to access DisplayAttribute.Name values at compile time for form labels without using reflection."

**Example**:

```csharp
// Input
public class Product
{
    [Display(Name = "Product Name", Description = "The name of the product", ShortName = "Name", Order = 1)]
    public string Name { get; set; } = string.Empty;
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class Product
    {
        public static partial class Name
        {
            public const string DisplayName = "Product Name";
            public const string Description = "The name of the product";
            public const string ShortName = "Name";
            public const int Order = 1;
        }
    }
}

// Usage
var label = AnnotationConstants.Product.Name.DisplayName; // "Product Name"
```

**Generated Constants**:

| Constant | Type | Source |
|----------|------|--------|
| DisplayName | string | Display.Name |
| Description | string | Display.Description |
| ShortName | string | Display.ShortName |
| GroupName | string | Display.GroupName |
| Prompt | string | Display.Prompt |
| Order | int | Display.Order |

---

### 2. Required Attribute Support

**Priority**: 🔴 **High**
**Status**: ✅ **Implemented**
**Inspiration**: System.ComponentModel.DataAnnotations.RequiredAttribute

**Description**: Extract Required attribute and generate IsRequired constant.

**Example**:

```csharp
// Input
public class Customer
{
    [Required(ErrorMessage = "Name is required", AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class Customer
    {
        public static partial class Name
        {
            public const bool IsRequired = true;
            public const bool AllowEmptyStrings = false;
            public const string RequiredErrorMessage = "Name is required";
        }
    }
}
```

---

### 3. StringLength Attribute Support

**Priority**: 🔴 **High**
**Status**: ✅ **Implemented**
**Inspiration**: System.ComponentModel.DataAnnotations.StringLengthAttribute

**Description**: Extract StringLength min/max values as constants.

**Example**:

```csharp
// Input
public class Product
{
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3-100 characters")]
    public string Name { get; set; } = string.Empty;
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class Product
    {
        public static partial class Name
        {
            public const int MinimumLength = 3;
            public const int MaximumLength = 100;
            public const string StringLengthErrorMessage = "Name must be 3-100 characters";
        }
    }
}

// Usage - send to client-side JavaScript
const maxLength = @AnnotationConstants.Product.Name.MaximumLength;  // 100
```

---

### 4. Range Attribute Support

**Priority**: 🔴 **High**
**Status**: ✅ **Implemented**
**Inspiration**: System.ComponentModel.DataAnnotations.RangeAttribute

**Description**: Extract Range min/max values. Since Range supports different types (int, double, decimal), values are stored as strings with an OperandType property.

**Example**:

```csharp
// Input
public class Product
{
    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    public decimal Price { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class Product
    {
        public static partial class Price
        {
            public const string Minimum = "0.01";
            public const string Maximum = "999999.99";
            public static readonly System.Type OperandType = typeof(decimal);
            public const string RangeErrorMessage = "Price must be between $0.01 and $999,999.99";
        }

        public static partial class Quantity
        {
            public const string Minimum = "1";
            public const string Maximum = "100";
            public static readonly System.Type OperandType = typeof(int);
        }
    }
}

// Usage - parse to appropriate type
decimal minPrice = decimal.Parse(AnnotationConstants.Product.Price.Minimum);  // 0.01m
```

**Design Decision**: Range values are stored as strings because:

- Range attribute accepts `typeof(int)`, `typeof(double)`, `typeof(decimal)`, etc.
- No single numeric type can represent all possibilities
- Consumer can parse using the provided `OperandType`
- `OperandType` is `static readonly` (not `const`) because `Type` cannot be a constant

---

## 💡 Nice to Have (Medium Priority)

### 5. MinLength/MaxLength Attribute Support

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented**

**Description**: Extract MinLength and MaxLength attribute values.

**Example**:

```csharp
// Input
public class Product
{
    [MinLength(3)]
    [MaxLength(100)]
    public string[] Tags { get; set; } = [];
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class Product
    {
        public static partial class Tags
        {
            public const int MinimumLength = 3;
            public const int MaximumLength = 100;
        }
    }
}
```

---

### 6. RegularExpression Attribute Support

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented**

**Description**: Extract regex pattern from RegularExpression attribute.

**Example**:

```csharp
// Input
public class User
{
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class User
    {
        public static partial class Email
        {
            public const string Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            public const string RegularExpressionErrorMessage = "Invalid email";
        }
    }
}
```

---

### 7. Data Type Attributes Support

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented**

**Description**: Extract data type information from EmailAddress, Phone, Url, CreditCard, and DataType attributes.

**Example**:

```csharp
// Input
public class Contact
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Url]
    public string Website { get; set; } = string.Empty;

    [CreditCard]
    public string CardNumber { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class Contact
    {
        public static partial class Email
        {
            public const bool IsEmailAddress = true;
        }

        public static partial class PhoneNumber
        {
            public const bool IsPhone = true;
        }

        public static partial class Website
        {
            public const bool IsUrl = true;
        }

        public static partial class CardNumber
        {
            public const bool IsCreditCard = true;
        }

        public static partial class Password
        {
            public const int DataType = 11; // DataType.Password enum value
        }
    }
}
```

---

### 8. Metadata Attributes Support

**Priority**: 🟢 **Low**
**Status**: ✅ **Implemented**

**Description**: Extract metadata from Key, Editable, ScaffoldColumn, Timestamp, and Compare attributes.

**Example**:

```csharp
// Input
public class Entity
{
    [Key]
    public Guid Id { get; set; }

    [Editable(false)]
    public DateTimeOffset CreatedAt { get; set; }

    [ScaffoldColumn(false)]
    public string InternalData { get; set; } = string.Empty;

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    [Compare(nameof(Email))]
    public string ConfirmEmail { get; set; } = string.Empty;
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class Entity
    {
        public static partial class Id
        {
            public const bool IsKey = true;
        }

        public static partial class CreatedAt
        {
            public const bool IsEditable = false;
        }

        public static partial class InternalData
        {
            public const bool IsScaffoldColumn = false;
        }

        public static partial class RowVersion
        {
            public const bool IsTimestamp = true;
        }

        public static partial class ConfirmEmail
        {
            public const string CompareProperty = "Email";
        }
    }
}
```

---

### 9. Configuration via .editorconfig

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented**

**Description**: Allow customization of generator behavior via .editorconfig or MSBuild properties.

**Configuration Options**:

```ini
# .editorconfig
[*.cs]
# Include properties without any DataAnnotation attributes (default: false)
atc_annotation_constants.include_unannotated_properties = true

# Namespace for generated constants (default: same as source type)
atc_annotation_constants.namespace = MyApp.Generated

# Generate error messages (default: true)
atc_annotation_constants.include_error_messages = false
```

**MSBuild Properties**:

```xml
<PropertyGroup>
  <AtcAnnotationConstantsIncludeUnannotatedProperties>true</AtcAnnotationConstantsIncludeUnannotatedProperties>
</PropertyGroup>
```

---

### 10. Atc Attributes Support

**Priority**: 🟡 **Medium**
**Status**: ✅ **Implemented**

**Description**: Extract validation attributes from the Atc package when referenced. These include IPAddressAttribute, UriAttribute, StringAttribute, KeyStringAttribute, IsoCurrencySymbolAttribute, and metadata attributes like IgnoreDisplayAttribute, EnumGuidAttribute, and CasingStyleDescriptionAttribute.

**Example**:

```csharp
// Add Atc package reference: <PackageReference Include="Atc" Version="2.*" />

public class NetworkConfig
{
    [Display(Name = "Server IP")]
    [IPAddress(Required = true)]
    public string ServerAddress { get; set; } = string.Empty;

    [Display(Name = "API Endpoint")]
    [UriAttribute(Required = true, AllowHttp = false, AllowHttps = true, AllowFtp = false, AllowFtps = false, AllowFile = false, AllowOpcTcp = false)]
    public string ApiEndpoint { get; set; } = string.Empty;

    [Display(Name = "Currency")]
    [IsoCurrencySymbol(IsoCurrencySymbols = new[] { "USD", "EUR", "GBP" })]
    public string Currency { get; set; } = "USD";

    [Display(Name = "Service Key")]
    [KeyString]
    public string ServiceKey { get; set; } = string.Empty;
}

// Generated
public static partial class AnnotationConstants
{
    public static partial class NetworkConfig
    {
        public static partial class ServerAddress
        {
            public const string DisplayName = "Server IP";
            public const bool IsIPAddress = true;
            public const bool IPAddressRequired = true;
        }

        public static partial class ApiEndpoint
        {
            public const string DisplayName = "API Endpoint";
            public const bool IsAtcUri = true;
            public const bool AtcUriRequired = true;
            public const bool AtcUriAllowHttp = false;
            public const bool AtcUriAllowHttps = true;
            // ... other URI scheme flags
        }

        public static partial class Currency
        {
            public const string DisplayName = "Currency";
            public const bool IsIsoCurrencySymbol = true;
            public static readonly string[] AllowedIsoCurrencySymbols = new[] { "USD", "EUR", "GBP" };
        }

        public static partial class ServiceKey
        {
            public const string DisplayName = "Service Key";
            public const bool IsAtcString = true;
            public const bool IsKeyString = true;
        }
    }
}
```

**Supported Atc Attributes**:

| Attribute | Generated Constants |
|----------|-------------------|
| `IPAddressAttribute` | `IsIPAddress`, `IPAddressRequired` |
| `IsoCurrencySymbolAttribute` | `IsIsoCurrencySymbol`, `IsoCurrencySymbolRequired`, `AllowedIsoCurrencySymbols` |
| `StringAttribute` | `IsAtcString`, `AtcStringRequired`, `AtcStringMinLength`, `AtcStringMaxLength`, `AtcStringRegularExpression`, `AtcStringInvalidCharacters`, `AtcStringInvalidPrefixStrings` |
| `KeyStringAttribute` | `IsKeyString` (plus all StringAttribute constants) |
| `UriAttribute` | `IsAtcUri`, `AtcUriRequired`, `AtcUriAllowHttp`, `AtcUriAllowHttps`, `AtcUriAllowFtp`, `AtcUriAllowFtps`, `AtcUriAllowFile`, `AtcUriAllowOpcTcp` |
| `IgnoreDisplayAttribute` | `IsIgnoreDisplay` |
| `EnumGuidAttribute` | `EnumGuid` |
| `CasingStyleDescriptionAttribute` | `CasingStyleDefault`, `CasingStylePrefix` |

---

### 11. Multi-Assembly Support

**Priority**: 🟢 **Low**
**Status**: ❌ **Not Implemented**

**Description**: Generate constants from types across multiple assemblies with smart naming to avoid conflicts.

**Example**:

```csharp
// If "Contracts" suffix is unique:
// MyApp.Contracts.Product → AnnotationConstants.Product

// If multiple assemblies have same type name:
// MyApp.Contracts.Product → AnnotationConstantsFromContracts.Product
// OtherApp.Contracts.Product → AnnotationConstantsFromOtherAppContracts.Product
```

---

## ⛔ Not Planned (Out of Scope)

### Runtime Validation Support

**Reason**: This generator focuses on compile-time constant extraction. Validation is handled by DataAnnotations at runtime.

**Status**: 🚫 Not Planned

---

### Custom Attribute Support

**Reason**: Focus on standard System.ComponentModel.DataAnnotations attributes. Custom attributes can be added in future versions based on demand.

**Status**: 🚫 Not Planned (v1.0)

---

### Localization Support

**Reason**: Display.ResourceType for localized strings requires runtime access. Constants are compile-time only.

**Status**: 🚫 Not Planned

---

## 📅 Implementation Status

All planned features for v1.0 have been implemented:

### Phase 1: Core Extraction ✅ Complete

**Goal**: Extract essential validation metadata

1. ✅ **Display Attribute Support** - DisplayName, Description, ShortName, GroupName, Prompt, Order
2. ✅ **Required Attribute Support** - IsRequired flag, AllowEmptyStrings, ErrorMessage
3. ✅ **StringLength Attribute Support** - Min/Max length with ErrorMessage
4. ✅ **Range Attribute Support** - Min/Max values with OperandType and ErrorMessage

---

### Phase 2: Extended Validation ✅ Complete

**Goal**: Additional validation attribute support

1. ✅ **MinLength/MaxLength Attribute Support** - Separate length attributes
2. ✅ **RegularExpression Attribute Support** - Pattern with ErrorMessage
3. ✅ **Data Type Attributes Support** - EmailAddress, Phone, Url, CreditCard, DataType

---

### Phase 3: Configuration & Metadata ✅ Complete

**Goal**: Customization and metadata support

1. ✅ **Configuration via .editorconfig** - Include unannotated properties option
2. ✅ **Metadata Attributes Support** - Key, Editable, ScaffoldColumn, Timestamp, Compare

---

### Phase 4: Atc Package Integration ✅ Complete

**Goal**: Support Atc-specific validation attributes

1. ✅ **Atc Attributes Support** - IPAddress, Uri, String, KeyString, IsoCurrencySymbol, IgnoreDisplay, EnumGuid, CasingStyleDescription

---

### Future: Multi-Assembly Support 🔮 Planned

1. ❌ **Multi-Assembly Support** - Smart naming for types across assemblies (not yet implemented)

---

## 🎯 Success Metrics

1. **Adoption** - Track NuGet downloads and GitHub stars
2. **Performance** - Measure compile-time impact (should be minimal)
3. **Coverage** - % of DataAnnotation attributes supported
4. **Community Feedback** - Issue tracker activity and feature requests
5. **AOT Compatibility** - Verify zero reflection in generated code

---

## 📝 Notes

### Design Philosophy

- **Compile-time constants** - All values are compile-time constants where possible
- **Type safety** - Use appropriate types (int for lengths, string for names)
- **Discoverability** - Nested class structure mirrors source type structure
- **No reflection** - Generated code uses no reflection APIs
- **AOT compatible** - Works with Native AOT and IL trimming

### Key Design Decisions

1. **Range values as strings**: Since Range attribute accepts different numeric types, min/max are stored as strings with a Type property
2. **No opt-in attribute**: Generator scans all types automatically (unlike other generators in this project)
3. **Partial classes**: Generated classes are partial to allow manual extensions
4. **Same namespace**: Constants are generated in the same namespace as the source type

---

## 🔗 Related Resources

- **System.ComponentModel.DataAnnotations**: <https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations>
- **Blazor Form Validation**: <https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation>
- **Sample Projects**: See `/sample/Atc.SourceGenerators.AnnotationConstants`

---

**Last Updated**: 2025-12-19
**Version**: 1.0 (Implemented)
**Maintained By**: Atc.SourceGenerators Team
