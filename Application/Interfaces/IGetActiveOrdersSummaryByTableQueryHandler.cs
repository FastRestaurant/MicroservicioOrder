using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetActiveOrdersSummaryByTable;

namespace OrderService.Application.Interfaces;

public interface IGetActiveOrdersSummaryByTableQueryHandler
{
    Task<TableOrdersSummaryDto> Handle(GetActiveOrdersSummaryByTableQuery query, CancellationToken cancellationToken = default);
}
