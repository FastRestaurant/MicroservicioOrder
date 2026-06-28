using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Application.Realtime;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommandHandler : IChangeOrderStatusCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStatusRepository _statusRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IStockClient _stockClient;
    private readonly IKitchenClient _kitchenClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotifier _orderNotifier;
    private readonly ILogger<ChangeOrderStatusCommandHandler> _logger;

    public ChangeOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IStatusRepository statusRepository,
        IUserServiceClient userServiceClient,
        IStockClient stockClient,
        IKitchenClient kitchenClient,
        IUnitOfWork unitOfWork,
        IOrderNotifier orderNotifier,
        ILogger<ChangeOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _statusRepository = statusRepository;
        _userServiceClient = userServiceClient;
        _stockClient = stockClient;
        _kitchenClient = kitchenClient;
        _unitOfWork = unitOfWork;
        _orderNotifier = orderNotifier;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(ChangeOrderStatusCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (string.IsNullOrWhiteSpace(command.NewStatus))
            throw new ValidationException("El estado solicitado es obligatorio.");

        if (command.ChangedByUserId == Guid.Empty)
            throw new ValidationException("El id del usuario que modifica el estado es obligatorio.");

        var userExists = await _userServiceClient.ExistsAsync(command.ChangedByUserId, cancellationToken);
        if (!userExists)
            throw new NotFoundException("User", command.ChangedByUserId);

        var newStatus = await _statusRepository.GetByNameAsync(command.NewStatus, StatusTypes.Order, cancellationToken)
            ?? throw new DomainException($"'{command.NewStatus}' no es un estado valido para una orden.");

        var order = await _orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        if (newStatus.Id == OrderStatusIds.Cancelled)
        {
            var kitchenResult = await _kitchenClient.CancelByOrderIdAsync(order.Id, cancellationToken);
            if (kitchenResult.Blocked)
                throw new ConflictException(kitchenResult.Message ?? "No se puede cancelar la orden porque la cocina ya comenzo a prepararla.");
            if (!kitchenResult.Success)
                throw new DomainException(kitchenResult.Message ?? "No se pudo cancelar la orden en la cocina.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            order.ChangeStatus(newStatus.Id, command.ChangedByUserId);

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

        if (newStatus.Id == OrderStatusIds.Cancelled)
            await ReleaseOrderStockAsync(updatedOrder.Id, updatedOrder.Items, cancellationToken);

        var response = OrderMapper.ToResponse(updatedOrder);

        await NotifyOrderStatusChangedAsync(response, cancellationToken);

        return response;
    }

    private async Task NotifyOrderStatusChangedAsync(OrderResponseDto order, CancellationToken cancellationToken)
    {
        try
        {
            await _orderNotifier.NotifyOrderStatusChangedAsync(order, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real el cambio de estado de la orden {OrderId}.", order.Id);
        }
    }

    private async Task ReleaseOrderStockAsync(Guid orderId, IEnumerable<OrderItem> items, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            try
            {
                var releaseResult = await _stockClient.ReleaseForOrderAsync(new StockReleaseRequestDto
                {
                    OrderId = orderId,
                    OrderItemId = item.Id
                }, cancellationToken);

                if (!releaseResult.Success)
                    _logger.LogWarning("No se pudo liberar el stock al cancelar la orden {OrderId}, item {OrderItemId}. {Message}", orderId, item.Id, releaseResult.Message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo liberar el stock al cancelar la orden {OrderId}, item {OrderItemId}.", orderId, item.Id);
            }
        }
    }
}
