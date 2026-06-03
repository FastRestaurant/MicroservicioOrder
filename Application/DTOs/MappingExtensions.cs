using OrderService.Domain.Entities;

namespace OrderService.Application.DTOs;

public static class MappingExtensions
{
    public static OrderResponseDto ToResponseDto(this Order o) => new(
        o.Id, o.TableNumber, o.WaiterId, o.Status, o.Total,
        o.CreatedAt, o.UpdatedAt, o.ClosedAt, o.Version,
        o.Items.Select(i => i.ToDto()).ToList(),
        o.Notes.Select(n => n.ToDto()).ToList(),
        o.StatusHistory.Select(h => h.ToDto()).ToList()
    );

    public static OrderSummaryDto ToSummaryDto(this Order o) => new(
        o.Id, o.TableNumber, o.WaiterId, o.Status, o.Total, o.CreatedAt, o.Items.Count
    );

    public static OrderItemResponseDto ToDto(this OrderItem i) => new(
        i.Id, i.ProductId, i.ProductType, i.ProductNameSnapshot,
        i.UnitPriceSnapshot, i.Quantity, i.Status, i.Notes,
        i.SentToKitchenAt, i.ReadyAt
    );

    public static OrderNoteResponseDto ToDto(this OrderNote n) => new(
        n.Id, n.CreatedByUserId, n.Note, n.CreatedAt
    );

    public static OrderStatusHistoryDto ToDto(this OrderStatusHistory h) => new(
        h.Id, h.PreviousStatus, h.NewStatus, h.ChangedByUserId, h.ChangedAt
    );
}
