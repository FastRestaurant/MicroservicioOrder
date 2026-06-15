using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetOrderById;

namespace OrderService.Application.Interfaces;

public interface IGetOrderByIdQueryHandler
{
    Task<OrderResponseDto> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default);
}