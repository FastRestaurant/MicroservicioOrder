namespace OrderService.Application.UseCases.Orders.Commands.RemoveItemFromOrder;

public sealed class RemoveItemFromOrderCommand
{
    public Guid OrderId { get; init; }
    public Guid ItemId { get; init; }
}
