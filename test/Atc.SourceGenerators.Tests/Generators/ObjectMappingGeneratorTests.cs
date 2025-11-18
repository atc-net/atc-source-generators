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
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
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
        Assert.Contains("Id = source.Id", output, StringComparison.Ordinal);
        Assert.Contains("Name = source.Name", output, StringComparison.Ordinal);
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
}