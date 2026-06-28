using System.Text.Json;

namespace OrderService.Infrastructure.ExternalServices;

internal static class HttpResultReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T? Deserialize<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
