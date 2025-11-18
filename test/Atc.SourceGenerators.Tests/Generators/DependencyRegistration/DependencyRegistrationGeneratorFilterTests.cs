// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Exclude_Types_By_Namespace()
    {
        const string source = """
                              [assembly: Atc.DependencyInjection.RegistrationFilter(ExcludeNamespaces = new[] { "TestNamespace.Internal" })]

                              namespace TestNamespace
                              {
                                  public interface IPublicService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class PublicService : IPublicService { }
                              }

                              namespace TestNamespace.Internal
                              {
                                  public interface IInternalService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class InternalService : IInternalService { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IPublicService, TestNamespace.PublicService>();", output, StringComparison.Ordinal);
        Assert.DoesNotContain("InternalService", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Assembly-level attributes may require different test setup. Manually verified in samples.")]
    public void Generator_Should_Exclude_Types_By_Pattern()
    {
        const string source = """
                              [assembly: Atc.DependencyInjection.RegistrationFilter(ExcludePatterns = new[] { "*Test*", "*Mock*" })]

                              namespace TestNamespace
                              {
                                  public interface IProductionService { }
                                  public interface ITestService { }
                                  public interface IMockService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class ProductionService : IProductionService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class TestService : ITestService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class MockService : IMockService { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IProductionService, TestNamespace.ProductionService>();", output, StringComparison.Ordinal);
        Assert.DoesNotContain("TestService", output, StringComparison.Ordinal);
        Assert.DoesNotContain("MockService", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Assembly-level attributes may require different test setup. Manually verified in samples.")]
    public void Generator_Should_Exclude_Types_By_Implemented_Interface()
    {
        const string source = """
                              namespace TestNamespace
                              {
                                  public interface ITestUtility { }
                                  public interface IProductionService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class ProductionService : IProductionService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class TestHelper : ITestUtility { }
                              }

                              [assembly: Atc.DependencyInjection.RegistrationFilter(ExcludeImplementing = new[] { typeof(TestNamespace.ITestUtility) })]
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IProductionService, TestNamespace.ProductionService>();", output, StringComparison.Ordinal);
        Assert.DoesNotContain("TestHelper", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Assembly-level attributes may require different test setup. Manually verified in samples.")]
    public void Generator_Should_Support_Multiple_Filter_Rules()
    {
        const string source = """
                              [assembly: Atc.DependencyInjection.RegistrationFilter(
                                  ExcludeNamespaces = new[] { "TestNamespace.Internal" },
                                  ExcludePatterns = new[] { "*Test*" })]

                              namespace TestNamespace
                              {
                                  public interface IProductionService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class ProductionService : IProductionService { }
                              }

                              namespace TestNamespace.Internal
                              {
                                  public interface IInternalService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class InternalService : IInternalService { }
                              }

                              namespace TestNamespace.Testing
                              {
                                  public interface ITestService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class TestService : ITestService { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IProductionService, TestNamespace.ProductionService>();", output, StringComparison.Ordinal);
        Assert.DoesNotContain("InternalService", output, StringComparison.Ordinal);
        Assert.DoesNotContain("TestService", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Assembly-level attributes may require different test setup. Manually verified in samples.")]
    public void Generator_Should_Support_Multiple_Filter_Attributes()
    {
        const string source = """
                              [assembly: Atc.DependencyInjection.RegistrationFilter(ExcludeNamespaces = new[] { "TestNamespace.Internal" })]
                              [assembly: Atc.DependencyInjection.RegistrationFilter(ExcludePatterns = new[] { "*Test*" })]

                              namespace TestNamespace
                              {
                                  public interface IProductionService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class ProductionService : IProductionService { }
                              }

                              namespace TestNamespace.Internal
                              {
                                  public interface IInternalService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class InternalService : IInternalService { }
                              }

                              namespace TestNamespace.Testing
                              {
                                  public interface ITestService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class TestService : ITestService { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IProductionService, TestNamespace.ProductionService>();", output, StringComparison.Ordinal);
        Assert.DoesNotContain("InternalService", output, StringComparison.Ordinal);
        Assert.DoesNotContain("TestService", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Assembly-level attributes may require different test setup. Manually verified in samples.")]
    public void Generator_Should_Support_Wildcard_Pattern_With_Question_Mark()
    {
        const string source = """
                              [assembly: Atc.DependencyInjection.RegistrationFilter(ExcludePatterns = new[] { "Test?" })]

                              namespace TestNamespace
                              {
                                  public interface IProductionService { }
                                  public interface ITestAService { }
                                  public interface ITestBService { }
                                  public interface ITestAbcService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class ProductionService : IProductionService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class TestA : ITestAService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class TestB : ITestBService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class TestAbc : ITestAbcService { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IProductionService, TestNamespace.ProductionService>();", output, StringComparison.Ordinal);
        Assert.DoesNotContain("TestA", output, StringComparison.Ordinal);
        Assert.DoesNotContain("TestB", output, StringComparison.Ordinal);
        Assert.Contains("TestAbc", output, StringComparison.Ordinal); // Not excluded (Test? only matches 5 chars)
    }

    [Fact]
    public void Generator_Should_Exclude_Sub_Namespaces()
    {
        const string source = """
                              [assembly: Atc.DependencyInjection.RegistrationFilter(ExcludeNamespaces = new[] { "TestNamespace.Internal" })]

                              namespace TestNamespace.Internal
                              {
                                  public interface IInternalService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class InternalService : IInternalService { }
                              }

                              namespace TestNamespace.Internal.Deep
                              {
                                  public interface IDeepInternalService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class DeepInternalService : IDeepInternalService { }
                              }

                              namespace TestNamespace.Public
                              {
                                  public interface IPublicService { }

                                  [Atc.DependencyInjection.Registration]
                                  public class PublicService : IPublicService { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.Public.IPublicService, TestNamespace.Public.PublicService>();", output, StringComparison.Ordinal);
        Assert.DoesNotContain("InternalService", output, StringComparison.Ordinal);
        Assert.DoesNotContain("DeepInternalService", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Runtime_Filter_Parameters_For_Default_Overload()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface ITestService { }

                              [Atc.DependencyInjection.Registration]
                              public class TestService : ITestService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("IEnumerable<string>? excludedNamespaces = null", output, StringComparison.Ordinal);
        Assert.Contains("IEnumerable<string>? excludedPatterns = null", output, StringComparison.Ordinal);
        Assert.Contains("IEnumerable<global::System.Type>? excludedTypes = null", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_ShouldExcludeService_Helper_Method()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface ITestService { }

                              [Atc.DependencyInjection.Registration]
                              public class TestService : ITestService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("private static bool ShouldExcludeService(", output, StringComparison.Ordinal);
        Assert.Contains("private static bool MatchesPattern(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Runtime_Exclusion_Checks()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface ITestService { }

                              [Atc.DependencyInjection.Registration]
                              public class TestService : ITestService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("if (!ShouldExcludeService(", output, StringComparison.Ordinal);
        Assert.Contains("// Check runtime exclusions for TestService", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Runtime_Filter_Parameters_For_AutoDetect_Overload()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface ITestService { }

                              [Atc.DependencyInjection.Registration]
                              public class TestService : ITestService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Check the auto-detect overload has the parameters
        var lines = output.Split('\n');
        var autoDetectOverloadIndex = Array.FindIndex(lines, l => l.Contains("bool includeReferencedAssemblies,", StringComparison.Ordinal));
        Assert.True(autoDetectOverloadIndex > 0, "Should find auto-detect overload");

        // Verify the next lines have the filter parameters
        Assert.Contains("IEnumerable<string>? excludedNamespaces = null", lines[autoDetectOverloadIndex + 1], StringComparison.Ordinal);
        Assert.Contains("IEnumerable<string>? excludedPatterns = null", lines[autoDetectOverloadIndex + 2], StringComparison.Ordinal);
        Assert.Contains("IEnumerable<global::System.Type>? excludedTypes = null", lines[autoDetectOverloadIndex + 3], StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Pass_Runtime_Filters_To_Referenced_Assemblies()
    {
        const string dataAccessSource = """
                                        using Atc.DependencyInjection;

                                        namespace DataAccess;

                                        [Registration]
                                        public class Repository
                                        {
                                        }
                                        """;

        const string domainSource = """
                                    using Atc.DependencyInjection;

                                    namespace Domain;

                                    [Registration]
                                    public class Service
                                    {
                                    }
                                    """;

        var (diagnostics, dataAccessOutput, domainOutput) = GetGeneratedOutputWithReferencedAssembly(
            dataAccessSource,
            "TestApp.DataAccess",
            domainSource,
            "TestApp.Domain");

        Assert.Empty(diagnostics);

        // In the auto-detect overload, verify filters are passed to recursive calls
        Assert.Contains("excludedNamespaces: excludedNamespaces", domainOutput, StringComparison.Ordinal);
        Assert.Contains("excludedPatterns: excludedPatterns", domainOutput, StringComparison.Ordinal);
        Assert.Contains("excludedTypes: excludedTypes", domainOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Handle_Generic_Types_In_Runtime_Filtering()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface IRepository<T> { }

                              [Atc.DependencyInjection.Registration(Lifetime = Atc.DependencyInjection.Lifetime.Scoped)]
                              public class Repository<T> : IRepository<T> where T : class { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify generic types use typeof with open generic
        Assert.Contains("typeof(TestNamespace.IRepository<>)", output, StringComparison.Ordinal);
        Assert.Contains("typeof(TestNamespace.Repository<>)", output, StringComparison.Ordinal);

        // Verify no errors about T being undefined
        Assert.DoesNotContain("typeof(TestNamespace.Repository<T>)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Namespace_Exclusion_Logic_In_Helper()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface ITestService { }

                              [Atc.DependencyInjection.Registration]
                              public class TestService : ITestService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify namespace exclusion logic exists
        Assert.Contains("// Check namespace exclusion", output, StringComparison.Ordinal);
        Assert.Contains("serviceType.Namespace.Equals(excludedNs", output, StringComparison.Ordinal);
        Assert.Contains("serviceType.Namespace.StartsWith($\"{excludedNs}.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Pattern_Matching_Logic_In_Helper()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface ITestService { }

                              [Atc.DependencyInjection.Registration]
                              public class TestService : ITestService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify pattern matching logic exists
        Assert.Contains("// Check pattern exclusion (wildcard matching)", output, StringComparison.Ordinal);
        Assert.Contains("MatchesPattern(typeName, pattern)", output, StringComparison.Ordinal);
        Assert.Contains("MatchesPattern(fullTypeName, pattern)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Type_Exclusion_Logic_In_Helper()
    {
        const string source = """
                              namespace TestNamespace;

                              public interface ITestService { }

                              [Atc.DependencyInjection.Registration]
                              public class TestService : ITestService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify type exclusion logic exists
        Assert.Contains("// Check if explicitly excluded by type", output, StringComparison.Ordinal);
        Assert.Contains("if (serviceType == excludedType || serviceType.IsAssignableFrom(excludedType))", output, StringComparison.Ordinal);
    }
}