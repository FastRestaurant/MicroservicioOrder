using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Queries.GetOrdersByTable;

public sealed class GetOrdersByTableQueryHandler : IGetOrdersByTableQueryHandler
{
    private const int MaxPageSize = 100;
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByTableQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResponseDto<OrderSummaryDto>> Handle(
        GetOrdersByTableQuery query, CancellationToken cancellationToken = default)
    {
        if (query.TableId == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        if (query.Page < 1)
            throw new ValidationException("El número de página debe ser mayor a cero.");

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            throw new ValidationException($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");

        var (orders, totalCount) = await _orderRepository.GetPagedByTableAsync(
            query.TableId, query.Page, query.PageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponseDto<OrderSummaryDto>
        {
            Items = orders.Select(OrderMapper.ToSummary).ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };
    }
}
