using OrderService.Application.DTOs;

namespace OrderService.Application.Realtime;

public interface IOrderNotifier
{
    Task NotifyOrderCreatedAsync(OrderResponseDto order, CancellationToken cancellationToken = default);

    Task NotifyOrderStatusChangedAsync(OrderResponseDto order, CancellationToken cancellationToken = default);

    Task NotifyOrderReadyToCloseAsync(OrderResponseDto order, CancellationToken cancellationToken = default);

    Task NotifyOrderItemAddedAsync(OrderResponseDto order, CancellationToken cancellationToken = default);

    Task NotifyOrderItemRemovedAsync(OrderResponseDto order, CancellationToken cancellationToken = default);

    Task NotifyOrderItemStatusChangedAsync(OrderResponseDto order, CancellationToken cancellationToken = default);

    Task NotifyOrderNoteAddedAsync(OrderResponseDto order, CancellationToken cancellationToken = default);
}
