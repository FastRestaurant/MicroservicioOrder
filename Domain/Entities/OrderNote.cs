namespace OrderService.Domain.Entities;

public class OrderNote
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Order Order { get; set; } = null!;
}
