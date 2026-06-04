using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUserServiceClient userServiceClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userServiceClient = userServiceClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TableNumber <= 0)
            throw new ValidationException("La mesa debe ser mayor a cero.");

        if (command.WaiterId == Guid.Empty)
            throw new ValidationException("El id del mozo es obligatorio.");

        var waiterExists = await _userServiceClient.ExistsAsync(command.WaiterId, cancellationToken);
        if (!waiterExists)
            throw new NotFoundException("User", command.WaiterId);

        var tableOccupied = await _orderRepository.HasActiveOrderForTableAsync(command.TableNumber, cancellationToken);
        if (tableOccupied)
            throw new DomainException(
                $"La mesa {command.TableNumber} ya tiene una orden activa. Cierrela antes de abrir una nueva.");

        var order = Order.Create(command.TableNumber, command.WaiterId);
        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), order.Id);

        return MapOrder(createdOrder);
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
