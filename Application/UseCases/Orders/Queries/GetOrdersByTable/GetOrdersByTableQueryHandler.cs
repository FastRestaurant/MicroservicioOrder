using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;

public sealed class GetOrdersByTableQueryHandler : IGetOrdersByTableQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByTableQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderSummaryDto>> Handle(
        GetOrdersByTableQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByTableAsync(query.TableId, cancellationToken);
        return orders.Select(MapOrder);
    }

    private static OrderSummaryDto MapOrder(Order order) => new()
    {
        Id = order.Id,
        TableId = order.TableId,
        TableNumber = order.Table.Number,
        WaiterId = order.WaiterId,
        Status = order.Status.Name,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        ItemCount = order.Items.Count
    };
}