namespace Atc.SourceGenerators.Tests.Generators.EnumMapping;

public partial class EnumMappingGeneratorTests
{
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
        Assert.Contains("global::TestNamespace.SourceStatus.None => global::TestNamespace.TargetStatus.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.Available => global::TestNamespace.TargetStatus.Available,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.Pending => global::TestNamespace.TargetStatus.Pending,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.Adopted => global::TestNamespace.TargetStatus.Adopted,", output, StringComparison.Ordinal);
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
        Assert.Contains("global::TestNamespace.PetStatusEntity.None => global::TestNamespace.PetStatusDto.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.PetStatusEntity.Pending => global::TestNamespace.PetStatusDto.Pending,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.PetStatusEntity.Available => global::TestNamespace.PetStatusDto.Available,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.PetStatusEntity.Adopted => global::TestNamespace.PetStatusDto.Adopted,", output, StringComparison.Ordinal);
        Assert.Contains("_ => throw new global::System.ArgumentOutOfRangeException(nameof(source), source, \"Unmapped enum value\"),", output, StringComparison.Ordinal);
    }
}