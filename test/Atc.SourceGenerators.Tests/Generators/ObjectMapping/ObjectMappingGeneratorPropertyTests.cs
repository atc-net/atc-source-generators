// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.ObjectMapping;

public partial class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Ignore_Properties_With_MapIgnore_Attribute()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;

                                  [MapIgnore]
                                  public byte[] PasswordHash { get; set; } = System.Array.Empty<byte>();

                                  [MapIgnore]
                                  public System.DateTime CreatedAt { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);

        // Should NOT contain ignored properties
        Assert.DoesNotContain("PasswordHash", output, StringComparison.Ordinal);
        Assert.DoesNotContain("CreatedAt", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Ignore_Target_Properties_With_MapIgnore_Attribute()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public partial class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;

                                  [MapIgnore]
                                  public System.DateTime UpdatedAt { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public System.DateTime UpdatedAt { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);

        // Should NOT contain ignored target property
        Assert.DoesNotContain("UpdatedAt", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Ignore_Properties_In_Nested_Objects()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetAddress
                              {
                                  public string Street { get; set; } = string.Empty;
                                  public string City { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public TargetAddress? Address { get; set; }
                              }

                              [MapTo(typeof(TargetAddress))]
                              public partial class SourceAddress
                              {
                                  public string Street { get; set; } = string.Empty;
                                  public string City { get; set; } = string.Empty;

                                  [MapIgnore]
                                  public string PostalCode { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public SourceAddress? Address { get; set; }

                                  [MapIgnore]
                                  public string InternalNotes { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetAddress", output, StringComparison.Ordinal);

        // Should NOT contain ignored properties
        Assert.DoesNotContain("InternalNotes", output, StringComparison.Ordinal);
        Assert.DoesNotContain("PostalCode", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Ignore_Properties_With_Bidirectional_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(Source), Bidirectional = true)]
                              public partial class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;

                                  [MapIgnore]
                                  public System.DateTime LastModified { get; set; }
                              }

                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public System.DateTime LastModified { get; set; }

                                  [MapIgnore]
                                  public byte[] Metadata { get; set; } = System.Array.Empty<byte>();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should NOT contain ignored properties in either direction
        Assert.DoesNotContain("LastModified", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Metadata", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Properties_With_Custom_Names_Using_MapProperty()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public int Id { get; set; }

                [MapProperty("FullName")]
                public string Name { get; set; } = string.Empty;

                [MapProperty("Age")]
                public int YearsOld { get; set; }
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string FullName { get; set; } = string.Empty;
                public int Age { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should map Name → FullName
        Assert.Contains("FullName = source.Name", output, StringComparison.Ordinal);

        // Should map YearsOld → Age
        Assert.Contains("Age = source.YearsOld", output, StringComparison.Ordinal);

        // Should still map Id normally
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Properties_With_Bidirectional_Custom_Names()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(PersonDto), Bidirectional = true)]
            public partial class Person
            {
                public int Id { get; set; }

                [MapProperty("DisplayName")]
                public string FullName { get; set; } = string.Empty;
            }

            public partial class PersonDto
            {
                public int Id { get; set; }

                [MapProperty("FullName")]
                public string DisplayName { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        // Forward mapping: Person → PersonDto
        Assert.Contains("MapToPersonDto", output, StringComparison.Ordinal);
        Assert.Contains("DisplayName = source.FullName", output, StringComparison.Ordinal);

        // Reverse mapping: PersonDto → Person
        Assert.Contains("MapToPerson", output, StringComparison.Ordinal);
        Assert.Contains("FullName = source.DisplayName", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_MapProperty_Target_Does_Not_Exist()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public int Id { get; set; }

                [MapProperty("NonExistentProperty")]
                public string Name { get; set; } = string.Empty;
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string FullName { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        var errorDiagnostics = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        var errors = errorDiagnostics.ToList();
        Assert.NotEmpty(errors);

        // Should report that target property doesn't exist
        var mapPropertyError = errors.FirstOrDefault(d => d.Id == "ATCMAP003");
        Assert.NotNull(mapPropertyError);

        var message = mapPropertyError.GetMessage(CultureInfo.InvariantCulture);
        Assert.Contains("NonExistentProperty", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_MapProperty_With_Nested_Objects()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(PersonDto))]
            public partial class Person
            {
                public int Id { get; set; }

                [MapProperty("HomeAddress")]
                public Address Address { get; set; } = new();
            }

            [MapTo(typeof(AddressDto))]
            public partial class Address
            {
                public string Street { get; set; } = string.Empty;
                public string City { get; set; } = string.Empty;
            }

            public class PersonDto
            {
                public int Id { get; set; }
                public AddressDto HomeAddress { get; set; } = new();
            }

            public class AddressDto
            {
                public string Street { get; set; } = string.Empty;
                public string City { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToPersonDto", output, StringComparison.Ordinal);

        // Should map Address → HomeAddress with nested mapping
        Assert.Contains("HomeAddress = source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Flatten_Nested_Properties_When_EnableFlattening_Is_True()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(UserDto), EnableFlattening = true)]
            public partial class User
            {
                public int Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public Address Address { get; set; } = new();
            }

            public class Address
            {
                public string City { get; set; } = string.Empty;
                public string Street { get; set; } = string.Empty;
                public string PostalCode { get; set; } = string.Empty;
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string AddressCity { get; set; } = string.Empty;
                public string AddressStreet { get; set; } = string.Empty;
                public string AddressPostalCode { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should flatten Address.City → AddressCity
        Assert.Contains("AddressCity = source.Address?.City", output, StringComparison.Ordinal);

        // Should flatten Address.Street → AddressStreet
        Assert.Contains("AddressStreet = source.Address?.Street", output, StringComparison.Ordinal);

        // Should flatten Address.PostalCode → AddressPostalCode
        Assert.Contains("AddressPostalCode = source.Address?.PostalCode", output, StringComparison.Ordinal);

        // Should still map direct properties
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Flatten_When_EnableFlattening_Is_False()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(UserDto))]
            public partial class User
            {
                public int Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public Address Address { get; set; } = new();
            }

            public class Address
            {
                public string City { get; set; } = string.Empty;
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string AddressCity { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should NOT flatten when EnableFlattening is false (default)
        // Check that the mapping method doesn't contain AddressCity assignment
        Assert.DoesNotContain("AddressCity =", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Flatten_Multiple_Nested_Objects()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(PersonDto), EnableFlattening = true)]
            public partial class Person
            {
                public int Id { get; set; }
                public Address HomeAddress { get; set; } = new();
                public Address WorkAddress { get; set; } = new();
            }

            public class Address
            {
                public string City { get; set; } = string.Empty;
                public string Street { get; set; } = string.Empty;
            }

            public class PersonDto
            {
                public int Id { get; set; }
                public string HomeAddressCity { get; set; } = string.Empty;
                public string HomeAddressStreet { get; set; } = string.Empty;
                public string WorkAddressCity { get; set; } = string.Empty;
                public string WorkAddressStreet { get; set; } = string.Empty;
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToPersonDto", output, StringComparison.Ordinal);

        // Should flatten HomeAddress properties
        Assert.Contains("HomeAddressCity = source.HomeAddress?.City", output, StringComparison.Ordinal);
        Assert.Contains("HomeAddressStreet = source.HomeAddress?.Street", output, StringComparison.Ordinal);

        // Should flatten WorkAddress properties
        Assert.Contains("WorkAddressCity = source.WorkAddress?.City", output, StringComparison.Ordinal);
        Assert.Contains("WorkAddressStreet = source.WorkAddress?.Street", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Flatten_With_Nullable_Nested_Objects()
    {
        // Arrange
        const string source = """
            namespace TestNamespace;

            using Atc.SourceGenerators.Annotations;

            [MapTo(typeof(UserDto), EnableFlattening = true)]
            public partial class User
            {
                public int Id { get; set; }
                public Address? Address { get; set; }
            }

            public class Address
            {
                public string City { get; set; } = string.Empty;
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string? AddressCity { get; set; }
            }
            """;

        // Act
        var (diagnostics, output) = GetGeneratedOutput(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should handle nullable source with null-conditional operator
        Assert.Contains("AddressCity = source.Address?.City", output, StringComparison.Ordinal);
    }
}