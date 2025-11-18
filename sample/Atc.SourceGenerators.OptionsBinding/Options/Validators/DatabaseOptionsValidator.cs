namespace Atc.SourceGenerators.OptionsBinding.Options.Validators;

/// <summary>
/// Custom validator for DatabaseOptions.
/// Enforces business rules that can't be expressed with DataAnnotations.
/// </summary>
public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        DatabaseOptions options)
    {
        var failures = new List<string>();

        // Ensure timeout is at least 10 seconds (database calls need reasonable timeouts)
        if (options.TimeoutSeconds < 10)
        {
            failures.Add("TimeoutSeconds must be at least 10 seconds for reliable database operations");
        }

        // Ensure the connection string looks valid (contains Server or Data Source)
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            var connStr = options.ConnectionString.ToLowerInvariant();
            if (!connStr.Contains("server=", StringComparison.Ordinal) &&
                !connStr.Contains("data source=", StringComparison.Ordinal))
            {
                failures.Add("ConnectionString must contain 'Server=' or 'Data Source=' parameter");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}