namespace OrderService.Application.DTOs;

public sealed class OrderResponseDto
{
    public Guid Id { get; init; }
    public int TableNumber { get; init; }
    public Guid WaiterId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public byte[] Version { get; init; } = [];
    public IReadOnlyCollection<OrderItemResponseDto> Items { get; init; } = Array.Empty<OrderItemResponseDto>();
    public IReadOnlyCollection<OrderNoteResponseDto> Notes { get; init; } = Array.Empty<OrderNoteResponseDto>();
    public IReadOnlyCollection<OrderStatusHistoryDto> StatusHistory { get; init; } = Array.Empty<OrderStatusHistoryDto>();
}
