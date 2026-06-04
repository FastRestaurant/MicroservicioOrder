namespace OrderService.Application.DTOs;

public sealed class ProductCatalogItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool Available { get; init; }
    public int Duration { get; init; }
}
