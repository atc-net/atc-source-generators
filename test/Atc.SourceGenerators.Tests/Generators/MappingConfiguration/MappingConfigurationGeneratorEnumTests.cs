// ReSharper disable StringLiteralTypo
namespace Atc.SourceGenerators.Tests.Generators.MappingConfiguration;

public partial class MappingConfigurationGeneratorTests
{
    [Fact]
    public void Config_Should_AutoDetect_Enum_Mapping_For_Matching_Properties()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public enum ContactType { Individual, Company, Government }

                                  public class Contact
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public ContactType Type { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public enum CustomerCategory { Individual, Company, Government }

                                  public class Customer
                                  {
                                      public int Id { get; set; }
                                      public string Name { get; set; } = string.Empty;
                                      public CustomerCategory Type { get; set; }
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Customer MapToCustomer(this ExternalLib.Contact source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MapToCustomerCategory", output, StringComparison.Ordinal);
        Assert.Contains("source.Type.MapToCustomerCategory()", output, StringComparison.Ordinal);

        // Should generate the enum mapping method
        Assert.Contains("global::ExternalLib.ContactType.Individual => global::MyApp.CustomerCategory.Individual", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Handle_Same_Enum_Type_Direct_Assignment()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace Shared
                              {
                                  public enum Status { Active, Inactive }
                              }

                              namespace ExternalLib
                              {
                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public Shared.Status Status { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public Shared.Status Status { get; set; }
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Same enum type should use direct assignment, not conversion
        Assert.Contains("Status = source.Status", output, StringComparison.Ordinal);
        Assert.DoesNotContain("MapToStatus", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_AutoDetect_SpecialCases_None_Unknown()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace ExternalLib
                              {
                                  public enum ExternalStatus { None, Active, Inactive }

                                  public class Source
                                  {
                                      public int Id { get; set; }
                                      public ExternalStatus Status { get; set; }
                                  }
                              }

                              namespace MyApp
                              {
                                  public enum InternalStatus { Unknown, Active, Inactive }

                                  public class Target
                                  {
                                      public int Id { get; set; }
                                      public InternalStatus Status { get; set; }
                                  }
                              }

                              namespace MyApp.Mappings
                              {
                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial MyApp.Target MapToTarget(this ExternalLib.Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Should map None → Unknown via special case detection
        Assert.Contains("global::ExternalLib.ExternalStatus.None => global::MyApp.InternalStatus.Unknown", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Config_Should_Escape_AutoDetected_Enum_Members_Named_Like_Keywords()
    {
        const string source = """
                              using Atc.SourceGenerators.Annotations;

                              namespace TestNamespace
                              {
                                  public enum TargetKind { @class, @event }

                                  public enum SourceKind { @class, @event }

                                  public class Source
                                  {
                                      public SourceKind Kind { get; set; }
                                  }

                                  public class Target
                                  {
                                      public TargetKind Kind { get; set; }
                                  }

                                  [MappingConfiguration]
                                  public static partial class Mappings
                                  {
                                      public static partial Target MapToTarget(this Source source);
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source, "CS");

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("SourceKind.@class => global::TestNamespace.TargetKind.@class,", output, StringComparison.Ordinal);
    }
}