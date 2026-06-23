using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IKitchenClient
{
    Task<KitchenEnqueueResultDto> EnqueueOrderAsync(KitchenTicketRequestDto request, CancellationToken cancellationToken = default);
}
