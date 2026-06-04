namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommand
{
    public int TableNumber { get; init; }
    public Guid WaiterId { get; init; }
}
