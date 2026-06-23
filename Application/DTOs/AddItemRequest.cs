namespace OrderService.Application.DTOs;

public sealed class AddItemRequest
{
    public Guid ProductId { get; init; }
    public string ProductType { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}
