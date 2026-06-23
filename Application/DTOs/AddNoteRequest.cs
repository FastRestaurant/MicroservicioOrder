namespace OrderService.Application.DTOs;

public sealed class AddNoteRequest
{
    public string Note { get; init; } = string.Empty;
}
