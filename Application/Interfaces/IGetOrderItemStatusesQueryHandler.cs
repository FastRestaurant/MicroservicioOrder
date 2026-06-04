using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetOrderItemStatuses;

namespace OrderService.Application.Interfaces;

public interface IGetOrderItemStatusesQueryHandler
{
    Task<IEnumerable<StatusResponseDto>> Handle(GetOrderItemStatusesQuery query, CancellationToken cancellationToken = default);
}
