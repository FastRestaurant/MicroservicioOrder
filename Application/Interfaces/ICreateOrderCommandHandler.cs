using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;

namespace OrderService.Application.Interfaces;

public interface ICreateOrderCommandHandler
{
    Task<OrderResponseDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default);
}
