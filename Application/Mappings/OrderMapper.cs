using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappings;

internal static class OrderMapper
{
    public static OrderResponseDto ToResponse(Order order) => new()
    {
        Id = order.Id,
        TableId = order.TableId,
        TableNumber = order.Table.Number,
        WaiterId = order.WaiterId,
        Status = order.Status.Name,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
        ClosedAt = order.ClosedAt,
        Version = order.Version,
        Items = order.Items.Select(ToResponse).ToArray(),
        Notes = order.Notes.Select(ToResponse).ToArray(),
        StatusHistory = order.StatusHistory.Select(ToResponse).ToArray()
    };

    public static OrderItemResponseDto ToResponse(OrderItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductType = item.ProductType,
        ProductNameSnapshot = item.ProductNameSnapshot,
        UnitPriceSnapshot = item.UnitPriceSnapshot,
        DurationMinutesSnapshot = item.DurationMinutesSnapshot,
        Quantity = item.Quantity,
        Status = item.Status.Name,
        Notes = item.Notes,
        SentToKitchenAt = item.SentToKitchenAt,
        ReadyAt = item.ReadyAt
    };

    public static OrderNoteResponseDto ToResponse(OrderNote note) => new()
    {
        Id = note.Id,
        CreatedByUserId = note.CreatedByUserId,
        Note = note.Note,
        CreatedAt = note.CreatedAt
    };

    public static OrderStatusHistoryDto ToResponse(OrderStatusHistory history) => new()
    {
        Id = history.Id,
        PreviousStatusName = history.PreviousStatus?.Name,
        NewStatus = history.NewStatus.Name,
        ChangedByUserId = history.ChangedByUserId,
        ChangedAt = history.ChangedAt
    };

    public static OrderSummaryDto ToSummary(Order order) => new()
    {
        Id = order.Id,
        TableId = order.TableId,
        TableNumber = order.Table.Number,
        WaiterId = order.WaiterId,
        Status = order.Status.Name,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        ItemCount = order.Items.Count
    };
}
