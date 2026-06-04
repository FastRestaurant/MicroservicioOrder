using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public static class AppDbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var statuses = new[]
        {
            new Status(OrderStatusIds.Open, "Open", StatusTypes.Order),
            new Status(OrderStatusIds.InProgress, "InProgress", StatusTypes.Order),
            new Status(OrderStatusIds.ReadyToClose, "ReadyToClose", StatusTypes.Order),
            new Status(OrderStatusIds.Closed, "Closed", StatusTypes.Order),
            new Status(OrderStatusIds.Cancelled, "Cancelled", StatusTypes.Order),
            new Status(OrderItemStatusIds.Pending, "Pending", StatusTypes.OrderItem),
            new Status(OrderItemStatusIds.SentToKitchen, "SentToKitchen", StatusTypes.OrderItem),
            new Status(OrderItemStatusIds.Ready, "Ready", StatusTypes.OrderItem),
            new Status(OrderItemStatusIds.Delivered, "Delivered", StatusTypes.OrderItem),
            new Status(OrderItemStatusIds.Cancelled, "Cancelled", StatusTypes.OrderItem)
        };

        var existingStatusIds = await context.Statuses
            .Select(status => status.Id)
            .ToListAsync(cancellationToken);

        var missingStatuses = statuses
            .Where(status => !existingStatusIds.Contains(status.Id))
            .ToArray();

        if (missingStatuses.Length == 0)
            return;

        await context.Statuses.AddRangeAsync(missingStatuses, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
