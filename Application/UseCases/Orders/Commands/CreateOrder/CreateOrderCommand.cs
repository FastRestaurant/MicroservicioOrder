namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommand
{
    public Guid TableId { get; init; }
    public Guid WaiterId { get; init; }
    public IReadOnlyCollection<CreateOrderItemCommand> Items { get; init; } = Array.Empty<CreateOrderItemCommand>();
}

public sealed class CreateOrderItemCommand
{
    public Guid ProductId { get; init; }
    public string ProductType { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}
