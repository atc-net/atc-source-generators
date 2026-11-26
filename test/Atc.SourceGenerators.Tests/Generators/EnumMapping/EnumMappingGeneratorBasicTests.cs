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
        Assert.Contains("public static global::TestNamespace.TargetStatus MapToTargetStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this global::TestNamespace.SourceStatus source)", output, StringComparison.Ordinal);
        Assert.Contains("=> source switch", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.Available => global::TestNamespace.TargetStatus.Available,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.Pending => global::TestNamespace.TargetStatus.Pending,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.Adopted => global::TestNamespace.TargetStatus.Adopted,", output, StringComparison.Ordinal);
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
        Assert.Contains("global::TestNamespace.SourceStatus.Available => global::TestNamespace.TargetStatus.available,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.Pending => global::TestNamespace.TargetStatus.PENDING,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceStatus.ADOPTED => global::TestNamespace.TargetStatus.Adopted,", output, StringComparison.Ordinal);
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
        Assert.Contains("public static global::Domain.Models.Status MapToStatus(", output, StringComparison.Ordinal);
        Assert.Contains("this global::DataAccess.Entities.StatusEntity source)", output, StringComparison.Ordinal);
        Assert.Contains("=> source switch", output, StringComparison.Ordinal);
        Assert.Contains("global::DataAccess.Entities.StatusEntity.None => global::Domain.Models.Status.Unknown,", output, StringComparison.Ordinal);
        Assert.Contains("global::DataAccess.Entities.StatusEntity.Active => global::Domain.Models.Status.Active,", output, StringComparison.Ordinal);
        Assert.Contains("global::DataAccess.Entities.StatusEntity.Inactive => global::Domain.Models.Status.Inactive,", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Escape_Enum_Members_Named_Like_Keywords()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace;

                              public enum TargetKind
                              {
                                  @class,
                                  @event,
                              }

                              [MapTo(typeof(TargetKind))]
                              public enum SourceKind
                              {
                                  @class,
                                  @event,
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("global::TestNamespace.SourceKind.@class => global::TestNamespace.TargetKind.@class,", output, StringComparison.Ordinal);
        Assert.Contains("global::TestNamespace.SourceKind.@event => global::TestNamespace.TargetKind.@event,", output, StringComparison.Ordinal);
    }
}