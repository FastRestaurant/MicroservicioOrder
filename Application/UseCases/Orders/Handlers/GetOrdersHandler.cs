using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Queries;

namespace OrderService.Application.UseCases.Orders.Handlers;

public class GetAllOrdersHandler(IOrderRepository repository)
{
    public async Task<IEnumerable<OrderSummaryDto>> HandleAsync(GetAllOrdersQuery query, CancellationToken ct = default)
    {
        var orders = await repository.GetAllAsync(ct);
        return orders.Select(o => o.ToSummaryDto());
    }
}

public class GetOrdersByStatusHandler(IOrderRepository repository)
{
    public async Task<IEnumerable<OrderSummaryDto>> HandleAsync(GetOrdersByStatusQuery query, CancellationToken ct = default)
    {
        var orders = await repository.GetByStatusAsync(query.Status, ct);
        return orders.Select(o => o.ToSummaryDto());
    }
}

public class GetOrdersByTableHandler(IOrderRepository repository)
{
    public async Task<IEnumerable<OrderSummaryDto>> HandleAsync(GetOrdersByTableQuery query, CancellationToken ct = default)
    {
        var orders = await repository.GetByTableAsync(query.TableNumber, ct);
        return orders.Select(o => o.ToSummaryDto());
    }
}
