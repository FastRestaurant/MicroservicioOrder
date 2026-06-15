namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommand
{
    public Guid TableId { get; init; }
    public Guid WaiterId { get; init; }
}