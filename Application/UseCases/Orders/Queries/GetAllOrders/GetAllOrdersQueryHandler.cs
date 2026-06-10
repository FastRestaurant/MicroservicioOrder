using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersQueryHandler : IGetAllOrdersQueryHandler
{
    private const int MaxPageSize = 100;
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResponseDto<OrderSummaryDto>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1)
            throw new ValidationException("El numero de pagina debe ser mayor a cero.");

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            throw new ValidationException($"El tamanio de pagina debe estar entre 1 y {MaxPageSize}.");

        var (orders, totalCount) = await _orderRepository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
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
