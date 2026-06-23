using System.Net.Http.Json;
using System.Text.Json;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.ExternalServices;

public sealed class StockClient : IStockClient
{
    private readonly HttpClient _httpClient;

    public StockClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<StockOperationResultDto> ConsumeForOrderAsync(StockConsumptionRequestDto request, CancellationToken cancellationToken = default)
        => SendStockOperationAsync("api/v1/stocks/consumptions", request, cancellationToken);

    public Task<StockOperationResultDto> ReleaseForOrderAsync(StockReleaseRequestDto request, CancellationToken cancellationToken = default)
        => SendStockOperationAsync("api/v1/stocks/releases", request, cancellationToken);

    private async Task<StockOperationResultDto> SendStockOperationAsync<TRequest>(string path, TRequest request, CancellationToken cancellationToken)
    {
        if (_httpClient.BaseAddress is null)
            return new StockOperationResultDto { Success = false, Message = "El servicio de stock no esta configurado." };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(path, request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new StockOperationResultDto { Success = false, Message = "No se pudo verificar el stock en este momento." };
        }
        catch (HttpRequestException)
        {
            return new StockOperationResultDto { Success = false, Message = "No se pudo verificar el stock en este momento." };
        }

        using var _ = response;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = Deserialize<StockOperationResultDto>(content);

        if (result is not null)
        {
            if (response.IsSuccessStatusCode)
                return result;

            return new StockOperationResultDto
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(result.Message)
                    ? "No se pudo verificar el stock del producto."
                    : result.Message,
                MissingItems = result.MissingItems
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = Deserialize<ErrorResponseDto>(content);
            return new StockOperationResultDto
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(error?.Message)
                    ? "No se pudo verificar el stock del producto."
                    : error.Message
            };
        }

        return new StockOperationResultDto { Success = false, Message = "No se pudo confirmar la operacion de stock." };
    }

    private static T? Deserialize<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
