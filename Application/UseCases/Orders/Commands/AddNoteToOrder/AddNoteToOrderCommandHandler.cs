using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;

public sealed class AddNoteToOrderCommandHandler : IAddNoteToOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUnitOfWork _unitOfWork;

    public AddNoteToOrderCommandHandler(
        IOrderRepository orderRepository,
        IUserServiceClient userServiceClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userServiceClient = userServiceClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(AddNoteToOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (command.CreatedByUserId == Guid.Empty)
            throw new ValidationException("El id del usuario que crea la nota es obligatorio.");

        if (string.IsNullOrWhiteSpace(command.Note))
            throw new ValidationException("La nota es obligatoria.");

        var userExists = await _userServiceClient.ExistsAsync(command.CreatedByUserId, cancellationToken);
        if (!userExists)
            throw new NotFoundException("User", command.CreatedByUserId);

        var order = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        order.AddNote(command.CreatedByUserId, command.Note);
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
            TableId = order.TableId,
            TableNumber = order.Table.Number,
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
