using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IMenuCatalogClient
{
    Task<ProductCatalogItemDto?> GetProductAsync(Guid productId, string productType, CancellationToken cancellationToken = default);
}
