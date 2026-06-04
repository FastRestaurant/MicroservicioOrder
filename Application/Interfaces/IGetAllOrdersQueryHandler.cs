using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetAllOrders;

namespace OrderService.Application.Interfaces;

public interface IGetAllOrdersQueryHandler
{
    Task<PagedResponseDto<OrderSummaryDto>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken = default);
}
