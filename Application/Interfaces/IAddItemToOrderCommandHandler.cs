using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.AddItemToOrder;

namespace OrderService.Application.Interfaces;

public interface IAddItemToOrderCommandHandler
{
    Task<OrderResponseDto> Handle(AddItemToOrderCommand command, CancellationToken cancellationToken = default);
}
