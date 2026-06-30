namespace OrderService.Application.UseCases.Orders.Commands.NotifyOrderReadyByKitchen;

public sealed class NotifyOrderReadyByKitchenCommand
{
    public Guid OrderId { get; init; }
    public bool WasDelayed { get; init; }
}
