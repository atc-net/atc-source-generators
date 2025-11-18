namespace PetStore.Domain.Options.Validators;

/// <summary>
/// Custom validator for PetStoreOptions.
/// Enforces business rules beyond DataAnnotations.
/// </summary>
public class PetStoreOptionsValidator : IValidateOptions<PetStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, PetStoreOptions options)
    {
        var failures = new List<string>();

        // Ensure MaxPetsPerPage is a reasonable value for pagination (multiple of 5 or 10)
        if (options.MaxPetsPerPage % 5 != 0)
        {
            failures.Add("MaxPetsPerPage should be a multiple of 5 for better pagination UX (e.g., 5, 10, 15, 20, 25, ...)");
        }

        // Warn if MaxPetsPerPage is too large (performance concern)
        if (options.MaxPetsPerPage > 50)
        {
            failures.Add("MaxPetsPerPage should not exceed 50 to maintain good performance and user experience");
        }

        // Ensure store name doesn't contain invalid characters
        if (options.StoreName.Contains('<') || options.StoreName.Contains('>'))
        {
            failures.Add("StoreName cannot contain HTML/XML tags (< or > characters)");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
