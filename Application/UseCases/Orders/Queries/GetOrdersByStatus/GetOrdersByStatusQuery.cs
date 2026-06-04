namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;

public sealed class GetOrdersByStatusQuery
{
    public string Status { get; init; } = string.Empty;
}
