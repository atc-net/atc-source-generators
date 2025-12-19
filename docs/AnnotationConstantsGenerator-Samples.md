# 🎯 Annotation Constants Generator - Sample Projects

This document provides working examples demonstrating the **AnnotationConstantsGenerator** in realistic scenarios.

## 📁 Sample Project Location

```
sample/
└── Atc.SourceGenerators.AnnotationConstants/
    ├── Atc.SourceGenerators.AnnotationConstants.csproj
    ├── Models/
    │   ├── Product.cs
    │   └── Customer.cs
    └── Program.cs
```

## 🚀 Running the Sample

```bash
cd sample/Atc.SourceGenerators.AnnotationConstants
dotnet run
```

**Expected Output:**

```bash
=== Annotation Constants Generator Demo ===

Product.Name:
  DisplayName: Product Name
  MaximumLength: 100
  MinimumLength: 3
  IsRequired: true

Product.Price:
  DisplayName: Price
  Minimum: 0.01
  Maximum: 999999.99
  OperandType: System.Decimal
  IsRequired: true

Product.Sku:
  DisplayName: SKU
  MaximumLength: 20
  Pattern: ^[A-Z]{3}-\d{4}$
  IsRequired: false

Customer.Email:
  DisplayName: Email Address
  IsRequired: true
  IsEmailAddress: true

Customer.Phone:
  DisplayName: Phone Number
  IsPhone: true
```

---

## 📋 Sample Code

### Models/Product.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace Atc.SourceGenerators.Sample.Models;

/// <summary>
/// Demonstrates DataAnnotation extraction for a product entity.
/// </summary>
public class Product
{
    /// <summary>
    /// Product name with display metadata and validation constraints.
    /// </summary>
    [Display(Name = "Product Name", Description = "The display name of the product")]
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3-100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price with Range validation using decimal type.
    /// </summary>
    [Display(Name = "Price", Description = "The retail price of the product")]
    [Required]
    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    public decimal Price { get; set; }

    /// <summary>
    /// Stock Keeping Unit with regex pattern validation.
    /// </summary>
    [Display(Name = "SKU", ShortName = "SKU", Description = "Stock Keeping Unit identifier")]
    [StringLength(20)]
    [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "SKU must be in format AAA-0000")]
    public string? Sku { get; set; }

    /// <summary>
    /// Product quantity with integer Range validation.
    /// </summary>
    [Display(Name = "Quantity in Stock")]
    [Range(0, 10000)]
    public int Quantity { get; set; }

    /// <summary>
    /// Product category with display grouping.
    /// </summary>
    [Display(Name = "Category", GroupName = "Classification")]
    [Required]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Internal notes - not editable via forms.
    /// </summary>
    [Display(Name = "Internal Notes")]
    [Editable(false)]
    [ScaffoldColumn(false)]
    public string? InternalNotes { get; set; }
}
```

### Models/Customer.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace Atc.SourceGenerators.Sample.Models;

/// <summary>
/// Demonstrates DataAnnotation extraction for a customer entity.
/// </summary>
public class Customer
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Customer's full name.
    /// </summary>
    [Display(Name = "Full Name", Prompt = "Enter customer's full name")]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address with specialized validation.
    /// </summary>
    [Display(Name = "Email Address", Description = "Primary contact email")]
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Email confirmation for registration.
    /// </summary>
    [Display(Name = "Confirm Email")]
    [Compare(nameof(Email), ErrorMessage = "Emails must match")]
    public string ConfirmEmail { get; set; } = string.Empty;

    /// <summary>
    /// Phone number with specialized validation.
    /// </summary>
    [Display(Name = "Phone Number")]
    [Phone]
    public string? Phone { get; set; }

    /// <summary>
    /// Customer website URL.
    /// </summary>
    [Display(Name = "Website")]
    [Url]
    public string? Website { get; set; }

    /// <summary>
    /// Credit card for payment.
    /// </summary>
    [Display(Name = "Credit Card")]
    [CreditCard]
    public string? CreditCard { get; set; }

    /// <summary>
    /// Customer age with range validation.
    /// </summary>
    [Display(Name = "Age")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int? Age { get; set; }

    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    [Display(Name = "Created At")]
    [Editable(false)]
    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optimistic concurrency token.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
```

### Program.cs

```csharp
using Atc.SourceGenerators.Sample.Models;

Console.WriteLine("=== Annotation Constants Generator Demo ===");
Console.WriteLine();

// ===== Product Annotations =====
Console.WriteLine("Product.Name:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.Name.DisplayName}");
Console.WriteLine($"  Description: {AnnotationConstants.Product.Name.Description}");
Console.WriteLine($"  MaximumLength: {AnnotationConstants.Product.Name.MaximumLength}");
Console.WriteLine($"  MinimumLength: {AnnotationConstants.Product.Name.MinimumLength}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Product.Name.IsRequired}");
Console.WriteLine($"  ErrorMessage: {AnnotationConstants.Product.Name.StringLengthErrorMessage}");
Console.WriteLine();

Console.WriteLine("Product.Price:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.Price.DisplayName}");
Console.WriteLine($"  Minimum: {AnnotationConstants.Product.Price.Minimum}");
Console.WriteLine($"  Maximum: {AnnotationConstants.Product.Price.Maximum}");
Console.WriteLine($"  OperandType: {AnnotationConstants.Product.Price.OperandType}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Product.Price.IsRequired}");
Console.WriteLine();

Console.WriteLine("Product.Sku:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.Sku.DisplayName}");
Console.WriteLine($"  ShortName: {AnnotationConstants.Product.Sku.ShortName}");
Console.WriteLine($"  MaximumLength: {AnnotationConstants.Product.Sku.MaximumLength}");
Console.WriteLine($"  Pattern: {AnnotationConstants.Product.Sku.Pattern}");
Console.WriteLine();

Console.WriteLine("Product.Quantity:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.Quantity.DisplayName}");
Console.WriteLine($"  Minimum: {AnnotationConstants.Product.Quantity.Minimum}");
Console.WriteLine($"  Maximum: {AnnotationConstants.Product.Quantity.Maximum}");
Console.WriteLine();

Console.WriteLine("Product.InternalNotes:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Product.InternalNotes.DisplayName}");
Console.WriteLine($"  IsEditable: {AnnotationConstants.Product.InternalNotes.IsEditable}");
Console.WriteLine($"  IsScaffoldColumn: {AnnotationConstants.Product.InternalNotes.IsScaffoldColumn}");
Console.WriteLine();

// ===== Customer Annotations =====
Console.WriteLine("Customer.Id:");
Console.WriteLine($"  IsKey: {AnnotationConstants.Customer.Id.IsKey}");
Console.WriteLine();

Console.WriteLine("Customer.Name:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Customer.Name.DisplayName}");
Console.WriteLine($"  Prompt: {AnnotationConstants.Customer.Name.Prompt}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Customer.Name.IsRequired}");
Console.WriteLine();

Console.WriteLine("Customer.Email:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Customer.Email.DisplayName}");
Console.WriteLine($"  IsRequired: {AnnotationConstants.Customer.Email.IsRequired}");
Console.WriteLine($"  IsEmailAddress: {AnnotationConstants.Customer.Email.IsEmailAddress}");
Console.WriteLine();

Console.WriteLine("Customer.ConfirmEmail:");
Console.WriteLine($"  CompareProperty: {AnnotationConstants.Customer.ConfirmEmail.CompareProperty}");
Console.WriteLine();

Console.WriteLine("Customer.Phone:");
Console.WriteLine($"  DisplayName: {AnnotationConstants.Customer.Phone.DisplayName}");
Console.WriteLine($"  IsPhone: {AnnotationConstants.Customer.Phone.IsPhone}");
Console.WriteLine();

Console.WriteLine("Customer.Website:");
Console.WriteLine($"  IsUrl: {AnnotationConstants.Customer.Website.IsUrl}");
Console.WriteLine();

Console.WriteLine("Customer.CreditCard:");
Console.WriteLine($"  IsCreditCard: {AnnotationConstants.Customer.CreditCard.IsCreditCard}");
Console.WriteLine();

Console.WriteLine("Customer.Age:");
Console.WriteLine($"  Minimum: {AnnotationConstants.Customer.Age.Minimum}");
Console.WriteLine($"  Maximum: {AnnotationConstants.Customer.Age.Maximum}");
Console.WriteLine($"  RangeErrorMessage: {AnnotationConstants.Customer.Age.RangeErrorMessage}");
Console.WriteLine();

Console.WriteLine("Customer.CreatedAt:");
Console.WriteLine($"  IsEditable: {AnnotationConstants.Customer.CreatedAt.IsEditable}");
Console.WriteLine($"  DataType: {AnnotationConstants.Customer.CreatedAt.DataType}");
Console.WriteLine();

Console.WriteLine("Customer.RowVersion:");
Console.WriteLine($"  IsTimestamp: {AnnotationConstants.Customer.RowVersion.IsTimestamp}");
Console.WriteLine();

// ===== Practical Usage Examples =====
Console.WriteLine("=== Practical Usage Examples ===");
Console.WriteLine();

// Example 1: Client-side validation rules
Console.WriteLine("1. Generate JavaScript Validation Rules:");
Console.WriteLine($$"""
   const productValidation = {
       name: {
           required: {{AnnotationConstants.Product.Name.IsRequired.ToString().ToLowerInvariant()}},
           maxLength: {{AnnotationConstants.Product.Name.MaximumLength}},
           minLength: {{AnnotationConstants.Product.Name.MinimumLength}}
       },
       price: {
           required: {{AnnotationConstants.Product.Price.IsRequired.ToString().ToLowerInvariant()}},
           min: {{AnnotationConstants.Product.Price.Minimum}},
           max: {{AnnotationConstants.Product.Price.Maximum}}
       },
       sku: {
           pattern: '{{AnnotationConstants.Product.Sku.Pattern}}'
       }
   };
   """);
Console.WriteLine();

// Example 2: Form field generation
Console.WriteLine("2. Generate Form Fields:");
GenerateFormField("Name", AnnotationConstants.Product.Name.DisplayName,
    AnnotationConstants.Product.Name.Description,
    AnnotationConstants.Product.Name.MaximumLength,
    AnnotationConstants.Product.Name.IsRequired);
Console.WriteLine();

// Example 3: API documentation
Console.WriteLine("3. Generate API Documentation:");
Console.WriteLine($$"""
   ## POST /api/products

   | Field | Description | Required | Constraints |
   |-------|-------------|----------|-------------|
   | name | {{AnnotationConstants.Product.Name.Description}} | {{AnnotationConstants.Product.Name.IsRequired}} | {{AnnotationConstants.Product.Name.MinimumLength}}-{{AnnotationConstants.Product.Name.MaximumLength}} chars |
   | price | {{AnnotationConstants.Product.Price.Description}} | {{AnnotationConstants.Product.Price.IsRequired}} | ${{AnnotationConstants.Product.Price.Minimum}}-${{AnnotationConstants.Product.Price.Maximum}} |
   | sku | {{AnnotationConstants.Product.Sku.Description}} | No | Pattern: {{AnnotationConstants.Product.Sku.Pattern}} |
   """);

static void GenerateFormField(string fieldName, string displayName, string? description, int maxLength, bool required)
{
    var requiredAttr = required ? " required" : "";
    Console.WriteLine($$"""
       <div class="form-group">
           <label for="{{fieldName}}">{{displayName}}{{(required ? " *" : "")}}</label>
           <input type="text" id="{{fieldName}}" name="{{fieldName}}"
                  maxlength="{{maxLength}}"{{requiredAttr}} />
           {{(description != null ? $"<small>{description}</small>" : "")}}
       </div>
       """);
}
```

---

## 🏗️ Project Structure

### Generated Code Structure

When you build the sample project, the generator creates:

```bash
obj/
└── Debug/
    └── net10.0/
        └── generated/
            └── Atc.SourceGenerators/
                └── Atc.SourceGenerators.AnnotationConstantsGenerator/
                    ├── AnnotationConstants.Atc.SourceGenerators.Sample.Models.Product.g.cs
                    └── AnnotationConstants.Atc.SourceGenerators.Sample.Models.Customer.g.cs
```

### Generated File Example

**AnnotationConstants.Atc.SourceGenerators.Sample.Models.Product.g.cs:**

```csharp
// <auto-generated/>
#nullable enable

namespace Atc.SourceGenerators.Sample.Models;

/// <summary>
/// Annotation constants for types in Atc.SourceGenerators.Sample.Models.
/// </summary>
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
            public const string Description = "The display name of the product";
            public const int MinimumLength = 3;
            public const int MaximumLength = 100;
            public const bool IsRequired = true;
            public const string RequiredErrorMessage = "Product name is required";
            public const string StringLengthErrorMessage = "Name must be 3-100 characters";
        }

        public static partial class Price
        {
            public const string DisplayName = "Price";
            public const string Description = "The retail price of the product";
            public const string Minimum = "0.01";
            public const string Maximum = "999999.99";
            public static readonly global::System.Type OperandType = typeof(decimal);
            public const bool IsRequired = true;
            public const string RangeErrorMessage = "Price must be between $0.01 and $999,999.99";
        }

        public static partial class Sku
        {
            public const string DisplayName = "SKU";
            public const string ShortName = "SKU";
            public const string Description = "Stock Keeping Unit identifier";
            public const int MaximumLength = 20;
            public const string Pattern = @"^[A-Z]{3}-\d{4}$";
            public const string RegularExpressionErrorMessage = "SKU must be in format AAA-0000";
        }

        public static partial class Quantity
        {
            public const string DisplayName = "Quantity in Stock";
            public const string Minimum = "0";
            public const string Maximum = "10000";
            public static readonly global::System.Type OperandType = typeof(int);
        }

        public static partial class Category
        {
            public const string DisplayName = "Category";
            public const string GroupName = "Classification";
            public const bool IsRequired = true;
        }

        public static partial class InternalNotes
        {
            public const string DisplayName = "Internal Notes";
            public const bool IsEditable = false;
            public const bool IsScaffoldColumn = false;
        }
    }
}
```

---

## 🎨 Real-World Use Cases

### Use Case 1: Blazor Form Generation

```razor
@* FormField.razor - Generic form field component *@
@typeparam TModel

<div class="form-group @(Required ? "required" : "")">
    <label for="@FieldName">@DisplayName</label>

    <InputText id="@FieldName"
               @bind-Value="@Value"
               class="form-control"
               maxlength="@MaxLength"
               placeholder="@Prompt" />

    @if (!string.IsNullOrEmpty(Description))
    {
        <small class="form-text text-muted">@Description</small>
    }

    <ValidationMessage For="@(() => Value)" />
</div>

@code {
    [Parameter] public string FieldName { get; set; } = string.Empty;
    [Parameter] public string DisplayName { get; set; } = string.Empty;
    [Parameter] public string? Description { get; set; }
    [Parameter] public string? Prompt { get; set; }
    [Parameter] public int MaxLength { get; set; } = 255;
    [Parameter] public bool Required { get; set; }
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
}
```

**Usage with generated constants:**

```razor
<FormField FieldName="name"
           DisplayName="@AnnotationConstants.Product.Name.DisplayName"
           Description="@AnnotationConstants.Product.Name.Description"
           MaxLength="@AnnotationConstants.Product.Name.MaximumLength"
           Required="@AnnotationConstants.Product.Name.IsRequired"
           @bind-Value="product.Name" />
```

### Use Case 2: Minimal API Documentation

```csharp
app.MapPost("/api/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithOpenApi(operation =>
{
    operation.Description = "Creates a new product";
    operation.Parameters[0].Description =
        $"{AnnotationConstants.Product.Name.DisplayName}: " +
        $"{AnnotationConstants.Product.Name.MinimumLength}-{AnnotationConstants.Product.Name.MaximumLength} characters, " +
        $"Required: {AnnotationConstants.Product.Name.IsRequired}";
    return operation;
});
```

### Use Case 3: TypeScript Type Generation

```csharp
// Generate TypeScript interfaces with validation metadata
var tsGenerator = new TypeScriptGenerator();

tsGenerator.AddType("Product", new Dictionary<string, PropertyMetadata>
{
    ["name"] = new(
        DisplayName: AnnotationConstants.Product.Name.DisplayName,
        Required: AnnotationConstants.Product.Name.IsRequired,
        MaxLength: AnnotationConstants.Product.Name.MaximumLength,
        MinLength: AnnotationConstants.Product.Name.MinimumLength
    ),
    ["price"] = new(
        DisplayName: AnnotationConstants.Product.Price.DisplayName,
        Required: AnnotationConstants.Product.Price.IsRequired,
        Min: decimal.Parse(AnnotationConstants.Product.Price.Minimum),
        Max: decimal.Parse(AnnotationConstants.Product.Price.Maximum)
    ),
    ["sku"] = new(
        DisplayName: AnnotationConstants.Product.Sku.DisplayName,
        Pattern: AnnotationConstants.Product.Sku.Pattern
    )
});

var typescript = tsGenerator.Generate();
// Output:
// interface Product {
//     name: string;  // Product Name, required, 3-100 chars
//     price: number; // Price, required, 0.01-999999.99
//     sku?: string;  // SKU, pattern: ^[A-Z]{3}-\d{4}$
// }
```

---

## 🔗 Related Documentation

- [Main Documentation](AnnotationConstantsGenerator.md)
- [Feature Roadmap](AnnotationConstantsGenerator-FeatureRoadmap.md)
