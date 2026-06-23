namespace OrderService.Application.DTOs;

public sealed class CreateTableRequest
{
    public string Number { get; init; } = string.Empty;
    public int SeatCount { get; init; }
    public string Location { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
}
