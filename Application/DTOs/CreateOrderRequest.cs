namespace OrderService.Application.DTOs;

public sealed class CreateOrderRequest
{
    public Guid TableId { get; init; }
    public IReadOnlyCollection<CreateOrderItemRequest> Items { get; init; } = Array.Empty<CreateOrderItemRequest>();
}
