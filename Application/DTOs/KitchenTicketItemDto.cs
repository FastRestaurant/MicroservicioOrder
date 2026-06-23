namespace OrderService.Application.DTOs;

public sealed class KitchenTicketItemDto
{
    public Guid OrderItemId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductType { get; init; } = string.Empty;
    public int DurationMinutes { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}
