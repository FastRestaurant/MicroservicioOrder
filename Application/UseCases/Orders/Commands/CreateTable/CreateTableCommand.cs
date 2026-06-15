namespace OrderService.Application.UseCases.Tables.Commands.CreateTable;

public sealed class CreateTableCommand
{
    public string Number { get; init; } = string.Empty;
    public int SeatCount { get; init; }
    public string Location { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
}
