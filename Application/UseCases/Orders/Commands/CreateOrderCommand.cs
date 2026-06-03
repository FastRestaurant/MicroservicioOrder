namespace OrderService.Application.UseCases.Orders.Commands;

public record CreateOrderCommand(int TableNumber, Guid WaiterId);

public record AddItemToOrderCommand(
    Guid OrderId,
    Guid ProductId,
    string ProductType,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string? Notes
);

public record RemoveItemFromOrderCommand(Guid OrderId, Guid ItemId);

public record ChangeOrderStatusCommand(Guid OrderId, string NewStatus, Guid ChangedByUserId);

public record AddNoteToOrderCommand(Guid OrderId, Guid CreatedByUserId, string Note);

public record UpdateItemStatusCommand(Guid OrderId, Guid ItemId, string NewStatus);
