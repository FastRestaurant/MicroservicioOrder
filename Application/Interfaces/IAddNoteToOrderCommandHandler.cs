using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;

namespace OrderService.Application.Interfaces;

public interface IAddNoteToOrderCommandHandler
{
    Task<OrderResponseDto> Handle(AddNoteToOrderCommand command, CancellationToken cancellationToken = default);
}
