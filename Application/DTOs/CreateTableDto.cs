namespace OrderService.Application.DTOs;

public sealed class CreateTableDto
{
	public string Number { get; init; } = string.Empty;
    public int SeatCount { get; init; }
    public string Location { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
}
