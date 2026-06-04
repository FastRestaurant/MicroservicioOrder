namespace OrderService.Application.DTOs;

public sealed class OrderNoteResponseDto
{
    public Guid Id { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Note { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
