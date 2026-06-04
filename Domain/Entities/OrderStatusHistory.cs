namespace OrderService.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int? PreviousStatusId { get; set; }
    public int NewStatusId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; }

    public Order Order { get; set; } = null!;
    public Status? PreviousStatus { get; set; }
    public Status NewStatus { get; set; } = null!;
}
