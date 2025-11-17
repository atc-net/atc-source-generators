namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// PayPal payment processor - registered with key "PayPal".
/// Demonstrates keyed service registration for multiple implementations of the same interface.
/// </summary>
[Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "PayPal")]
public class PayPalPaymentProcessor : IPaymentProcessor
{
    public string ProviderName => "PayPal";

    public Task<bool> ProcessPaymentAsync(
        decimal amount,
        string currency)
    {
        Console.WriteLine($"PayPalPaymentProcessor: Processing ${amount} {currency}");
        return Task.FromResult(true);
    }
}