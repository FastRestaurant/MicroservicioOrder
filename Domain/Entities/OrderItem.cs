using OrderService.Domain.Constants;
using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class OrderItem
{
    public const int MaxQuantityPerItem = 50;
    public const int MaxNotesLength = 500;

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductType { get; private set; } = string.Empty;
    public string ProductNameSnapshot { get; private set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; private set; }
    public int DurationMinutesSnapshot { get; private set; }
    public int Quantity { get; private set; }
    public int StatusId { get; private set; } = OrderItemStatusIds.Pending;
    public string? Notes { get; private set; }
    public DateTime? SentToKitchenAt { get; private set; }
    public DateTime? ReadyAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Order Order { get; private set; } = null!;
    public Status Status { get; private set; } = null!;

    private OrderItem() { }

    public static void ValidateRequest(Guid productId, string productType, int quantity, string? notes)
    {
        if (productId == Guid.Empty)
            throw new ValidationException("El id del producto es obligatorio.");

        if (string.IsNullOrWhiteSpace(productType))
            throw new ValidationException("El tipo de producto es obligatorio.");

        if (!ProductTypes.IsValid(productType))
            throw new DomainException("El producto seleccionado no es válido.");

        if (quantity <= 0)
            throw new ValidationException("La cantidad debe ser mayor a cero.");

        if (quantity > MaxQuantityPerItem)
            throw new ValidationException($"La cantidad no puede superar las {MaxQuantityPerItem} unidades por item.");

        if (notes?.Length > MaxNotesLength)
            throw new ValidationException($"Las notas del item no pueden superar los {MaxNotesLength} caracteres.");
    }

    public static OrderItem Create(Guid orderId, Guid productId, string productType,
        string productName, decimal unitPrice, int durationMinutes, int quantity, string? notes = null)
    {
        ValidateRequest(productId, productType, quantity, notes);

        if (unitPrice < 0)
            throw new DomainException("El precio del producto no es válido.");

        if (durationMinutes < 0)
            throw new DomainException("El tiempo de preparación del producto no es válido.");

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductType = productType,
            ProductNameSnapshot = productName,
            UnitPriceSnapshot = unitPrice,
            DurationMinutesSnapshot = durationMinutes,
            Quantity = quantity,
            StatusId = OrderItemStatusIds.Pending,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(int newStatusId)
    {
        if (!OrderItemStatusIds.IsValid(newStatusId))
            throw new DomainException(
                "El estado solicitado no es válido para este producto.");

        if (OrderItemStatusIds.IsTerminal(StatusId))
            throw new DomainException(
                "Este producto ya está cerrado y no admite cambios.");

        if (!OrderItemStatusIds.IsValidTransition(StatusId, newStatusId))
            throw new DomainException(
                "No se pudo actualizar el estado del producto. Actualizá la pantalla e intentá nuevamente.");

        StatusId = newStatusId;
        UpdatedAt = DateTime.UtcNow;

        if (newStatusId == OrderItemStatusIds.SentToKitchen)
            SentToKitchenAt = DateTime.UtcNow;
        else if (newStatusId == OrderItemStatusIds.Ready)
            ReadyAt = DateTime.UtcNow;
    }
}
