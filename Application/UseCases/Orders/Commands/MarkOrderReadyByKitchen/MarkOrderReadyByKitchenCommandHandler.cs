using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Application.Realtime;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.MarkOrderReadyByKitchen;

public sealed class MarkOrderReadyByKitchenCommandHandler : IMarkOrderReadyByKitchenCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderNotifier _orderNotifier;
    private readonly ILogger<MarkOrderReadyByKitchenCommandHandler> _logger;

    public MarkOrderReadyByKitchenCommandHandler(
        IOrderRepository orderRepository,
        IOrderNotifier orderNotifier,
        ILogger<MarkOrderReadyByKitchenCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _orderNotifier = orderNotifier;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(MarkOrderReadyByKitchenCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        var order = await _orderRepository.GetByIdForReadAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        if (order.StatusId is OrderStatusIds.Closed or OrderStatusIds.Cancelled)
            return OrderMapper.ToResponse(order);

        var response = OrderMapper.ToResponse(order);

        await NotifyOrderFinishedAsync(response, command.WasDelayed, cancellationToken);

        return response;
    }

    private async Task NotifyOrderFinishedAsync(OrderResponseDto order, bool wasDelayed, CancellationToken cancellationToken)
    {
        try
        {
            if (wasDelayed)
                await _orderNotifier.NotifyOrderDelayedAsync(order, CancellationToken.None);
            else
                await _orderNotifier.NotifyOrderReadyToCloseAsync(order, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real que la orden {OrderId} esta lista para cerrar.", order.Id);
        }
    }
}
