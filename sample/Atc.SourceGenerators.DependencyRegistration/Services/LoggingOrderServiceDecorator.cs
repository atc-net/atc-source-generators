namespace Atc.SourceGenerators.DependencyRegistration.Services;

[Registration(Lifetime.Scoped, As = typeof(IOrderService), Decorator = true)]
public class LoggingOrderServiceDecorator : IOrderService
{
    private readonly IOrderService inner;

    public LoggingOrderServiceDecorator(IOrderService inner)
    {
        this.inner = inner;
    }

    public async Task PlaceOrderAsync(string orderId)
    {
        Console.WriteLine($"[LoggingDecorator] Before placing order {orderId}");
        await this.inner.PlaceOrderAsync(orderId);
        Console.WriteLine($"[LoggingDecorator] After placing order {orderId}");
    }
}
