namespace OrderService.Application.DTOs;

public sealed class TableResponseDto
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public int SeatCount { get; init; }
    public string Location { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string OperationalStatus { get; init; } = string.Empty;
    public Guid? ActiveWaiterId { get; init; }
}
