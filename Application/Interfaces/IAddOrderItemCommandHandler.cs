using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.AddOrderItem;

namespace OrderService.Application.Interfaces;

public interface IAddOrderItemCommandHandler
{
    Task<OrderResponseDto> Handle(AddOrderItemCommand command, CancellationToken cancellationToken = default);
}
