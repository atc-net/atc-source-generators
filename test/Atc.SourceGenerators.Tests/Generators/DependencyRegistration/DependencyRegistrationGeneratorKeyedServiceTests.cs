// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Register_Keyed_Service_With_String_Key()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IPaymentProcessor
                              {
                                  void ProcessPayment(decimal amount);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Stripe")]
                              public class StripePaymentProcessor : IPaymentProcessor
                              {
                                  public void ProcessPayment(decimal amount) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedScoped<TestNamespace.IPaymentProcessor, TestNamespace.StripePaymentProcessor>(\"Stripe\")", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Multiple_Keyed_Services_With_Different_Keys()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IPaymentProcessor
                              {
                                  void ProcessPayment(decimal amount);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Stripe")]
                              public class StripePaymentProcessor : IPaymentProcessor
                              {
                                  public void ProcessPayment(decimal amount) { }
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "PayPal")]
                              public class PayPalPaymentProcessor : IPaymentProcessor
                              {
                                  public void ProcessPayment(decimal amount) { }
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Square")]
                              public class SquarePaymentProcessor : IPaymentProcessor
                              {
                                  public void ProcessPayment(decimal amount) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedScoped<TestNamespace.IPaymentProcessor, TestNamespace.StripePaymentProcessor>(\"Stripe\")", output, StringComparison.Ordinal);
        Assert.Contains("services.AddKeyedScoped<TestNamespace.IPaymentProcessor, TestNamespace.PayPalPaymentProcessor>(\"PayPal\")", output, StringComparison.Ordinal);
        Assert.Contains("services.AddKeyedScoped<TestNamespace.IPaymentProcessor, TestNamespace.SquarePaymentProcessor>(\"Square\")", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Keyed_Singleton_Service()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface ICacheProvider
                              {
                                  object? Get(string key);
                              }

                              [Registration(Lifetime.Singleton, As = typeof(ICacheProvider), Key = "Redis")]
                              public class RedisCacheProvider : ICacheProvider
                              {
                                  public object? Get(string key) => null;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedSingleton<TestNamespace.ICacheProvider, TestNamespace.RedisCacheProvider>(\"Redis\")", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Keyed_Transient_Service()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface INotificationService
                              {
                                  void Send(string message);
                              }

                              [Registration(Lifetime.Transient, As = typeof(INotificationService), Key = "Email")]
                              public class EmailNotificationService : INotificationService
                              {
                                  public void Send(string message) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedTransient<TestNamespace.INotificationService, TestNamespace.EmailNotificationService>(\"Email\")", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Keyed_Generic_Service()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T> where T : class
                              {
                                  T? GetById(int id);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IRepository<>), Key = "Primary")]
                              public class PrimaryRepository<T> : IRepository<T> where T : class
                              {
                                  public T? GetById(int id) => default;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedScoped(typeof(TestNamespace.IRepository<>), \"Primary\", typeof(TestNamespace.PrimaryRepository<>))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Mixed_Keyed_And_NonKeyed_Services()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IPaymentProcessor
                              {
                                  void ProcessPayment(decimal amount);
                              }

                              public interface IUserService
                              {
                                  void CreateUser(string name);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Stripe")]
                              public class StripePaymentProcessor : IPaymentProcessor
                              {
                                  public void ProcessPayment(decimal amount) { }
                              }

                              [Registration(Lifetime.Scoped)]
                              public class UserService : IUserService
                              {
                                  public void CreateUser(string name) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedScoped<TestNamespace.IPaymentProcessor, TestNamespace.StripePaymentProcessor>(\"Stripe\")", output, StringComparison.Ordinal);
        Assert.Contains("services.AddScoped<TestNamespace.IUserService, TestNamespace.UserService>()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Escape_String_Key()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IPaymentProcessor
                              {
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Pay\"Pal\\EU")]
                              public class PayPalProcessor : IPaymentProcessor
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedScoped<TestNamespace.IPaymentProcessor, TestNamespace.PayPalProcessor>(\"Pay\\\"Pal\\\\EU\")", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Emit_Bool_And_Numeric_Keys_As_Invariant_Literals()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IA
                              {
                              }

                              public interface IB
                              {
                              }

                              public interface IC
                              {
                              }

                              [Registration(As = typeof(IA), Key = true)]
                              public class BoolKeyed : IA
                              {
                              }

                              [Registration(As = typeof(IB), Key = 1.5)]
                              public class DoubleKeyed : IB
                              {
                              }

                              [Registration(As = typeof(IC), Key = 42L)]
                              public class LongKeyed : IC
                              {
                              }
                              """;

        // A culture with ',' as decimal separator must not leak into the generated code
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("da-DK");

            var (diagnostics, output) = GetGeneratedOutput(source);

            Assert.Empty(diagnostics);
            Assert.Contains("services.AddKeyedSingleton<TestNamespace.IA, TestNamespace.BoolKeyed>(true)", output, StringComparison.Ordinal);
            Assert.Contains("services.AddKeyedSingleton<TestNamespace.IB, TestNamespace.DoubleKeyed>(1.5D)", output, StringComparison.Ordinal);
            Assert.Contains("services.AddKeyedSingleton<TestNamespace.IC, TestNamespace.LongKeyed>(42L)", output, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Generator_Should_Emit_Enum_Key_With_Its_Enum_Type()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public enum PaymentKind
                              {
                                  Card,
                                  Invoice,
                              }

                              public interface IPaymentProcessor
                              {
                              }

                              [Registration(As = typeof(IPaymentProcessor), Key = PaymentKind.Invoice)]
                              public class InvoiceProcessor : IPaymentProcessor
                              {
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddKeyedSingleton<TestNamespace.IPaymentProcessor, TestNamespace.InvoiceProcessor>(global::TestNamespace.PaymentKind.Invoice)", output, StringComparison.Ordinal);
    }
}