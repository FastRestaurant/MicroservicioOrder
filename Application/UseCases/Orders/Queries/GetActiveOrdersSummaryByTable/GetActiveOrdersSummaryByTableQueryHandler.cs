using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Queries.GetActiveOrdersSummaryByTable;

public sealed class GetActiveOrdersSummaryByTableQueryHandler : IGetActiveOrdersSummaryByTableQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetActiveOrdersSummaryByTableQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<TableOrdersSummaryDto> Handle(
        GetActiveOrdersSummaryByTableQuery query, CancellationToken cancellationToken = default)
    {
        if (query.TableId == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        var (orders, total, canClose) = await _orderRepository.GetActiveSummaryByTableAsync(query.TableId, cancellationToken);

        return new TableOrdersSummaryDto
        {
            TableId = query.TableId,
            Total = total,
            CanClose = canClose,
            Orders = orders
        };
    }
}
