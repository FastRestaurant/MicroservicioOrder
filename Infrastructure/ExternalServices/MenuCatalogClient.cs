using System.Net;
using System.Net.Http.Json;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Constants;

namespace OrderService.Infrastructure.ExternalServices;

public sealed class MenuCatalogClient : IMenuCatalogClient
{
    private readonly HttpClient _httpClient;

    public MenuCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductCatalogItemDto?> GetProductAsync(Guid productId, string productType, CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
            return null;

        var resource = productType == ProductTypes.Dish ? "dishes" : "drinks";
        var response = await _httpClient.GetAsync($"api/menu-integration/{resource}/{productId}/for-order", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<MenuProductResponseDto>(cancellationToken: cancellationToken);

        return product is null
            ? null
            : new ProductCatalogItemDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Available = product.Available,
                Duration = product.Duration > 0 ? product.Duration : product.EstimatedPreparationMinutes
            };
    }

    private sealed class MenuProductResponseDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public bool Available { get; init; }
        public int Duration { get; init; }
        public int EstimatedPreparationMinutes { get; init; }
    }
}
