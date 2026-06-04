using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.RemoveItemFromOrder;

public sealed class RemoveItemFromOrderCommandHandler : IRemoveItemFromOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveItemFromOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(RemoveItemFromOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (command.ItemId == Guid.Empty)
            throw new ValidationException("El id del item es obligatorio.");

        var order = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        order.RemoveItem(command.ItemId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        return MapOrder(updatedOrder);
    }

    private static OrderResponseDto MapOrder(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            TableNumber = order.TableNumber,
            WaiterId = order.WaiterId,
            Status = order.Status.Name,
            Total = order.Total,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            ClosedAt = order.ClosedAt,
            Version = order.Version,
            Items = order.Items.Select(MapItem).ToArray(),
            Notes = order.Notes.Select(MapNote).ToArray(),
            StatusHistory = order.StatusHistory.Select(MapStatusHistory).ToArray()
        };
    }

    private static OrderItemResponseDto MapItem(OrderItem item)
    {
        return new OrderItemResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductType = item.ProductType,
            ProductNameSnapshot = item.ProductNameSnapshot,
            UnitPriceSnapshot = item.UnitPriceSnapshot,
            DurationMinutesSnapshot = item.DurationMinutesSnapshot,
            Quantity = item.Quantity,
            Status = item.Status.Name,
            Notes = item.Notes,
            SentToKitchenAt = item.SentToKitchenAt,
            ReadyAt = item.ReadyAt
        };
    }

    private static OrderNoteResponseDto MapNote(OrderNote note)
    {
        return new OrderNoteResponseDto
        {
            Id = note.Id,
            CreatedByUserId = note.CreatedByUserId,
            Note = note.Note,
            CreatedAt = note.CreatedAt
        };
    }

    private static OrderStatusHistoryDto MapStatusHistory(OrderStatusHistory history)
    {
        return new OrderStatusHistoryDto
        {
            Id = history.Id,
            PreviousStatusName = history.PreviousStatus?.Name,
            NewStatus = history.NewStatus.Name,
            ChangedByUserId = history.ChangedByUserId,
            ChangedAt = history.ChangedAt
        };
    }
}
