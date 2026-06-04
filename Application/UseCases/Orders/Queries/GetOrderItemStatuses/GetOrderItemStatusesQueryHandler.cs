using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;

namespace OrderService.Application.UseCases.Orders.Queries.GetOrderItemStatuses;

public sealed class GetOrderItemStatusesQueryHandler : IGetOrderItemStatusesQueryHandler
{
    private readonly IStatusRepository _statusRepository;

    public GetOrderItemStatusesQueryHandler(IStatusRepository statusRepository)
    {
        _statusRepository = statusRepository;
    }

    public async Task<IEnumerable<StatusResponseDto>> Handle(GetOrderItemStatusesQuery query, CancellationToken cancellationToken = default)
    {
        var statuses = await _statusRepository.GetByTypeAsync(StatusTypes.OrderItem, cancellationToken);
        return statuses.Select(MapStatus);
    }

    private static StatusResponseDto MapStatus(Status status)
    {
        return new StatusResponseDto
        {
            Id = status.Id,
            Name = status.Name,
            Type = status.Type
        };
    }
}
