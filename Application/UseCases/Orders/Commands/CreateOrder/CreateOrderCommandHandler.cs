using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Application.Realtime;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IMenuCatalogClient _menuCatalogClient;
    private readonly IStockClient _stockClient;
    private readonly IKitchenClient _kitchenClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotifier _orderNotifier;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ITableRepository tableRepository,
        IUserServiceClient userServiceClient,
        IMenuCatalogClient menuCatalogClient,
        IStockClient stockClient,
        IKitchenClient kitchenClient,
        IUnitOfWork unitOfWork,
        IOrderNotifier orderNotifier,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _tableRepository = tableRepository;
        _userServiceClient = userServiceClient;
        _menuCatalogClient = menuCatalogClient;
        _stockClient = stockClient;
        _kitchenClient = kitchenClient;
        _unitOfWork = unitOfWork;
        _orderNotifier = orderNotifier;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TableId == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        if (command.WaiterId == Guid.Empty)
            throw new ValidationException("El id del mozo es obligatorio.");

        if (command.Items.Count == 0)
            throw new ValidationException("La orden debe tener al menos un item.");

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

        var order = Order.Create(command.TableId, command.WaiterId);
        var orderItems = new List<OrderItem>();

        foreach (var requestedItem in command.Items)
        {
            OrderItem.ValidateRequest(requestedItem.ProductId, requestedItem.ProductType, requestedItem.Quantity, requestedItem.Notes);
            orderItems.Add(await CreateOrderItemAsync(order.Id, requestedItem, cancellationToken));
        }

        var consumedItems = new List<OrderItem>();
        var transactionStarted = false;

        try
        {
            foreach (var item in orderItems)
            {
                await ConsumeStockAsync(order.Id, item, cancellationToken);
                consumedItems.Add(item);
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            transactionStarted = true;

            foreach (var item in orderItems)
                order.AddItem(item);

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            transactionStarted = false;
        }
        catch
        {
            if (transactionStarted)
                await TryRollbackAsync(cancellationToken);

            await TryReleaseConsumedStockAsync(order.Id, consumedItems, cancellationToken);
            throw;
        }

        var kitchenResult = await SendToKitchenAsync(order, command.WaiterId, table.Number, orderItems, cancellationToken);

        if (kitchenResult.Success)
            await MarkAsInProgressAsync(order, command.WaiterId, cancellationToken);
        else
            _logger.LogWarning("La orden {OrderId} quedo en Open porque no se pudo enviar a la cocina. {Message}", order.Id, kitchenResult.Message);

        var createdOrder = await _orderRepository.GetByIdForReadAsync(order.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), order.Id);

        var response = OrderMapper.ToResponse(createdOrder);

        await NotifyOrderCreatedAsync(response, cancellationToken);

        return response;
    }

    private async Task NotifyOrderCreatedAsync(OrderResponseDto order, CancellationToken cancellationToken)
    {
        try
        {
            await _orderNotifier.NotifyOrderCreatedAsync(order, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real la creacion de la orden {OrderId}.", order.Id);
        }
    }

    private async Task<OrderItem> CreateOrderItemAsync(Guid orderId, CreateOrderItemCommand requestedItem, CancellationToken cancellationToken)
    {
        var product = await _menuCatalogClient.GetProductAsync(requestedItem.ProductId, requestedItem.ProductType, cancellationToken)
            ?? throw new NotFoundException(requestedItem.ProductType, requestedItem.ProductId);

        if (!product.Available)
            throw new DomainException($"{requestedItem.ProductType} '{product.Name}' no esta disponible.");

        return OrderItem.Create(
            orderId,
            requestedItem.ProductId,
            requestedItem.ProductType,
            product.Name,
            product.Price,
            product.Duration,
            requestedItem.Quantity,
            requestedItem.Notes);
    }

    private async Task ConsumeStockAsync(Guid orderId, OrderItem item, CancellationToken cancellationToken)
    {
        var stockResult = await _stockClient.ConsumeForOrderAsync(new StockConsumptionRequestDto
        {
            OrderId = orderId,
            OrderItemId = item.Id,
            ProductId = item.ProductId,
            ProductType = item.ProductType,
            Quantity = item.Quantity
        }, cancellationToken);

        if (!stockResult.Success)
            throw new DomainException(string.IsNullOrWhiteSpace(stockResult.Message)
                ? "No hay stock suficiente para el producto solicitado."
                : stockResult.Message);
    }

    private async Task TryRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo revertir la transaccion al crear la orden.");
        }
    }

    private async Task TryReleaseConsumedStockAsync(Guid orderId, IEnumerable<OrderItem> consumedItems, CancellationToken cancellationToken)
    {
        foreach (var item in consumedItems)
        {
            try
            {
                var releaseResult = await _stockClient.ReleaseForOrderAsync(new StockReleaseRequestDto
                {
                    OrderId = orderId,
                    OrderItemId = item.Id
                }, cancellationToken);
                if (!releaseResult.Success)
                    _logger.LogWarning("No se pudo liberar el stock reservado para la orden {OrderId}, item {OrderItemId}. {Message}", orderId, item.Id, releaseResult.Message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo liberar el stock reservado para la orden {OrderId}, item {OrderItemId}.", orderId, item.Id);
            }
        }
    }

    private async Task<KitchenEnqueueResultDto> SendToKitchenAsync(Order order, Guid waiterId, string tableNumber, IEnumerable<OrderItem> items, CancellationToken cancellationToken)
    {
        var ticket = new KitchenTicketRequestDto
        {
            OrderId = order.Id,
            TableId = order.TableId,
            TableNumber = int.TryParse(tableNumber, out var parsed) ? parsed : 0,
            WaiterId = waiterId,
            CreatedAtUtc = order.CreatedAt,
            Items = items.Select(item => new KitchenTicketItemDto
            {
                OrderItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductNameSnapshot,
                ProductType = item.ProductType,
                DurationMinutes = item.DurationMinutesSnapshot,
                Quantity = item.Quantity,
                Notes = item.Notes
            }).ToArray()
        };

        try
        {
            return await _kitchenClient.EnqueueOrderAsync(ticket, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar la orden {OrderId} a la cocina.", order.Id);
            return new KitchenEnqueueResultDto { Success = false, Message = "No se pudo avisar a la cocina en este momento." };
        }
    }

    private async Task MarkAsInProgressAsync(Order order, Guid waiterId, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            order.ChangeStatus(OrderStatusIds.InProgress, waiterId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
