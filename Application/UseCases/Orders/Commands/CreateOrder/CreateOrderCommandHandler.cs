using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICreateOrderCommandHandler
{
    private const int MaxQuantityPerItem = 50;
    private readonly IOrderRepository _orderRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IMenuCatalogClient _menuCatalogClient;
    private readonly IStockClient _stockClient;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ITableRepository tableRepository,
        IUserServiceClient userServiceClient,
        IMenuCatalogClient menuCatalogClient,
        IStockClient stockClient,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _tableRepository = tableRepository;
        _userServiceClient = userServiceClient;
        _menuCatalogClient = menuCatalogClient;
        _stockClient = stockClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TableId == Guid.Empty)
            throw new ValidationException("El id de la mesa es obligatorio.");

        if (command.WaiterId == Guid.Empty)
            throw new ValidationException("El id del mozo es obligatorio.");

        if (command.Items.Count == 0)
            throw new ValidationException("La orden debe tener al menos un item.");

        foreach (var item in command.Items)
            ValidateItem(item);

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
            orderItems.Add(await CreateOrderItemAsync(order.Id, requestedItem, cancellationToken));

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

        var createdOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), order.Id);

        return OrderMapper.ToResponse(createdOrder);
    }

    private static void ValidateItem(CreateOrderItemCommand item)
    {
        if (item.ProductId == Guid.Empty)
            throw new ValidationException("El id del producto es obligatorio.");

        if (string.IsNullOrWhiteSpace(item.ProductType))
            throw new ValidationException("El tipo de producto es obligatorio.");

        if (!ProductTypes.IsValid(item.ProductType))
            throw new DomainException($"'{item.ProductType}' no es un tipo de producto valido.");

        if (item.Quantity <= 0)
            throw new ValidationException("La cantidad debe ser mayor a cero.");

        if (item.Quantity > MaxQuantityPerItem)
            throw new ValidationException($"La cantidad no puede superar las {MaxQuantityPerItem} unidades por item.");

        if (item.Notes?.Length > 500)
            throw new ValidationException("Las notas del item no pueden superar los 500 caracteres.");
    }

    private async Task<OrderItem> CreateOrderItemAsync(Guid orderId, CreateOrderItemCommand requestedItem, CancellationToken cancellationToken)
    {
        var product = await _menuCatalogClient.GetProductAsync(requestedItem.ProductId, requestedItem.ProductType, cancellationToken)
            ?? throw new NotFoundException(requestedItem.ProductType, requestedItem.ProductId);

        if (!product.Available)
            throw new DomainException($"{requestedItem.ProductType} '{product.Name}' no esta disponible.");

        if (product.Duration < 0)
            throw new DomainException($"La duracion de preparacion de '{product.Name}' no es valida.");

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
        catch
        {
        }
    }

    private async Task TryReleaseConsumedStockAsync(Guid orderId, IEnumerable<OrderItem> consumedItems, CancellationToken cancellationToken)
    {
        foreach (var item in consumedItems)
        {
            try
            {
                await _stockClient.ReleaseForOrderAsync(new StockReleaseRequestDto
                {
                    OrderId = orderId,
                    OrderItemId = item.Id
                }, cancellationToken);
            }
            catch
            {
            }
        }
    }
}
