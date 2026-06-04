namespace OrderService.Application.DTOs;

public sealed class OrderStatusHistoryDto
{
    public Guid Id { get; init; }
    public string? PreviousStatusName { get; init; }
    public string NewStatus { get; init; } = string.Empty;
    public Guid ChangedByUserId { get; init; }
    public DateTime ChangedAt { get; init; }
}
