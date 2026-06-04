namespace OrderService.Application.UseCases.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommand
{
    public Guid OrderId { get; init; }
    public string NewStatus { get; init; } = string.Empty;
    public Guid ChangedByUserId { get; init; }
}
