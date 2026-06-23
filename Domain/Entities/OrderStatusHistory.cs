namespace OrderService.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public int? PreviousStatusId { get; private set; }
    public int NewStatusId { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }

    public Order Order { get; private set; } = null!;
    public Status? PreviousStatus { get; private set; }
    public Status NewStatus { get; private set; } = null!;

    private OrderStatusHistory() { }

    public static OrderStatusHistory Create(Guid orderId, int? previousStatusId, int newStatusId, Guid changedByUserId) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        PreviousStatusId = previousStatusId,
        NewStatusId = newStatusId,
        ChangedByUserId = changedByUserId,
        ChangedAt = DateTime.UtcNow
    };
}
