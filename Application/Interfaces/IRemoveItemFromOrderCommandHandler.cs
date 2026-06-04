using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.RemoveItemFromOrder;

namespace OrderService.Application.Interfaces;

public interface IRemoveItemFromOrderCommandHandler
{
    Task<OrderResponseDto> Handle(RemoveItemFromOrderCommand command, CancellationToken cancellationToken = default);
}
