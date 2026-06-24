using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.MarkOrderReadyByKitchen;

namespace OrderService.Application.Interfaces;

public interface IMarkOrderReadyByKitchenCommandHandler
{
    Task<OrderResponseDto> Handle(MarkOrderReadyByKitchenCommand command, CancellationToken cancellationToken = default);
}
