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
    public int Quantity { get; set; }
    public string Status { get; set; } = OrderItemStatus.Pending;
    public string? Notes { get; set; }
    public DateTime? SentToKitchenAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Order Order { get; set; } = null!;

    public static OrderItem Create(Guid orderId, Guid productId, string productType,
        string productName, decimal unitPrice, int quantity, string? notes = null)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductType = productType,
            ProductNameSnapshot = productName,
            UnitPriceSnapshot = unitPrice,
            Quantity = quantity,
            Status = OrderItemStatus.Pending,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(string newStatus)
    {
        var validStatuses = new[]
        {
        OrderItemStatus.Pending,
        OrderItemStatus.SentToKitchen,
        OrderItemStatus.Ready,
        OrderItemStatus.Delivered,
        OrderItemStatus.Cancelled
    };

        if (!validStatuses.Contains(newStatus))
            throw new DomainException(
                $"'{newStatus}' is not a valid item status. Valid values: {string.Join(", ", validStatuses)}");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderItemStatus.SentToKitchen)
            SentToKitchenAt = DateTime.UtcNow;
        else if (newStatus == OrderItemStatus.Ready)
            ReadyAt = DateTime.UtcNow;
    }
}
