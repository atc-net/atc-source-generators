namespace PetStore.Domain.Options;

/// <summary>
/// Configuration options for the pet maintenance background service.
/// </summary>
[OptionsBinding("PetMaintenanceService")]
public partial class PetMaintenanceServiceOptions
{
    /// <summary>
    /// Gets or sets the repeat interval in seconds.
    /// Default: 60 seconds.
    /// </summary>
    public int RepeatIntervalInSeconds { get; set; } = 60;
}