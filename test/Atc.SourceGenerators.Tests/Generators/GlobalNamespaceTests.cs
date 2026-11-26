// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators;

/// <summary>
/// Types declared without a namespace (the global namespace) must produce valid generated code:
/// no "&lt;global namespace&gt;" in using directives, namespace declarations or type names, and no
/// generator failure because of an invalid generated file (hint) name.
/// </summary>
public class GlobalNamespaceTests
{
    [Fact]
    public void DependencyRegistration_Should_Support_Global_Namespace()
        => AssertValidOutput(
            new DependencyRegistrationGenerator(),
            """
            using Atc.SourceGenerators.Annotations;

            public interface IGlobalService
            {
            }

            [Registration]
            public class GlobalService : IGlobalService
            {
            }
            """);

    [Fact]
    public void OptionsBinding_Should_Support_Global_Namespace()
        => AssertValidOutput(
            new OptionsBindingGenerator(),
            """
            using Atc.SourceGenerators.Annotations;

            [OptionsBinding("Global")]
            public partial class GlobalOptions
            {
                public string Value { get; set; } = string.Empty;
            }
            """);

    [Fact]
    public void ObjectMapping_Should_Support_Global_Namespace()
        => AssertValidOutput(
            new ObjectMappingGenerator(),
            """
            using Atc.SourceGenerators.Annotations;

            public class GlobalDto
            {
                public int Id { get; set; }
            }

            [MapTo(typeof(GlobalDto))]
            public partial class GlobalSource
            {
                public int Id { get; set; }
            }

            public class ResultDto<T>
            {
                public T? Value { get; set; }
            }

            [MapTo(typeof(ResultDto<>), UpdateTarget = true)]
            public partial class Result<T>
            {
                public T? Value { get; set; }
            }
            """);

    /// <summary>
    /// [MapTo] is declared by the ObjectMapping generator, so both generators run, as in a real project.
    /// </summary>
    [Fact]
    public void EnumMapping_Should_Support_Global_Namespace()
        => AssertValidOutput(
            [new EnumMappingGenerator(), new ObjectMappingGenerator()],
            """
            using Atc.SourceGenerators.Annotations;

            public enum GlobalStatusDto
            {
                Active,
            }

            [MapTo(typeof(GlobalStatusDto))]
            public enum GlobalStatus
            {
                Active,
            }
            """);

    [Fact]
    public void MappingConfiguration_Should_Support_Global_Namespace()
        => AssertValidOutput(
            new MappingConfigurationGenerator(),
            """
            using Atc.SourceGenerators.Annotations;

            public class GlobalDto
            {
                public int Id { get; set; }
            }

            public class GlobalSource
            {
                public int Id { get; set; }
            }

            [MappingConfiguration]
            public static partial class GlobalMappings
            {
                public static partial GlobalDto MapToGlobalDto(this GlobalSource source);
            }
            """);

    [Fact]
    public void AnnotationConstants_Should_Support_Global_Namespace()
        => AssertValidOutput(
            new AnnotationConstantsGenerator(),
            """
            using System.ComponentModel.DataAnnotations;

            public class GlobalProduct
            {
                [Required]
                public string Name { get; set; } = string.Empty;
            }
            """);

    private static void AssertValidOutput(
        IIncrementalGenerator generator,
        string source)
        => AssertValidOutput([generator], source);

    private static void AssertValidOutput(
        IIncrementalGenerator[] generators,
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(generators);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        // The generator must not fail (e.g. an invalid hint name is reported as CS8785)
        Assert.Empty(generatorDiagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning));

        var generatedTrees = outputCompilation.SyntaxTrees.Skip(1).ToList();
        Assert.NotEmpty(generatedTrees);

        foreach (var tree in generatedTrees)
        {
            var text = tree.ToString();
            Assert.DoesNotContain("<global namespace>", text, StringComparison.Ordinal);
            Assert.DoesNotContain(tree.GetDiagnostics(), d => d.Severity == DiagnosticSeverity.Error);
        }
    }
}