namespace PetStore.Domain.Services;

/// <summary>
/// Decorator that adds logging to pet service operations.
/// Demonstrates the decorator pattern for cross-cutting concerns like logging/auditing.
/// </summary>
[Registration(Decorator = true)]
public class LoggingPetServiceDecorator : IPetService
{
    private readonly IPetService inner;
    private readonly ILogger<LoggingPetServiceDecorator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingPetServiceDecorator"/> class.
    /// </summary>
    /// <param name="inner">The inner pet service implementation.</param>
    /// <param name="logger">The logger instance.</param>
    public LoggingPetServiceDecorator(
        IPetService inner,
        ILogger<LoggingPetServiceDecorator> logger)
    {
        this.inner = inner;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public Pet? GetById(Guid id)
    {
        logger.LogInformation("GetById called with id: {PetId}", id);
        var result = inner.GetById(id);
        logger.LogInformation("GetById returned: {Found}", result != null ? "found" : "not found");
        return result;
    }

    /// <inheritdoc/>
    public IEnumerable<Pet> GetAll()
    {
        logger.LogInformation("GetAll called");
        var result = inner
            .GetAll()
            .ToList();
        logger.LogInformation("GetAll returned {Count} pets", result.Count);
        return result;
    }

    /// <inheritdoc/>
    public IEnumerable<Pet> GetByStatus(Models.PetStatus status)
    {
        logger.LogInformation("GetByStatus called with status: {Status}", status);
        var result = inner
            .GetByStatus(status)
            .ToList();
        logger.LogInformation("GetByStatus returned {Count} pets", result.Count);
        return result;
    }

    /// <inheritdoc/>
    public Pet CreatePet(CreatePetRequest request)
    {
        logger.LogInformation("CreatePet called for pet: {PetName} ({Species})", request.Name, request.Species);
        var result = inner.CreatePet(request);
        logger.LogInformation("CreatePet created pet with id: {PetId}", result.Id);
        return result;
    }
}
