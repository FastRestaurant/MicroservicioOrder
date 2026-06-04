using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.UpdateItemStatus;

namespace OrderService.Application.Interfaces;

public interface IUpdateItemStatusCommandHandler
{
    Task<OrderResponseDto> Handle(UpdateItemStatusCommand command, CancellationToken cancellationToken = default);
}
