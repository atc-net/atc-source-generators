// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators;

public class ObjectMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Simple_Mapping()
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
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.Source source)", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Nested_Object_And_Enum_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum SourceStatus { Active = 0, Inactive = 1 }
                              public enum TargetStatus { Active = 0, Inactive = 1 }

                              public class TargetAddress
                              {
                                  public string Street { get; set; } = string.Empty;
                                  public string City { get; set; } = string.Empty;
                              }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public TargetStatus Status { get; set; }
                                  public TargetAddress? Address { get; set; }
                              }

                              [MapTo(typeof(TargetAddress))]
                              public partial class SourceAddress
                              {
                                  public string Street { get; set; } = string.Empty;
                                  public string City { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public SourceStatus Status { get; set; }
                                  public SourceAddress? Address { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("MapToTargetAddress", output, StringComparison.Ordinal);
        Assert.Contains("(TestNamespace.TargetStatus)source.Status", output, StringComparison.Ordinal);
        Assert.Contains("source.Address?.MapToTargetAddress()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Class_Is_Not_Partial()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public class Source
                              {
                                  public int Id { get; set; }
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCMAP001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generator_Should_Generate_Bidirectional_Mapping_When_Bidirectional_Is_True()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto), Bidirectional = true)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: Source.MapToTargetDto()
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.Source source)", output, StringComparison.Ordinal);

        // Reverse mapping: TargetDto.MapToSource()
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.Source MapToSource(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.TargetDto source)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Reverse_Mapping_When_Bidirectional_Is_False()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TargetDto), Bidirectional = false)]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: Source.MapToTargetDto() - should exist
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Reverse mapping: TargetDto.MapToSource() - should NOT exist
        Assert.DoesNotContain("MapToSource", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Enum_Mapping_Method_When_Enum_Has_MapTo_Attribute()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus { Active = 0, Inactive = 1 }

                              public enum TargetStatus { Active = 0, Inactive = 1 }

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                                  public TargetStatus Status { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public SourceStatus Status { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should use enum mapping method instead of cast
        Assert.Contains("Status = source.Status.MapToTargetStatus()", output, StringComparison.Ordinal);
        Assert.DoesNotContain("(TestNamespace.TargetStatus)source.Status", output, StringComparison.Ordinal);
    }

    [SuppressMessage("", "S1854:Remove this useless assignment to local variable 'driver'", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput(
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Run both ObjectMappingGenerator and EnumMappingGenerator
        // (EnumMappingGenerator generates the MapToXxx extension methods for enums)
        var objectMappingGenerator = new ObjectMappingGenerator();
        var enumMappingGenerator = new EnumMappingGenerator();
        var driver = CSharpGeneratorDriver.Create(objectMappingGenerator, enumMappingGenerator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = outputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCMAP", StringComparison.Ordinal))
            .ToImmutableArray();

        var output = string.Join(
            "\n",
            outputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, output);
    }

    [Fact]
    public void Generator_Should_Generate_List_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class AddressDto
                              {
                                  public string Street { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(AddressDto))]
                              public partial class Address
                              {
                                  public string Street { get; set; } = string.Empty;
                              }

                              public class UserDto
                              {
                                  public int Id { get; set; }
                                  public System.Collections.Generic.List<AddressDto> Addresses { get; set; } = new();
                              }

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public System.Collections.Generic.List<Address> Addresses { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);
        Assert.Contains("Addresses = source.Addresses?.Select(x => x.MapToAddressDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IEnumerable_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ItemDto
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(ItemDto))]
                              public partial class Item
                              {
                                  public string Name { get; set; } = string.Empty;
                              }

                              public class ContainerDto
                              {
                                  public System.Collections.Generic.IEnumerable<ItemDto> Items { get; set; } = null!;
                              }

                              [MapTo(typeof(ContainerDto))]
                              public partial class Container
                              {
                                  public System.Collections.Generic.IEnumerable<Item> Items { get; set; } = null!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToContainerDto", output, StringComparison.Ordinal);
        Assert.Contains("Items = source.Items?.Select(x => x.MapToItemDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_IReadOnlyList_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TagDto
                              {
                                  public string Value { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(TagDto))]
                              public partial class Tag
                              {
                                  public string Value { get; set; } = string.Empty;
                              }

                              public class PostDto
                              {
                                  public System.Collections.Generic.IReadOnlyList<TagDto> Tags { get; set; } = null!;
                              }

                              [MapTo(typeof(PostDto))]
                              public partial class Post
                              {
                                  public System.Collections.Generic.IReadOnlyList<Tag> Tags { get; set; } = null!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToPostDto", output, StringComparison.Ordinal);
        Assert.Contains("Tags = source.Tags?.Select(x => x.MapToTagDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Array_Collection_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ElementDto
                              {
                                  public int Value { get; set; }
                              }

                              [MapTo(typeof(ElementDto))]
                              public partial class Element
                              {
                                  public int Value { get; set; }
                              }

                              public class ArrayContainerDto
                              {
                                  public ElementDto[] Elements { get; set; } = null!;
                              }

                              [MapTo(typeof(ArrayContainerDto))]
                              public partial class ArrayContainer
                              {
                                  public Element[] Elements { get; set; } = null!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToArrayContainerDto", output, StringComparison.Ordinal);
        Assert.Contains("Elements = source.Elements?.Select(x => x.MapToElementDto()).ToArray()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Collection_ObjectModel_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class ValueDto
                              {
                                  public string Data { get; set; } = string.Empty;
                              }

                              [MapTo(typeof(ValueDto))]
                              public partial class Value
                              {
                                  public string Data { get; set; } = string.Empty;
                              }

                              public class CollectionDto
                              {
                                  public System.Collections.ObjectModel.Collection<ValueDto> Values { get; set; } = new();
                              }

                              [MapTo(typeof(CollectionDto))]
                              public partial class CollectionContainer
                              {
                                  public System.Collections.ObjectModel.Collection<Value> Values { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCollectionDto", output, StringComparison.Ordinal);
        Assert.Contains("new global::System.Collections.ObjectModel.Collection<TestNamespace.ValueDto>(source.Values?.Select(x => x.MapToValueDto()).ToList()!)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Class_To_Record()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);

        // Should use constructor call (constructor mapping feature)
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Record_To_Record()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto))]
                              public partial record Source(int Id, string Name);
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);

        // Should use constructor call (constructor mapping feature)
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Map_Record_To_Class()
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
                              public partial record Source(int Id, string Name);
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetDto MapToTargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_For_Simple_Record()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should use constructor call instead of object initializer
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);

        // Should NOT use object initializer syntax
        Assert.DoesNotContain("Id = source.Id", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Name = source.Name", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_For_Record_With_All_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record UserDto(int Id, string Name, string Email, int Age);

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                                  public int Age { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should use constructor call with all parameters
        Assert.Contains("return new TestNamespace.UserDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("source.Email,", output, StringComparison.Ordinal);
        Assert.Contains("source.Age", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Mixed_Constructor_And_Initializer_For_Record_With_Extra_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name)
                              {
                                  public string Email { get; set; } = string.Empty;
                                  public int Age { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public string Email { get; set; } = string.Empty;
                                  public int Age { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should use constructor for primary parameters
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);

        // Should use object initializer for extra properties
        Assert.Contains("Email = source.Email,", output, StringComparison.Ordinal);
        Assert.Contains("Age = source.Age", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_For_Bidirectional_Record_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int Id, string Name);

                              [MapTo(typeof(TargetDto), Bidirectional = true)]
                              public partial record Source(int Id, string Name);
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: Source.MapToTargetDto() - should use constructor
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);

        // Reverse mapping: TargetDto.MapToSource() - should also use constructor
        Assert.Contains("MapToSource", output, StringComparison.Ordinal);
        Assert.Contains("return new TestNamespace.Source(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_With_Nested_Object_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record AddressDto(string Street, string City);

                              [MapTo(typeof(AddressDto))]
                              public partial record Address(string Street, string City);

                              public record UserDto(int Id, string Name, AddressDto? Address);

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public Address? Address { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should use constructor with nested mapping
        Assert.Contains("return new TestNamespace.UserDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("source.Address?.MapToAddressDto()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_With_Enum_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus { Active = 0, Inactive = 1 }

                              public enum TargetStatus { Active = 0, Inactive = 1 }

                              public record UserDto(int Id, string Name, TargetStatus Status);

                              [MapTo(typeof(UserDto))]
                              public partial class User
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                                  public SourceStatus Status { get; set; }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToUserDto", output, StringComparison.Ordinal);

        // Should use constructor with enum mapping method
        Assert.Contains("return new TestNamespace.UserDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name,", output, StringComparison.Ordinal);
        Assert.Contains("source.Status.MapToTargetStatus()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Use_Constructor_With_Collection_In_Initializer()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TagDto(string Value);

                              [MapTo(typeof(TagDto))]
                              public partial record Tag(string Value);

                              public record PostDto(int Id, string Title)
                              {
                                  public System.Collections.Generic.List<TagDto> Tags { get; set; } = new();
                              }

                              [MapTo(typeof(PostDto))]
                              public partial class Post
                              {
                                  public int Id { get; set; }
                                  public string Title { get; set; } = string.Empty;
                                  public System.Collections.Generic.List<Tag> Tags { get; set; } = new();
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToPostDto", output, StringComparison.Ordinal);

        // Should use constructor for Id and Title
        Assert.Contains("return new TestNamespace.PostDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Title", output, StringComparison.Ordinal);

        // Should use initializer for collection
        Assert.Contains("Tags = source.Tags?.Select(x => x.MapToTagDto()).ToList()!", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Case_Insensitive_Constructor_Parameter_Matching()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public record TargetDto(int id, string name);

                              [MapTo(typeof(TargetDto))]
                              public partial class Source
                              {
                                  public int Id { get; set; }
                                  public string Name { get; set; } = string.Empty;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetDto", output, StringComparison.Ordinal);

        // Should match properties to constructor parameters case-insensitively
        Assert.Contains("return new TestNamespace.TargetDto(", output, StringComparison.Ordinal);
        Assert.Contains("source.Id,", output, StringComparison.Ordinal);
        Assert.Contains("source.Name", output, StringComparison.Ordinal);
    }

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