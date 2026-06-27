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
    private readonly IStatusRepository _statusRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotifier _orderNotifier;
    private readonly ILogger<MarkOrderReadyByKitchenCommandHandler> _logger;

    public MarkOrderReadyByKitchenCommandHandler(
        IOrderRepository orderRepository,
        IStatusRepository statusRepository,
        IUnitOfWork unitOfWork,
        IOrderNotifier orderNotifier,
        ILogger<MarkOrderReadyByKitchenCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _statusRepository = statusRepository;
        _unitOfWork = unitOfWork;
        _orderNotifier = orderNotifier;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(MarkOrderReadyByKitchenCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        var readyToClose = await _statusRepository.GetByNameAsync("ReadyToClose", StatusTypes.Order, cancellationToken)
            ?? throw new DomainException("'ReadyToClose' no es un estado valido para una orden.");

        var order = await _orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        if (order.StatusId == OrderStatusIds.ReadyToClose)
        {
            var alreadyReady = await _orderRepository.GetByIdForReadAsync(command.OrderId, cancellationToken)
                ?? throw new NotFoundException(nameof(Order), command.OrderId);
            return OrderMapper.ToResponse(alreadyReady);
        }

        if (order.StatusId != OrderStatusIds.InProgress)
            throw new ConflictException($"La orden {command.OrderId} no se puede marcar como lista desde el estado actual ('{order.StatusId}'). Debe estar en preparacion.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            order.ChangeStatus(readyToClose.Id, SystemActors.KitchenService);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var updated = await _orderRepository.GetByIdForReadAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        var response = OrderMapper.ToResponse(updated);

        await NotifyOrderReadyToCloseAsync(response, cancellationToken);

        return response;
    }

    private async Task NotifyOrderReadyToCloseAsync(OrderResponseDto order, CancellationToken cancellationToken)
    {
        try
        {
            await _orderNotifier.NotifyOrderReadyToCloseAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real que la orden {OrderId} esta lista para cerrar.", order.Id);
        }
    }
}
