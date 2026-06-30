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

    public static string? ReadErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return null;

            if (TryGetString(root, "message", out var message))
                return message;
            if (TryGetString(root, "detail", out var detail))
                return detail;
            if (TryGetString(root, "title", out var title))
                return title;

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool TryGetString(JsonElement parent, string name, out string? value)
    {
        value = null;

        if (parent.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.String)
        {
            value = element.GetString();
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }
}
