using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.NotifyOrderReadyByKitchen;

namespace OrderService.Application.Interfaces;

public interface INotifyOrderReadyByKitchenCommandHandler
{
    Task<OrderResponseDto> Handle(NotifyOrderReadyByKitchenCommand command, CancellationToken cancellationToken = default);
}
