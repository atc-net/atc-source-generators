namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T>
    where T : class, IEntity
{
    T? GetById(int id);

    IEnumerable<T> GetAll();

    void Add(T entity);

    void Delete(int id);
}