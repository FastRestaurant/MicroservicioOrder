using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByStatus;

public sealed class GetOrdersByStatusQueryHandler : IGetOrdersByStatusQueryHandler
{
    private const int MaxPageSize = 100;
    private readonly IOrderRepository _orderRepository;
    private readonly IStatusRepository _statusRepository;

    public GetOrdersByStatusQueryHandler(
        IOrderRepository orderRepository,
        IStatusRepository statusRepository)
    {
        _orderRepository = orderRepository;
        _statusRepository = statusRepository;
    }

    public async Task<PagedResponseDto<OrderSummaryDto>> Handle(GetOrdersByStatusQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1)
            throw new ValidationException("El número de página debe ser mayor a cero.");

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            throw new ValidationException($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");

        var status = await _statusRepository.GetByNameAsync(query.Status, StatusTypes.Order, cancellationToken)
            ?? throw new DomainException($"'{query.Status}' no es un estado valido para una orden.");

        var (orders, totalCount) = await _orderRepository.GetPagedByStatusAsync(
            status.Id, query.Page, query.PageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponseDto<OrderSummaryDto>
        {
            Items = orders.Select(MapOrder).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };
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
