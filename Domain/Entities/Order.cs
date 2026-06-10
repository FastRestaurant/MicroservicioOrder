using OrderService.Domain.Constants;
using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public Guid WaiterId { get; private set; }
    public int StatusId { get; private set; } = OrderStatusIds.Open;
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public byte[] Version { get; private set; } = [];

    public Status Status { get; private set; } = null!;
    public Table Table { get; private set; } = null!;

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public ICollection<OrderNote> Notes { get; private set; } = new List<OrderNote>();
    public ICollection<OrderStatusHistory> StatusHistory { get; private set; } = new List<OrderStatusHistory>();

    private Order() { }

    public static Order Create(Guid tableId, Guid waiterId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            TableId = tableId,
            WaiterId = waiterId,
            StatusId = OrderStatusIds.Open,
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
            ?? throw new DomainException($"El item {itemId} no fue encontrado en la orden.");
        Items.Remove(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public OrderStatusHistory ChangeStatus(int newStatusId, Guid changedByUserId)
    {
        if (!OrderStatusIds.IsValidTransition(StatusId, newStatusId))
            throw new DomainException($"No se puede cambiar del estado '{StatusId}' al estado '{newStatusId}'.");

        var history = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = Id,
            PreviousStatusId = StatusId,
            NewStatusId = newStatusId,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow
        };

        StatusHistory.Add(history);
        StatusId = newStatusId;
        UpdatedAt = DateTime.UtcNow;

        if (newStatusId == OrderStatusIds.Closed)
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
        if (StatusId == OrderStatusIds.Closed || StatusId == OrderStatusIds.Cancelled)
            throw new DomainException(
                $"No se puede modificar una orden en estado '{StatusId}'. Solo se pueden modificar ordenes abiertas.");
    }
}