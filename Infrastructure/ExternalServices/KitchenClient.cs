using System.Net;
using System.Net.Http.Json;
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
            response = await _httpClient.PostAsJsonAsync("api/v1/kitchenOrders", request, cancellationToken);
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
        var result = HttpResultReader.Deserialize<KitchenEnqueueResultDto>(content);

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
            var error = HttpResultReader.ReadErrorMessage(content);
            return new KitchenEnqueueResultDto
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(error)
                    ? "No se pudo enviar el pedido a la cocina."
                    : error
            };
        }

        return new KitchenEnqueueResultDto { Success = false, Message = "No se pudo confirmar el envio a la cocina." };
    }

    public async Task<KitchenCancelResultDto> CancelByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
            return new KitchenCancelResultDto { Success = false, Message = "El servicio de cocina no esta configurado." };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PatchAsync($"api/v1/kitchenOrders/by-order/{orderId}/cancel", null, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new KitchenCancelResultDto { Success = false, Message = "No se pudo conectar con la cocina en este momento." };
        }
        catch (HttpRequestException)
        {
            return new KitchenCancelResultDto { Success = false, Message = "No se pudo conectar con la cocina en este momento." };
        }

        using var _ = response;

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            return new KitchenCancelResultDto { Success = true };

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var message = HttpResultReader.ReadErrorMessage(content);
            return new KitchenCancelResultDto
            {
                Success = false,
                Blocked = true,
                Message = string.IsNullOrWhiteSpace(message) ? "La cocina ya comenzo a preparar la orden." : message
            };
        }

        var error = HttpResultReader.ReadErrorMessage(content);
        return new KitchenCancelResultDto
        {
            Success = false,
            Message = string.IsNullOrWhiteSpace(error) ? "No se pudo cancelar en la cocina." : error
        };
    }
}
