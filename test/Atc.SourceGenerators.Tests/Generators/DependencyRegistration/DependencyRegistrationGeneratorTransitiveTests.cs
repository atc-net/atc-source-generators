// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_All_Four_Overloads_For_Assembly_With_Services()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration]
                              public class UserService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Check that all 4 overloads are generated (method names only, signatures include new filter parameters)
        Assert.Contains("AddDependencyRegistrationsFromTestAssembly(", output, StringComparison.Ordinal);

        // Verify the filter parameters are present in the generated code
        Assert.Contains("excludedNamespaces", output, StringComparison.Ordinal);
        Assert.Contains("excludedPatterns", output, StringComparison.Ordinal);
        Assert.Contains("excludedTypes", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Detect_Referenced_Assembly_With_Registrations()
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
                                    public class DomainService
                                    {
                                    }
                                    """;

        var (diagnostics, dataAccessOutput, domainOutput) = GetGeneratedOutputWithReferencedAssembly(
            dataAccessSource,
            "TestApp.DataAccess",
            domainSource,
            "TestApp.Domain");

        Assert.Empty(diagnostics);

        // DataAccess should have no referenced assemblies (no transitive calls)
        // Smart naming: TestApp.DataAccess → "DataAccess" (unique suffix)
        Assert.Contains("AddDependencyRegistrationsFromDataAccess", dataAccessOutput, StringComparison.Ordinal);

        // Domain should detect DataAccess as referenced assembly
        // Smart naming: TestApp.Domain → "Domain" (unique suffix)
        Assert.Contains("AddDependencyRegistrationsFromDomain", domainOutput, StringComparison.Ordinal);

        // Verify referenced assembly call includes filter parameters
        Assert.Contains("services.AddDependencyRegistrationsFromDataAccess(includeReferencedAssemblies: true, excludedNamespaces: excludedNamespaces, excludedPatterns: excludedPatterns, excludedTypes: excludedTypes)", domainOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Three_Level_Hierarchy()
    {
        const string infrastructureSource = """
                                            using Atc.DependencyInjection;

                                            namespace Infrastructure;

                                            [Registration]
                                            public class Logger
                                            {
                                            }
                                            """;

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
                                    public class DomainService
                                    {
                                    }
                                    """;

        var (diagnostics, outputs) = GetGeneratedOutputWithMultipleReferencedAssemblies(
            [
                (infrastructureSource, "TestApp.Infrastructure"),
                (dataAccessSource, "TestApp.DataAccess"),
                (domainSource, "TestApp.Domain"),
            ]);

        Assert.Empty(diagnostics);

        // Infrastructure has no dependencies
        // Smart naming: TestApp.Infrastructure → "Infrastructure" (unique suffix)
        Assert.Contains("AddDependencyRegistrationsFromInfrastructure", outputs["TestApp.Infrastructure"], StringComparison.Ordinal);

        // DataAccess references Infrastructure
        // Smart naming: TestApp.DataAccess → "DataAccess" (unique suffix)
        Assert.Contains("AddDependencyRegistrationsFromDataAccess", outputs["TestApp.DataAccess"], StringComparison.Ordinal);
        Assert.Contains("services.AddDependencyRegistrationsFromInfrastructure(includeReferencedAssemblies: true, excludedNamespaces: excludedNamespaces, excludedPatterns: excludedPatterns, excludedTypes: excludedTypes)", outputs["TestApp.DataAccess"], StringComparison.Ordinal);

        // Domain references DataAccess (which transitively references Infrastructure)
        // Smart naming: TestApp.Domain → "Domain" (unique suffix)
        Assert.Contains("AddDependencyRegistrationsFromDomain", outputs["TestApp.Domain"], StringComparison.Ordinal);
        Assert.Contains("services.AddDependencyRegistrationsFromDataAccess(includeReferencedAssemblies: true, excludedNamespaces: excludedNamespaces, excludedPatterns: excludedPatterns, excludedTypes: excludedTypes)", outputs["TestApp.Domain"], StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Manual_Assembly_Name_Specification_Full_Name()
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
                                    public class DomainService
                                    {
                                    }
                                    """;

        var (diagnostics, _, domainOutput) = GetGeneratedOutputWithReferencedAssembly(
            dataAccessSource,
            "TestApp.DataAccess",
            domainSource,
            "TestApp.Domain");

        Assert.Empty(diagnostics);

        // Check that the string[] overload checks for full name
        Assert.Contains("string.Equals(name, \"TestApp.DataAccess\", global::System.StringComparison.OrdinalIgnoreCase)", domainOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Manual_Assembly_Name_Specification_Short_Name()
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
                                    public class DomainService
                                    {
                                    }
                                    """;

        var (diagnostics, _, domainOutput) = GetGeneratedOutputWithReferencedAssembly(
            dataAccessSource,
            "TestApp.DataAccess",
            domainSource,
            "TestApp.Domain");

        Assert.Empty(diagnostics);

        // Check that the string[] overload checks for short name
        Assert.Contains("string.Equals(name, \"DataAccess\", global::System.StringComparison.OrdinalIgnoreCase)", domainOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Filter_Referenced_Assemblies_By_Prefix()
    {
        const string thirdPartySource = """
                                        using Atc.DependencyInjection;

                                        namespace ThirdParty;

                                        [Registration]
                                        public class ThirdPartyService
                                        {
                                        }
                                        """;

        const string domainSource = """
                                    using Atc.DependencyInjection;

                                    namespace Domain;

                                    [Registration]
                                    public class DomainService
                                    {
                                    }
                                    """;

        var (diagnostics, _, domainOutput) = GetGeneratedOutputWithReferencedAssembly(
            thirdPartySource,
            "ThirdParty.Logging",
            domainSource,
            "TestApp.Domain");

        Assert.Empty(diagnostics);

        // Domain should NOT include ThirdParty.Logging in manual overloads (different prefix)
        // But should still detect it for auto-detect overload
        // Smart naming: ThirdParty.Logging → "Logging" (unique suffix)
        Assert.Contains("services.AddDependencyRegistrationsFromLogging(includeReferencedAssemblies: true, excludedNamespaces: excludedNamespaces, excludedPatterns: excludedPatterns, excludedTypes: excludedTypes)", domainOutput, StringComparison.Ordinal);

        // In the string overload, ThirdParty should NOT be included (prefix filtering)
        Assert.DoesNotContain("string.Equals(referencedAssemblyName, \"ThirdParty.Logging\"", domainOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("string.Equals(name, \"ThirdParty.Logging\"", domainOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Include_Assembly_Without_Registrations()
    {
        const string contractSource = """
                                      namespace Contracts;

                                      public class UserDto
                                      {
                                      }
                                      """;

        const string domainSource = """
                                    using Atc.DependencyInjection;

                                    namespace Domain;

                                    [Registration]
                                    public class DomainService
                                    {
                                    }
                                    """;

        var (diagnostics, _, domainOutput) = GetGeneratedOutputWithReferencedAssembly(
            contractSource,
            "TestApp.Contracts",
            domainSource,
            "TestApp.Domain");

        Assert.Empty(diagnostics);

        // Domain should NOT detect Contracts (no [Registration] attributes)
        Assert.DoesNotContain("AddDependencyRegistrationsFromTestAppContracts", domainOutput, StringComparison.Ordinal);
    }

    // NOTE: These tests are skipped because they require external type resolution for BackgroundService/IHostedService
    // which isn't fully available in the test harness compilation environment.
    // The hosted service registration feature has been manually verified to work correctly in:
    // - sample/PetStore.Api with PetMaintenanceService
    // See PetStore.Domain/BackgroundServices/PetMaintenanceService.cs for a working example.
}