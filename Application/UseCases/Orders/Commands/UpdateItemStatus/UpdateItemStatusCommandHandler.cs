using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Application.Realtime;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.UpdateItemStatus;

public sealed class UpdateItemStatusCommandHandler : IUpdateItemStatusCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStatusRepository _statusRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotifier _orderNotifier;
    private readonly ILogger<UpdateItemStatusCommandHandler> _logger;

    public UpdateItemStatusCommandHandler(
        IOrderRepository orderRepository,
        IStatusRepository statusRepository,
        IUnitOfWork unitOfWork,
        IOrderNotifier orderNotifier,
        ILogger<UpdateItemStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _statusRepository = statusRepository;
        _unitOfWork = unitOfWork;
        _orderNotifier = orderNotifier;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(UpdateItemStatusCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (command.ItemId == Guid.Empty)
            throw new ValidationException("El id del item es obligatorio.");

        if (string.IsNullOrWhiteSpace(command.NewStatus))
            throw new ValidationException("El estado solicitado es obligatorio.");

        var newStatus = await _statusRepository.GetByNameAsync(command.NewStatus, StatusTypes.OrderItem, cancellationToken)
            ?? throw new DomainException($"'{command.NewStatus}' no es un estado valido para un item de la orden.");

        var order = await _orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        var item = order.Items.FirstOrDefault(i => i.Id == command.ItemId)
            ?? throw new NotFoundException(nameof(OrderItem), command.ItemId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            item.UpdateStatus(newStatus.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var updatedOrder = await _orderRepository.GetByIdForReadAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        var response = OrderMapper.ToResponse(updatedOrder);

        await NotifyOrderItemStatusChangedAsync(response, cancellationToken);

        return response;
    }

    private async Task NotifyOrderItemStatusChangedAsync(OrderResponseDto order, CancellationToken cancellationToken)
    {
        try
        {
            await _orderNotifier.NotifyOrderItemStatusChangedAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real el cambio de estado del item de la orden {OrderId}.", order.Id);
        }
    }
}
