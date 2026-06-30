namespace OrderService.Application.UseCases.Tables.Commands.UpdateTablePosition;

public sealed class UpdateTablePositionCommand
{
    public Guid TableId { get; init; }
    public decimal PositionX { get; init; }
    public decimal PositionY { get; init; }
}
