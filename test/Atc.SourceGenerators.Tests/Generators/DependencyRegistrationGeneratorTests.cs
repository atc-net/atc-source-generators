// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators;

public class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Attribute_Definition()
    {
        var source = string.Empty;
        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public enum Lifetime", output, StringComparison.Ordinal);
        Assert.Contains("public sealed class RegistrationAttribute", output, StringComparison.Ordinal);
        Assert.Contains("Singleton = 0", output, StringComparison.Ordinal);
        Assert.Contains("Scoped = 1", output, StringComparison.Ordinal);
        Assert.Contains("Transient = 2", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Simple_Service_With_Default_Singleton_Lifetime()
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
        Assert.Contains("AddDependencyRegistrationsFromTestAssembly", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.UserService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Service_With_Singleton_Lifetime()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration(Lifetime.Singleton)]
                              public class CacheService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.CacheService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Service_With_Transient_Lifetime()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration(Lifetime.Transient)]
                              public class LoggerService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddTransient<TestNamespace.LoggerService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Service_As_Interface()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService
                              {
                              }

                              [Registration(As = typeof(IUserService))]
                              public class UserService : IUserService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IUserService, TestNamespace.UserService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Service_As_Interface_And_Self()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService
                              {
                              }

                              [Registration(As = typeof(IUserService), AsSelf = true)]
                              public class UserService : IUserService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IUserService, TestNamespace.UserService>()", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.UserService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Multiple_Services()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration(Lifetime.Singleton)]
                              public class CacheService
                              {
                              }

                              [Registration]
                              public class UserService
                              {
                              }

                              [Registration(Lifetime.Transient)]
                              public class LoggerService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.CacheService>()", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.UserService>()", output, StringComparison.Ordinal);
        Assert.Contains("services.AddTransient<TestNamespace.LoggerService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_As_Type_Is_Not_Interface()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public class BaseService
                              {
                              }

                              [Registration(As = typeof(BaseService))]
                              public class UserService : BaseService
                              {
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCDIR001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        var message = diagnostic.GetMessage(null);
        Assert.Contains("BaseService", message, StringComparison.Ordinal);
        Assert.Contains("must be an interface", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Class_Does_Not_Implement_Interface()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService
                              {
                              }

                              [Registration(As = typeof(IUserService))]
                              public class UserService
                              {
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        Assert.NotEmpty(diagnostics);
        var diagnostic = Assert.Single(diagnostics, d => d.Id == "ATCDIR002");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        var message = diagnostic.GetMessage(null);
        Assert.Contains("UserService", message, StringComparison.Ordinal);
        Assert.Contains("does not implement interface", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Warn_About_Duplicate_Registrations_With_Different_Lifetimes()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService
                              {
                              }

                              [Registration(Lifetime.Singleton, As = typeof(IUserService))]
                              public class UserServiceSingleton : IUserService
                              {
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IUserService))]
                              public class UserServiceScoped : IUserService
                              {
                              }
                              """;

        var (diagnostics, _) = GetGeneratedOutput(source);

        var warning = Assert.Single(diagnostics, d => d.Id == "ATCDIR003");
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("registered multiple times", warning.GetMessage(null), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Not_Generate_Extension_Method_When_No_Services_Decorated()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public class UserService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("AddDependencyRegistrationsFromTestAssembly", output, StringComparison.Ordinal);
        Assert.Contains("public enum Lifetime", output, StringComparison.Ordinal); // Attribute should still be generated
    }

    [Fact]
    public void Generator_Should_Auto_Detect_Single_Interface()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService
                              {
                              }

                              [Registration]
                              public class UserService : IUserService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IUserService, TestNamespace.UserService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Auto_Detect_Multiple_Interfaces()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailService
                              {
                              }

                              public interface INotificationService
                              {
                              }

                              [Registration]
                              public class EmailService : IEmailService, INotificationService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IEmailService, TestNamespace.EmailService>()", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.INotificationService, TestNamespace.EmailService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Filter_Out_System_Interfaces()
    {
        const string source = """
                              using Atc.DependencyInjection;
                              using System;

                              namespace TestNamespace;

                              [Registration]
                              public class CacheService : IDisposable
                              {
                                  public void Dispose() { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.CacheService>()", output, StringComparison.Ordinal);
        Assert.DoesNotContain("IDisposable", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Support_Explicit_As_Parameter_Override()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService
                              {
                              }

                              public interface INotificationService
                              {
                              }

                              [Registration(As = typeof(IUserService))]
                              public class UserService : IUserService, INotificationService
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.IUserService, TestNamespace.UserService>()", output, StringComparison.Ordinal);
        Assert.DoesNotContain("INotificationService", output, StringComparison.Ordinal);
    }

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

        // Check that all 4 overloads are generated
        Assert.Contains("AddDependencyRegistrationsFromTestAssembly(this IServiceCollection services)", output, StringComparison.Ordinal);
        Assert.Contains("AddDependencyRegistrationsFromTestAssembly(\n        this IServiceCollection services,\n        bool includeReferencedAssemblies)", output, StringComparison.Ordinal);
        Assert.Contains("AddDependencyRegistrationsFromTestAssembly(\n        this IServiceCollection services,\n        string referencedAssemblyName)", output, StringComparison.Ordinal);
        Assert.Contains("AddDependencyRegistrationsFromTestAssembly(\n        this IServiceCollection services,\n        params string[] referencedAssemblyNames)", output, StringComparison.Ordinal);
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
        Assert.Contains("services.AddDependencyRegistrationsFromDataAccess(includeReferencedAssemblies: true)", domainOutput, StringComparison.Ordinal);
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
        Assert.Contains("services.AddDependencyRegistrationsFromInfrastructure(includeReferencedAssemblies: true)", outputs["TestApp.DataAccess"], StringComparison.Ordinal);

        // Domain references DataAccess (which transitively references Infrastructure)
        // Smart naming: TestApp.Domain → "Domain" (unique suffix)
        Assert.Contains("AddDependencyRegistrationsFromDomain", outputs["TestApp.Domain"], StringComparison.Ordinal);
        Assert.Contains("services.AddDependencyRegistrationsFromDataAccess(includeReferencedAssemblies: true)", outputs["TestApp.Domain"], StringComparison.Ordinal);
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
        Assert.Contains("services.AddDependencyRegistrationsFromLogging(includeReferencedAssemblies: true)", domainOutput, StringComparison.Ordinal);

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

        var generator = new DependencyRegistrationGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = outputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCDIR", StringComparison.Ordinal))
            .ToImmutableArray();

        var output = string.Join(
            "\n",
            outputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, output);
    }

    [SuppressMessage("", "S1854:Remove this useless assignment to local variable", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, string ReferencedOutput, string CurrentOutput) GetGeneratedOutputWithReferencedAssembly(
        string referencedSource,
        string referencedAssemblyName,
        string currentSource,
        string currentAssemblyName)
    {
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        // Step 1: Compile referenced assembly
        var referencedSyntaxTree = CSharpSyntaxTree.ParseText(referencedSource);
        var referencedCompilation = CSharpCompilation.Create(
            referencedAssemblyName,
            [referencedSyntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var referencedGenerator = new DependencyRegistrationGenerator();
        var referencedDriver = CSharpGeneratorDriver.Create(referencedGenerator);
        referencedDriver = (CSharpGeneratorDriver)referencedDriver.RunGeneratorsAndUpdateCompilation(
            referencedCompilation,
            out var referencedOutputCompilation,
            out _);

        // Get referenced assembly output
        var referencedOutput = string.Join(
            "\n",
            referencedOutputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        // Step 2: Compile current assembly with reference to the first
        var currentSyntaxTree = CSharpSyntaxTree.ParseText(currentSource);

        // Create an in-memory reference to the referenced compilation
        var referencedAssemblyReference = referencedOutputCompilation.ToMetadataReference();
        var currentReferences = references
            .Concat([referencedAssemblyReference])
            .ToList();

        var currentCompilation = CSharpCompilation.Create(
            currentAssemblyName,
            [currentSyntaxTree],
            currentReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var currentGenerator = new DependencyRegistrationGenerator();
        var currentDriver = CSharpGeneratorDriver.Create(currentGenerator);
        currentDriver = (CSharpGeneratorDriver)currentDriver.RunGeneratorsAndUpdateCompilation(
            currentCompilation,
            out var currentOutputCompilation,
            out var generatorDiagnostics);

        var allDiagnostics = currentOutputCompilation
            .GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                        d.Id.StartsWith("ATCDIR", StringComparison.Ordinal))
            .ToImmutableArray();

        var currentOutput = string.Join(
            "\n",
            currentOutputCompilation
                .SyntaxTrees
                .Skip(1)
                .Select(tree => tree.ToString()));

        return (allDiagnostics, referencedOutput, currentOutput);
    }

    [SuppressMessage("", "S1854:Remove this useless assignment to local variable", Justification = "OK")]
    private static (ImmutableArray<Diagnostic> Diagnostics, Dictionary<string, string> Outputs) GetGeneratedOutputWithMultipleReferencedAssemblies(
        List<(string Source, string AssemblyName)> assemblies)
    {
        var references = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                               !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilations = new Dictionary<string, Compilation>(StringComparer.Ordinal);
        var outputs = new Dictionary<string, string>(StringComparer.Ordinal);
        var allDiagnostics = new List<Diagnostic>();

        // Compile assemblies in order, adding each to references for the next
        foreach (var (source, assemblyName) in assemblies)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(
                assemblyName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new DependencyRegistrationGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var generatorDiagnostics);

            allDiagnostics.AddRange(
                outputCompilation
                    .GetDiagnostics()
                    .Concat(generatorDiagnostics)
                    .Where(d => d.Severity >= DiagnosticSeverity.Warning &&
                                d.Id.StartsWith("ATCDIR", StringComparison.Ordinal)));

            var output = string.Join(
                "\n",
                outputCompilation
                    .SyntaxTrees
                    .Skip(1)
                    .Select(tree => tree.ToString()));

            outputs[assemblyName] = output;
            compilations[assemblyName] = outputCompilation;

            // Add this compilation as a reference for subsequent compilations
            references.Add(outputCompilation.ToMetadataReference());
        }

        return (allDiagnostics.ToImmutableArray(), outputs);
    }
}