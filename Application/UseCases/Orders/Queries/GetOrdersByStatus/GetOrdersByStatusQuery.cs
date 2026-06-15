namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;

public sealed class GetOrdersByStatusQuery
{
    public string Status { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
