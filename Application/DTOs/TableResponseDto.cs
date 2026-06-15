namespace OrderService.Application.DTOs;

public sealed class TableResponseDto
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public bool Status { get; init; }
}