using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.AddItemToOrder;

public sealed class AddItemToOrderCommandHandler : IAddItemToOrderCommandHandler
{
    private const int MaxQuantityPerItem = 50;

    private readonly IOrderRepository _orderRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IMenuCatalogClient _menuCatalogClient;
    private readonly IStockClient _stockClient;
    private readonly IUnitOfWork _unitOfWork;

    public AddItemToOrderCommandHandler(
        IOrderRepository orderRepository,
        ITableRepository tableRepository,
        IMenuCatalogClient menuCatalogClient,
        IStockClient stockClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _tableRepository = tableRepository;
        _menuCatalogClient = menuCatalogClient;
        _stockClient = stockClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(AddItemToOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (command.ProductId == Guid.Empty)
            throw new ValidationException("El id del producto es obligatorio.");

        if (string.IsNullOrWhiteSpace(command.ProductType))
            throw new ValidationException("El tipo de producto es obligatorio.");

        if (command.Quantity <= 0)
            throw new ValidationException("La cantidad debe ser mayor a cero.");

        if (command.Quantity > MaxQuantityPerItem)
            throw new ValidationException($"La cantidad no puede superar las {MaxQuantityPerItem} unidades por item.");

        if (command.Notes?.Length > 500)
            throw new ValidationException("Las notas del item no pueden superar los 500 caracteres.");

        if (!ProductTypes.IsValid(command.ProductType))
            throw new DomainException($"'{command.ProductType}' no es un tipo de producto valido.");

        var order = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        if (order.StatusId == OrderStatusIds.ReadyToClose)
            throw new DomainException("No se pueden agregar productos porque la cuenta ya fue solicitada.");

        var table = await _tableRepository.GetByIdAsync(order.TableId, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), order.TableId);

        if (!table.IsEnabled)
            throw new DomainException($"La mesa '{table.Number}' esta deshabilitada. No se pueden agregar productos.");

        var product = await _menuCatalogClient.GetProductAsync(command.ProductId, command.ProductType, cancellationToken)
            ?? throw new NotFoundException(command.ProductType, command.ProductId);

        if (!product.Available)
            throw new DomainException($"{command.ProductType} '{product.Name}' no esta disponible.");

        if (product.Duration < 0)
            throw new DomainException($"La duracion de preparacion de '{product.Name}' no es valida.");

        var item = OrderItem.Create(
            command.OrderId,
            command.ProductId,
            command.ProductType,
            product.Name,
            product.Price,
            product.Duration,
            command.Quantity,
            command.Notes);

        var stockResult = await _stockClient.ConsumeForOrderAsync(new StockConsumptionRequestDto
        {
            OrderId = command.OrderId,
            OrderItemId = item.Id,
            ProductId = command.ProductId,
            ProductType = command.ProductType,
            Quantity = command.Quantity
        }, cancellationToken);

        if (!stockResult.Success)
            throw new DomainException(string.IsNullOrWhiteSpace(stockResult.Message)
                ? "No hay stock suficiente para el producto solicitado."
                : stockResult.Message);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            order.AddItem(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await TryRollbackAsync(cancellationToken);
            await TryReleaseStockAsync(command.OrderId, item.Id, cancellationToken);
            throw;
        }

        var updatedOrder = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        return MapOrder(updatedOrder);
    }

    private async Task TryRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        }
        catch
        {
        }
    }

    private async Task TryReleaseStockAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken)
    {
        try
        {
            await _stockClient.ReleaseForOrderAsync(new StockReleaseRequestDto
            {
                OrderId = orderId,
                OrderItemId = orderItemId
            }, cancellationToken);
        }
        catch
        {
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