using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;

public sealed class GetOrdersByStatusQueryHandler : IGetOrdersByStatusQueryHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStatusRepository _statusRepository;

    public GetOrdersByStatusQueryHandler(
        IOrderRepository orderRepository,
        IStatusRepository statusRepository)
    {
        _orderRepository = orderRepository;
        _statusRepository = statusRepository;
    }

    public async Task<IEnumerable<OrderSummaryDto>> Handle(GetOrdersByStatusQuery query, CancellationToken cancellationToken = default)
    {
        var status = await _statusRepository.GetByNameAsync(query.Status, StatusTypes.Order, cancellationToken)
            ?? throw new DomainException($"'{query.Status}' no es un estado valido para una orden.");

        var orders = await _orderRepository.GetByStatusAsync(status.Id, cancellationToken);
        return orders.Select(MapOrder);
    }

    private static OrderSummaryDto MapOrder(Order order)
    {
        return new OrderSummaryDto
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
}
