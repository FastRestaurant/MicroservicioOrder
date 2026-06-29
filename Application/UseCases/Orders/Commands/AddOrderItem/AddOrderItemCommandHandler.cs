using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Application.Realtime;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.AddOrderItem;

public sealed class AddOrderItemCommandHandler : IAddOrderItemCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IMenuCatalogClient _menuCatalogClient;
    private readonly IStockClient _stockClient;
    private readonly IKitchenClient _kitchenClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotifier _orderNotifier;
    private readonly ILogger<AddOrderItemCommandHandler> _logger;

    public AddOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IUserServiceClient userServiceClient,
        IMenuCatalogClient menuCatalogClient,
        IStockClient stockClient,
        IKitchenClient kitchenClient,
        IUnitOfWork unitOfWork,
        IOrderNotifier orderNotifier,
        ILogger<AddOrderItemCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _userServiceClient = userServiceClient;
        _menuCatalogClient = menuCatalogClient;
        _stockClient = stockClient;
        _kitchenClient = kitchenClient;
        _unitOfWork = unitOfWork;
        _orderNotifier = orderNotifier;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(AddOrderItemCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

        if (command.RequestedByUserId == Guid.Empty)
            throw new ValidationException("El id del usuario es obligatorio.");

        OrderItem.ValidateRequest(command.ProductId, command.ProductType, command.Quantity, command.Notes);

        var userExists = await _userServiceClient.ExistsAsync(command.RequestedByUserId, cancellationToken);
        if (!userExists)
            throw new NotFoundException("User", command.RequestedByUserId);

        var order = await _orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        var product = await _menuCatalogClient.GetProductAsync(command.ProductId, command.ProductType, cancellationToken)
            ?? throw new NotFoundException(command.ProductType, command.ProductId);

        if (!product.Available)
            throw new DomainException($"{command.ProductType} '{product.Name}' no esta disponible.");

        var item = OrderItem.Create(
            order.Id,
            command.ProductId,
            command.ProductType,
            product.Name,
            product.Price,
            product.Duration,
            command.Quantity,
            command.Notes);

        if (!RequiresKitchen(item))
            item.UpdateStatus(OrderItemStatusIds.Delivered);

        await ConsumeStockAsync(order.Id, item, cancellationToken);

        var transactionStarted = false;
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            transactionStarted = true;

            order.AddItem(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            transactionStarted = false;
        }
        catch
        {
            if (transactionStarted)
                await TryRollbackAsync(cancellationToken);

            await TryReleaseConsumedStockAsync(order.Id, item, cancellationToken);
            throw;
        }

        var requiresKitchen = RequiresKitchen(item);
        var kitchenResult = requiresKitchen
            ? await SendToKitchenAsync(order, item, cancellationToken)
            : new KitchenEnqueueResultDto { Success = true };

        if (kitchenResult.Success && requiresKitchen)
            await MarkItemAsSentToKitchenAsync(order.Id, item.Id, cancellationToken);
        else if (requiresKitchen)
        {
            await CancelAddedItemAsync(order.Id, item.Id, cancellationToken);
            await TryReleaseConsumedStockAsync(order.Id, item, cancellationToken);
            _logger.LogWarning("El item {OrderItemId} se agrego a la orden {OrderId}, pero no se pudo enviar a cocina. {Message}", item.Id, order.Id, kitchenResult.Message);
            throw new DomainException(kitchenResult.Message ?? "No se pudo enviar el producto a cocina. Intentá nuevamente.");
        }

        var updatedOrder = await _orderRepository.GetByIdForReadAsync(order.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), order.Id);

        var response = OrderMapper.ToResponse(updatedOrder);
        await NotifyOrderItemAddedAsync(response, cancellationToken);

        return response;
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
            throw new DomainException(BuildStockErrorMessage(item, stockResult));
    }

    private static string BuildStockErrorMessage(OrderItem item, StockOperationResultDto stockResult)
    {
        if (IsProductStockIssue(stockResult))
            return $"No hay stock suficiente para \"{item.ProductNameSnapshot}\".";

        return string.IsNullOrWhiteSpace(stockResult.Message)
            ? $"No hay stock suficiente para \"{item.ProductNameSnapshot}\"."
            : stockResult.Message;
    }

    private static bool IsProductStockIssue(StockOperationResultDto stockResult)
        => stockResult.MissingItems.Count > 0 ||
           stockResult.Message.Contains("stock suficiente", StringComparison.OrdinalIgnoreCase) ||
           stockResult.Message.Contains("Stock insuficiente", StringComparison.OrdinalIgnoreCase) ||
           stockResult.Message.Contains("stock configurado", StringComparison.OrdinalIgnoreCase);

    private async Task<KitchenEnqueueResultDto> SendToKitchenAsync(Order order, OrderItem item, CancellationToken cancellationToken)
    {
        try
        {
            return await _kitchenClient.EnqueueOrderAsync(new KitchenTicketRequestDto
            {
                OrderId = order.Id,
                TableId = order.TableId,
                TableNumber = int.TryParse(order.Table.Number, out var parsed) ? parsed : 0,
                WaiterId = order.WaiterId,
                CreatedAtUtc = order.CreatedAt,
                Items = new[]
                {
                    new KitchenTicketItemDto
                    {
                        OrderItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductNameSnapshot,
                        ProductType = item.ProductType,
                        DurationMinutes = item.DurationMinutesSnapshot,
                        Quantity = item.Quantity,
                        Notes = item.Notes
                    }
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el item {OrderItemId} de la orden {OrderId} a la cocina.", item.Id, order.Id);
            return new KitchenEnqueueResultDto { Success = false, Message = "No se pudo avisar a la cocina en este momento." };
        }
    }

    private async Task TryRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo revertir la transaccion al agregar un item a la orden.");
        }
    }

    private async Task TryReleaseConsumedStockAsync(Guid orderId, OrderItem item, CancellationToken cancellationToken)
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

    private async Task NotifyOrderItemAddedAsync(OrderResponseDto order, CancellationToken cancellationToken)
    {
        try
        {
            await _orderNotifier.NotifyOrderItemAddedAsync(order, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real el nuevo item de la orden {OrderId}.", order.Id);
        }
    }

    private async Task MarkItemAsSentToKitchenAsync(Guid orderId, Guid itemId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdForUpdateAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);

        var item = order.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException(nameof(OrderItem), itemId);

        if (item.StatusId != OrderItemStatusIds.Pending)
            return;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            item.UpdateStatus(OrderItemStatusIds.SentToKitchen);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task CancelAddedItemAsync(Guid orderId, Guid itemId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdForUpdateAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);

        var item = order.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException(nameof(OrderItem), itemId);

        if (item.StatusId == OrderItemStatusIds.Cancelled)
            return;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            item.UpdateStatus(OrderItemStatusIds.Cancelled);
            order.RefreshTotal();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static bool RequiresKitchen(OrderItem item)
        => item.ProductType == ProductTypes.Dish && item.DurationMinutesSnapshot > 0;
}
