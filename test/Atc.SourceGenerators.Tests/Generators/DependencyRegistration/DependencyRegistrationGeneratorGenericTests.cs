// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
namespace Atc.SourceGenerators.Tests.Generators.DependencyRegistration;

public partial class DependencyRegistrationGeneratorTests
{
    [Fact]
    public void Generator_Should_Register_Generic_Repository_With_One_Type_Parameter()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T> where T : class
                              {
                                  T? GetById(int id);
                                  void Save(T entity);
                              }

                              [Registration(Lifetime.Scoped)]
                              public class Repository<T> : IRepository<T> where T : class
                              {
                                  public T? GetById(int id) => default;
                                  public void Save(T entity) { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped(typeof(TestNamespace.IRepository<>), typeof(TestNamespace.Repository<>))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Generic_Handler_With_Two_Type_Parameters()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IHandler<TRequest, TResponse>
                              {
                                  TResponse Handle(TRequest request);
                              }

                              [Registration(Lifetime.Transient)]
                              public class Handler<TRequest, TResponse> : IHandler<TRequest, TResponse>
                              {
                                  public TResponse Handle(TRequest request) => default!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddTransient(typeof(TestNamespace.IHandler<,>), typeof(TestNamespace.Handler<,>))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Generic_Service_With_Explicit_As_Parameter()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T> where T : class
                              {
                                  T? GetById(int id);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IRepository<>))]
                              public class Repository<T> : IRepository<T> where T : class
                              {
                                  public T? GetById(int id) => default;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped(typeof(TestNamespace.IRepository<>), typeof(TestNamespace.Repository<>))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Generic_Service_With_Multiple_Constraints()
    {
        const string source = """
                              using System;
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IEntity
                              {
                                  int Id { get; }
                              }

                              public interface IRepository<T> where T : class, IEntity, new()
                              {
                                  T? GetById(int id);
                              }

                              [Registration(Lifetime.Scoped)]
                              public class Repository<T> : IRepository<T> where T : class, IEntity, new()
                              {
                                  public T? GetById(int id) => default;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped(typeof(TestNamespace.IRepository<>), typeof(TestNamespace.Repository<>))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Generic_Service_With_Three_Type_Parameters()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IMapper<TSource, TTarget, TContext>
                              {
                                  TTarget Map(TSource source, TContext context);
                              }

                              [Registration(Lifetime.Singleton)]
                              public class Mapper<TSource, TTarget, TContext> : IMapper<TSource, TTarget, TContext>
                              {
                                  public TTarget Map(TSource source, TContext context) => default!;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddSingleton(typeof(TestNamespace.IMapper<,,>), typeof(TestNamespace.Mapper<,,>))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Generic_Service_As_Self()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T> where T : class
                              {
                                  T? GetById(int id);
                              }

                              [Registration(Lifetime.Scoped, As = typeof(IRepository<>), AsSelf = true)]
                              public class Repository<T> : IRepository<T> where T : class
                              {
                                  public T? GetById(int id) => default;
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped(typeof(TestNamespace.IRepository<>), typeof(TestNamespace.Repository<>))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddScoped(typeof(TestNamespace.Repository<>))", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_Should_Register_Both_Generic_And_NonGeneric_Services()
    {
        const string source = """
                              using Atc.DependencyInjection;

                              namespace TestNamespace;

                              public interface IRepository<T> where T : class
                              {
                                  T? GetById(int id);
                              }

                              public interface IUserService
                              {
                                  void DoWork();
                              }

                              [Registration(Lifetime.Scoped)]
                              public class Repository<T> : IRepository<T> where T : class
                              {
                                  public T? GetById(int id) => default;
                              }

                              [Registration]
                              public class UserService : IUserService
                              {
                                  public void DoWork() { }
                              }
                              """;

        var (diagnostics, output) = GetGeneratedOutput(source);

        Assert.Empty(diagnostics);
        Assert.Contains("services.AddScoped(typeof(TestNamespace.IRepository<>), typeof(TestNamespace.Repository<>))", output, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<TestNamespace.IUserService, TestNamespace.UserService>()", output, StringComparison.Ordinal);
    }
}