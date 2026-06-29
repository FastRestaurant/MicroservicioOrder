namespace OrderService.Application.UseCases.Orders.Commands.MarkOrderReadyByKitchen;

public sealed class MarkOrderReadyByKitchenCommand
{
    public Guid OrderId { get; init; }
    public bool WasDelayed { get; init; }
}
