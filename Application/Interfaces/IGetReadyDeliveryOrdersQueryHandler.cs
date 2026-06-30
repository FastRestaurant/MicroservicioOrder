using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetReadyDeliveryOrders;

namespace OrderService.Application.Interfaces;

public interface IGetReadyDeliveryOrdersQueryHandler
{
    Task<IReadOnlyCollection<OrderResponseDto>> Handle(GetReadyDeliveryOrdersQuery query, CancellationToken cancellationToken = default);
}
