namespace OrderService.Application.UseCases.Tables.Commands.UpdateTable;

public sealed class UpdateTableCommand
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public int SeatCount { get; init; }
    public string Location { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
