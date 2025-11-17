// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedParameter.Local
namespace PetStore.Domain.BackgroundServices;

/// <summary>
/// Background service that performs periodic pet maintenance tasks every 30 seconds.
/// </summary>
[Registration(Lifetime.Singleton)]
public class PetMaintenanceService : BackgroundService
{
    private readonly IPetRepository petRepository;
    private readonly ILogger<PetMaintenanceService> logger;
    private readonly TimeSpan interval;

    /// <summary>
    /// Initializes a new instance of the <see cref="PetMaintenanceService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The service options.</param>
    /// <param name="petRepository">The pet repository.</param>
    public PetMaintenanceService(
        ILogger<PetMaintenanceService> logger,
        IOptions<PetMaintenanceServiceOptions> options,
        IPetRepository petRepository)
    {
        this.logger = logger;
        this.petRepository = petRepository;
        this.interval = TimeSpan.FromSeconds(options.Value.RepeatIntervalInSeconds);
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "PetMaintenanceService started. Will execute every {Interval} seconds",
            interval.TotalSeconds);

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("PetMaintenanceService is stopping");
        }
    }

    /// <summary>
    /// Performs the maintenance work on all pets.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "OK - Testing")]
    private Task DoWorkAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PetMaintenanceService: Starting pet maintenance");

        var now = DateTimeOffset.UtcNow;
        const string modifiedBy = nameof(PetMaintenanceService);

        // Get all pets and update their modification tracking
        var pets = petRepository
            .GetAll()
            .ToList();

        foreach (var pet in pets)
        {
            pet.ModifiedAt = now;
            pet.ModifiedBy = modifiedBy;
        }

        logger.LogInformation(
            "PetMaintenanceService: Updated {Count} pets with ModifiedAt={ModifiedAt}, ModifiedBy={ModifiedBy}",
            pets.Count,
            now,
            modifiedBy);

        return Task.CompletedTask;
    }
}