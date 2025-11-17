namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Stripe payment processor - registered with key "Stripe".
/// Demonstrates keyed service registration for multiple implementations of the same interface.
/// </summary>
[Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Stripe")]
public class StripePaymentProcessor : IPaymentProcessor
{
    public string ProviderName => "Stripe";

    public Task<bool> ProcessPaymentAsync(
        decimal amount,
        string currency)
    {
        Console.WriteLine($"StripePaymentProcessor: Processing ${amount} {currency}");
        return Task.FromResult(true);
    }
}