namespace OrderService.Application.UseCases.Orders.Commands.AddOrderItem;

public sealed class AddOrderItemCommand
{
    public Guid OrderId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductType { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}
