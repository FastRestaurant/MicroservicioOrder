namespace OrderService.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; }

    public Order Order { get; set; } = null!;
}
