namespace OrderService.Application.DTOs;

public sealed class TableOrdersSummaryDto
{
    public Guid TableId { get; init; }
    public decimal Total { get; init; }
    public bool CanClose { get; init; }
    public IReadOnlyCollection<OrderSummaryDto> Orders { get; init; } = Array.Empty<OrderSummaryDto>();
}
