// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
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
}