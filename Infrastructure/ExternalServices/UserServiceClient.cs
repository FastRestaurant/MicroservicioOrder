using System.Net;
using OrderService.Application.Interfaces;
using OrderService.Domain.Exceptions;

namespace OrderService.Infrastructure.ExternalServices;

public sealed class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;

    public UserServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
            return true;

        try
        {
            var response = await _httpClient.GetAsync($"api/v1/users/{userId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new DomainException("No se pudo verificar el usuario en este momento.");
        }
        catch (HttpRequestException)
        {
            throw new DomainException("No se pudo verificar el usuario en este momento.");
        }
    }
}
