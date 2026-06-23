namespace OrderService.Application.DTOs;

public sealed class ChangeStatusRequest
{
    public string NewStatus { get; init; } = string.Empty;
}
