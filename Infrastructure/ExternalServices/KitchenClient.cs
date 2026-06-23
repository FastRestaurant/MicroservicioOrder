using System.Net.Http.Json;
using System.Text.Json;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.ExternalServices;

public sealed class KitchenClient : IKitchenClient
{
    private readonly HttpClient _httpClient;

    public KitchenClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<KitchenEnqueueResultDto> EnqueueOrderAsync(KitchenTicketRequestDto request, CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
            return new KitchenEnqueueResultDto { Success = false, Message = "El servicio de cocina no esta configurado." };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("api/KitchenOrders", request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new KitchenEnqueueResultDto { Success = false, Message = "No se pudo avisar a la cocina en este momento." };
        }
        catch (HttpRequestException)
        {
            return new KitchenEnqueueResultDto { Success = false, Message = "No se pudo avisar a la cocina en este momento." };
        }

        using var _ = response;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = Deserialize<KitchenEnqueueResultDto>(content);

        if (result is not null)
        {
            if (response.IsSuccessStatusCode)
                return result;

            return new KitchenEnqueueResultDto
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(result.Message)
                    ? "No se pudo enviar el pedido a la cocina."
                    : result.Message,
                KitchenOrderId = result.KitchenOrderId
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = Deserialize<ErrorResponseDto>(content);
            return new KitchenEnqueueResultDto
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(error?.Message)
                    ? "No se pudo enviar el pedido a la cocina."
                    : error.Message
            };
        }

        return new KitchenEnqueueResultDto { Success = false, Message = "No se pudo confirmar el envio a la cocina." };
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
