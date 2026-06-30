namespace OrderService.Application.DTOs;

public sealed class StockMissingItemDto
{
    public Guid? IngredientId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal RequiredQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public string? UnitType { get; init; }
}
