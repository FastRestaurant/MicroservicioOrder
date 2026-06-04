namespace OrderService.Application.DTOs;

public sealed class OrderItemResponseDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductType { get; init; } = string.Empty;
    public string ProductNameSnapshot { get; init; } = string.Empty;
    public decimal UnitPriceSnapshot { get; init; }
    public int DurationMinutesSnapshot { get; init; }
    public int Quantity { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime? SentToKitchenAt { get; init; }
    public DateTime? ReadyAt { get; init; }
}
