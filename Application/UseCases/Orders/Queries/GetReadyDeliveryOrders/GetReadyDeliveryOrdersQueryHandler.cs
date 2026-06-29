using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;

namespace OrderService.Application.UseCases.Orders.Queries.GetReadyDeliveryOrders;

public sealed class GetReadyDeliveryOrdersQueryHandler : IGetReadyDeliveryOrdersQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetReadyDeliveryOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyCollection<OrderResponseDto>> Handle(GetReadyDeliveryOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetActiveWithReadyItemsAsync(cancellationToken);
        return orders.Select(OrderMapper.ToResponse).ToArray();
    }
}
