namespace OrderService.Application.DTOs;

public sealed class StockOperationResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyCollection<StockMissingItemDto> MissingItems { get; init; } = Array.Empty<StockMissingItemDto>();
}
