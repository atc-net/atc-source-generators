namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Generic repository implementation - scoped lifetime for database context scenarios.
/// Demonstrates open generic interface registration: typeof(IRepository&lt;&gt;), typeof(Repository&lt;&gt;).
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
[Registration(Lifetime.Scoped)]
public class Repository<T> : IRepository<T>
    where T : class, IEntity
{
    private readonly List<T> storage = [];

    public T? GetById(int id)
    {
        var entity = storage.FirstOrDefault(e => e.Id == id);
        Console.WriteLine($"Repository<{typeof(T).Name}>.GetById({id}) -> {(entity != null ? "Found" : "Not found")}");
        return entity;
    }

    public IEnumerable<T> GetAll()
    {
        Console.WriteLine($"Repository<{typeof(T).Name}>.GetAll() -> {storage.Count} entities");
        return storage;
    }

    public void Add(T entity)
    {
        storage.Add(entity);
        Console.WriteLine($"Repository<{typeof(T).Name}>.Add() -> Added entity with Id: {entity.Id}");
    }

    public void Delete(int id)
    {
        var entity = storage.FirstOrDefault(e => e.Id == id);
        if (entity is not null)
        {
            storage.Remove(entity);
            Console.WriteLine($"Repository<{typeof(T).Name}>.Delete({id}) -> Deleted");
        }
        else
        {
            Console.WriteLine($"Repository<{typeof(T).Name}>.Delete({id}) -> Not found");
        }
    }
}
