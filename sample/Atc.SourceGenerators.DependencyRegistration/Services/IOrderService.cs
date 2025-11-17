namespace Atc.SourceGenerators.DependencyRegistration.Services;

public interface IOrderService
{
    Task PlaceOrderAsync(string orderId);
}
