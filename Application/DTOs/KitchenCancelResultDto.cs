namespace OrderService.Application.DTOs;

public sealed class KitchenCancelResultDto
{
    public bool Success { get; init; }
    public bool Blocked { get; init; }
    public string? Message { get; init; }
}
