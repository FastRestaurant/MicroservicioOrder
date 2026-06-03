using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public int TableNumber { get; private set; }
    public Guid WaiterId { get; private set; }
    public string Status { get; private set; } = OrderStatus.Open;
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public byte[] Version { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public ICollection<OrderNote> Notes { get; private set; } = new List<OrderNote>();
    public ICollection<OrderStatusHistory> StatusHistory { get; private set; } = new List<OrderStatusHistory>();

    private Order() { }

    public static Order Create(int tableNumber, Guid waiterId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            TableNumber = tableNumber,
            WaiterId = waiterId,
            Status = OrderStatus.Open,
            Total = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(OrderItem item)
    {
        EnsureOrderIsModifiable();
        Items.Add(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureOrderIsModifiable();

        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} not found in order.");
        Items.Remove(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public OrderStatusHistory ChangeStatus(string newStatus, Guid changedByUserId)
    {
        if (!OrderStatus.IsValidTransition(Status, newStatus))
            throw new DomainException($"Cannot transition from '{Status}' to '{newStatus}'.");

        var history = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = Id,
            PreviousStatus = Status,
            NewStatus = newStatus,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow
        };

        StatusHistory.Add(history);
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Closed)
            ClosedAt = DateTime.UtcNow;

        return history;
    }


    public OrderNote AddNote(Guid createdByUserId, string noteText)
    {
        EnsureOrderIsModifiable();

        var note = new OrderNote
        {
            Id = Guid.NewGuid(),
            OrderId = Id,
            CreatedByUserId = createdByUserId,
            Note = noteText,
            CreatedAt = DateTime.UtcNow
        };
        Notes.Add(note);
        UpdatedAt = DateTime.UtcNow;
        return note;
    }

    private void RecalculateTotal()
    {
        Total = Items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
    }

    private void EnsureOrderIsModifiable()
    {
        if (Status == OrderStatus.Closed || Status == OrderStatus.Cancelled)
            throw new DomainException(
                $"Cannot modify order in '{Status}' status. Only open orders can be modified.");
    }
}
