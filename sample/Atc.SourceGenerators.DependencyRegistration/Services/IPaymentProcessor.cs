namespace Atc.SourceGenerators.DependencyRegistration.Services;

/// <summary>
/// Interface for payment processing.
/// </summary>
public interface IPaymentProcessor
{
    string ProviderName { get; }

    Task<bool> ProcessPaymentAsync(
        decimal amount,
        string currency);
}