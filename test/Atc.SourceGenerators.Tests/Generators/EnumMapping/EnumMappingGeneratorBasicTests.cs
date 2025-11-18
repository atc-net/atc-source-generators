namespace Atc.SourceGenerators.Tests.Generators.EnumMapping;

public partial class EnumMappingGeneratorTests
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
        Assert.Contains("=> source switch", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Available => TestNamespace.TargetStatus.Available,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Pending => TestNamespace.TargetStatus.Pending,", output, StringComparison.Ordinal);
        Assert.Contains("TestNamespace.SourceStatus.Adopted => TestNamespace.TargetStatus.Adopted,", output, StringComparison.Ordinal);
        Assert.Contains("_ => throw new global::System.ArgumentOutOfRangeException(nameof(source), source, \"Unmapped enum value\"),", output, StringComparison.Ordinal);
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
        Assert.Contains("=> source switch", output, StringComparison.Ordinal);
        Assert.Contains("DataAccess.Entities.StatusEntity.None => Domain.Models.Status.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("DataAccess.Entities.StatusEntity.Active => Domain.Models.Status.Active,", output, StringComparison.Ordinal);
        Assert.Contains("DataAccess.Entities.StatusEntity.Inactive => Domain.Models.Status.Inactive,", output, StringComparison.Ordinal);
    }
}