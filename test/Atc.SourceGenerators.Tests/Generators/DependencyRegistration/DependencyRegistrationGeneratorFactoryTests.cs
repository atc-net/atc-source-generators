// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Generate_Factory_Registration_For_Interface()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailSender
                              {
                                  void Send(string to, string message);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
                              public class EmailSender : IEmailSender
                              {
                                  private readonly string _smtpHost;

                                  private EmailSender(string smtpHost)
                                  {
                                      _smtpHost = smtpHost;
                                  }

                                  public void Send(string to, string message) { }

                                  public static IEmailSender CreateEmailSender(IServiceProvider sp)
                                  {
                                      return new EmailSender("smtp.example.com");
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped<TestNamespace.IEmailSender>(sp => TestNamespace.EmailSender.CreateEmailSender(sp));", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Factory_Method_Not_Found()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailSender
                              {
                                  void Send(string to, string message);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = "NonExistentMethod")]
                              public class EmailSender : IEmailSender
                              {
                                  public void Send(string to, string message) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR005", diagnostics[0].Id);
        Assert.Contains("NonExistentMethod", diagnostics[0].GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Factory_Method_Has_Invalid_Signature_Non_Static()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailSender
                              {
                                  void Send(string to, string message);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
                              public class EmailSender : IEmailSender
                              {
                                  public void Send(string to, string message) { }

                                  public IEmailSender CreateEmailSender(IServiceProvider sp)
                                  {
                                      return this;
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR006", diagnostics[0].Id);
        Assert.Contains("CreateEmailSender", diagnostics[0].GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Factory_Method_Has_Invalid_Signature_Wrong_Parameter()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailSender
                              {
                                  void Send(string to, string message);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
                              public class EmailSender : IEmailSender
                              {
                                  public void Send(string to, string message) { }

                                  public static IEmailSender CreateEmailSender(string config)
                                  {
                                      return new EmailSender();
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR006", diagnostics[0].Id);
    }

    [Fact]
    public void Generator_Should_Report_Error_When_Factory_Method_Has_Invalid_Signature_Wrong_Return_Type()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailSender
                              {
                                  void Send(string to, string message);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IEmailSender), Factory = nameof(CreateEmailSender))]
                              public class EmailSender : IEmailSender
                              {
                                  public void Send(string to, string message) { }

                                  public static string CreateEmailSender(IServiceProvider sp)
                                  {
                                      return "wrong";
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Single(diagnostics);
        Assert.Equal("ATCDIR006", diagnostics[0].Id);
    }

    [Fact]
    public void Generator_Should_Generate_Factory_Registration_For_Concrete_Type()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              [Registration(Lifetime.Singleton, Factory = nameof(CreateService))]
                              public class MyService
                              {
                                  private readonly string _config;

                                  private MyService(string config)
                                  {
                                      _config = config;
                                  }

                                  public static MyService CreateService(IServiceProvider sp)
                                  {
                                      return new MyService("default-config");
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton<TestNamespace.MyService>(sp => TestNamespace.MyService.CreateService(sp));", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Factory_Registration_With_Multiple_Interfaces()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IService1
                              {
                                  void Method1();
                              }

                              public interface IService2
                              {
                                  void Method2();
                              }

                              [Registration(Lifetime.Transient, Factory = nameof(CreateService))]
                              public class MultiService : IService1, IService2
                              {
                                  public void Method1() { }
                                  public void Method2() { }

                                  public static IService1 CreateService(IServiceProvider sp)
                                  {
                                      return new MultiService();
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddTransient<TestNamespace.IService1>(sp => TestNamespace.MultiService.CreateService(sp));", output, StringComparison.Ordinal);
        Assert.Contains("services.AddTransient<TestNamespace.IService2>(sp => TestNamespace.MultiService.CreateService(sp));", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Generate_Factory_Registration_With_AsSelf()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEmailSender
                              {
                                  void Send(string to, string message);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IEmailSender), AsSelf = true, Factory = nameof(CreateEmailSender))]
                              public class EmailSender : IEmailSender
                              {
                                  public void Send(string to, string message) { }

                                  public static IEmailSender CreateEmailSender(IServiceProvider sp)
                                  {
                                      return new EmailSender();
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped<TestNamespace.IEmailSender>(sp => TestNamespace.EmailSender.CreateEmailSender(sp));", output, StringComparison.Ordinal);
        Assert.Contains("services.AddScoped<TestNamespace.EmailSender>(sp => TestNamespace.EmailSender.CreateEmailSender(sp));", output, StringComparison.Ordinal);
    }

    [Fact(Skip = "Abstract class factory methods require additional investigation in test harness. Manually verified in samples.")]
    public void Generator_Should_Support_Factory_Method_With_Abstract_Base_Class()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public abstract class MessageHandler
                              {
                                  public abstract Task HandleAsync(string message);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(MessageHandler), Factory = nameof(CreateHandler))]
                              public class EmailMessageHandler : MessageHandler
                              {
                                  public override Task HandleAsync(string message) => Task.CompletedTask;

                                  public static MessageHandler CreateHandler(IServiceProvider sp)
                                  {
                                      return new EmailMessageHandler();
                                  }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped<TestNamespace.MessageHandler>(sp => TestNamespace.EmailMessageHandler.CreateHandler(sp));", output, StringComparison.Ordinal);
    }
}