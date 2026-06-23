namespace OrderService.Application.DTOs;

public sealed class KitchenEnqueueResultDto
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public Guid? KitchenOrderId { get; init; }
}
