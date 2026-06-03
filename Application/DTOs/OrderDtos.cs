namespace OrderService.Application.DTOs;

public record OrderResponseDto(
    Guid Id,
    int TableNumber,
    Guid WaiterId,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ClosedAt,
    byte[] Version,
    List<OrderItemResponseDto> Items,
    List<OrderNoteResponseDto> Notes,
    List<OrderStatusHistoryDto> StatusHistory
);

public record OrderSummaryDto(
    Guid Id,
    int TableNumber,
    Guid WaiterId,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    int ItemCount
);

public record OrderItemResponseDto(
    Guid Id,
    Guid ProductId,
    string ProductType,
    string ProductNameSnapshot,
    decimal UnitPriceSnapshot,
    int Quantity,
    string Status,
    string? Notes,
    DateTime? SentToKitchenAt,
    DateTime? ReadyAt
    );

public record OrderNoteResponseDto(
    Guid Id,
    Guid CreatedByUserId,
    string Note,
    DateTime CreatedAt
);

public record OrderStatusHistoryDto(
    Guid Id,
    string? PreviousStatus,
    string NewStatus,
    Guid ChangedByUserId,
    DateTime ChangedAt
);
