using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;

namespace OrderService.Application.Interfaces;

public interface IGetOrdersByStatusQueryHandler
{
    Task<PagedResponseDto<OrderSummaryDto>> Handle(GetOrdersByStatusQuery query, CancellationToken cancellationToken = default);
}
