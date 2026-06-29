namespace OrderService.Application.DTOs;

public sealed class UpdateTablePositionRequest
{
    public decimal PositionX { get; init; }
    public decimal PositionY { get; init; }
}
