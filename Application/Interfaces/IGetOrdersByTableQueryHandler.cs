using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;

namespace OrderService.Application.Interfaces;

public interface IGetOrdersByTableQueryHandler
{
    Task<IEnumerable<OrderSummaryDto>> Handle(GetOrdersByTableQuery query, CancellationToken cancellationToken = default);
}
