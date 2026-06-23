using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.AddItemToOrder;

public sealed class AddItemToOrderCommandHandler : IAddItemToOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IMenuCatalogClient _menuCatalogClient;
    private readonly IStockClient _stockClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddItemToOrderCommandHandler> _logger;

    public AddItemToOrderCommandHandler(
        IOrderRepository orderRepository,
        ITableRepository tableRepository,
        IMenuCatalogClient menuCatalogClient,
        IStockClient stockClient,
        IUnitOfWork unitOfWork,
        ILogger<AddItemToOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _tableRepository = tableRepository;
        _menuCatalogClient = menuCatalogClient;
        _stockClient = stockClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(AddItemToOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrderId == Guid.Empty)
            throw new ValidationException("El id de la orden es obligatorio.");

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

        return OrderMapper.ToResponse(updatedOrder);
    }

    private async Task TryRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo revertir la transaccion al agregar el item a la orden.");
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo liberar el stock reservado para la orden {OrderId}, item {OrderItemId}.", orderId, orderItemId);
        }
    }
}
