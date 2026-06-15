using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ITableRepository tableRepository,
        IUserServiceClient userServiceClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _tableRepository = tableRepository;
        _userServiceClient = userServiceClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TableId == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        if (command.WaiterId == Guid.Empty)
            throw new ValidationException("El id del mozo es obligatorio.");

        var waiterExists = await _userServiceClient.ExistsAsync(command.WaiterId, cancellationToken);
        if (!waiterExists)
            throw new NotFoundException("User", command.WaiterId);

        var table = await _tableRepository.GetByIdAsync(command.TableId, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), command.TableId);

        if (!table.IsEnabled)
            throw new DomainException($"La mesa '{table.Number}' está deshabilitada.");

        var tableOccupied = await _orderRepository.HasActiveOrderForTableAsync(command.TableId, cancellationToken);
        if (tableOccupied)
            throw new ConflictException($"La mesa '{table.Number}' ya tiene una orden activa. Ciérrela antes de abrir una nueva.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = Order.Create(command.TableId, command.WaiterId);
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var createdOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Order), order.Id);

            return MapOrder(createdOrder);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
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

    private static OrderItemResponseDto MapItem(OrderItem item) => new()
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

    private static OrderNoteResponseDto MapNote(OrderNote note) => new()
    {
        Id = note.Id,
        CreatedByUserId = note.CreatedByUserId,
        Note = note.Note,
        CreatedAt = note.CreatedAt
    };

    private static OrderStatusHistoryDto MapStatusHistory(OrderStatusHistory history) => new()
    {
        Id = history.Id,
        PreviousStatusName = history.PreviousStatus?.Name,
        NewStatus = history.NewStatus.Name,
        ChangedByUserId = history.ChangedByUserId,
        ChangedAt = history.ChangedAt
    };
}
