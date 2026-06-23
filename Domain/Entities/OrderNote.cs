namespace OrderService.Domain.Entities;

public class OrderNote
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string Note { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public Order Order { get; private set; } = null!;

    private OrderNote() { }

    public static OrderNote Create(Guid orderId, Guid createdByUserId, string note) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        CreatedByUserId = createdByUserId,
        Note = note,
        CreatedAt = DateTime.UtcNow
    };
}
