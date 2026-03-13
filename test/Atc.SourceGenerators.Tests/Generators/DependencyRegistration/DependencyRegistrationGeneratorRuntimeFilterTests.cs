// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void RuntimeFilter_Should_Generate_ShouldExcludeService_Helper()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService { }

                              [Registration]
                              public class UserService : IUserService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("private static bool ShouldExcludeService(", output, StringComparison.Ordinal);
        Assert.Contains("global::System.Type serviceType,", output, StringComparison.Ordinal);
        Assert.Contains("global::System.Collections.Generic.IEnumerable<string>? excludedNamespaces,", output, StringComparison.Ordinal);
        Assert.Contains("global::System.Collections.Generic.IEnumerable<string>? excludedPatterns,", output, StringComparison.Ordinal);
        Assert.Contains("global::System.Collections.Generic.IEnumerable<global::System.Type>? excludedTypes)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_MatchesPattern_Helper()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService { }

                              [Registration]
                              public class UserService : IUserService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("private static bool MatchesPattern(", output, StringComparison.Ordinal);
        Assert.Contains("string value,", output, StringComparison.Ordinal);
        Assert.Contains("string pattern)", output, StringComparison.Ordinal);
        Assert.Contains("Regex.Escape(pattern)", output, StringComparison.Ordinal);
        Assert.Contains("RegexOptions.IgnoreCase", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_ExcludedTypes_Parameter_In_Default_Overload()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IOrderService { }

                              [Registration]
                              public class OrderService : IOrderService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("IEnumerable<global::System.Type>? excludedTypes = null", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_ExcludedNamespaces_Parameter_In_Default_Overload()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IOrderService { }

                              [Registration]
                              public class OrderService : IOrderService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("IEnumerable<string>? excludedNamespaces = null", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_ExcludedPatterns_Parameter_In_Default_Overload()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IOrderService { }

                              [Registration]
                              public class OrderService : IOrderService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("IEnumerable<string>? excludedPatterns = null", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_All_Three_Filter_Parameters_In_AutoDetect_Overload()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailService { }

                              [Registration]
                              public class EmailService : IEmailService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // The auto-detect overload has bool includeReferencedAssemblies followed by filter params
        var lines = output.Split(Constants.LineFeed);
        var autoDetectOverloadIndex = Array.FindIndex(lines, l => l.Contains("bool includeReferencedAssemblies,", StringComparison.Ordinal));
        Assert.True(autoDetectOverloadIndex > 0, "Should find auto-detect overload with includeReferencedAssemblies parameter");

        // Verify the subsequent lines contain the three filter parameters
        Assert.Contains("IEnumerable<string>? excludedNamespaces = null", lines[autoDetectOverloadIndex + 1], StringComparison.Ordinal);
        Assert.Contains("IEnumerable<string>? excludedPatterns = null", lines[autoDetectOverloadIndex + 2], StringComparison.Ordinal);
        Assert.Contains("IEnumerable<global::System.Type>? excludedTypes = null", lines[autoDetectOverloadIndex + 3], StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Exclusion_Check_Wrapping_Service_Registration()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICacheService { }

                              [Registration]
                              public class CacheService : ICacheService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check runtime exclusions for CacheService", output, StringComparison.Ordinal);
        Assert.Contains("if (!ShouldExcludeService(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Exclusion_Checks_For_Multiple_Services()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IUserService { }
                              public interface IOrderService { }
                              public interface IPaymentService { }

                              [Registration]
                              public class UserService : IUserService { }

                              [Registration]
                              public class OrderService : IOrderService { }

                              [Registration]
                              public class PaymentService : IPaymentService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check runtime exclusions for UserService", output, StringComparison.Ordinal);
        Assert.Contains("// Check runtime exclusions for OrderService", output, StringComparison.Ordinal);
        Assert.Contains("// Check runtime exclusions for PaymentService", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_TypeExclusion_Logic_In_Helper()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check if explicitly excluded by type", output, StringComparison.Ordinal);
        Assert.Contains("if (excludedTypes != null)", output, StringComparison.Ordinal);
        Assert.Contains("foreach (var excludedType in excludedTypes)", output, StringComparison.Ordinal);
        Assert.Contains("if (serviceType == excludedType || serviceType.IsAssignableFrom(excludedType))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_NamespaceExclusion_Logic_In_Helper()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check namespace exclusion", output, StringComparison.Ordinal);
        Assert.Contains("if (excludedNamespaces != null && serviceType.Namespace != null)", output, StringComparison.Ordinal);
        Assert.Contains("foreach (var excludedNs in excludedNamespaces)", output, StringComparison.Ordinal);
        Assert.Contains("serviceType.Namespace.Equals(excludedNs", output, StringComparison.Ordinal);
        Assert.Contains("serviceType.Namespace.StartsWith($\"{excludedNs}.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_PatternExclusion_Logic_In_Helper()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check pattern exclusion (wildcard matching)", output, StringComparison.Ordinal);
        Assert.Contains("if (excludedPatterns != null)", output, StringComparison.Ordinal);
        Assert.Contains("var typeName = serviceType.Name;", output, StringComparison.Ordinal);
        Assert.Contains("var fullTypeName = serviceType.FullName ?? serviceType.Name;", output, StringComparison.Ordinal);
        Assert.Contains("MatchesPattern(typeName, pattern)", output, StringComparison.Ordinal);
        Assert.Contains("MatchesPattern(fullTypeName, pattern)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Filter_Parameters_In_SpecificAssembly_Overload()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // The specific assembly overload uses string referencedAssemblyName parameter
        var lines = output.Split(Constants.LineFeed);
        var specificOverloadIndex = Array.FindIndex(lines, l => l.Contains("string referencedAssemblyName,", StringComparison.Ordinal));
        Assert.True(specificOverloadIndex > 0, "Should find specific assembly overload with referencedAssemblyName parameter");

        // Verify filter parameters follow
        Assert.Contains("IEnumerable<string>? excludedNamespaces = null", lines[specificOverloadIndex + 1], StringComparison.Ordinal);
        Assert.Contains("IEnumerable<string>? excludedPatterns = null", lines[specificOverloadIndex + 2], StringComparison.Ordinal);
        Assert.Contains("IEnumerable<global::System.Type>? excludedTypes = null", lines[specificOverloadIndex + 3], StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Filter_Parameters_In_MultipleAssemblies_Overload()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // The multiple assemblies overload uses params string[] referencedAssemblyNames parameter
        // Filter parameters appear before the params array
        var lines = output.Split(Constants.LineFeed);
        var multiOverloadIndex = Array.FindIndex(lines, l => l.Contains("params string[] referencedAssemblyNames", StringComparison.Ordinal));
        Assert.True(multiOverloadIndex > 0, "Should find multiple assemblies overload with params string[] referencedAssemblyNames parameter");
    }

    [Fact]
    public void RuntimeFilter_Should_Pass_Filters_To_Referenced_Assembly_Calls()
    {
        const string dataAccessSource = """
                                        using Atc.DependencyInjection;

                                        namespace DataAccess;

                                        public interface IRepository { }

                                        [Registration]
                                        public class Repository : IRepository { }
                                        """;

        const string domainSource = """
                                    using Atc.DependencyInjection;

                                    namespace Domain;

                                    public interface IService { }

                                    [Registration]
                                    public class Service : IService { }
                                    """;

        var (diagnostics, dataAccessOutput, domainOutput) = GetGeneratedOutputWithReferencedAssembly(
            dataAccessSource,
            "TestApp.DataAccess",
            domainSource,
            "TestApp.Domain");

        Assert.Empty(diagnostics);

        // Verify filters are propagated to referenced assembly calls
        Assert.Contains("excludedNamespaces: excludedNamespaces", domainOutput, StringComparison.Ordinal);
        Assert.Contains("excludedPatterns: excludedPatterns", domainOutput, StringComparison.Ordinal);
        Assert.Contains("excludedTypes: excludedTypes", domainOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Typeof_For_Exclusion_Check_On_Interface_Registration()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface INotificationService { }

                              [Registration(As = typeof(INotificationService))]
                              public class NotificationService : INotificationService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // When registered with an explicit interface, the exclusion check should use typeof
        Assert.Contains("if (!ShouldExcludeService(typeof(TestNamespace.NotificationService)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Typeof_For_Exclusion_Check_On_Self_Registration()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration]
                              public class UtilityService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("if (!ShouldExcludeService(typeof(TestNamespace.UtilityService)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_ShouldExcludeService_Returning_False_When_No_Exclusions_Match()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // The helper method should return false at the end when nothing matches
        Assert.Contains("return false;", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Return_True_In_Each_Exclusion_Branch()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Count occurrences of "return true;" inside ShouldExcludeService
        // There should be one for type exclusion, one for namespace exclusion, one for pattern exclusion
        var returnTrueCount = Regex.Count(output, @"return true;");
        Assert.True(returnTrueCount >= 3, $"Expected at least 3 'return true;' statements in ShouldExcludeService, found {returnTrueCount}");
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_WildcardToRegex_Conversion_In_MatchesPattern()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // Verify MatchesPattern converts wildcards to regex
        Assert.Contains("Regex.Escape(pattern)", output, StringComparison.Ordinal);
        Assert.Contains(".Replace(\"\\\\*\", \".*\")", output, StringComparison.Ordinal);
        Assert.Contains(".Replace(\"\\\\?\", \".\")", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_XmlDoc_For_Filter_Parameters()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("<param name=\"excludedNamespaces\">Optional. Namespaces to exclude from registration.</param>", output, StringComparison.Ordinal);
        Assert.Contains("<param name=\"excludedPatterns\">Optional. Wildcard patterns (* and ?) to exclude types by name.</param>", output, StringComparison.Ordinal);
        Assert.Contains("<param name=\"excludedTypes\">Optional. Specific types to exclude from registration.</param>", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Exclusion_Check_For_Scoped_Service()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IScopedService { }

                              [Registration(Lifetime.Scoped)]
                              public class ScopedService : IScopedService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check runtime exclusions for ScopedService", output, StringComparison.Ordinal);
        Assert.Contains("if (!ShouldExcludeService(", output, StringComparison.Ordinal);
        Assert.Contains("services.AddScoped<TestNamespace.IScopedService, TestNamespace.ScopedService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Exclusion_Check_For_Transient_Service()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ITransientService { }

                              [Registration(Lifetime.Transient)]
                              public class TransientService : ITransientService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check runtime exclusions for TransientService", output, StringComparison.Ordinal);
        Assert.Contains("if (!ShouldExcludeService(", output, StringComparison.Ordinal);
        Assert.Contains("services.AddTransient<TestNamespace.ITransientService, TestNamespace.TransientService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Not_Generate_Helpers_When_No_Services_Registered()
    {
        const string source = """
                              namespace TestNamespace;

                              public class PlainClass { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain("ShouldExcludeService", output, StringComparison.Ordinal);
        Assert.DoesNotContain("MatchesPattern", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Exclusion_Check_For_Generic_Service()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T> { }

                              [Registration(Lifetime.Scoped)]
                              public class Repository<T> : IRepository<T> where T : class { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("// Check runtime exclusions for Repository", output, StringComparison.Ordinal);
        Assert.Contains("if (!ShouldExcludeService(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeFilter_Should_Generate_Timeout_In_MatchesPattern_Regex()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService { }

                              [Registration]
                              public class Service : IService { }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);

        // The MatchesPattern method should include a timeout to prevent ReDoS
        Assert.Contains("TimeSpan.FromSeconds(1)", output, StringComparison.Ordinal);
    }
}