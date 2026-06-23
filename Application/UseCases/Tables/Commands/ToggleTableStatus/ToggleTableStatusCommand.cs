namespace OrderService.Application.UseCases.Tables.Commands.ToggleTableStatus;

public sealed class ToggleTableStatusCommand
{
    public Guid TableId { get; init; }
    public bool Enable { get; init; }
}