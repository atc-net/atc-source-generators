namespace Atc.SourceGenerators.DependencyRegistration.Services;

[Registration(Lifetime.Scoped, As = typeof(IOrderService))]
public class OrderService : IOrderService
{
    public Task PlaceOrderAsync(string orderId)
    {
        Console.WriteLine($"[OrderService] Processing order {orderId}");
        return Task.CompletedTask;
    }
}
