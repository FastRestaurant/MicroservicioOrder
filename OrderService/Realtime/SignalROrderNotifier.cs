using Microsoft.AspNetCore.SignalR;
using OrderService.Application.DTOs;
using OrderService.Application.Realtime;
using OrderService.Presentation.Hubs;

namespace OrderService.Presentation.Realtime;

/// <summary>
/// Implementacion concreta de <see cref="IOrderNotifier"/> basada en SignalR.
/// Traduce cada evento de dominio/aplicacion en un mensaje enviado a los grupos
/// de rol correspondientes del <see cref="OrderHub"/>.
/// </summary>
public sealed class SignalROrderNotifier : IOrderNotifier
{
    private const string OrderCreatedMethod = "OrderCreated";
    private const string OrderStatusChangedMethod = "OrderStatusChanged";
    private const string OrderReadyToCloseMethod = "OrderReadyToClose";
    private const string OrderItemAddedMethod = "OrderItemAdded";
    private const string OrderItemRemovedMethod = "OrderItemRemoved";
    private const string OrderItemStatusChangedMethod = "OrderItemStatusChanged";
    private const string OrderNoteAddedMethod = "OrderNoteAdded";

    private readonly IHubContext<OrderHub> _hubContext;

    public SignalROrderNotifier(IHubContext<OrderHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyOrderCreatedAsync(OrderResponseDto order, CancellationToken cancellationToken = default) =>
        SendToGroupsAsync(OrderCreatedMethod, order, cancellationToken, OrderHubGroups.Kitchen, OrderHubGroups.Admin);

    public Task NotifyOrderStatusChangedAsync(OrderResponseDto order, CancellationToken cancellationToken = default) =>
        SendToGroupsAsync(OrderStatusChangedMethod, order, cancellationToken, OrderHubGroups.Waitress, OrderHubGroups.Kitchen, OrderHubGroups.Admin);

    public Task NotifyOrderReadyToCloseAsync(OrderResponseDto order, CancellationToken cancellationToken = default) =>
        SendToGroupsAsync(OrderReadyToCloseMethod, order, cancellationToken, OrderHubGroups.Waitress, OrderHubGroups.Admin);

    public Task NotifyOrderItemAddedAsync(OrderResponseDto order, CancellationToken cancellationToken = default) =>
        SendToGroupsAsync(OrderItemAddedMethod, order, cancellationToken, OrderHubGroups.Kitchen, OrderHubGroups.Admin);

    public Task NotifyOrderItemRemovedAsync(OrderResponseDto order, CancellationToken cancellationToken = default) =>
        SendToGroupsAsync(OrderItemRemovedMethod, order, cancellationToken, OrderHubGroups.Kitchen, OrderHubGroups.Admin);

    public Task NotifyOrderItemStatusChangedAsync(OrderResponseDto order, CancellationToken cancellationToken = default) =>
        SendToGroupsAsync(OrderItemStatusChangedMethod, order, cancellationToken, OrderHubGroups.Waitress, OrderHubGroups.Admin);

    public Task NotifyOrderNoteAddedAsync(OrderResponseDto order, CancellationToken cancellationToken = default) =>
        SendToGroupsAsync(OrderNoteAddedMethod, order, cancellationToken, OrderHubGroups.Waitress, OrderHubGroups.Kitchen, OrderHubGroups.Admin);

    private Task SendToGroupsAsync(
        string method,
        OrderResponseDto order,
        CancellationToken cancellationToken,
        params string[] groups) =>
        _hubContext.Clients.Groups(groups).SendAsync(method, order, cancellationToken);
}
