using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;

namespace OrderService.Application.Interfaces;

public interface IChangeOrderStatusCommandHandler
{
    Task<OrderResponseDto> Handle(ChangeOrderStatusCommand command, CancellationToken cancellationToken = default);
}
