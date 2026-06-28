using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Constants;
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

        var orders = await _orderRepository.GetActiveByTableAsync(query.TableId, cancellationToken);

        return new TableOrdersSummaryDto
        {
            TableId = query.TableId,
            Total = orders.Sum(order => order.Total),
            CanClose = orders.Count > 0 && orders.All(order => order.StatusId == OrderStatusIds.ReadyToClose),
            Orders = orders.Select(OrderMapper.ToSummary).ToArray()
        };
    }
}
