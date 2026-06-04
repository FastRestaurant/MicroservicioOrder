using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetOrderStatuses;

namespace OrderService.Application.Interfaces;

public interface IGetOrderStatusesQueryHandler
{
    Task<IEnumerable<StatusResponseDto>> Handle(GetOrderStatusesQuery query, CancellationToken cancellationToken = default);
}
