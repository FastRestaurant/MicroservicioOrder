namespace OrderService.Application.DTOs;

public sealed class StockReleaseRequestDto
{
    public Guid OrderId { get; init; }
    public Guid OrderItemId { get; init; }
}
