namespace OrderService.Application.DTOs;

public sealed class ErrorResponseDto
{
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public DateTime Timestamp { get; init; }
}
