namespace OrderService.Application.DTOs;

public sealed class StatusResponseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}
