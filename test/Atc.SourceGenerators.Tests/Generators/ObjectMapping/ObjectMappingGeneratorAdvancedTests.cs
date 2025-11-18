// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Convert_DateTime_To_String()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;
            using System;

            [MapTo(typeof(EventDto))]
            public partial class Event
            {
                public Guid Id { get; set; }
                public DateTime StartTime { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
            }

            public class EventDto
            {
                public string Id { get; set; } = string.Empty;
                public string StartTime { get; set; } = string.Empty;
                public string CreatedAt { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToEventDto", output, StringComparison.Ordinal);

        // Should convert DateTime to string using ISO 8601 format
        Assert.Contains("StartTime = source.StartTime.ToString(\"O\"", output, StringComparison.Ordinal);

        // Should convert DateTimeOffset to string using ISO 8601 format
        Assert.Contains("CreatedAt = source.CreatedAt.ToString(\"O\"", output, StringComparison.Ordinal);

        // Should convert Guid to string
        Assert.Contains("Id = source.Id.ToString()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Convert_String_To_DateTime()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;
            using System;

            [MapTo(typeof(Event))]
            public partial class EventDto
            {
                public string Id { get; set; } = string.Empty;
                public string StartTime { get; set; } = string.Empty;
                public string CreatedAt { get; set; } = string.Empty;
            }

            public class Event
            {
                public Guid Id { get; set; }
                public DateTime StartTime { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToEvent", output, StringComparison.Ordinal);

        // Should convert string to DateTime
        Assert.Contains("StartTime = global::System.DateTime.Parse(source.StartTime, global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);

        // Should convert string to DateTimeOffset
        Assert.Contains("CreatedAt = global::System.DateTimeOffset.Parse(source.CreatedAt, global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);

        // Should convert string to Guid
        Assert.Contains("Id = global::System.Guid.Parse(source.Id)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Convert_Numeric_Types_To_String()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(ProductDto))]
            public partial class Product
            {
                public int Quantity { get; set; }
                public long StockNumber { get; set; }
                public decimal Price { get; set; }
                public double Weight { get; set; }
                public bool IsAvailable { get; set; }
            }

            public class ProductDto
            {
                public string Quantity { get; set; } = string.Empty;
                public string StockNumber { get; set; } = string.Empty;
                public string Price { get; set; } = string.Empty;
                public string Weight { get; set; } = string.Empty;
                public string IsAvailable { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToProductDto", output, StringComparison.Ordinal);

        // Should convert numeric types to string using invariant culture
        Assert.Contains("Quantity = source.Quantity.ToString(global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);
        Assert.Contains("StockNumber = source.StockNumber.ToString(global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);
        Assert.Contains("Price = source.Price.ToString(global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);
        Assert.Contains("Weight = source.Weight.ToString(global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);

        // Should convert bool to string
        Assert.Contains("IsAvailable = source.IsAvailable.ToString()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Convert_String_To_Numeric_Types()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(Product))]
            public partial class ProductDto
            {
                public string Quantity { get; set; } = string.Empty;
                public string StockNumber { get; set; } = string.Empty;
                public string Price { get; set; } = string.Empty;
                public string Weight { get; set; } = string.Empty;
                public string IsAvailable { get; set; } = string.Empty;
            }

            public class Product
            {
                public int Quantity { get; set; }
                public long StockNumber { get; set; }
                public decimal Price { get; set; }
                public double Weight { get; set; }
                public bool IsAvailable { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToProduct", output, StringComparison.Ordinal);

        // Should convert string to numeric types using invariant culture
        Assert.Contains("Quantity = int.Parse(source.Quantity, global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);
        Assert.Contains("StockNumber = long.Parse(source.StockNumber, global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);
        Assert.Contains("Price = decimal.Parse(source.Price, global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);
        Assert.Contains("Weight = double.Parse(source.Weight, global::System.Globalization.CultureInfo.InvariantCulture)", output, StringComparison.Ordinal);

        // Should convert string to bool
        Assert.Contains("IsAvailable = bool.Parse(source.IsAvailable)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Warning_For_Missing_Required_Property()
    {
        // Arrange
        const string source = """
            using System;
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

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

                // This property is required but not mapped
                public required string Email { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var warning = diagnostics.FirstOrDefault(d => d.Id == "ATCMAP004");
        Assert.NotNull(warning);
        Assert.Equal(DiagnosticSeverity.Warning, warning!.Severity);
        Assert.Contains("Email", warning.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("UserDto", warning.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Warning_When_All_Required_Properties_Are_Mapped()
    {
        // Arrange
        const string source = """
            using System;
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string Email { get; set; } = string.Empty;
            }

            public class UserDto
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;

                // This property is required AND is mapped
                public required string Email { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var warning = diagnostics.FirstOrDefault(d => d.Id == "ATCMAP004");
        Assert.Null(warning);

        // Verify mapping was generated
        Assert.Contains("Email = source.Email", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Warning_For_Multiple_Missing_Required_Properties()
    {
        // Arrange
        const string source = """
            using System;
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public Guid Id { get; set; }
                // Missing: Name and Email properties
            }

            public class UserDto
            {
                public Guid Id { get; set; }
                public required string Name { get; set; }
                public required string Email { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var warnings = diagnostics
            .Where(d => d.Id == "ATCMAP004")
            .ToList();
        Assert.Equal(2, warnings.Count);

        // Check that both properties are reported
        var messages = warnings
            .Select(w => w.GetMessage(CultureInfo.InvariantCulture))
            .ToList();
        Assert.Contains(messages, m => m.Contains("Name", StringComparison.Ordinal));
        Assert.Contains(messages, m => m.Contains("Email", StringComparison.Ordinal));
    }

    [Fact]
    public void Generator_Should_Not_Generate_Warning_For_Non_Required_Properties()
    {
        // Arrange
        const string source = """
            using System;
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                // Missing: Email property (but it's not required)
            }

            public class UserDto
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;

                // This property is NOT required
                public string Email { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var warning = diagnostics.FirstOrDefault(d => d.Id == "ATCMAP004");
        Assert.Null(warning);
    }

    [Fact]
    public void Generator_Should_Generate_Polymorphic_Mapping_With_Switch_Expression()
    {
        // Arrange
        const string source = """
            using System;
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            public abstract class AnimalEntity { }

            [MapTo(typeof(Dog))]
            public partial class DogEntity : AnimalEntity
            {
                public string Breed { get; set; } = string.Empty;
            }

            [MapTo(typeof(Cat))]
            public partial class CatEntity : AnimalEntity
            {
                public int Lives { get; set; }
            }

            [MapTo(typeof(Animal))]
            [MapDerivedType(typeof(DogEntity), typeof(Dog))]
            [MapDerivedType(typeof(CatEntity), typeof(Cat))]
            public abstract partial class AnimalEntity { }

            public abstract class Animal { }
            public class Dog : Animal { public string Breed { get; set; } = string.Empty; }
            public class Cat : Animal { public int Lives { get; set; } }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        // Should generate switch expression
        Assert.Contains("return source switch", output, StringComparison.Ordinal);
        Assert.Contains("DogEntity", output, StringComparison.Ordinal);
        Assert.Contains("CatEntity", output, StringComparison.Ordinal);
        Assert.Contains("MapToDog()", output, StringComparison.Ordinal);
        Assert.Contains("MapToCat()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Single_Derived_Type_Mapping()
    {
        // Arrange
        const string source = """
            using System;
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            public abstract class VehicleEntity { }

            [MapTo(typeof(Car))]
            public partial class CarEntity : VehicleEntity
            {
                public string Model { get; set; } = string.Empty;
            }

            [MapTo(typeof(Vehicle))]
            [MapDerivedType(typeof(CarEntity), typeof(Car))]
            public abstract partial class VehicleEntity { }

            public abstract class Vehicle { }
            public class Car : Vehicle { public string Model { get; set; } = string.Empty; }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        // Should still generate switch expression even with single derived type
        Assert.Contains("return source switch", output, StringComparison.Ordinal);
        Assert.Contains("CarEntity", output, StringComparison.Ordinal);
        Assert.Contains("MapToCar()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Multiple_Polymorphic_Mappings()
    {
        // Arrange
        const string source = """
            using System;
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            public abstract class ShapeEntity { }

            [MapTo(typeof(Circle))]
            public partial class CircleEntity : ShapeEntity
            {
                public double Radius { get; set; }
            }

            [MapTo(typeof(Square))]
            public partial class SquareEntity : ShapeEntity
            {
                public double Side { get; set; }
            }

            [MapTo(typeof(Triangle))]
            public partial class TriangleEntity : ShapeEntity
            {
                public double Base { get; set; }
                public double Height { get; set; }
            }

            [MapTo(typeof(Shape))]
            [MapDerivedType(typeof(CircleEntity), typeof(Circle))]
            [MapDerivedType(typeof(SquareEntity), typeof(Square))]
            [MapDerivedType(typeof(TriangleEntity), typeof(Triangle))]
            public abstract partial class ShapeEntity { }

            public abstract class Shape { }
            public class Circle : Shape { public double Radius { get; set; } }
            public class Square : Shape { public double Side { get; set; } }
            public class Triangle : Shape { public double Base { get; set; } public double Height { get; set; } }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        // Should generate switch with all three derived types
        Assert.Contains("CircleEntity", output, StringComparison.Ordinal);
        Assert.Contains("SquareEntity", output, StringComparison.Ordinal);
        Assert.Contains("TriangleEntity", output, StringComparison.Ordinal);
        Assert.Contains("MapToCircle()", output, StringComparison.Ordinal);
        Assert.Contains("MapToSquare()", output, StringComparison.Ordinal);
        Assert.Contains("MapToTriangle()", output, StringComparison.Ordinal);
    }
}