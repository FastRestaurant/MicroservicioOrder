namespace OrderService.Application.DTOs;

public sealed class OrderSummaryDto
{
    public Guid Id { get; init; }
    public int TableNumber { get; init; }
    public Guid WaiterId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ItemCount { get; init; }
}
