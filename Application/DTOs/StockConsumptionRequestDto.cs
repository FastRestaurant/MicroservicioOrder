namespace OrderService.Application.DTOs;

public sealed class StockConsumptionRequestDto
{
    public Guid OrderId { get; init; }
    public Guid OrderItemId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductType { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
