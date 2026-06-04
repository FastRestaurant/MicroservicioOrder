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
        var response = await _httpClient.GetAsync($"api/v1/{resource}/{productId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductCatalogItemDto>(cancellationToken: cancellationToken);
    }
}
