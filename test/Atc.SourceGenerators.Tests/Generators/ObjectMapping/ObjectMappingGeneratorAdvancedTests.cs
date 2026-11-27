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
        var (diagnostics, output) = GetGeneratedOutput(source, allowGeneratedCodeErrors: true);

        // Assert
        var warning = diagnostics.FirstOrDefault(d => d.Id == "ATCMAP004");
        Assert.NotNull(warning);
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
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
        var (diagnostics, output) = GetGeneratedOutput(source, allowGeneratedCodeErrors: true);

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

    [Fact]
    public void Generator_Should_Generate_Polymorphic_Mapping_For_Derived_Types_Named_Like_Keywords()
    {
        // The switch arm variable is derived from the type name: "Event" must not become the keyword "event"
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            [MapTo(typeof(EventDto))]
            public partial class Event : Message
            {
                public string Name { get; set; } = string.Empty;
            }

            [MapTo(typeof(OperatorDto))]
            public partial class Operator : Message
            {
                public int Level { get; set; }
            }

            [MapTo(typeof(MessageDto))]
            [MapDerivedType(typeof(Event), typeof(EventDto))]
            [MapDerivedType(typeof(Operator), typeof(OperatorDto))]
            public abstract partial class Message { }

            public abstract class MessageDto { }
            public class EventDto : MessageDto { public string Name { get; set; } = string.Empty; }
            public class OperatorDto : MessageDto { public int Level { get; set; } }
            """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("@event => @event.MapToEventDto()", output, StringComparison.Ordinal);
        Assert.Contains("@operator => @operator.MapToOperatorDto()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Duplicate_Reverse_Mapping_When_Target_Declares_It_Explicitly()
    {
        // A declares Bidirectional = true, and B also declares [MapTo(typeof(A))]:
        // only one MapToA(this B) may be generated
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(B), Bidirectional = true)]
                              public partial class A
                              {
                                  public int Id { get; set; }
                              }

                              [MapTo(typeof(A))]
                              public partial class B
                              {
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Equal(1, CountOccurrences(output, " MapToA("));
        Assert.Equal(1, CountOccurrences(output, " MapToB("));
    }

    [Fact]
    public void Generator_Should_Report_Duplicate_Mapping_Method_For_Targets_With_The_Same_Name()
    {
        // Two targets named TargetDto in different namespaces would both produce MapToTargetDto(this Source)
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Api.V1
                              {
                                  public class TargetDto { public int Id { get; set; } }
                              }

                              namespace Api.V2
                              {
                                  public class TargetDto { public int Id { get; set; } }
                              }

                              namespace TestNamespace
                              {
                                  [MapTo(typeof(Api.V1.TargetDto))]
                                  [MapTo(typeof(Api.V2.TargetDto))]
                                  public partial class Source
                                  {
                                      public int Id { get; set; }
                                  }
                              }
                              """;

        var (compileDiagnostics, _) = GetGeneratedOutput(source, "CS");
        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.Empty(compileDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        var duplicate = Assert.Single(diagnostics, d => d.Id == "ATCMAP012");
        Assert.Contains("MapToTargetDto", duplicate.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Unmapped_Target_Properties_As_Info()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            public class UserDto
            {
                public int Id { get; set; }

                public string DisplayName { get; set; } = string.Empty;

                [MapIgnore]
                public string Ignored { get; set; } = string.Empty;
            }

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public int Id { get; set; }
            }
            """;

        // Act
        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMAP014", allowInfoDiagnostics: true);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        Assert.Contains("DisplayName", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Report_Unmapped_Target_Properties_When_All_Are_Mapped()
    {
        // Arrange
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            public class UserDto
            {
                public int Id { get; set; }

                public string Name { get; set; } = string.Empty;
            }

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public int Id { get; set; }

                public string Name { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, _) = GetGeneratedOutput(source, "ATCMAP014", allowInfoDiagnostics: true);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Generator_Should_Report_Unmapped_Target_Properties_As_Error_When_Configured()
    {
        // Strict mapping: "dotnet_diagnostic.ATCMAP014.severity = error" ends up as a specific diagnostic option
        const string source = """
            using Atc.SourceGenerators.Annotations;

            namespace TestNamespace;

            public class UserDto
            {
                public int Id { get; set; }

                public string DisplayName { get; set; } = string.Empty;
            }

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public int Id { get; set; }
            }
            """;

        var compilation = GeneratorTestHarness
            .CreateCompilation(source)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>(StringComparer.Ordinal)
                {
                    ["ATCMAP014"] = ReportDiagnostic.Error,
                }));

        CSharpGeneratorDriver
            .Create(new ObjectMappingGenerator(), new EnumMappingGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var generatorDiagnostics, TestContext.Current.CancellationToken);

        var diagnostic = Assert.Single(generatorDiagnostics, d => d.Id == "ATCMAP014");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Theory]
    [InlineData("string", "Uri", "Value = new global::System.Uri(source.Value)")]
    [InlineData("Uri", "string", "Value = source.Value.ToString()")]
    [InlineData("string", "Color", "Value = global::System.Enum.TryParse<global::TestNamespace.Color>(source.Value, out var __valueParsed) ? __valueParsed : default(global::TestNamespace.Color)")]
    [InlineData("Color", "string", "Value = source.Value.ToString()")]
    [InlineData("int?", "string?", "Value = source.Value?.ToString(global::System.Globalization.CultureInfo.InvariantCulture)")]
    [InlineData("string?", "int?", "Value = string.IsNullOrEmpty(source.Value) ? null : (int?)int.Parse(source.Value, global::System.Globalization.CultureInfo.InvariantCulture)")]
    public void Generator_Should_Apply_The_Same_Built_In_Conversions_As_Configuration_Mappings(
        string sourceType,
        string targetType,
        string expected)
    {
        var source = $$"""
                       #nullable enable
                       using System;
                       using Atc.SourceGenerators.Annotations;

                       namespace TestNamespace;

                       public enum Color
                       {
                           Red,
                           Green,
                       }

                       public class TargetDto
                       {
                           public {{targetType}} Value { get; set; } = default!;
                       }

                       [MapTo(typeof(TargetDto))]
                       public partial class Source
                       {
                           public {{sourceType}} Value { get; set; } = default!;
                       }
                       """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains(expected, output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Cast_Numeric_Types()
    {
        const string source = """
                              #nullable enable
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ProductDto
                              {
                                  public short Count { get; set; }

                                  public double Price { get; set; }

                                  public long Total { get; set; }
                              }

                              [MapTo(typeof(ProductDto))]
                              public partial class Product
                              {
                                  public int Count { get; set; }

                                  public decimal Price { get; set; }

                                  public int Total { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("Count = (short)source.Count", output, StringComparison.Ordinal);
        Assert.Contains("Price = (double)source.Price", output, StringComparison.Ordinal);
        Assert.Contains("Total = (long)source.Total", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Cast_Numeric_Type_For_Constructor_Parameter()
    {
        const string source = """
                              #nullable enable
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public sealed record ProductDto(string Name, short Count);

                              [MapTo(typeof(ProductDto))]
                              public partial class Product
                              {
                                  public string Name { get; set; } = string.Empty;

                                  public int Count { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("return new global::TestNamespace.ProductDto(", output, StringComparison.Ordinal);
        Assert.Contains("(short)source.Count", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Convert_Two_Strings_To_Enums_In_One_Mapping()
    {
        const string source = """
                              #nullable enable
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum Status { Active, Inactive }

                              public enum Kind { Small, Large }

                              public class TargetDto
                              {
                                  public Status Status { get; set; }

                                  public Kind Kind { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public string Status { get; set; } = string.Empty;

                                  public string Kind { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("out var __statusParsed) ? __statusParsed", output, StringComparison.Ordinal);
        Assert.Contains("out var __kindParsed) ? __kindParsed", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("int?", "long", "Value = (long)(source.Value ?? default)")]
    [InlineData("int?", "long?", "Value = (long?)source.Value")]
    [InlineData("Color?", "Shade", "Value = (global::TestNamespace.Shade)(source.Value ?? default)")]
    public void Generator_Should_Cast_Nullable_Source_To_Non_Nullable_Target_Without_Throwing(
        string sourceType,
        string targetType,
        string expected)
    {
        var source = $$"""
                       #nullable enable
                       using Atc.SourceGenerators.Annotations;

                       namespace TestNamespace;

                       public enum Color { Red, Green }

                       public enum Shade { Light, Dark }

                       public class TargetDto
                       {
                           public {{targetType}} Value { get; set; }
                       }

                       [MapTo(typeof(TargetDto))]
                       public partial class Source
                       {
                           public {{sourceType}} Value { get; set; }
                       }
                       """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains(expected, output, StringComparison.Ordinal);
    }
}