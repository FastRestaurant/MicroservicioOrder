using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IStockClient
{
    Task<StockOperationResultDto> ConsumeForOrderAsync(StockConsumptionRequestDto request, CancellationToken cancellationToken = default);
    Task<StockOperationResultDto> ReleaseForOrderAsync(StockReleaseRequestDto request, CancellationToken cancellationToken = default);
}
