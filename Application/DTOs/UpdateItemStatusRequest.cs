namespace OrderService.Application.DTOs;

public sealed class UpdateItemStatusRequest
{
    public string NewStatus { get; init; } = string.Empty;
}
