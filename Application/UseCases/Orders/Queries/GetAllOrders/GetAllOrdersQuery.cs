namespace OrderService.Application.UseCases.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
