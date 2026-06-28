namespace OrderService.Application.UseCases.Orders.Queries.GetActiveOrdersSummaryByTable;

public sealed class GetActiveOrdersSummaryByTableQuery
{
    public Guid TableId { get; init; }
}
