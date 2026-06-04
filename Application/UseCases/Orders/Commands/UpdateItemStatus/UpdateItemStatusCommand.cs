namespace OrderService.Application.UseCases.Orders.Commands.UpdateItemStatus;

public sealed class UpdateItemStatusCommand
{
    public Guid OrderId { get; init; }
    public Guid ItemId { get; init; }
    public string NewStatus { get; init; } = string.Empty;
}
