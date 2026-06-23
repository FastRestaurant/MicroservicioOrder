namespace OrderService.Application.DTOs;

public sealed class KitchenTicketRequestDto
{
    public Guid OrderId { get; init; }
    public Guid TableId { get; init; }
    public int TableNumber { get; init; }
    public Guid WaiterId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public IReadOnlyCollection<KitchenTicketItemDto> Items { get; init; } = Array.Empty<KitchenTicketItemDto>();
}
