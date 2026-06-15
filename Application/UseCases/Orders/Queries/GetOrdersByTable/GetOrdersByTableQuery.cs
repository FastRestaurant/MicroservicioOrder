namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;

public sealed class GetOrdersByTableQuery
{
    public Guid TableId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
