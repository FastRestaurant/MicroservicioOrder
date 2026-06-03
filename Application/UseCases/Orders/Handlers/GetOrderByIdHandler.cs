using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Orders.Queries;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Handlers;

public class GetOrderByIdHandler(IOrderRepository repository)
{
    public async Task<OrderResponseDto> HandleAsync(GetOrderByIdQuery query, CancellationToken ct = default)
    {
        var order = await repository.GetByIdWithDetailsAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(Order), query.Id);
        return order.ToResponseDto();
    }
}
