namespace OrderService.Application.DTOs;

public sealed class StockMissingItemDto
{
    public Guid? IngredientId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int RequiredQuantity { get; init; }
    public int AvailableQuantity { get; init; }
}
