namespace OrderService.Application.DTOs;

public sealed class OrderSummaryDto
{
    public Guid Id { get; init; }
    public Guid TableId { get; init; }
    public string TableNumber { get; init; } = string.Empty;
    public Guid WaiterId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ItemCount { get; init; }
}