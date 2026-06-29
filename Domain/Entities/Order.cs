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

    public void RefreshTotal()
    {
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public OrderStatusHistory ChangeStatus(int newStatusId, Guid changedByUserId)
    {
        if (OrderStatusIds.IsTerminal(StatusId))
            throw new DomainException(
                "Este pedido ya fue cerrado y no admite cambios.");

        if ((newStatusId == OrderStatusIds.ReadyToClose || newStatusId == OrderStatusIds.Closed) && !CanBeClosed())
            throw new DomainException(
                "Todavía quedan productos pendientes de entrega.");

        if (!OrderStatusIds.IsValidTransition(StatusId, newStatusId))
            throw new DomainException(
                $"No se puede pasar el pedido de {GetStatusLabel(StatusId)} a {GetStatusLabel(newStatusId)}.");

        var history = OrderStatusHistory.Create(Id, StatusId, newStatusId, changedByUserId);

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

        var note = OrderNote.Create(Id, createdByUserId, noteText);
        Notes.Add(note);
        UpdatedAt = DateTime.UtcNow;
        return note;
    }

    private void RecalculateTotal()
    {
        Total = Items
            .Where(i => i.StatusId != OrderItemStatusIds.Cancelled)
            .Sum(i => i.UnitPriceSnapshot * i.Quantity);
    }

    private void EnsureOrderIsModifiable()
    {
        if (StatusId == OrderStatusIds.ReadyToClose || StatusId == OrderStatusIds.Closed || StatusId == OrderStatusIds.Cancelled)
            throw new DomainException(
                "No se pueden agregar productos porque la cuenta ya fue solicitada o el pedido está cerrado.");
    }

    private bool CanBeClosed()
        => Items.Count > 0 && Items.All(item => item.StatusId is OrderItemStatusIds.Delivered or OrderItemStatusIds.Cancelled);

    private static string GetStatusLabel(int statusId) => statusId switch
    {
        OrderStatusIds.Open => "abierto",
        OrderStatusIds.InProgress => "en preparación",
        OrderStatusIds.ReadyToClose => "cuenta solicitada",
        OrderStatusIds.Closed => "cerrado",
        OrderStatusIds.Cancelled => "cancelado",
        _ => "otro estado"
    };
}
