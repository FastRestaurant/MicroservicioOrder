namespace OrderService.Application.UseCases.Orders.Commands.AddNoteToOrder;

public sealed class AddNoteToOrderCommand
{
    public Guid OrderId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Note { get; init; } = string.Empty;
}
