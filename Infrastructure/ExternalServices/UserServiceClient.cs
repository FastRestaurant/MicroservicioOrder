using System.Net;
using OrderService.Application.Interfaces;

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
            return false;

        var response = await _httpClient.GetAsync($"api/v1/users/{userId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        return true;
    }
}
