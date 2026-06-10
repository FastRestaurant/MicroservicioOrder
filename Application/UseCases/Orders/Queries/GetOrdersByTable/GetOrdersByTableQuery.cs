namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;

public sealed class GetOrdersByTableQuery
{
    public Guid TableId { get; init; }
}