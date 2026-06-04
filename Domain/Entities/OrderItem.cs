using OrderService.Domain.Constants;
using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public int DurationMinutesSnapshot { get; set; }
    public int Quantity { get; set; }
    public int StatusId { get; set; } = OrderItemStatusIds.Pending;
    public string? Notes { get; set; }
    public DateTime? SentToKitchenAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Order Order { get; set; } = null!;
    public Status Status { get; set; } = null!;

    public static OrderItem Create(Guid orderId, Guid productId, string productType,
        string productName, decimal unitPrice, int durationMinutes, int quantity, string? notes = null)
    {
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
                $"'{newStatusId}' no es un estado valido para un item.");

        StatusId = newStatusId;
        UpdatedAt = DateTime.UtcNow;

        if (newStatusId == OrderItemStatusIds.SentToKitchen)
            SentToKitchenAt = DateTime.UtcNow;
        else if (newStatusId == OrderItemStatusIds.Ready)
            ReadyAt = DateTime.UtcNow;
    }
}
