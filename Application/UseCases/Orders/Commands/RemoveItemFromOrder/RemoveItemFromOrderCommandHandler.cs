using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.RemoveItemFromOrder;

public sealed class RemoveItemFromOrderCommandHandler : IRemoveItemFromOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStockClient _stockClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveItemFromOrderCommandHandler> _logger;

    public RemoveItemFromOrderCommandHandler(
        IOrderRepository orderRepository,
        IStockClient stockClient,
        IUnitOfWork unitOfWork,
        ILogger<RemoveItemFromOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _stockClient = stockClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(RemoveItemFromOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (command.ItemId == Guid.Empty)
            throw new ValidationException("El id del item es obligatorio.");

        var order = await _orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        var item = order.Items.FirstOrDefault(i => i.Id == command.ItemId)
            ?? throw new NotFoundException(nameof(OrderItem), command.ItemId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            order.RemoveItem(command.ItemId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        await ReleaseItemStockAsync(order.Id, item.Id, cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdForReadAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        return OrderMapper.ToResponse(updatedOrder);
    }

    private async Task ReleaseItemStockAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken)
    {
        try
        {
            var releaseResult = await _stockClient.ReleaseForOrderAsync(new StockReleaseRequestDto
            {
                OrderId = orderId,
                OrderItemId = orderItemId
            }, cancellationToken);

            if (!releaseResult.Success)
                _logger.LogWarning("No se pudo liberar el stock del item {OrderItemId} de la orden {OrderId}. {Message}", orderItemId, orderId, releaseResult.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo liberar el stock del item {OrderItemId} de la orden {OrderId}.", orderItemId, orderId);
        }
    }
}
