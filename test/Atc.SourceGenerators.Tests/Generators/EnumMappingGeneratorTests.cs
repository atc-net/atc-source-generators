// ReSharper disable RedundantAssignment
// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators;

public class EnumMappingGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Exact_Name_Match_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Available,
                                  Pending,
                                  Adopted,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                                  Adopted,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToTargetStatus", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetStatus MapToTargetStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.SourceStatus source)", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Available => TestNamespace.TargetStatus.Available,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Pending => TestNamespace.TargetStatus.Pending,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Adopted => TestNamespace.TargetStatus.Adopted,", output, StringComparison.Ordinal);
        Assert.Contains("_ => throw new global::System.ArgumentOutOfRangeException(nameof(source), source, \"Unmapped enum value\"),", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Special_Case_Mapping_None_To_Unknown()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Unknown,
                                  Available,
                                  Pending,
                                  Adopted,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  None,
                                  Pending,
                                  Available,
                                  Adopted,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceStatus.None => TestNamespace.TargetStatus.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Available => TestNamespace.TargetStatus.Available,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Pending => TestNamespace.TargetStatus.Pending,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Adopted => TestNamespace.TargetStatus.Adopted,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Case_Insensitive_Matching()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  available,
                                  PENDING,
                                  Adopted,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                                  ADOPTED,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("TestNamespace.SourceStatus.Available => TestNamespace.TargetStatus.available,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Pending => TestNamespace.TargetStatus.PENDING,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.ADOPTED => TestNamespace.TargetStatus.Adopted,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Warning_For_Unmapped_Values()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Available,
                                  Pending,
                              }

                              [MapTo(typeof(TargetStatus))]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                                  Adopted,
                                  Deleted,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        // Should have warnings for unmapped values
        Assert.NotEmpty(diagnostics);
        var adoptedWarning = Assert.Single(
            diagnostics,
            d => d.Id == "ATCENUM002" &&
                 d
                     .GetMessage(CultureInfo.InvariantCulture)
                     .Contains("Adopted", StringComparison.Ordinal));
        Assert.Equal(DiagnosticSeverity.Warning, adoptedWarning.Severity);

        var deletedWarning = Assert.Single(
            diagnostics,
            d => d.Id == "ATCENUM002" &&
                 d
                     .GetMessage(CultureInfo.InvariantCulture)
                     .Contains("Deleted", StringComparison.Ordinal));
        Assert.Equal(DiagnosticSeverity.Warning, deletedWarning.Severity);

        // Generated code should still work for mapped values
        Assert.Contains("TestNamespace.SourceStatus.Available => TestNamespace.TargetStatus.Available,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Pending => TestNamespace.TargetStatus.Pending,", output, StringComparison.Ordinal);

        // Unmapped values should not appear in switch expression
        Assert.DoesNotContain("SourceStatus.Adopted =>", output, StringComparison.Ordinal);
        Assert.DoesNotContain("SourceStatus.Deleted =>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Bidirectional_Mapping_When_Bidirectional_Is_True()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Available,
                                  Pending,
                              }

                              [MapTo(typeof(TargetStatus), Bidirectional = true)]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: SourceStatus.MapToTargetStatus()
        Assert.Contains("MapToTargetStatus", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.TargetStatus MapToTargetStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.SourceStatus source)", output, StringComparison.Ordinal);

        // Reverse mapping: TargetStatus.MapToSourceStatus()
        Assert.Contains("MapToSourceStatus", output, StringComparison.Ordinal);
        Assert.Contains("public static TestNamespace.SourceStatus MapToSourceStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this TestNamespace.TargetStatus source)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Reverse_Mapping_When_Bidirectional_Is_False()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetStatus
                              {
                                  Available,
                                  Pending,
                              }

                              [MapTo(typeof(TargetStatus), Bidirectional = false)]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Forward mapping: SourceStatus.MapToTargetStatus() - should exist
        Assert.Contains("MapToTargetStatus", output, StringComparison.Ordinal);

        // Reverse mapping: TargetStatus.MapToSourceStatus() - should NOT exist
        Assert.DoesNotContain("MapToSourceStatus", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Target_Is_Not_Enum()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public class TargetDto
                              {
                                  public int Id { get; set; }
                              }

                              [MapTo(typeof(TargetDto))]
                              public enum SourceStatus
                              {
                                  Available,
                                  Pending,
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCENUM001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("must be an enum type", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Complex_Scenario_With_Mixed_Mappings()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum PetStatusDto
                              {
                                  Unknown,
                                  Available,
                                  Pending,
                                  Adopted,
                              }

                              [MapTo(typeof(PetStatusDto))]
                              public enum PetStatusEntity
                              {
                                  None,
                                  Pending,
                                  Available,
                                  Adopted,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToPetStatusDto", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.PetStatusEntity.None => TestNamespace.PetStatusDto.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.PetStatusEntity.Pending => TestNamespace.PetStatusDto.Pending,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.PetStatusEntity.Available => TestNamespace.PetStatusDto.Available,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.PetStatusEntity.Adopted => TestNamespace.PetStatusDto.Adopted,", output, StringComparison.Ordinal);
        Assert.Contains("_ => throw new global::System.ArgumentOutOfRangeException(nameof(source), source, \"Unmapped enum value\"),", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Cross_Namespace_Mapping()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Domain.Models
                              {
                                  public enum Status
                                  {
                                      Unknown,
                                      Active,
                                      Inactive,
                                  }
                              }

                              namespace DataAccess.Entities
                              {
                                  [MapTo(typeof(Domain.Models.Status))]
                                  public enum StatusEntity
                                  {
                                      None,
                                      Active,
                                      Inactive,
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToStatus", output, StringComparison.Ordinal);
        Assert.Contains("public static Domain.Models.Status MapToStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this DataAccess.Entities.StatusEntity source)", output, StringComparison.Ordinal);
        Assert.Contains("DataAccess.Entities.StatusEntity.None => Domain.Models.Status.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("DataAccess.Entities.StatusEntity.Active => Domain.Models.Status.Active,", output, StringComparison.Ordinal);
        Assert.Contains("DataAccess.Entities.StatusEntity.Inactive => Domain.Models.Status.Inactive,", output, StringComparison.Ordinal);
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

        // Run both ObjectMappingGenerator (for MapToAttribute) and EnumMappingGenerator
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
                        d.Id.StartsWith("ATCENUM", StringComparison.Ordinal))
            .ToImmutableArray();

        var output = string.Join(
            "\n",
            outputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, output);
    }
}