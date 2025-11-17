namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Square payment processor - registered with key "Square".
/// Demonstrates keyed service registration for multiple implementations of the same interface.
/// </summary>
[Registration(Lifetime.Scoped, As = typeof(IPaymentProcessor), Key = "Square")]
public class SquarePaymentProcessor : IPaymentProcessor
{
    public string ProviderName => "Square";

    public Task<bool> ProcessPaymentAsync(
        decimal amount,
        string currency)
    {
        Console.WriteLine($"SquarePaymentProcessor: Processing ${amount} {currency}");
        return Task.FromResult(true);
    }
}